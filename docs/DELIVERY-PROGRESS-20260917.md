# Delivery production checkpoint — 17 September 2026

Status: implementation in progress. Not approved for client shipping.

## Implemented
- Original transparent 16-frame wand sheet; independent idle, run, air and cast clips, per-frame bounds and stable body anchors. Original sword sheet preserved. Both-facing contact sheet and live screenshot retained.
- Persistent master/music/effects preferences, pause music/effects sliders, standalone Quit, and correct live mute/volume handling on both sides of a music crossfade.
- Seven original procedural WAVs: three stone footsteps, three wood footsteps and an eight-second victory resolve. Runtime footsteps use movement relative to support; victory plays once on the music bus. Reproducible source and technical validation in docs/audio/sunleaf-finish.
- Data-driven scene bounds, camera, kill plane, enemy patrols, boss and ordered checkpoints. Existing Main retains its layout. Background draw order remains behind actors on long routes.
- Editable Sunleaf_DeliveryCandidate scene across x0–288: twelve sections, fourteen patrols, three lifts, six checkpoints, four required two-unit gaps, two lower recovery shelves, Spark, weapon targets, gate and remote Guardian arena. This is a route candidate, not achieved pacing or final environment approval.
- Fixed review finding: checkpoint at x94 previously respawned over pit91–93; moved to safe landing. Next checkpoint now respawns beyond lower shelf.
- Bezi produced the five-player/device/client release qualification checklist and staged a room guidance component for reviewed integration.

## Evidence so far
- Main regression 10/10, original audio foundation 6/6 and animation audit 6/6 passed earlier in this batch before latest footstep/candidate integration. Results are under docs/evidence/delivery-20260917.
- Wand sheet inspected frame-by-frame in both directions; pause sliders visually inspected.
- Candidate bootstraps live with one hero, fourteen patrols and Guardian at x278. Initial gameplay camera inspected. This is not a full traversal.
- The first new verification attempt was invalidated by a test compilation error; no pass is claimed from it. Corrected checks are being run separately.

## Still required
- Dedicated air-jump/backflip/defeat and sword continuity/crop repairs. Supplementary generated sheets repeatedly contained an opaque checkerboard and were rejected; they are not game assets.
- Refine long-route topology, distinct landmarks, platform joins/variation and mechanic combinations. Validate all clearances and real movement. Existing candidate does not establish 10–15 minutes; no artificial waits will be used to meet duration.
- Full controlled playthrough, audio listening/mix, final Windows build and performance evidence after integration.
- Physical input-device observations, five fresh players with measured times, and client art/listening acceptance remain unobserved.

No OneDrive used. Generated accepted originals, metadata, audio source and test evidence remain in the repository.

## Follow-up verification and route repairs
- New audio runtime checks passed 3/3: mix reload, live crossfade mute/volume and non-looping victory, and support-relative footstep suppression.
- Candidate remote boss/camera/restart and four-pit fall recovery passed. Sequential checkpoint test now crosses each trigger through real movement and observes activation before falling; all six checkpoint recoveries passed in the focused rerun. Initial failure was a test assumption about teleported trigger contacts; preserve both reports.
- Browser/Unity shared animation data and behavior tests passed 10/10.
- Bezi's room guidance component was reviewed, corrected for re-enable state safety and accurate room copy, imported and attached to candidate only.
- First normal-input automated traversal reached x93.53 and exposed low Ledge 91 over the required 91–93 gap. Removed that obstruction from candidate and builder. The diagnostic's stale no-progress timer after respawn was corrected separately.
- Visual capture exposed stretched cropped ground strips concealing real pits. Nine cropped renderers had inherited their original Sliced width. Corrected to Simple rendering with explicit world fitting and correct pivot placement; builder now preserves this correction. Native central terrain strips cover only contiguous equal-height joins and never bridge the required pits. Source textures and colliders are unchanged.
- Supplementary ability sheets still fail alpha validation; four attempts are retained locally and excluded from the game. Permission requested for local background removal; not yet performed.


## Closing repair batch
- Normal-input controlled traversal reached victory in 77.77 real seconds with one death. No teleport, invulnerability, health grant, Spark grant or forced victory was used in this traversal. This establishes reachability, not first-player pacing; the requested 10–15 minute experience remains unproven and needs substantial route work.
- Friction-free hero contact fixes wall/corner sticking. Controlled traversal, wall sliding and two moving-platform checks passed 4/4; see contact-and-traversal-results.xml.
- Keyboard menu focus/navigation improved, including slider navigation. Down then Return on Reduced Effects toggled the option without starting the quest in a live UI check. Physical controller verification remains open.
- Airborne victory now cancels upward/player-driven movement while allowing gravity to land Logan. Final regression results will be recorded below.
- Bezi's embedded connector independently updated from 0.115.12 to 0.115.16 during this batch; preserve separately from gameplay changes.
- No new shipping build is claimed for this repair batch. Dedicated supplementary animation cleanup still awaits the requested permission, and human/device/client acceptance remains open.
- Final broader regression: 10 passed; the initial victory test failed because its timed yield did not advance enough physics steps. A first ray-origin correction did not resolve it. After replacing the timed yield with 80 fixed simulation steps, the focused victory test passed 1/1. Both failed reports and the passing rerun are retained. Earlier speculation about a raised platform was not established and is superseded by this test-runner finding.
