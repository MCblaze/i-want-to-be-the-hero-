# C1-F source contract for art and feedback

Prepared from local `ec8e7e5`, 14 September 2026. This fills source facts missing from Bezi's **Sunleaf C1-F Art Handoff** Page. It is a handoff specification; C1-F artwork/effects are not implemented by this document.

## Current renderer and manifest

`unity/Assets/Resources/Art/Animations/manifest.json` defines Logan as `logan`, using `logan-animations.png`, 4 columns × 6 rows, facing right (`1`), and `unityHeight: 1.62`. The runtime frame names are `logan_00` through `logan_23`. Existing art therefore has 24 Logan slots; the planned 42-clip/218-slot combat-art budget describes a larger future production set, not the current Logan sheet.

| Clip key | Frame indices | FPS | Loop |
| --- | --- | --- | --- |
| idle | 0–3 | 5 | yes |
| run | 4–7 | 11 | yes |
| rise / apex / fall | 8 / 9 / 10 | 1 | no |
| land | 11 | 10 | no |
| attack | 12–15 | 12.5 | no |
| dash | 16–19 | 23.53 | no |
| hurt | 20–21 | 10 | no |
| victory | 22–23 | 4 | yes |

`PixelSpriteAnimator` creates an **Animated Sprite** child at the collider's foot baseline. Frames use manifest crop regions and per-frame pivots. Pivot Y is converted from image-down coordinates with `1 - py`. The default pivot `(0.5, 0.875)` is overridden by the supplied per-frame array; do not replace those per-frame anchors with one generic pivot. Pixels per unit currently come from sheet cell height divided by `unityHeight`, not a global 32 PPU setting. Point filtering and clamp wrapping are applied at runtime.

The visual child owns facing through `SpriteRenderer.flipX`. The physics root stays unscaled/unrotated by animation. Maintain the current sheet/manifest path and lowercase clip keys until the integration owner adds reviewed keys. Naming examples in a creative brief are not automatic instructions to rename runtime assets. Keep the corresponding web animation manifest in sync when changing shared source rectangles/timing.

## Timing and state ownership

The controller accepts jump input immediately and applies velocity on a physics update. Do not insert a takeoff anticipation delay. The extra jump uses velocity 8.5 after the first jump's 10.6; its accepted input is the event, not reaching an animation frame.

Sword duration is 0.32 seconds, with real damage checks at attack age 0.08–0.20 seconds and one hit per target per swing. A wand shot is created on accepted attack input; its recovery pose lasts 0.20 seconds and firing cooldown is 0.45 seconds. Switching weapons cannot convert an attack already in progress or reset either cooldown.

Dash duration is 0.17 seconds. Backflip lasts 0.40 seconds, preserves facing, and avoids damage only at age 0.08–0.22 seconds. Both share a 0.70-second cooldown in the lab. Backflip currently rotates the existing apex pose on the visual child; the final clip must replace that provisional rotation, not rotate the collider.

Current presentation precedence is won → not-started → hurt → backflip → dash → attack → airborne rise/apex/fall → landing → run/idle. The animator currently has no general frame-event metadata. C1-F must deliberately extend presentation/event handling rather than assuming Animator Controller or Unity Animation Events are already connected.

## Proposed feedback interface

The controller/combat system emits accepted jump, extra jump, landing, attack release, hit, hurt and evade events. Include actor, event kind, contact/world position, surface, intensity, facing and a monotonically increasing event ID within the session. Presentation consumes these; it never independently decides damage or jump success.

Footstep markers additionally require real ground contact and locomotion. Ground dust uses the actual contact point. Deduplicate landing events; suppress setup, teleport and respawn landings. Clear effect history and active pooled bursts on restart. Pool reset includes lifetime, transform, velocity, tint, frame and owner. Water may use an independent loop period; foliage shares a coherent wind direction with varied phase. Optional reduced feedback must preserve essential landing/hazard cues.

Acceptance: before/after movement measurements remain within the existing test tolerances; no airborne footsteps, duplicate or respawn dust, stale effects, collider movement, attack-timing drift or stuck poses. Review both facings at gameplay scale and preserve keyboard/controller mappings. Final frame allocation, sprite exports, effect artwork/audio and target-device performance remain open.
