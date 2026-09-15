"""Deterministic procedural audio pack for the Sunleaf 2D platformer demo.

All synthesis uses only Python's standard library. No recorded, sampled, or
third-party source material is used.
"""

from __future__ import annotations

import hashlib
import json
import math
import random
import struct
import wave
from pathlib import Path


RATE = 44100
ROOT = Path(__file__).resolve().parent
OUT = ROOT / "wav"
OUT.mkdir(exist_ok=True)


def clamp(x: float) -> float:
    return max(-1.0, min(1.0, x))


def soft(x: float) -> float:
    return math.tanh(x)


def env(t: float, duration: float, attack: float, release: float) -> float:
    return min(1.0, t / max(attack, 1e-6), (duration - t) / max(release, 1e-6))


def pan(x: float, p: float) -> tuple[float, float]:
    angle = (p + 1.0) * math.pi / 4.0
    return x * math.cos(angle), x * math.sin(angle)


def write_wav(name: str, channels: list[list[float]], target_peak_db: float) -> dict:
    frames = len(channels[0])
    peak = max(abs(v) for channel in channels for v in channel) or 1.0
    target = 10 ** (target_peak_db / 20.0)
    scale = min(1.0, target / peak)
    quantized = [[int(round(clamp(v * scale) * 32767)) for v in c] for c in channels]
    path = OUT / name
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(len(channels))
        wav.setsampwidth(2)
        wav.setframerate(RATE)
        payload = bytearray()
        for i in range(frames):
            for channel in quantized:
                payload.extend(struct.pack("<h", channel[i]))
        wav.writeframes(payload)
    raw = path.read_bytes()
    measured = max(abs(v) for c in quantized for v in c) / 32767.0
    return {
        "file": f"wav/{name}",
        "sha256": hashlib.sha256(raw).hexdigest(),
        "sample_rate_hz": RATE,
        "channels": len(channels),
        "bit_depth": 16,
        "frames": frames,
        "duration_seconds": round(frames / RATE, 6),
        "peak_dbfs": round(20 * math.log10(max(measured, 1e-12)), 3),
    }


def periodic_note(buf_l, buf_r, start, length, freq, amp, position, color=0.25):
    # A compact, warm pulse/pluck whose release reaches zero within the loop.
    end = min(len(buf_l), start + length)
    for i in range(start, end):
        t = (i - start) / RATE
        e = (1.0 - math.exp(-t * 55.0)) * math.exp(-t * 5.2)
        phase = 2 * math.pi * freq * t
        x = amp * e * (math.sin(phase) + color * math.sin(2 * phase) + 0.10 * math.sin(3 * phase))
        l, r = pan(x, position)
        buf_l[i] += l
        buf_r[i] += r


