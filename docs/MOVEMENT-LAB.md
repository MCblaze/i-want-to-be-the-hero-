# Logan movement and feel lab

Implementation C1 · 14 September 2026

Project: `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-\unity`. No OneDrive.

## Open and play

In Unity choose **Tools → Hero → Open Movement and Feel Test**, then press Play. The saved scene is `Assets/HeroDemo/Movement/MovementAndFeel_Test.unity`; its reusable geometry is `MovementRoom.prefab` in the same folder. The yellow box marks Logan's spawn in Edit Mode. Logan's existing animated sprite replaces it during play.

| Action | Keyboard | Controller mapping |
| --- | --- | --- |
| Move | A/D or arrows | Horizontal axis |
| Jump; press again for one extra jump | Space | A |
| Attack | J or X | X |
| Switch sword / Sunseed Wand | E | Y |
| Evade | Shift or K | B |
| Reset lab | R | Keyboard / station buttons |
| Travel to practice station | 1–5 or screen buttons | Screen buttons |

Hold a direction and evade to dash. Neutral evade while grounded backflips away from the facing direction; Logan keeps looking toward the target. Air evade is a dash. Both share one cooldown. Backflip is optional spacing/defence; the required route must not depend on it. Double jump provides an in-air correction and optional-route reach; only a real landing refills it. Switching weapons keeps the outgoing attack and individual cooldowns intact. Sword is close range; wand trades attack rate for reach.

Controller button names describe the current legacy mapping; physical-device qualification remains a release check. Existing Main touch input remains available; the new practice HUD targets desktop testing.

## Authored stations

1. Flat runway: acceleration, jump height, short hop, double jump and evade.
2. Gaps: 2.0, 2.75 and 3.5 units, with a lower recovery shelf.
3. Platforms: jump-through gold ledge and solid teal moving platform; nearby checkpoint.
4. Combat: hit-count target and thin projectile-blocking wall.
5. Ceiling: head collision and constrained jumping.

The camera follows the hero and clamps to the room. Use station buttons to recover without walking the entire lab. This room is a testing environment, not the art showcase or the full demo level.

## Runtime contract

New abilities use `MovementTuning.asset` and are enabled only in this room. Main keeps its established progression and combat balance until integration. The room creates the existing game/controller at runtime while its platforms and markers remain authored scene objects. No package or renderer migration is part of C1.

| Setting | Initial value |
| --- | --- |
| Run speed | 5.2 units/s |
| First / second jump velocity | 10.6 / 8.5 units/s |
| Gravity | 24 units/s² downward |
| Coyote / jump buffer | 0.12 / 0.14 s |
| Dash speed / duration | 12.5 units/s / 0.17 s |
| Shared evade cooldown | 0.70 s |
| Backflip horizontal / launch speed | 4.5 / 4.0 units/s |
| Backflip duration / damage-avoidance window | 0.40 s / 0.08–0.22 s |
| Wand cooldown / speed / range | 0.45 s / 12 units/s / 10 units |
| Maximum live seeds | 4 |

These settings are not measured jump distances. Physics, input timing and collision margins determine the usable geometry. Landing art and final gap spacing follow the measurements.

The backflip currently rotates the visual child using existing poses. Authored backflip/double-jump frames, distinct wand poses, contact dust, sound and camera polish belong to C1-F and the asset register. The collider never rotates with that temporary visual.

## Verification

Run **Tools → Hero → Run Lifecycle Regression Tests**. The suite includes movement tests and the existing restart/death regressions. Detailed machine results are written to `Logs/hero-lifecycle-results.xml`; successful measurement tests also write `Logs/movement-measurements.csv`. These logs are local generated evidence, not source assets.

Nine movement/camera tests passed across the final movement run (8/8) and targeted aspect-ratio follow-up (1/1). The four existing Main lifecycle tests passed after the grounding/platform fixes: 20 restarts, 10 checkpoint falls, lethal combat death and lost scene-callback recovery. The tests drive the real controller through the shared input adapter; these are automated playtests, not physical controller or new-player sessions.

| Measured action | Rise | Time until landing |
| --- | --- | --- |
| Full held jump | 2.24 units | about 0.90 s |
| Early-release short hop | 0.54–0.61 units | about 0.40–0.45 s |
| Second jump near first apex | 3.66 units total | about 1.33–1.38 s |

Observed run displacement was 5.00–5.13 units/s against the configured 5.2 units/s, sampled over approximately half a second. Editor update sampling affects the reported displacement and short-hop timing; use the actual traversal results rather than turning these samples into exact maximum-distance claims.

Real input-driven traversal crossed all three authored level-ground gaps (2.0, 2.75 and 3.5 units) with a single jump and a run-up. Preserve a margin for first-time players: use 2.0–2.75-unit gaps for early teaching, and validate any height difference or new landing arrangement individually. Do not make 3.5 units the default required jump. The upper route may use double jump after a safe demonstration; backflip remains optional.

Additional passing checks cover coyote input after leaving an actual ledge, landing input buffering after spending the air jump, third-jump rejection, evade direction/shared cooldown, the backflip immunity/recovery window, one-way head passage and landing, moving-platform carry, low-ceiling collision, one-hit sword swings, swap/cooldown persistence, in-flight projectiles and thin-wall blocking, three clean lab resets and a real lab-checkpoint fall. Camera checks keep Logan visible at both room edges at square, 16:10 and 16:9 aspects.

The first test pass found idle grounding and moving-platform carry bugs; both were fixed and re-tested. A reset assertion was corrected to await Unity's deferred projectile destruction. Visual inspection then found that the narrow Game window could hide Logan and stretch HUD text; adaptive camera bounds and uniform text scaling fixed both. The flat camera background is saved with the scene. Final native Game-view inspection showed Logan, the runway and readable controls.

Not run: physical gamepad qualification, a standalone Windows build, target-frame-rate sweeps, projectile-cap stress testing, user enjoyment/pacing sessions, final animation/lighting/audio acceptance. This completes the tested C1 practice-room prototype; release qualification and C1-F presentation remain open.

## Efficient tool handoff

Use the existing Unity connection to run a bounded command or test, and return only status, failure details and changed paths. Save verbose reports to files. Read the relevant source section instead of dumping the project; take a camera image when judging appearance. A test run can temporarily disconnect the Unity tool during assembly reloads: check fresh result timestamps and test counts rather than re-running blindly.

For Bezi's next scene assignment, pin `BEZI-NEXT-TASK.md` and the measured movement section of this document. Keep one writer per scene or script. The lab folder remains Codex-owned; Bezi's B1 output scope remains `Assets/HeroDemo/Greybox/CanopyCrossing/`. No additional tool is required for C1. Bezi ACP/Skills are optional workflow improvements that need a separate availability check, not prerequisites for implementing this room.
