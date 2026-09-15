# Sunleaf Procedural Audio Batch 1

This isolated staging pack contains original, deterministic procedural audio for the Sunleaf 2D platformer demo. It uses no recordings, samples, sound libraries, or third-party source assets.

## Contents

- Two seamless stereo music loops: title and exploration
- Player movement: jump, double jump, and land
- Weapons: sword swing and wand cast
- Rewards: leaf pickup and checkpoint bloom
- Combat feedback: enemy hit and defeat
- Interface: confirm and pause

All files are RIFF/WAVE PCM, 16-bit, 44.1 kHz. Music is stereo; one-shots are mono. Exact durations, peak levels, hashes, and loop bounds are recorded in `audio_manifest.json`. Automated validation is recorded in `verification_report.json`.

## Provenance and use

The synthesis code in `generate_audio.py` constructs every waveform mathematically with fixed constants and PRNG seeds. The project may use, modify, and redistribute the resulting audio royalty-free. Regenerate with Python 3 by running `python generate_audio.py`, followed by `python validate_audio.py`.

Automated checks confirm file structure, declared metadata, hashes, peak levels, clipping, and music-loop boundary continuity. No subjective listening test is claimed.
