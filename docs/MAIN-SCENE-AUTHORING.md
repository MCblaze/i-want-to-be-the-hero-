# Editing the Main Scene

The `Main` scene is now a saved, editable Unity scene. Its camera, background, platforms, hazards, checkpoint, Hero Spark, gate, and position markers remain visible when Play Mode is stopped.

## Open the editable level

1. In Unity, choose **Tools > Hero > Open Editable Main Scene**.
2. Open the **Hierarchy** tab on the left.
3. Expand **Main Game Session**.
4. Expand **Platforms - duplicate and edit**.

The scene is also stored at `unity/Assets/Scenes/Main.unity` and can be opened normally from the Project panel.

## Edit a platform

Each platform has a clearly named parent ending in **- edit or duplicate**. Select that parent to move the whole platform. Duplicate the parent to create another platform.

Expand a platform to edit its two visual parts:

- **Stone Body** controls the main solid block. Change its Transform scale to alter width or height.
- **Grass Edge** is the top strip. Match its width to the Stone Body and move it vertically if the platform height changes.

The collider is on the platform parent, so resize its `Box Collider 2D` when changing the platform dimensions. Press **Ctrl+S** to save.

## Other editable groups

- **Hazards - move or duplicate** contains the five hazard markers.
- **Progression - checkpoint, Spark and gate** contains the checkpoint, Hero Spark, and exit gate.
- **Markers - move these transforms** contains Logan's starting position.
- **Environment - edit these objects** contains the saved camera and background.

Logan's gameplay character, enemies, and the HUD are created when Play Mode starts because they reset during retries. A visible Logan spawn preview remains in Edit Mode so the start position can still be placed precisely.

## Recovery command

**Tools > Hero > Build Editable Main Scene** rebuilds the generated starter layout. Unity asks for confirmation before replacing the current Main Game Session, including any platform edits. Use it only when you intentionally want to reset the layout.

## Verification

The combined Unity Edit Mode suite passes all five tests, including the saved-scene authoring check. The result is stored in `docs/evidence/main-authoring/main-scene-authoring-results.xml`.

- `edit-mode-camera.png` shows the stopped Game view rendering through the saved camera.
- `scene-overview.png` shows the full authored level in the Scene view.

This project and all evidence are stored in the GitHub checkout. OneDrive is not used.
