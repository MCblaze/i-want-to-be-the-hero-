# QP4 Release Qualification

## Scope and evidence boundary

- **Start commit:** `4510a2d`
- This record documents Thread 6 release qualification only.
- The generic test action that timed out, overlapped, and was cancelled is **discarded**. It is not evidence and is not counted as a failure or pass.
- Thread 6's original evidence boundary is preserved below. Thread 7 qualification was subsequently run from an isolated local worktree at commit `19caaf2`; its verified evidence is recorded at the end of this file.

## Qualification status

| Area | Status | Verified result / record |
|---|---|---|
| Animation audit | **Passed** | 6/6 passed earlier. |
| QP2 clean `MovementTests` | **Passed** | 10/10 passed. |
| QP3 full `MovementTests` batch | **Passed with warning** | 9/10. `JumpMeasurements` was 0.0057 units below tolerance. |
| QP3 isolated `JumpMeasurements` rerun | **Passed** | Exact test rerun passed 1/1. |
| QP3 structure | **Passed** | 6 bands; 6 parallax anchors; 15 bounded motions; 1 resettable ambient inhabitant; 68 active `SpriteRenderer`s; 1 `ParticleSystem`; 1 `AudioSource`. |
| QP4 clean `MovementTests` | **Passed** | 10/10 passed in isolated Unity batch mode; the earlier timing discrepancy is cleared. |
| Generic test action | **Discarded** | Timed out/overlapped and was cancelled; no evidentiary value. |
| Lifecycle qualification | **Passed** | 4/4 `LifecycleTests` passed in isolated Unity batch mode, including 20 restarts and 10 falls. |
| Windows build | **Passed** | Windows x64 build succeeded: 0 errors, 1 warning, 117,269,713 bytes, Main scene included. |
| Standalone launch | **Passed with shutdown limitation** | Built player reached a running, responsive state in two checks. Automated graceful close was unavailable; forced test cleanup generated shutdown crash traces and is not treated as a gameplay crash. |
| Physical controller qualification | **Not run / open** | No verified result. |
| Human five-player observations | **Not run / open** | No verified result. |
| Reduced-effects controls | **Implemented; automated pass** | F1/title control stops optional particles and presentation motion; targeted test passed. Fresh-player readability remains open. |
| Muted-audio cue qualification | **Implemented; automated pass** | F2/title control mutes `AudioListener` while retaining visible status/message cues; targeted test passed. Human cue reading remains open. |
| Fresh-player timing | **Not run / open** | No verified result. |

## Passed evidence

1. Animation audit: 6/6 passed.
2. QP2 clean `MovementTests`: 10/10 passed.
3. QP3 structure inventory passed with the six-band, six-anchor, 15-motion, one-inhabitant, 68-sprite-renderer, one-particle-system, and one-audio-source counts listed above.
4. The exact QP3 `JumpMeasurements` case passed when rerun in isolation: 1/1.

## Known warnings

- The QP3 full-suite result was 9/10 because `JumpMeasurements` measured 0.0057 units below tolerance in that batch. Thread 7's clean, non-overlapping 10/10 `MovementTests` run passed `JumpMeasurements`, clearing that timing discrepancy.
- The generic test action timed out/overlapped and was cancelled. Treat it as discarded, not as a failed test and not as supporting evidence.
- The original in-editor Test Runner transition remained unreliable. Thread 7 bypassed it with an isolated worktree and Unity batch mode; lifecycle tests and the Windows build then completed successfully.
- The standalone process was responsive after startup, but the automation surface could not issue a normal window close. Forced cleanup generated expected crash-handler output, so clean shutdown remains unqualified.
- All human, controller, accessibility/effects, muted-audio, and fresh-player timing evidence remains open.

## Release blockers

The following remain release blockers until explicitly qualified:

1. Physical controller input is verified through the intended movement and jump route.
2. Five-player human observation session is completed against the sheet below.
3. Reduced-effects and muted-audio visual fallbacks are checked by fresh players for required readability.
4. Fresh-player timing is measured against the Thread 6 target and recorded.

## Five-player observation sheet and targets

Complete one row per fresh player. Do not substitute developer observation for a player row. Record the actual result, not an estimate.

