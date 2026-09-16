# Sunleaf: twelve-room candidate route

Date: 17 September 2026. Status: implementation proposal, not a constructed or qualified level.

Preserve Main and author a separate editable `Assets/Scenes/Sunleaf_DeliveryCandidate.unity`. Keep existing source art and metadata. The candidate spans x=0–288, with twelve rooms grouped into six zones. Do not replace Main or the client build until the candidate passes traversal and visual checks.

## Design basis

`docs/literature-reviews/LEVEL-DESIGN-SYNTHESIS.md` supports teaching one idea safely, practicing under modest pressure, then combining familiar actions. It also requires readable landings, short recovery, visible goals and optional mastery. Its older 5–8 minute timing target is superseded by the current delivery plan's **10–15 minute first successful playthrough** target.

Every platform must have a named purpose: safe instruction, recovery, encounter spacing, optional reward or route transition. Backflip remains optional. Weapon changes happen on safe ground before mixed encounters. Do not add mandatory waiting or repetitive enemy waves to reach a timing target.

## Geometry contract

- Main ground walkline: y=-3.4. Initial player center: (2,-2.8). Camera center y=0 initially; kill plane y=-8.
- Logan's collision capsule is .52 wide by1.15 high. Preserve at least **1.41 vertical clearance** wherever the player must walk under geometry; add additional visual headroom when artwork extends past colliders.
- Initial required gaps: no more than2.5 units edge to edge. Broad stable landings: at least2 units wide, preferably3–4 near encounters.
- Initial step rise: approximately1.6 units or less. Suggested first ledge tops:-1.8, then-.2. For ledges above the base floor, keep underside at or above-1.99; avoid placing a fight behind a low solid ceiling.
- After Spark, a safe double-jump teaching landing can be3.1 units above its takeoff: floor-3.4 to ledge-.3. Put recovery ground underneath and leave a generous landing width.
- No mandatory backflip gap. Optional branches rejoin within the same room and never hide the required exit.
- Use modular platform caps/undersides and retain collider alignment. Do not scale a single stone sprite across an entire room.
- Thornlings need broad patrol areas without exposed fall edges; patrol coordinates alone do not prevent physics falls. Keep threats away from mandatory takeoff edges until the mechanic is learned.

## Room map

Coordinates below are authoring proposals, not measured movement acceptance. Gaps and stepping stones must be placed inside each room without breaking the safe entry/exit connection.

| Room | Bounds | Entry and goal | Geometry / encounter | Exit and recovery |
|---|---|---|---|---|
| Z0A Trailhead |0–24|Spawn(2,-2.8); learn run/jump|Ledges x10–14/top-1.8 and18–22/top-1.8; continuous safe floor; exit landmark visible|Safe floor into24; no mandatory damage|
| Z0B First courage |24–48|First voluntary fight|Broad sword arena31–38, ranged target perch42–45; clear underpass to all ground enemies|Checkpoint marker46; respawn44.5,-2.8|
| Z1A Broken approach |48–72|Read a landing before taking off|Two gaps<=2.5 with recovery shelf; ledges55–59/top-1.8 and63–67/top-.2; optional upper reward|Visible stable exit70–72|
| Z1B Ruin crossing |72–96|Combine practiced jump with combat|Separate takeoff zone from patrol lane; broad fight floor80–88; optional upper detour rejoins92|Checkpoint94; respawn92.5,-2.8|
| Z2A Canopy lesson |96–120|Observe then ride moving support|Moving support around103–111 over continuous recovery floor; safe waiting ledge; no enemy on first transfer|Stable landing116–120|
| Z2B Canopy choice |120–144|Two transfers with visible timing|At safe transfer phase each edge gap<=2.5; wide recovery floor; optional high route rejoins142|Checkpoint142; respawn140.5,-2.8|
| Z3A Shrine approach |144–168|Recognize shrine landmark and recover|Light encounter151–158, then quieter stable ascent; contrasting shrine silhouette|Safe shrine approach164–168|
| Z3B Spark shrine |168–192|Acquire Spark near176 and learn second jump|Wide post-Spark landing181–187/top-.3 from floor-3.4; nonlethal floor below; lesson feedback clear|Checkpoint189; respawn188,-2.8; Spark retained on falls|
| Z4A Weapon gallery |192–216|Choose useful weapon on safe floor|Sword-favored target199; ranged perch target208; enemy approaches with>=1.41 clearance|Wide exit212–216; no forced airborne swap|
| Z4B Mastery crossing |216–240|Combine familiar movement/weapon choices|Double-jump and moving-support route; optional backflip cache; gate236; no new required mechanic|Checkpoint238; respawn237,-2.8|
| Z5A Court threshold |240–264|Short final approach and anticipation|One readable encounter, then quiet vista and safe recovery; no arbitrary wait|Boss checkpoint260; respawn258.5,-2.8|
| Z5B Guardian court |264–288|Prove established verbs|Flat arena268–284; Guardian spawn278,-2.05; wake268 only after Spark; preserve telegraphs and feet visibility|Ending vista286; no new traversal during boss fight|

