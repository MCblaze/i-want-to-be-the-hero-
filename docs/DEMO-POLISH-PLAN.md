# Sunleaf demo: movement feel and world polish

Revision 1.3 · 14 September 2026 · C1 prototype tested; B1 Canopy composition accepted

**User priority update: animation audit and repairs first.** See [Animation audit](ANIMATION-AUDIT.md) for all 56 authored frames, both-facing screenshots, runtime findings and fixes. Resolve weapon continuity and missing dedicated poses using the [art repair brief](ANIMATION-ART-REPAIR-BRIEF.md) before further Canopy work. B2/C2 and broader scenery polish wait behind this animation pass.

Bezi completed the Canopy greybox and the separate art-handoff Page. Codex repaired saved annotation/font issues and verified the prefab, scene and 16:9 camera capture. See [B1 acceptance](B1-CANOPY-REVIEW.md). C1-F presentation, B2/C2 controller traversal/integration, final art and release qualification remain open.

[Master demo plan](https://app.notion.com/p/3dba2b7d211481e58e4ffda252533eaf) · [Detailed Notion polish plan](https://app.notion.com/p/3dba2b7d211481fa9448cd95575b7db6) · [Asset register](https://app.notion.com/p/3dba2b7d2114813daf1bff41107fe80a) · [Bezi workflow](BEZI-DEMO-WORKFLOW.md)

## Source and reconciliation

This update incorporates the user-designated [Platformer Feel & World Polish — Research & Build Plan](https://app.notion.com/p/3dba2b7d211481fa9448cd95575b7db6) and its [Drive implementation guide](https://docs.google.com/document/d/1e5bKqk4xa1VPCercPbuzrVpEIIoCl3pgUXKV72LUKu0/edit). The Drive guide remains the original research snapshot; the master demo plan and this revision define the reconciled implementation scope.

The working project remains `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-\unity`, Unity 6000.3.6f1. **Do not use OneDrive.** Gameplay baseline: `a564b5c`; earlier Bezi documentation milestone: `f9aeb75`. Check the current revision before implementation.

The current project uses built-in rendering, `PixelSpriteAnimator` on a visual child, runtime-created sprites and legacy `UnityEngine.Input`. URP, Cinemachine, Timeline and the newer Input System are not declared dependencies in the inspected manifest. A research reference to an Animator, Input Actions or a 2D light does not mean it already exists in this project.

Preserve these decisions:

- Windows keyboard/controller demo first, with 16:9 composition and 16:10 checks. Preserve existing touch behavior when touching shared input; full tablet qualification remains a later milestone.
- Keep six scenery bands and the existing 8–10-minute first-run target. Brief intro/ending moments fit inside that pacing budget.
- Keep double jump, dash, optional backflip and sword/wand switching in the movement contract. Art must express their actual timing and preserve their distinct purposes.
- Start with authored pixel-art poses; a skeletal rig is an optional experiment for a specific asset. Animation does not delay jump input or deform the gameplay collider.
- Test the proposed 32 PPU / 640×360 convention before locking it for bulk artwork. Prove compatible rendering and sprite imports before adding URP-only lights or normal maps.
- Use the current camera until the camera package decision is proven separately. Unity 6.3's package listings identify Cinemachine 3.1 and Timeline 1.8 as the relevant reference families; resolve exact versions at implementation time. A newer documentation URL alone is not a compatibility decision. [Cinemachine compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.cinemachine.html), [Timeline compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.timeline.html).

## One ordered backlog

Prefix the research stages **QP0–QP3** so they cannot be confused with the master plan's P0–P5. Restart/encounter reliability and the C1 movement-room prototype are implemented. C1-F presentation and the later quality tasks remain planned.

| Quality stage | Master plan / Bezi mapping | Owner and result | Exit evidence |
| --- | --- | --- | --- |
| QP0: movement, pose and contact feedback | P1; C1, then C1-F | Codex creates `MovementAndFeel_Test`, measures abilities, then adds presentation separately | Input/physics regression, pose/contact captures, camera-boundary cases |
| QP1: living Canopy showcase | P2; B1 → B2 → C2 → B3a/B3b/B3c | Bezi authors scoped scene/prefab work; Codex integrates behavior and renderer dependencies | Saved scene, actual traversal, motion/grayscale review, baseline performance capture |
| QP2: encounter, music and story | P3–P4; C3a/C3b/C3c | Codex integrates feedback/audio/cutscene state; Bezi authors assigned assets and shots | Correct event timing; skip/retry/reload and reward-once checks |
| QP3: accessibility and release | P5; C3d | Codex coordinates settings, target-device profiling and new-player sessions | Windows build, keyboard/controller checks, accessibility and complete lifecycle pass |

Establish reduced-feedback settings and a performance baseline early; QP3 verifies them rather than introducing them only at the end. B1 can establish provisional composition while movement is measured, with one writer per scene/file. B2 waits for real movement limits. Every package migration is its own bounded task.

## QP0: movement, poses, contact and camera

Create a saved, visible-in-Edit-Mode test scene with a flat runway, three labelled jump gaps, a wall and low ceiling, a one-way ledge, a landing target, moving-platform test space, a checkpoint/fall recovery area, weapon targets and camera-boundary markers. Record before/after video when available, or timed frame captures plus actual input/state logs. Measure jump envelopes before final platform spacing.

Define a presentation contract driven by actual controller state: grounded, horizontal/vertical speed, facing, jump-start, ascent, apex, descent, landing, attack, hurt, dash, double jump, backflip and defeat. Include clip interruption priorities and reset behavior. Map the existing poses first; extend the current custom presenter or approve a separate Animator migration. Keep this presentation change separate from physics tuning.

For Logan, show knee/hip compression, a decisive launch, extension or tuck in flight, a readable apex/fall, and landing compression/recovery. Takeoff responds immediately to the accepted jump input. Foot baseline, sword hand and silhouette remain consistent in both directions. Backflip preserves facing and collider stability; double-jump visuals distinguish the single extra jump.

Create a small feedback event contract containing actor, event kind, world position, surface, intensity, facing and an event identifier where duplicate delivery is possible. Controller/combat state owns jump, landing, hit and damage decisions; presentation selects the response. Footstep markers must also check grounded state and movement. Spawn dust at the real contact point; use one shared dust sheet for footstep/takeoff/landing/skid variants. Suppress false landing bursts after teleport, respawn and scene setup. Returning a pooled effect must clear its previous state.

The camera must reveal the landing before commitment, preserve pixel stability and snap correctly on respawn. Check turns, vertical jumps, room edges, aspect changes and reverse travel. Tune a small dead zone and restrained look-ahead. If Cinemachine is adopted, prefer authored room bounds and controlled transitions rather than repeatedly rebuilding one confiner. [Confiner 2D](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineConfiner2D.html).

**Exit:** control timing and collider bounds unchanged by presentation; no airborne footsteps, duplicate landing effects or continuing effects after restart; keyboard/controller still work; movement and camera cases pass at the planned frame rates. Touch adapters receive targeted regression checks only when shared input changes.

## QP1: environment, collision and life

Give solid ground, one-way ledges, hazards, triggers and decoration explicit roles. Art and physics layers are different concepts: a visually blended grassy platform still needs a readable landing edge and predictable collision. Use Tilemaps only where appropriate; configure the Physics 2D interaction matrix and contact behavior as a dedicated integration step. Keep decorative layers nonblocking and test seams, corners, sides of one-way ledges and moving-platform edges.

Retain `01_Sky`, `02_FarRuins`, `03_DistantForest`, `04_NearTrunks`, `05_PlayPlane`, `06_Foreground`. Use the master plan's anchored camera-relative parallax factors and test reverse travel, camera extremes and respawn. Keep foliage out of jump destinations and enemy-warning space.

Start with three small environmental loops: grass, canopy/branch and water. Give repeated instances different starting phases and modest variations in period/amplitude. Use a shared wind direction for foliage and rare leaf drift; add occasional pollen and localized water response. Cap simultaneous effects and record representative counts before tuning budgets. World life should be visible during a quiet pause without becoming distracting during combat.

Add one small nonblocking ambient inhabitant for the showcase: idle → notice Logan → react → settle. Use a short cooldown and simple proximity/event input; an optional contextual line reuses this interaction. No quest system or general navigation is required. Distant decoration may use cheap updates, but offscreen enemy throttling must preserve gameplay and reset correctness.

Prove the renderer and sprite/secondary-texture pipeline in isolation. Once URP 2D is accepted, establish a base light and a few local focal accents. Use painted value separation first, selected normals/shadows second. Blend mood transitions across authored regions without flashing or making a route temporarily unreadable. Compare the painted and lit versions at the same camera/settings. [Unity 2D lighting](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/2d-index.html), [Pixel Perfect Camera](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/2d-pixelperfect-ref.html).

**Exit:** main route works with the measured controller; hazards and landing edges read in motion, grayscale and reduced-effects mode; loops do not move in lockstep; inhabitant and particles reset cleanly; no renderer/package changes slipped into the greybox assignment.

## QP2: sound, feedback and story

Route repeated action feedback through the small event contract. Prioritize footsteps, takeoff/landing, sword hits, wand shots, checkpoint, hurt, enemy tells and victory. Hits follow actual contact/damage events, not a second independent timer. Keep source counts and overlapping effects bounded. Essential information also has a visual cue.

Plan mixer groups Music, World, Player, Enemies, UI and Cinematic, with simple user-facing Music/SFX controls and captions for speech or essential sound-only meaning. Use reusable material variations, clean loop boundaries and a restrained exploration → danger → victory transition. Add bar-aligned transitions only if auditioning shows a benefit. [Audio Mixer](https://docs.unity3d.com/6000.3/Documentation/Manual/AudioMixerOverview.html), [scheduled playback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSource.PlayScheduled.html).

Storyboard a skippable intro of roughly 5–8 seconds as an initial target: reveal place, show the objective, give one emotional hook, then return control. Reuse the existing opening composition; avoid new lore screens or voice production by default. Fit the scan within Trailhead's pacing allowance.

Storyboard boss resolution as combat stop → defeat confirmation → animation/audio payoff → reward/story flag → restored-world or route reveal → ending/replay UI. On skip, apply the same logical result exactly once. The current demo ends at this point, so restored control means the intended ending UI unless a short epilogue is explicitly designed. Do not unfreeze a defeated combat encounter. Preserve the current 1.8-second ending-panel delay until the new sequence deliberately replaces it and passes checks.

Use Timeline only after compatible package/scene integration is approved, or use an equivalent small sequence controller. Specify who owns camera, input and time state. Normal completion, skip, cancellation, restart and scene unload must release their ownership consistently. Clear held input, temporary camera/audio state and subscriptions. Test repeated skip, skip during transitions, restart during a sequence, checkpoint/death interruption and Editor script reload. [Timeline workflows](https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/wf-overview.html).

**Exit:** no input/camera/time lock, doubled reward or repeated defeat effect; retries restore encounter state; music transitions and skip results match the intended game state; story timing stays within the demo pacing budget.

## QP3: accessibility and release evidence

Test remapping, pause, focus loss, device switching, readable prompts, captions, reduced shake/flashes and reduced ambience. Verify that disabling optional feedback does not remove essential warnings. Use keyboard/controller as release gates; schedule full tablet QA separately.

Capture CPU/GPU frame time, memory, active effects/lights/audio sources, draw calls and scene-load behavior on a named Windows baseline at the target resolution. Pool frequently reused effects; introduce atlases after correct sprite import and animation are established. Measure before and after an optimization. Keep existing 20-restart and 10-fall/death lifecycle coverage, and add only relevant effect/cutscene failure cases. Run representative standalone builds after major camera, renderer or sequence integration.

Observe five new players using the master plan's learning checks. Add whether they notice safe landings, understand inhabitant reactions, read boss tells and can skip/replay without assistance. Record confusion as well as completion time; automated traversal does not prove enjoyment.

## Asset additions and reuse

The existing Logan and enemy frame budgets remain 42 clips / 218 exported slots. Pose refinement, contact metadata, skid dust, restored-world composition and sound variation reuse existing budgeted sets where possible.

Add an explicitly separate small ambient inhabitant allowance: one design with idle 4, notice/react 2, settle 2 frame slots; its contextual interaction can reuse those poses. This is 3 clips / 8 slots outside the combat-character subtotal. Add three environment loop definitions (grass, canopy, water), a two-state lighting look sheet, contact/surface metadata, audio routing and variations, and two shot/skip storyboards. Environment loop methods and final source counts are selected after the first-room test; do not silently multiply every sprite's normal-map workload.

Every art handoff includes source file, palette/scale, pivot/contact anchors, clip durations, relevant event timing, facing check and a gameplay-scale preview. Every effect handoff includes spawn cause, lifetime/pool reset, sorting, maximum concurrency to tune and reduced-effects behavior.

## Bezi execution rule

Pin this revision and the specific task assets. B1 only reserves clearly labelled scenery/inhabitant/camera markers. Later B3a handles ambient art and loops, B3b the lighting proof, and B3c the inhabitant prefab, with Codex-owned behavior interfaces. C3a handles feedback/audio integration; C3b intro; C3c boss resolution; C3d release evidence. These are separate assignments, each using the reusable brief and completion report. Do not send the entire polish backlog as one Agent instruction.

Return saved paths, baseline, exact changed files, before/after evidence, Passed/Failed/Not run checks, assumptions and next owner. The Room Acceptance Review Skill should include moving-camera/grayscale checks, reduced-effects behavior and restart/skip evidence where applicable. A Bezi Proof can explore timing or intensity, but the corresponding Unity behavior must be verified after transfer.

## Status

C1 is implemented in the saved MovementAndFeel_Test scene: double jump, directional dash, neutral backflip, sword/wand switching, practice stations and adaptive camera bounds. Nine movement/camera tests and four original quest regression methods passed across the final relevant runs. See [Movement Lab](MOVEMENT-LAB.md) and its checked-in evidence for measurements and limits. C1-F presentation remains open. Bezi has received separate Canopy layout and art-handoff assignments; their outputs are reviewed separately from C1. No renderer/package migration or GitHub publication is included in this milestone.
