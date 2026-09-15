# QP4 Release Qualification

## Scope and evidence boundary

- **Start commit:** `4510a2d`
- This record documents Thread 6 release qualification only.
- The generic test action that timed out, overlapped, and was cancelled is **discarded**. It is not evidence and is not counted as a failure or pass.
- No Unity tests, Play Mode run, build, standalone launch, Notion update, or Git operation is performed for this record.

## Qualification status

| Area | Status | Verified result / record |
|---|---|---|
| Animation audit | **Passed** | 6/6 passed earlier. |
| QP2 clean `MovementTests` | **Passed** | 10/10 passed. |
| QP3 full `MovementTests` batch | **Passed with warning** | 9/10. `JumpMeasurements` was 0.0057 units below tolerance. |
| QP3 isolated `JumpMeasurements` rerun | **Passed** | Exact test rerun passed 1/1. |
| QP3 structure | **Passed** | 6 bands; 6 parallax anchors; 15 bounded motions; 1 resettable ambient inhabitant; 68 active `SpriteRenderer`s; 1 `ParticleSystem`; 1 `AudioSource`. |
| Generic test action | **Discarded** | Timed out/overlapped and was cancelled; no evidentiary value. |
| Windows build | **Not run / open** | Unity remained in a Test Runner Play Mode transition. |
| Standalone launch | **Not run / open** | Blocked by the same Unity Test Runner Play Mode transition. |
| Physical controller qualification | **Not run / open** | No verified result. |
| Human five-player observations | **Not run / open** | No verified result. |
| Reduced-effects controls | **Not run / open** | No verified result. |
| Muted-audio cue qualification | **Not run / open** | No verified result. |
| Fresh-player timing | **Not run / open** | No verified result. |

## Passed evidence

1. Animation audit: 6/6 passed.
2. QP2 clean `MovementTests`: 10/10 passed.
3. QP3 structure inventory passed with the six-band, six-anchor, 15-motion, one-inhabitant, 68-sprite-renderer, one-particle-system, and one-audio-source counts listed above.
4. The exact QP3 `JumpMeasurements` case passed when rerun in isolation: 1/1.

## Known warnings

- The QP3 full-suite result was 9/10 because `JumpMeasurements` measured 0.0057 units below tolerance in that batch. The isolated rerun passed 1/1; retain the batch discrepancy as a warning until a clean, non-overlapping qualification run is completed.
- The generic test action timed out/overlapped and was cancelled. Treat it as discarded, not as a failed test and not as supporting evidence.
- Unity was left in a Test Runner Play Mode transition, so build and launch evidence does not exist in this record.
- All human, controller, accessibility/effects, muted-audio, and fresh-player timing evidence remains open.

## Release blockers

The following remain release blockers until explicitly qualified:

1. Windows build completes successfully and the standalone launch is verified.
2. Physical controller input is verified through the intended movement and jump route.
3. Five-player human observation session is completed against the sheet below.
4. Reduced-effects controls are verified and do not remove required readability.
5. Muted-audio cue behavior is qualified as a usable visual/gameplay fallback.
6. Fresh-player timing is measured against the Thread 6 target and recorded.
7. A clean, non-overlapping MovementTests qualification run is recorded, with the QP3 batch discrepancy either explained or cleared.

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

1. Exit the lingering Test Runner Play Mode transition and return Unity to a stable editor state without treating the cancelled generic action as evidence.
2. Run one clean, non-overlapping `MovementTests` qualification pass and record the complete result, including `JumpMeasurements`.
3. Produce the Windows build, record whether it completes, and launch the standalone; record launch success and any blocking error.
4. Qualify the physical controller on the intended movement/jump route and record the result.
5. Run the five-player session using P1–P5 above; capture route completion, timing, deaths/resets, confusion points, controller behavior, reduced-effects behavior, and muted-audio cue behavior.
6. Compare fresh-player timing with the Thread 6 target and record pass/fail per player and for the group.
7. Resolve or formally accept the QP3 batch discrepancy only after the clean run and human qualification evidence are recorded.
8. Update this file with the resulting evidence and release decision; do not infer completion from the discarded generic action.
