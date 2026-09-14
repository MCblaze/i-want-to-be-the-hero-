# Bezi's next task: Canopy Crossing greybox

Task B1 · brief version 1.3 · 14 September 2026 · completed and reviewed; retained as the executed brief

The Canopy scene and prefab now exist. Do not dispatch this creation brief again. Read [B1 acceptance](B1-CANOPY-REVIEW.md) for saved paths, review repairs and remaining traversal/integration work. The next geometry assignment is B2, using the measured C1 contract and the existing prefab as its baseline.

[Notion workflow and assignment](https://app.notion.com/p/3dba2b7d2114815594f3fc1fb6e6b53f) · [Reusable playbook](BEZI-PLAYBOOK.md)

Use `BEZI-PLAYBOOK.md` and `BEZI-TEMPLATES.md` for the reusable method, and `BEZI-DEMO-WORKFLOW.md` for this project's ownership and dependencies.

## Target and context

Unity project: `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-\unity`. **Do not use OneDrive.**

Unity 6000.3.6f1; current built-in renderer. Current gameplay baseline: local `ec8e7e5`. Read MOVEMENT-LAB.md for measured C1 results and BEZI-CANOPY-REVIEWED-PLAN.txt for the reviewed exact geometry and camera framing. This original brief records the scope; the reviewed Plan supplies final construction dimensions. Confirm current revision, pre-existing local changes and the exact target before editing. Codex owns existing gameplay scripts, Main, packages and settings during this task.

Design source: [Sunleaf Ruins demo plan](https://app.notion.com/p/3dba2b7d211481e58e4ffda252533eaf), specifically Canopy Crossing, movement geometry and visual depth. Use the [asset register](https://app.notion.com/p/3dba2b7d2114813daf1bff41107fe80a) only for relevant naming/style context. Verify these sources are accessible in Bezi or supply dated excerpts in a Project Context Page. Pin that Page and the actual reference assets with the @ picker.

Read the QP1 section of [DEMO-POLISH-PLAN.md](DEMO-POLISH-PLAN.md) for future presentation requirements. This greybox reserves their space with markers; later assignments implement loops, effects, inhabitants and lighting. The current custom sprite animator and legacy input remain Codex-owned. Full tablet work is outside this Windows-first assignment.

## Mode and objective

First use Ask to inspect the correct project and available primitives/reference assets. Then use Plan for this one output. Review its scope, select Build, and let Agent complete the layout and evidence within the brief.

Create a separate **authoring-only** `CanopyCrossing_Greybox` scene and reusable room prefab so the intended route and visual composition can be inspected in Edit Mode. This assignment establishes composition, not verified traversability.

## Allowed output scope

- New assets under `Assets/HeroDemo/Greybox/CanopyCrossing/`, including necessary new parent-folder `.meta` files.
- Proposed scene: `Assets/HeroDemo/Greybox/CanopyCrossing/CanopyCrossing_Greybox.unity`.
- Proposed prefab: `Assets/HeroDemo/Greybox/CanopyCrossing/CanopyCrossing_Room.prefab`.
- Supporting greybox materials or primitives in the same folder; reuse appropriate existing assets by reference.
- Existing Main, scripts, source art, importers, packages, build settings and project settings are outside this assignment. No new gameplay script is required.

If a proposed output already exists, inspect it and report the conflict rather than overwriting another owner's work. Use built-in Actions and existing components. Stop before adding a dependency or expanding the folder scope.

## Room requirements

Include a labelled start marker, safe teaching ledge, main platform route, lower recovery shelf, optional upper branch that rejoins, waterfall composition placeholder, camera-bound markers and a checkpoint marker. Mark the slow moving-platform intention with a labelled placeholder.

Reserve labelled markers for a grass/canopy wind area, localized water response, one nonblocking ambient inhabitant and a focal-light/reveal composition. Keep them clear of landing edges and combat sightlines. These markers do not add scripts, working lights, dialogue or cutscene behavior.

Create six named scenery bands: `01_Sky`, `02_FarRuins`, `03_DistantForest`, `04_NearTrunks`, `05_PlayPlane`, `06_Foreground`. They establish hierarchy and composition here; parallax motion belongs to a later integration task.

Use simple distinguishable shapes and a clear legend. Differentiate solid ground, intended one-way platforms, decorative scenery and placeholders. Keep foreground clear of landings and enemy-warning space. Compose for 16:9 and provide a preview camera using existing supported components.

Label jump/dash arrangements **PROVISIONAL — traversal untested**. C1 measurements are available; differently elevated Canopy gaps still need actual controller validation. Final gap sizes, double-jump access and moving-platform timing depend on that validation. The saved preview camera is functional for authoring; checkpoint, waterfall and platform motion remain explicitly labelled placeholders.

## Acceptance and evidence

1. Scene and prefab are saved at the returned paths; reopening the scene in Edit Mode preserves the complete room. Return a hierarchy overview.
2. All required route elements and six scenery bands exist and are named. Return the relevant object list and Scene-view capture.
3. The main route, recovery shelf and rejoining optional branch are visually understandable. Return an annotated route description and a 16:9 camera capture.
4. Intended one-way platforms and nonfunctional placeholders are visibly identified. List every movement-dependent assumption; do not report successful traversal without a playable controller test.
5. No missing-script/import/compile errors are introduced. Report actual checks and remaining errors, separating pre-existing ones.
6. The changed-file report contains only the allowed new asset scope and its metadata. Resolve pending Bezi suggestions before handing ownership back.
7. The future wind/water/inhabitant/light markers are present and do not obscure the route. Return a gameplay-scale readability review; mark animation, lighting, contact feedback and cutscene checks Not run at this greybox stage.

Return Passed/Failed/Not run for each item, exact asset paths, evidence and unresolved assumptions. Codex then reviews the diff and integrates/tests gameplay before final art. This task does not publish to GitHub or install packages, Skills or connections.