| Player | Fresh player / no coaching | Time to first successful jump | Time to complete intended route | Deaths / resets | First confusion or hesitation | Controller result | Reduced-effects result | Muted-audio cue result | Pass / follow-up |
|---|---|---|---|---:|---|---|---|---|---|
| P1 | Open | Open | Open | Open | Open | Open | Open | Open | Open |
| P2 | Open | Open | Open | Open | Open | Open | Open | Open | Open |
| P3 | Open | Open | Open | Open | Open | Open | Open | Open | Open |
| P4 | Open | Open | Open | Open | Open | Open | Open | Open | Open |
| P5 | Open | Open | Open | Open | Open | Open | Open | Open | Open |

### Five-player targets

- **Sample:** 5 fresh players, recorded separately as P1–P5.
- **Route target:** each player attempts the intended route from the first playable start through the release-critical route; record where a player stops rather than silently skipping the route.
- **Timing target:** capture first successful jump time and full intended-route time for every player. Fresh-player timing remains open until the Thread 6 target is measured and met.
- **Observation target:** capture the first confusion/hesitation, any missed cue, and every reset/death for every player.
- **Controller target:** each player uses the physical controller configuration intended for release; record pass/fail and the failing action if applicable.
- **Reduced-effects target:** verify that each player can read and complete the route with reduced effects enabled; record any loss of route, hazard, jump, or feedback readability.
- **Muted-audio target:** with audio muted, verify the visual/gameplay fallback for required cues; record any cue that cannot be identified without sound.
- **Acceptance rule:** do not mark the five-player gate passed until all five rows contain observations and the open timing, controller, reduced-effects, and muted-audio targets have an explicit result.

## Exact next actions

1. Qualify the physical controller on the intended movement/jump route and record the result.
2. Run the five-player session using P1–P5 above; capture route completion, timing, deaths/resets, confusion points, controller behavior, reduced-effects behavior, and muted-audio cue behavior.
3. Compare fresh-player timing with the Thread 6 target and record pass/fail per player and for the group.
4. Update this file with the resulting evidence and release decision; do not infer completion from the discarded generic action.


## Thread 7 — Windows build and lifecycle qualification

- **Verified baseline:** Git commit `19caaf2`; isolated worktree `C:\GameProjects\I-Want-To-Be-The-Hero\qp4-qualification`; Unity `6000.3.6f1`; no OneDrive path used.
- **Lifecycle tests:** **Passed — 4/4, 0 failed, 0 skipped.** This includes `TwentyRestarts_RebuildExactlyOneWorldAndClearInputs`, `TenFalls_KeepCheckpointAndSpark_ResetEveryEnemy`, lost-callback recovery, and lethal-damage respawn.
- **Movement tests:** **Passed — 10/10, 0 failed, 0 skipped.** The clean post-change run includes `JumpMeasurements`, platform-relative idle animation, aspect-ratio camera checks, traversal and both weapons.
- **Accessibility controls:** **Implemented; 3/3 targeted tests passed.** F1 and a title-screen button toggle reduced effects; F2 and a title-screen button toggle audio. Reduced effects stop optional particles and bounded/ambient presentation motion. Muting preserves visible setting and cue messages.
- **Post-change regression:** `MovementTests` **10/10 passed** and `LifecycleTests` **4/4 passed** after the accessibility implementation. The combined headless all-fixture run is discarded because screenshot-heavy animation tests triggered a native no-graphics crash; the earlier graphical animation audit remains **6/6 passed**.
- **Windows x64 build:** **Passed.** `Assets/Scenes/Main.unity` included; 0 errors, 1 warning; 117,269,713 bytes; build duration 59.08 seconds.
- **Standalone startup:** **Passed with shutdown limitation.** The player reached a running, responsive state after 10–12 seconds in two launches. The automation surface did not expose a closable window, so cleanup used process termination; resulting crash-handler lines describe forced shutdown and are not startup failures.
- **Generated build:** retained only in the isolated local worktree under `unity/Builds/QP4-Windows`; generated output is not committed.
- **Human, physical-controller, reduced-effects readability, muted-audio cue reading, and timing gates:** **Not run/open.** No claims made.

**Remaining blockers:** physical-controller check; five fresh-player observations; human reduced-effects/muted-audio readability checks; timing targets.
