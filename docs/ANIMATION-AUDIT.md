# Animation audit and repair log

14 September 2026 · baseline `5f59a80` · Unity 6000.3.6f1

**Animation repair now comes before additional Canopy work.** The audit covers every currently authored character frame: Logan 24 frames / 10 clips, Thornling 16 / 5, and Mossback Guardian 16 / 5. All 56 frames were rendered by Unity in both facings (112 sampled views), plus live controller/action screenshots. These are actual Unity camera renders, not generated illustrations.

## Screenshot evidence

[Open the local screenshot gallery](evidence/animations/index.html). The gallery keeps the critical before/after pairs together, followed by every source frame and the live action captures.

- [Logan: all poses, both directions](evidence/animations/after/logan-all-frames.png)
- [Thornling: all poses, both directions](evidence/animations/after/thornling-all-frames.png)
- [Guardian: all poses, both directions](evidence/animations/after/mossback-guardian-all-frames.png)
- [Backflip before](evidence/animations/before/backflip-2.png) / [after](evidence/animations/after/backflip-2.png)
- [Wand firing before](evidence/animations/before/wand-fire-1.png) / [interim correction](evidence/animations/after/wand-fire-1.png)
- [First contact](evidence/animations/after/landing.png) / [settled landing](evidence/animations/after/landing-settled.png)

The full before/after folders also contain run, jump rise/apex/fall, second jump, landing, four sword samples, three wand samples, flip/dash phases, hurt, respawn and victory. CSV traces identify action, time, actual clip/frame, facing, rotation, sprite/collider lower bounds and weapon. The screenshot camera temporarily uses a close view for inspection, then restores its settings; it does not change saved gameplay cameras.

Some initial baseline captures occurred immediately after input, before the presenter advanced. The CSV records the actual frame; later captures explicitly wait for swap/hurt acceptance. These diagnostic captures are not a fixed-frame-rate performance recording.

## Findings

| ID | Priority | Finding and evidence | Resolution |
| --- | --- | --- | --- |
| AN-01 | High | Backflip rotated the foot-anchored apex sprite through the ground. Before midpoint: sprite lower bound about -1.04 while collider feet were +0.28. | Fixed presentation rotation around the pose centre, with clearance for diagonal poses; original foot offset restored afterward. Collider, velocity and evade timing unchanged. |
| AN-02 | High | Wand used the sword clip starting at 0.12 seconds, skipping wind-up and displaying baked sword arcs. | Removed sword playback for an outgoing wand shot; the existing empty-hand raised pose is an interim cast. Swap during recovery preserves the outgoing action. A proper wand set is still required. |
| AN-03 | Medium | Sword disappears in frames 4–11, 16–18 and 21; returns in idle, attacks, dash19, hurt20 and victory. Wand idle still shows the baked sword. | Open art repair: produce weapon-consistent poses or a deliberately separated weapon layer. Do not classify the interim cast as finished wand animation. |
| AN-04 | Medium | No authored double-jump, backflip, wand or Logan defeat clips exist. Double jump repeats rise; flip rotates apex; lethal damage returns through hurt/respawn. | Open art/design work. Keep the tested timing, then replace temporary poses with aligned exports. |
| AN-05 | Low | Logan dash18 loses boot-edge pixels and victory22 loses sword-tip pixels where neighboring poses overlap. | Open atlas repair. Widening rectangles alone imports the adjacent pose; separate/pad the source frames and preserve their anchors. |
| AN-06 | Low | Guardian hurt12 clips seven leaf pixels at x309, y1017–1023. | Fixed crop width 299→302 and pivotX 0.5→0.495033, preserving absolute body anchor. Canonical web manifest, browser data and Unity manifest synchronized; atlas builder preserves the correction. |
| AN-07 | Medium | Dash presentation sampled a hard-coded 0.17-second age even when the tuning asset supplied a different duration. | Fixed normalized playback across the actual configured duration. Default physics timing is unchanged. |
| AN-08 | Observation | First landing screenshot briefly appears above the floor. | The follow-up capture 0.05 seconds later is aligned; this is transient interpolated motion, not a permanently wrong landing pivot. Do not shift the entire landing clip to compensate. |
| AN-09 | High | Repeating the frame audit after Play Mode could return destroyed Sprite objects from the static cache and fail with a null reference. | Cache entries are now validated and recreated after their Sprite/Texture objects have been destroyed. An explicit destroyed-cache regression reproduces this lifecycle case. |

Hair, scarf, outfit and overall silhouettes remain recognizable. Source transparency is correct; a raw PNG viewer's brown RGB background is not an opaque-box runtime defect. Handedness changes were not established. Enemy sheets contain complete idle/run-or-telegraph/attack-or-charge/hurt/defeat sequences; sparse frame counts still limit smoothness and are an art-quality decision.

## Reproduction and checks

Use **Tools → Hero → Run Animation Audit** with Unity stopped. Outputs go to `unity/Logs/animation-audit/current/` unless an audit phase is set. Tests render every authored frame, exercise live actions and check foot clearance, both facings, collider stability, outgoing-weapon presentation, tuned dash timing and enemy transitions. Rendering every frame establishes coverage and references; it does not establish that the artwork is polished.

The initial capture-only run passed two methods. The first repair run exposed remaining diagonal backflip clearance and an enemy test startup problem. The test callback was moved after Play Mode's domain reload, and the enemy contact setup was corrected to stand in its patrol path. A later combined run passed all 13 existing movement/quest methods and four animation methods, but exposed the stale sprite cache in the repeated frame audit. That cache defect was then fixed and given its own regression. Before and failed-run evidence is preserved rather than silently discarded. Final result counts are recorded with the checked-in XML evidence.

The existing ten browser/manifest animation tests also passed after the metadata repair. Those tests are separate from Unity results. Standalone/device rendering, physical gamepad feel and new-player comprehension are not established by this audit. Screenshots and tests do not approve the missing production artwork.

Final verification: **6/6 animation checks passed** after the cache repair. All **13 existing movement/quest methods passed** in the preceding combined run, whose separate frame-audit failure led to that cache repair. The final animation run also re-entered both the lab and Main and captured live enemy transitions. This is evidence from separate runs, not a single 19-test run. See [evidence index](evidence/animations/README.md).

## Next animation assignment

Use [Animation art repair brief](ANIMATION-ART-REPAIR-BRIEF.md) before returning to Canopy. Priority is weapon continuity and dedicated wand/double-jump/backflip poses, then the two Logan atlas overlaps. Preserve this audit as the reference baseline and repeat the screenshots after replacement. Keep one owner for sprite sources/manifest and one owner for the Unity editor.

All work targets `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-`; no OneDrive. GitHub publication remains separate from the local milestone.
