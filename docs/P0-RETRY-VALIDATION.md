# Unity retry milestone — 14 September 2026

Implemented the first reliability slice of the [demo v0.2 plan](https://app.notion.com/p/3dba2b7d211481e58e4ffda252533eaf).

## Behavior

- Main now recreates its runtime level after scene reload, including Restart and the victory panel’s Play again button.
- Full restart returns to the title screen with one hero, camera, event system and enemy set. It clears the previous run’s Spark and pending touch inputs.
- Falling or dying preserves Logan’s checkpoint and collected Spark/opened gate, while rebuilding all five Thornlings and the Guardian. Enemies regain their initial health, position and behavior state.
- Respawn clears attack/dash cooldowns and pending touch input, restores the idle appearance, and snaps the camera to the safe return point.
- The objective changes to “Sunleaf Ruins restored!” after victory.
- The existing 1.8-second victory-panel delay is preserved.

## Validation

Unity 6000.3.6f1; baseline local commit 85f068d plus this change.

Three Unity regression methods passed, with zero failures or skips:
1. Twenty restarts, alternating direct restart and the Play again button’s event. Verify fresh session and exactly one runtime world, plus cleared progression/input.
2. Ten falls after activating the actual checkpoint trigger, unlocking Spark, damaging the boss and defeating an enemy. Verify safe return, retained progress, restored enemies and camera.
3. Lethal damage with the normal immunity timer, followed by the real delayed respawn. Verify health, checkpoint, Spark and reset boss attempt.

The tests arrange positions and damage through controlled fixtures; these are regression tests, not manual player usability sessions. The initial fixture waits were corrected to wait for actual physics, scene-load and game-time state.

A final input-driven gameplay check also reached victory in about 13 seconds, with Logan at 2/5 health. This used normal move/jump/attack inputs and no health, damage or position overrides. The short completion time remains a demo-design issue, outside this reliability fix.

Run again from Unity: **Tools → Hero → Run Lifecycle Regression Tests**. The suite is also available in the EditMode Test Runner. Tests enter and leave Play Mode and open Main, so save scene work before running them. XML results are written to `unity/Logs/hero-lifecycle-results.xml` (ignored by Git).

The runtime assembly definition separates game code from the Editor-only test assembly. No packages were added or upgraded.

## Scope remaining

Double jump, backflip, weapon switching, one-way platform improvements, the authored demo scene, camera/art overhaul and the full Canopy room remain planned. No Bezi task was dispatched; [the prepared handoff](BEZI-NEXT-TASK.md) defines its proposed first scene-layout assignment.

Unity’s [sceneLoaded event](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html) is used to rebuild Main on each intended scene load. The initial-only runtime hook remains an idempotent startup fallback.

All work targets `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-`. No OneDrive is used.
