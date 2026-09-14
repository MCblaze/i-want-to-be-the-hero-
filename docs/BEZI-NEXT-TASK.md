# Bezi’s next task: Canopy Crossing greybox

Use the full [Sunleaf Ruins demo plan](https://app.notion.com/p/3dba2b7d211481e58e4ffda252533eaf) as context, with the [asset register](https://app.notion.com/p/3dba2b7d2114813daf1bff41107fe80a) for later art work.

Work in `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-\unity`. Do not use OneDrive.

## Ownership

Codex leads gameplay code, controls, integration, regression tests and release checks. Bezi’s proposed first assignment is scene layout and composition. Hand off one reviewed scene at a time. Both tools should use the same agreed baseline and inspect local changes before editing.

This brief is prepared for a future handoff; no Bezi job has been dispatched by this document.

## Deliverable

Create a separate, authoring-only `CanopyCrossing_Greybox` scene and room prefab. Keep existing Main and gameplay scripts untouched. The scene must be visible and understandable in Edit Mode.

Include a start marker, a safe teaching ledge, a main platform route, a lower recovery shelf, an optional upper branch that rejoins, a waterfall composition placeholder, six named scenery depth bands, camera-bound markers and a checkpoint marker. Use simple distinguishable shapes. Mark jump/dash-dependent arrangements as provisional until Codex supplies measured movement limits.

Use the demo plan’s Canopy section as the layout brief. Ground and landing edges must read clearly at a 16:9 view. Keep foreground out of the main landing and enemy-warning areas. Label one-way platform intentions distinctly from solid ground.

This first handoff is layout only. Do not install render packages, replace the current art importer, change project settings or implement player/enemy scripts. Camera, light, checkpoint and moving-platform markers may remain labeled placeholders for Codex integration.

## Completion report

Return the scene/prefab paths, a Scene-view overview, a gameplay-camera composition view, intended player route, unresolved movement assumptions and any import/Console errors. Codex will integrate and test the result before final art production.
