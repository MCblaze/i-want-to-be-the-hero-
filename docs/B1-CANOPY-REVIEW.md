# Canopy Crossing B1 acceptance

14 September 2026 · Unity 6000.3.6f1 · Built-in renderer

Bezi created the authoring greybox from the reviewed Plan; Codex reviewed and repaired the saved result. **B1 composition is accepted. Runtime traversal, final art and showcase integration are not complete.**

## Saved output

- Scene: `unity/Assets/HeroDemo/Greybox/CanopyCrossing/CanopyCrossing_Greybox.unity`
- Prefab: `unity/Assets/HeroDemo/Greybox/CanopyCrossing/CanopyCrossing_Room.prefab`
- New folder/asset metadata accompanies those files. The existing `HeroDemo/Movement/GreyboxPixel.asset` Sprite subasset is referenced without modification.
- [Actual 16:9 preview-camera capture](evidence/b1/canopy-overview.png).

The scene has two roots: a connected room prefab instance and the preview camera at `(23.5, 2.5, -10)`, orthographic size 15. The prefab owns annotations plus `01_Sky`, `02_FarRuins`, `03_DistantForest`, `04_NearTrunks`, `05_PlayPlane` and `06_Foreground`.

## Review findings and corrections

The Plan needed one consolidated correction before Build: keep the lower recovery independent of the main route, make the upper branch rejoin, and widen the overview. The final dimensions are preserved in `BEZI-CANOPY-REVIEWED-PLAN.txt`.

Bezi recovered from one failed action batch and completed a 220-action checkpoint. Codex selected **Keep all** and took over the integration review. The first saved-state review found null font references in the serialized prefab, while the loaded Editor objects could display LegacyRuntime. More importantly, the first twelve label children had offsetting local positions that put their world positions at the origin. Four art reserve markers were also at the origin. A scene capture exposed the resulting overlapping text.

Codex assigned persistent built-in font/material references, reset label local transforms, placed the four reserve markers, shortened captions, and separated the legend, global provisional warning and route labels. All repairs remain inside the new Canopy prefab. The final prefab was saved and the scene reopened; raw serialization now contains no null font or sprite references. An automatically generated, unused `SceneTemplateSettings.json` was removed from the change set.

## Acceptance evidence

| Check | Result |
| --- | --- |
| Scene and prefab persist after reopening in Edit Mode | Passed; connected prefab, two scene roots, scene clean |
| Required main/recovery/upper routes and six scenery bands | Passed; geometry matches the reviewed dimensions |
| Component/reference audit | Passed; 88 GameObjects, 28 SpriteRenderers, 21 TextMeshes, no missing scripts/sprites/fonts |
| Ground roles | Passed; eight solid BoxCollider2D objects and five one-way BoxCollider2D/PlatformEffector2D pairs; all non-trigger, effectors enabled |
| Nonfunctional art reserves and decorations | Passed; no runtime scripts and no decorative colliders; waterfall/platform/checkpoint explicitly identified |
| Overview and label readability | Passed after correction; actual 1600×900 preview-camera PNG inspected; all route sections and legend visible |
| Foreground placement | Passed for greybox; framing sits at room sides/above upper route, with no opaque foreground over landings |
| Unity Console after saved-state review | Passed; Error/Exception query returned zero entries |
| Scope | Only new Greybox assets/metadata and review documentation/evidence; no existing gameplay, source art, packages or settings changed |
| Canopy movement, recovery, one-way traversal, checkpoint or moving-platform behavior | Not run; this scene has no playable controller or runtime mechanics |
| Parallax, final animation/audio, lighting, reduced-effects and player-experience qualification | Not run; later showcase work |

Main route: Start → Teaching → MainA → MainB → MainC → Rejoin → Finish. The long recovery shelf lies below; exit steps are at x=11 and x=37. Optional UpperA → UpperB → UpperC descends to Rejoin. The first upper ledge is intentionally above the measured first-jump rise, but its controller traversal remains provisional.

C1's previously measured 2.24-unit full-jump rise, 3.66-unit double rise and passed level-ground gaps are reference inputs, not proof that every differently elevated Canopy gap is safe. B2/C2 must verify the route with the actual controller before art hides its geometry.

## Next ownership

Codex owns gameplay integration and C1-F contact/presentation work. Bezi's separate `Sunleaf C1-F Art Handoff` Page is a specification, not new sprite artwork. Use `C1-F-SOURCE-CONTRACT.md` for exact frame/timing interfaces.

GitHub publication remains pending the existing account's repository write access. All work used `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-\unity`; no OneDrive.
