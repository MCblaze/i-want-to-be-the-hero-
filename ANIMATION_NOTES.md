# Character animation update

All three characters now use frame-by-frame sprite animation in the browser and Unity projects. The PNG sheets and JSON timing data are included as source assets.

## Play and inspect

- `PLAY_BROWSER.bat` starts the game on Windows. Other systems can open `web/index.html`.
- `PREVIEW_ANIMATIONS.bat` opens the animation viewer. Other systems can open `web/animation-preview.html`.
- The viewer shows every clip, with pause, speed and facing controls. It also works offline.
- Use Unity 6.3 (6000.3.x) for `unity/`; Unity 2019.4 cannot compile this project's language features and APIs.

## Behavior

Logan's animation follows movement and gameplay state: victory, hurt, dash, attack, airborne movement, landing, then running or idle. Running speed affects playback speed. Jump rise, apex and fall are selected from vertical velocity; a short landing crouch adds weight without delaying controls.

Sword attacks last 0.32 seconds. The first 0.08 seconds are anticipation; damage is active from 0.08 to 0.20 seconds, followed by recovery. Each target can be hit once per attack. Hurt and dash interrupt a swing. Facing stays committed during attack, hurt and dash.

Hero Dash unlocks with the Hero Spark, lasts 0.17 seconds, and leaves short-lived visual afterimages. Afterimages never change the collider. Browser pause freezes animation clocks and effects; the Unity prototype does not yet have an in-game pause menu.

Thornlings have idle, walking, contact lunge, hurt and leaf-pile defeat clips. A defeated Thornling stops participating in collisions immediately and remains visible briefly before fading. The contact lunge reacts to contact; it is not a separate enemy attack mechanic.

The Guardian has breathing, a visible charge warning, charging, hurt and kneeling defeat. It keeps its advertised facing direction throughout a charge. Logan celebrates for 1.8 seconds before the victory overlay appears.

## Files to change

| File | Purpose |
| --- | --- |
| `web/assets/animations/*.png` | Original generated RGBA sheets; one per character |
| `web/assets/animations/manifest.json` | Canonical frame regions, foot pivots, clips, speed and scale |
| `web/animations.js` | Playback clocks, frame selection, facing and Canvas drawing |
| `web/game.js` | State selection and combat timing |
| `unity/Assets/Scripts/PixelSpriteAnimator.cs` | Creates cached Unity sprites and animates a visual child |
| `unity/Assets/Scripts/HeroPrototype.cs` | Unity movement, combat and animation transitions |
| `unity/Assets/Editor/AnimationArtImporter.cs` | Point filtering, real alpha, uncompressed textures and original dimensions |

Frame regions use PNG coordinates: origin at top left, pixels for `x/y/w/h`. Pivot coordinates are normalized within each region and pin the visible feet to the ground. Unity converts Y coordinates when creating its sprites. Both renderers use a constant source-pixel scale across all frames; cropped poses do not stretch to fill a cell.

After editing the manifest, run `node tools/sync-animation-data.cjs`. It writes `web/animation-data.js` and the matching Unity manifest. If replacing art, copy the same PNGs to `unity/Assets/Resources/Art/Animations/`. Original single-pose art remains available as a fallback.

`tools/inspect-animation-art.cjs` and `tools/build-atlas-regions.cjs` read alpha components to identify poses without repainting the PNGs. They require `sharp`. The region builder includes a small manual trim between two closely spaced Logan poses; review or remove that adjustment if the sheet is replaced. Always inspect every frame after rebuilding regions.

## Verification and remaining work

`node --test tests/animations.test.cjs` runs ten dependency-free tests against the real browser animation and game code. They verify shared data, atlas bounds and scale, timing at 30/60/120 updates per second, sword damage windows, jump phases, dash unlock and afterimages, pause, hurt, respawn, defeat and victory. The browser boundary is mocked, so these tests do not validate DOM events, audio or actual hardware input.

All 56 poses were rendered and inspected on a contrasting background. The actual game's start and boss encounter were also rendered locally through Canvas. Optional `tools/render-animation-preview.cjs` and `tools/render-game-check.cjs` reproduce those checks with `@napi-rs/canvas`.

No Unity Editor was available, so this update has not been compiled or run in Unity. A real browser, controller and tablet playtest is still needed. The generated poses are suitable for prototype iteration; hand cleanup can improve limb alternation, silhouette consistency and the pixel grid for final production art.