Checkpoint coordinates require collision-free spawn verification after art and geometry are authored. Checkpoint progress must be monotonic unless deliberately restarting the quest.

## Scene and gameplay wiring

1. Bootstrap candidate by its enabled `MainSceneLayout` rather than Main-only path. Keep Main fallback and movement labs unchanged.
2. Set layout bounds(0,288), cameraCenterY0 and killPlaneY-8. Initialize the hero's first respawn from `heroSpawn`, not the old hardcoded coordinate.
3. Enable authored enemy spawns. Fill named encounter records with position and safe patrol bounds. Do not leave legacy enemies spawning at the old five coordinates in addition to candidate encounters.
4. Set boss spawn(278,-2.05), arena bounds(268,284), wakeX268. Boss movement, wake trigger and reset must all use this configuration.
5. Bind all checkpoint records in progress order. Prevent backtracking to an older marker from moving the active respawn backwards.
6. Reset the current encounter region on defeat where appropriate; avoid repopulating the entire cleared route unnecessarily.
7. Set six backdrop centers24,72,120,168,216,264 and fade widths approximately4–6. Rendering order must use fixed negative ranks, never world X coordinates. Existing viewport-cover scaling remains valid.
8. Check ambient motion/parallax origin positions after moving art groups. Preserve reduced-effects behavior and editable scene geometry.

## Movement evidence required

Current tuning: run5.2, jump10.6, second jump8.5, gravity24, coyote.12 seconds, jump buffer.14 seconds. Ideal constant-gravity estimates give2.34 units initial jump rise and4.59 units full-speed same-height travel; a second jump at the first apex gives approximately3.85 total rise. These are analytical estimates, **not test results**. Acceleration, key release, contact state and collision geometry alter actual reach.

Measure standing and running jump, short/full hold, early/apex/late second jump, moving-platform takeoff and narrowest supported camera framing. Record takeoff, landing and failure recovery for each required jump. No critical landing should depend on the theoretical maximum.

## Timing plan and caveat

At5.2 units/second,288 units of straight running takes only about55 seconds. Length is not proof of a10–15 minute demo. Budget roughly40–60 seconds for each early room through movement decisions, discovery and meaningful encounters; reserve90–150 seconds for Guardian/payoff. These are planning budgets, not validated timings. Avoid forcing long traversal of already-cleared rooms, inflating enemy health or slowing platforms to meet them.

Five uncoached first-time players must provide first-success times, deaths/locations, lost-route events, weapon use and ability comprehension. Tune content after measurement. Experienced direct-route runs should remain meaningfully faster.

## Acceptance gates

- Main unchanged and still passes its existing regression suite.
- Candidate opens visibly in Edit Mode, launches, restarts and returns to correct checkpoints.
- Full critical route passes without backflip, sequence breaks or concealed landings.
- Every ground enemy can be approached from the intended direction; no inaccessible low-ceiling fight.
- Moving-platform idle remains stationary relative to support; transfers work without jitter or forced damage.
- Spark is taught before required double jump, retained on ordinary defeat, reset on full restart.
- All checkpoint respawns are collision-free; no checkpoint downgrade on backtracking.
- Boss awakens only in the authored arena and remains within it; a loss gives a quick fair retry.
- Backgrounds remain behind gameplay at every zone; art caps align with collision tops; no repeated stretched texture blocks.
- Pause, saved volume levels, mute, reduced effects, focus loss and UI fit survive scene changes.
- Windows build and controlled route runs pass; fresh-player timing and visual/listening review remain separate human evidence gates.

Only after these checks should the candidate replace the shipping scene. This document and route fields alone do not constitute level completion.
