# Sunleaf release candidate checklist

## Delivery state

- Verified baseline: `5e6edc1`.
- Baseline record: gate and target art integrated; clean Windows build recorded at this baseline.
- Current status: release candidate work in progress; client-ready is **not declared**.
- Parallel ownership: Codex is implementing the dedicated wand sheet, persistent Music/SFX audio sliders, and the expanded-route foundation.
- Scope rule: this checklist records evidence only. Unobserved outcomes remain `Not run`; no completion is inferred from planned work or asset presence.

## Release gates

| Gate | Result | Evidence / acceptance condition | Owner / next action |
| --- | --- | --- | --- |
| Verified baseline is `5e6edc1` | Passed | Current delivery baseline supplied for this handoff | Codex: preserve as comparison point |
| Gate and target art integrated | Passed | Integrated state supplied for current baseline | Codex: confirm in the release candidate build |
| Clean Windows build | Passed | Clean build supplied for current baseline | Codex: repeat after wand/audio/route integration |
| Expanded route foundation | Not run | Candidate route exists and supports the intended progression without replacing the protected baseline prematurely | Codex: implement, then run route qualification |
| Dedicated wand sheet | Not run | Wand identity is distinct from sword pixels and reads correctly in gameplay | Codex: integrate, then verify in controlled route run |
| Persistent Music/SFX sliders | Not run | Settings survive restart and affect the intended channels without breaking mute/reduced-effects behavior | Codex: implement, then verify in build |
| Critical gameplay defects resolved | Not run | No release-blocking route, combat, checkpoint, input, audio, or pause/focus defects | Codex: record defects and retest |
| Client art approval | Not run | Client reviews integrated visual package and accepts outstanding visual decisions | Client/Codex: obtain explicit approval |
| Client listening approval | Not run | Client reviews integrated mix and accepts levels, repetition, music/SFX controls, and reduced-effects behavior | Client/Codex: obtain explicit approval |

## Player test protocol

### Test setup

- Use the release-candidate Windows build, not an editor run.
- Use five players who have not previously played this route. Record player IDs as `P1`–`P5`; do not record unnecessary personal data.
- Use the same documented resolution/settings for every session and record any deviation.
- Do not coach route solutions. Explain controls only, then let the player discover the route.
- Start timing when the player receives control. Stop timing at the first completed route/ending condition defined by the candidate build.
- Record exact timestamps, deaths, checkpoint recoveries, control device, observed confusion, defects, and player comments.
- A session is valid only when the build, route version, settings, and completion condition are recorded.

### Per-player session sheet

| Field | P1 | P2 | P3 | P4 | P5 |
| --- | --- | --- | --- | --- | --- |
| Build / route identifier | Not run | Not run | Not run | Not run | Not run |
| Device / input mode | Not run | Not run | Not run | Not run | Not run |
| First-completion time | Not run | Not run | Not run | Not run | Not run |
| Within 10–15 minute target | Not run | Not run | Not run | Not run | Not run |
| Death count and locations | Not run | Not run | Not run | Not run | Not run |
| Checkpoint reached/restarted correctly | Not run | Not run | Not run | Not run | Not run |
| Sword use understood and functional | Not run | Not run | Not run | Not run | Not run |
| Wand use understood and functional | Not run | Not run | Not run | Not run | Not run |
| Moving platform: waits/stands idle safely | Not run | Not run | Not run | Not run | Not run |
| Pause/resume behavior | Not run | Not run | Not run | Not run | Not run |
| Focus loss/regain behavior | Not run | Not run | Not run | Not run | Not run |
| Route comprehension / confusion | Not run | Not run | Not run | Not run | Not run |
| Completion condition understood | Not run | Not run | Not run | Not run | Not run |
| Player feedback | Not run | Not run | Not run | Not run | Not run |

### Session procedure

1. Confirm build version, route version, resolution, audio/effects settings, and input device.
2. Explain only the available controls and the objective to reach the ending.
3. Start the timer when control begins; observe without route coaching.
4. Log every death with location, cause, recovery result, and whether the checkpoint was clear and fair.
5. Verify sword and wand discovery, switching/selection, targeting, feedback, and any ability gate.
6. At the moving platform, record whether standing still/idle remains safe and readable before, during, and after motion.
7. Test pause/resume and focus loss/regain when naturally safe; if not reached or not safe to interrupt, record `Not run`.
8. Complete the route or stop at the session limit; record exact outcome and timestamp.
9. Ask what was unclear, frustrating, memorable, or missing. Do not convert opinions into pass/fail without an agreed criterion.

## Focused qualification matrix

| Area | Result | Required evidence |
| --- | --- | --- |
| First completion is measured for five fresh players | Not run | Five valid session sheets; report median, range, and each exact time |
| First-completion target is 10–15 minutes | Not run | Measured times from the five sessions; explain outliers rather than discarding them |
| Deaths and checkpoints | Not run | Death/recovery log covering each observed death and checkpoint restart |
| Sword | Not run | Controlled route evidence of discovery, use, feedback, and recovery after failure |
| Wand | Not run | Dedicated wand sheet integrated; evidence of distinct visuals, discovery, use, feedback, and route relevance |
| Moving platform idle behavior | Not run | Safe idle observation and behavior under platform motion |
| Pause and focus | Not run | Pause/resume, focus loss, focus regain, and input suppression/restoration evidence |
| Controller | Not run | Physical controller session covering navigation, combat, pause, restart, and exit |
| Touch | Not run | Physical touch session on the supported target device/window; record unsupported states explicitly |
| Sound | Not run | Music/SFX levels, mute, repetition, restart persistence, and gameplay feedback observation |
| Reduced effects | Not run | Optional motion/particle reduction retains readable gameplay feedback and survives restart if intended |
| Performance | Not run | Actual test-machine frame time/memory, resolution/settings, and attributable warnings |
| Client art approval | Not run | Explicit review decision on integrated art and remaining visual issues |
| Client listening approval | Not run | Explicit review decision on mix, controls, persistence, and reduced-effects behavior |
| Windows package | Not run | Repeat clean build after current Codex work; zero errors and resolved/attributed warnings |
| Graceful exit / README / known issues | Not run | Packaged executable, controls, provenance, README, known-issues list, and exit verification |

## Exit criteria

Release candidate can be proposed for client review only when the updated Windows build passes the combined qualification matrix, all five fresh-player results are recorded, critical defects are resolved or explicitly accepted, and remaining `Not run` items are either completed or declared as client-visible limitations. Client-ready requires explicit client art and listening approval in addition to the technical evidence.

## Current blockers

- Dedicated wand sheet is still being implemented by Codex.
- Persistent Music/SFX sliders are still being implemented by Codex.
- Expanded-route foundation has not yet been qualified against the 10–15 minute target.
- Five fresh-player sessions, physical controller/touch checks, pause/focus verification, reduced-effects/audio persistence, performance measurements, and client art/listening approvals are `Not run`.
- A post-integration clean Windows build and combined release qualification remain required after the current Codex changes.