def make_music(name: str, mode: str) -> dict:
    seconds = 16.0
    n = int(seconds * RATE)
    left = [0.0] * n
    right = [0.0] * n
    beat = int(0.5 * RATE)  # 120 BPM, 32 beats, exact loop length.
    if mode == "title":
        progression = [(261.626, 329.628, 392.0), (220.0, 261.626, 329.628),
                       (174.614, 220.0, 261.626), (196.0, 246.942, 293.665)]
        melody = [659.255, 783.991, 880.0, 783.991, 659.255, 587.33, 523.251, 587.33]
    else:
        progression = [(293.665, 369.994, 440.0), (246.942, 293.665, 369.994),
                       (196.0, 246.942, 293.665), (220.0, 277.183, 329.628)]
        melody = [587.33, 659.255, 739.989, 880.0, 739.989, 659.255, 587.33, 493.883]
    for b in range(32):
        chord = progression[(b // 8) % 4]
        for j, f in enumerate(chord):
            periodic_note(left, right, b * beat, int(0.44 * RATE), f, 0.16, [-0.45, 0.0, 0.45][j], 0.18)
        if b % 2 == 0:
            periodic_note(left, right, b * beat, int(0.38 * RATE), melody[(b // 2) % 8], 0.13, 0.30, 0.12)
        # Rounded bass, deliberately released before the boundary.
        periodic_note(left, right, b * beat, int(0.46 * RATE), chord[0] / 2, 0.20, -0.15, 0.06)
    # Gentle periodic air and a one-beat circular echo; fixed seed for reproducibility.
    rng = random.Random(1701 if mode == "title" else 1702)
    smooth_noise = 0.0
    for i in range(n):
        smooth_noise = 0.997 * smooth_noise + 0.003 * rng.uniform(-1, 1)
        shimmer = 0.009 * smooth_noise * (0.65 + 0.35 * math.sin(2 * math.pi * i / n * 8))
        left[i] += shimmer
        right[i] -= shimmer
    dry_l, dry_r = left[:], right[:]
    delay = beat
    for i in range(n):
        left[i] += 0.13 * dry_r[(i - delay) % n]
        right[i] += 0.13 * dry_l[(i - delay) % n]
    spec = write_wav(name, [left, right], -3.0)
    spec.update({"category": "music_loop", "loop_start_frame": 0, "loop_end_frame_exclusive": n})
    return spec


def tone_sfx(name, duration, synth, peak=-1.5, seed=0, category="sfx"):
    n = int(duration * RATE)
    rng = random.Random(seed)
    mono = [0.0] * n
    for i in range(n):
        t = i / RATE
        mono[i] = synth(t, duration, rng)
    spec = write_wav(name, [mono], peak)
    spec["category"] = category
    return spec


def sweep(t, d, f0, f1, curve=1.0):
    # Analytic approximation suitable for short game sounds.
    q = (t / d) ** curve
    return 2 * math.pi * (f0 * t + (f1 - f0) * t * q / (curve + 1.0))


assets = []
assets.append(make_music("music_title_sunrise_loop.wav", "title"))
assets.append(make_music("music_exploration_grove_loop.wav", "explore"))

assets.append(tone_sfx("sfx_player_jump.wav", 0.26,
    lambda t,d,r: env(t,d,.006,.055) * (0.78*math.sin(sweep(t,d,210,690,1.2)) + .13*math.sin(sweep(t,d,420,1380,1.2))), seed=101))
assets.append(tone_sfx("sfx_player_double_jump.wav", 0.34,
    lambda t,d,r: env(t,d,.006,.07) * (.54*math.sin(sweep(t,d,310,980,.8)) + .30*math.sin(sweep(t,d,460,1470,.8)) + .08*r.uniform(-1,1)*math.exp(-t*18)), seed=102))
assets.append(tone_sfx("sfx_player_land.wav", 0.20,
    lambda t,d,r: env(t,d,.002,.09) * (.50*r.uniform(-1,1)*math.exp(-t*28) + .45*math.sin(sweep(t,d,125,52,1))*math.exp(-t*18)), seed=103))
assets.append(tone_sfx("sfx_weapon_sword_swing.wav", 0.24,
    lambda t,d,r: env(t,d,.004,.055) * (.46*r.uniform(-1,1)*math.sin(math.pi*min(1,t/d)) + .42*math.sin(sweep(t,d,1250,210,.65))), seed=104))
assets.append(tone_sfx("sfx_weapon_wand_cast.wav", 0.42,
    lambda t,d,r: env(t,d,.005,.09) * (.40*math.sin(sweep(t,d,440,1450,.7)) + .27*math.sin(sweep(t,d,660,2175,.7)) + .12*math.sin(2*math.pi*13*t)*math.sin(sweep(t,d,880,1740,1))), seed=105))
assets.append(tone_sfx("sfx_pickup_leaf.wav", 0.46,
    lambda t,d,r: env(t,d,.004,.10) * sum(.24*math.sin(2*math.pi*f*t)*math.exp(-max(0,t-o)*10) * (1 if t>=o else 0) for f,o in [(659.255,0),(830.609,.07),(987.767,.14),(1318.51,.21)]), seed=106))
assets.append(tone_sfx("sfx_checkpoint_bloom.wav", 0.90,
    lambda t,d,r: env(t,d,.012,.18) * (.28*math.sin(2*math.pi*523.251*t)+.24*math.sin(2*math.pi*659.255*t)+.20*math.sin(2*math.pi*783.991*t)+.10*math.sin(2*math.pi*1046.5*t)), peak=-2.0, seed=107))
assets.append(tone_sfx("sfx_enemy_hit.wav", 0.23,
    lambda t,d,r: env(t,d,.002,.07) * (.48*r.uniform(-1,1)*math.exp(-t*24)+.54*math.sin(sweep(t,d,260,95,.8))*math.exp(-t*12)), seed=108))
assets.append(tone_sfx("sfx_enemy_defeat.wav", 0.58,
    lambda t,d,r: env(t,d,.003,.12) * (.34*r.uniform(-1,1)*math.exp(-t*8)+.48*math.sin(sweep(t,d,340,48,.7))+.15*math.sin(sweep(t,d,680,90,.7))), seed=109))
assets.append(tone_sfx("sfx_ui_confirm.wav", 0.18,
    lambda t,d,r: env(t,d,.002,.055) * (.48*math.sin(2*math.pi*740*t)+.34*math.sin(2*math.pi*1110*t))*math.exp(-t*10), seed=110, category="ui"))
assets.append(tone_sfx("sfx_ui_pause.wav", 0.27,
    lambda t,d,r: env(t,d,.002,.065) * (.42*math.sin(2*math.pi*(440 if t<.09 else 330)*t)+.24*math.sin(2*math.pi*(880 if t<.09 else 660)*t))*math.exp(-t*8), seed=111, category="ui"))

manifest = {
    "pack": "Sunleaf Procedural Audio Batch 1",
    "license": "Original work generated procedurally for this project; no samples, recordings, or third-party assets used. Project may use, modify, and redistribute these files royalty-free.",
    "generator": "generate_audio.py",
    "determinism": "Fixed algorithms, constants, and PRNG seeds; Python standard library only.",
    "format": "RIFF/WAVE PCM signed 16-bit little-endian; 44,100 Hz",
    "integration_notes": "Music files are stereo and intended to loop from frame 0 to loop_end_frame_exclusive. SFX/UI files are mono one-shots. Unity import suggestion: load music as Streaming with Loop enabled; use Decompress On Load for short effects.",
    "assets": assets,
}
(ROOT / "audio_manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
print(f"Generated {len(assets)} WAV assets in {OUT}")
