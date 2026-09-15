"""Validate the generated WAV pack and produce a machine-readable report."""

import hashlib
import json
import math
import struct
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parent
manifest = json.loads((ROOT / "audio_manifest.json").read_text(encoding="utf-8"))
results = []
all_ok = True
for declared in manifest["assets"]:
    path = ROOT / declared["file"]
    raw = path.read_bytes()
    with wave.open(str(path), "rb") as wav:
        channels = wav.getnchannels()
        width = wav.getsampwidth()
        rate = wav.getframerate()
        frames = wav.getnframes()
        compression = wav.getcomptype()
        payload = wav.readframes(frames)
    samples = struct.unpack("<" + "h" * (len(payload) // 2), payload)
    peak = max(abs(v) for v in samples)
    clipped = sum(abs(v) >= 32767 for v in samples)
    peak_db = 20 * math.log10(max(peak / 32767.0, 1e-12))
    checks = {
        "riff_wave_header": raw[:4] == b"RIFF" and raw[8:12] == b"WAVE",
        "pcm_16bit": width == 2 and compression == "NONE",
        "sample_rate_44100": rate == 44100,
        "declared_channels_match": channels == declared["channels"],
        "declared_frames_match": frames == declared["frames"],
        "sha256_match": hashlib.sha256(raw).hexdigest() == declared["sha256"],
        "no_clipped_samples": clipped == 0,
        "audible_nonzero_peak": peak > 100,
    }
    item = {
        "file": declared["file"], "duration_seconds": round(frames / rate, 6),
        "peak_dbfs": round(peak_db, 3), "clipped_samples": clipped, "checks": checks,
    }
    if declared["category"] == "music_loop":
        # Adjacent-sample discontinuity at wrap, normalized to full scale.
        seam_per_channel = []
        for channel in range(channels):
            first = samples[channel]
            last = samples[(frames - 1) * channels + channel]
            seam_per_channel.append(abs(first - last) / 32767.0)
        item["loop_seam_max_sample_jump"] = round(max(seam_per_channel), 8)
        item["checks"]["loop_seam_below_minus_40dbfs"] = max(seam_per_channel) < 0.01
        item["checks"]["exact_declared_loop_bounds"] = declared["loop_start_frame"] == 0 and declared["loop_end_frame_exclusive"] == frames
    item["ok"] = all(checks.values()) and all(item["checks"].values())
    all_ok = all_ok and item["ok"]
    results.append(item)

report = {"status": "PASS" if all_ok else "FAIL", "asset_count": len(results), "results": results}
(ROOT / "verification_report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report, indent=2))
raise SystemExit(0 if all_ok else 1)
