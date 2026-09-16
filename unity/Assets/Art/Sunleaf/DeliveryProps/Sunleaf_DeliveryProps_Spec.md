# Sunleaf DeliveryProps — Batch A production specification

Status: gate source supplied by Codex; pending final visual review. Remaining DeliveryProps sources are not generated.

## Ownership and boundary

Bezi-owned scope: `Assets/Art/Sunleaf/DeliveryProps` only.

Codex-owned source: `Assets/Art/Sunleaf/DeliveryGenerated`. Do not overwrite, move, reimport, or modify it.

Do not integrate into `Main`, alter existing art, edit scenes/scripts, run Play Mode, or change gameplay. Codex owns integration, testing, checkpointing, commits, and final acceptance.

## Corrected sizing contract

`32 PPU` is not an accepted Main contract and remains unproven by the animation brief. No universal PPU is specified here. Each asset must be reviewed as measured source pixels against its desired world-space size and collision/visual target.

Main target references supplied by Codex:

- Standard target/collision bounds: `0.96 × 0.96 world units`.
- Gate collider: `0.30 × 4.60 world units`, position `(40.25, -1.2)`.
- A six-unit arch is inappropriate for the gate target.

## Measured source and desired world size

| Asset / source | Measured source pixels | Desired world size / placement target | Status |
|---|---:|---|---|
| Codex gate source in `Assets/Art/Sunleaf/DeliveryGenerated/SunleafRuneBarrier.png` / `SunleafRuneBarrierSprite.asset` | Sprite rect `191×1290`; asset PPU `280.43478`; calculated sprite size approximately `0.681×4.600` | Visual source approximately `0.681×4.600`; gate collider remains `0.30×4.60` at `(40.25,-1.2)` | Supplied by Codex; pending final visual review |
| `Sunleaf_RuneInset_A` | Proposed source `64×64` pixels; final source measurement pending | Fit within the supplied gate’s inset; target no larger than `0.96×0.96` world bounds | Specification only |
| `Sunleaf_RuneInset_B` | Proposed source `64×64` pixels; final source measurement pending | Same footprint as dormant inset; active state must not change collision or bounds | Specification only |
| `Sunleaf_SwordTarget_A` | Proposed source `64×80` pixels; final source measurement pending | Fit within `0.96×0.96` world target bounds | Specification only |
| `Sunleaf_WandTarget_A` | Proposed source `64×80` pixels; final source measurement pending | Fit within `0.96×0.96` world target bounds | Specification only |
| `Sunleaf_ShrineAccent_A` | Proposed source `96×112` pixels; final source measurement pending | Fit within `0.96×0.96` world target bounds unless Codex assigns a separate decorative footprint | Specification only |

The existing reference import settings remain descriptive only, not a new contract: `GateReturn.png`/`SparkShrine.png` use `120 PPU`; `ProgressionProps.png` uses `100 PPU`; `StaticMossyLedge.png`/`MovingTealPlatform.png` use `256 PPU`. Their texture pixel dimensions were not exposed by the mounted importer inspection.

## Visual/source requirements

Transparent RGBA PNG, side-on pixel art, no opaque background, no baked UI/text, no unapproved post-import scaling, alpha from input, and source provenance recorded. Filtering, wrap, compression, and PPU must be selected during Codex review against the actual Main presentation; they are not asserted here as universal Main requirements.

## Logan weapon pose proof brief

Source sheet: `Assets/Resources/Art/Animations/logan-animations.png`.
Manifest: `4×6` layout, nominal pivot `(0.5,0.875)`, display height `96`, Unity height `1.62`. The nominal pivot is not the per-frame pivot; preserve the measured per-frame feet baseline.

Exact source-frame references:

- Costume and baseline reference: frames `0–3` (`idle`), regions `(54,19,197,240)`, `(289,18,212,241)`, `(551,19,196,240)`, `(806,18,186,241)`; per-frame pivots from checklist.
- Locomotion silhouette reference: frames `4–7` (`run`).
- Existing combat timing/silhouette reference only: frames `12–15` (`attack`), regions `(44,776,182,247)`, `(285,785,235,237)`, `(533,801,253,222)`, `(804,787,188,236)`. Weapon identity is unverified.
- Airborne baseline reference: frames `8–11` (`rise`, `apex`, `fall`, `land`).

Generate these four poses first, in this order:

1. `Logan_Sword_Idle_F_R1` — establishes hand, blade, costume, feet baseline, and facing.
2. `Logan_Sword_Attack_F_R1` — weapon-read and attack silhouette, using frames `12–15` only as unverified timing/composition references.
3. `Logan_Wand_Aim_F_R1` — establishes the distinct Sunseed Wand read and hand alignment without reusing sword pixels.
4. `Logan_Wand_Fire_F_R1` — establishes the readable cast/fire action and event-anchor candidate; do not infer final timing until the unavailable audit/brief is mounted.

Consistent rules for all four: preserve Logan’s established costume, proportions, face/hair/silhouette language, feet baseline, and stable gameplay collider; keep weapon hand attachment consistent across sword and wand; use explicit weapon/action/facing/revision names; avoid ambiguous `attack`/numeric-only exports; provide the opposite facing only after the forward-facing proof is accepted; do not bake weapons into the wrong weapon set; keep transparent padding and inter-frame separation measured, especially around Logan18 and Logan22, which remain uninspected.

## Evidence and gate state

- Existing art and importer references inspected: Passed.
- Gate source overwritten: Not run; intentionally prohibited.
- Gate final visual review: Not run; Codex-owned source pending review.
- Actual-size final DeliveryProps contact sheet: Not run; no generated DeliveryProps sources.
- Final alpha/pivot/world-size validation: Not run.
- Animation pixel-content and weapon-read verification: Not run; source PNG content remains uninspected.
- Generation retries: Not run.
- Handoff: stop for Codex integration/final visual review.
