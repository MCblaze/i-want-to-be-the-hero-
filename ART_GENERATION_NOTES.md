# Art Generation Notes

The game artwork was generated specifically for this prototype using the built-in image-generation workflow. It was not copied from another platformer repository.

## Shared direction

- Use case: stylized concept and game asset.
- Visual target: original, child-friendly, handcrafted 16-bit-era pixel art.
- Pixel treatment: crisp square pixels, coherent clusters, limited palettes and readable silhouettes.
- Avoid: text, logos, watermarks, blood, gore, photorealism, gradients, antialiasing, trademarked characters and cropped subjects.

## `logan.png`

Create Logan as one original fictional nine-year-old adventurer in a strict side view facing right, centered on a transparent background. Use tousled warm-brown hair, a teal-blue tunic over a cream shirt, a rust-red scarf, small brown boots, a wooden practice sword and a small golden star badge. Keep his proportions, expression and silhouette cheerful, brave and age appropriate; no multiple poses or sprite sheet.

## `sunleaf-ruins.png`

Create a wide 16:9 side-view fantasy forest ruin environment with mossy stonework, enormous oak trees, blue-green hills, waterfalls, golden morning light, small red mushrooms and magical motes. Keep the central play space comparatively low contrast and suggest distinct near, middle and far depth layers. Environment only; no characters, platforms, HUD or text.

## `mossback-guardian.png`

Create exactly one squat, ancient stone-golem guardian in strict side view facing left on a transparent background. Build it from rounded moss-covered ruin stones with amber eyes and a glowing star-shaped chest rune. Make it imposing but friendly-fierce and suitable for a child, with broad readable arms and no weapons.

## `thornling.png`

Create exactly one knee-high forest creature in strict side view facing left on a transparent background. Give it a round leaf-and-vine body, tiny feet, one curled twig horn, large amber eyes and a few soft orange thorns. It should read as playful fantasy danger, not horror.

## Implementation note

The original four images remain in the project. The animation update adds 24 Logan poses, 16 Thornling poses and 16 Guardian poses, generated from their original character images. The sheets use real RGBA transparency, point sampling, individual source rectangles and foot pivots. The actual returned dimensions are 1024×1536 for Logan and 1254×1254 for each creature; code does not assume the generator followed the requested cell grid.

The game and viewer render these frames directly. Generation did not perfectly preserve a strict pixel grid or consistent anatomy, so final production art would benefit from hand cleanup. Small trims around two neighbouring Logan poses prevent one sprite's outer tips from appearing in the other frame. No source PNG pixels were repainted by the atlas tooling.

## Animation generation prompts

The following prompts used each original character PNG as the design reference. Background extraction edits then requested removal of the painted checkerboard with real alpha transparency while keeping poses and positions. The final files were checked for alpha and rendered against a contrasting background.

### `logan-animations.png`

Reference: `logan.png`.

```text
Use case: stylized-concept. Asset type: production pixel-art animation sprite sheet for a side-scrolling game.
Input image 1 is the CHARACTER DESIGN REFERENCE: Logan, a fictional nine-year-old boy. Preserve brown tousled hair, large expressive eyes, teal tunic, cream sleeves, rust-red scarf, leather boots, gold star badge and short wooden practice sword. Child proportions, hopeful expression. Simplify into crisp readable 16-bit game sprites with deliberate square pixels.
Create EXACTLY 24 distinct poses in a STRICT FOUR COLUMNS by SIX ROWS evenly spaced grid on a 1024x1536 TRANSPARENT PNG canvas. Every cell is 256x256. No extra columns, no labels, no grid lines. Full character in each cell, no cropping, no shadows/backdrop/checkerboard art. All frames face SCREEN RIGHT, same camera and consistent body size (standing body about 160 pixels high), body centered at cell x=128, ground baseline y=224. Keep 18px empty margins and keep all weapons inside cell. Each row is one animation sequence, read left-to-right:
ROW 1: 4-frame idle breathing loop, scarf gently flicks, knees subtle flex, one blink.
ROW 2: 4-frame RUN cycle: front leg extended/rear back; legs passing with knees bent; opposite leg extended; opposite legs passing. Strong alternating leg and arm poses, scarf trails left.
ROW 3: ascending jump knees raised; airborne apex tucked legs; descending jump feet extended; landing crouch with knees bent. Keep same grounded cell origin; world motion is added by code.
ROW 4: wooden sword swing: sword drawn back and up in anticipation; sweeping forward reaching to the right; full extended follow-through sword down-right; recovery back to ready. A short cream pixel slash arc only in middle two frames.
ROW 5: dash launch crouch; forward lean dashing with scarf trailing; forward dash opposite legs and scarf; braking stance. Clear readable poses, no giant effects.
ROW 6: hit recoil leaning backward; recovering from hit; victory wooden sword lifted overhead; happy victory fist up and sword raised.
This is usable game sprite data, not an illustration of a sheet. Exact grid, all 24 cells occupied, transparent background, isolated consistent silhouettes, no text, no numbers, no scenery.
```

### `thornling-animations.png`

Reference: `thornling.png`.

```text
Use case: stylized-concept. Asset type: production pixel-art animation sprite sheet for side-scrolling game.
Input image 1 is the CHARACTER DESIGN REFERENCE: Thornling, cute small rounded leaf-and-vine woodland creature, one prominent amber eye, curly brown twig antenna, green layered leaves, little feet, soft orange thorns. Keep the design child-friendly. Crisp 16-bit pixel-art, clear square pixels and limited green/amber/brown palette.
Create EXACTLY 16 distinct poses on a STRICT FOUR COLUMNS by FOUR ROWS grid on a 1024x1024 TRANSPARENT PNG. Each cell 256x256. Same scale, 160px tall idle silhouette, facing SCREEN LEFT in every cell. Body horizontally centered x=128; ground baseline y=224 per cell. All limbs/antenna inside cells, 18px margins. No text, labels, grid lines, shadows, backdrop or painted checkerboard.
ROW 1: four-frame idle breathing loop with antenna gently swaying and amber eye blinking.
ROW 2: four-frame walk loop: step left foot forwards, passing feet with body dipping, opposite leg forwards, opposite passing feet. Clear alternating small legs, leaf body bouncing slightly.
ROW 3: four-frame bump/lunge: crouch to prepare, lean face forward to left, small forward hop with feet back, settle down in ready pose.
ROW 4: hit recoil with leaf edges flaring; recovering from hit; defeated drooping leaf creature sitting softly; a harmless compact pile of loose green leaves and twig, no gore.
Exact 4x4 game animation sheet, full consistent silhouettes, smooth sequential changes, alpha transparency.
```

### `mossback-guardian-animations.png`

Reference: `mossback-guardian.png`.

```text
Use case: stylized-concept. Asset type: production pixel-art boss animation sprite sheet.
Input image 1 is the CHARACTER DESIGN REFERENCE: Mossback Guardian, friendly imposing squat stone golem with moss shoulders, huge rock fists, gray stone plates, amber eyes, glowing golden star chest rune. Preserve recognizable mossy stone design. Crisp readable 16-bit pixel art with deliberate square pixels, no painting blur. Side view facing SCREEN LEFT.
Create EXACTLY 16 distinct animation poses in a strict FOUR COLUMNS by FOUR ROWS grid, 1024x1024 transparent PNG, each cell256x256. Same scale and camera in all frames, body around170px tall. Center at cell x128 with ground baseline y224; keep arms fully inside cells and leave 16px empty margins. No labels, grid, text, shadows, scenery or fake checkerboard.
ROW 1: four-frame idle loop: relaxed fists; slight stone chest lift and rune brightens; upper body lowered; return, moss and tiny leaves gently move.
ROW 2: four-frame attack warning: feet planted fists pulled back; deep crouch leaning back; rune shines and fists rise; full forward-lean ready-to-charge anticipation. No projectiles or new limbs.
ROW 3: four-frame CHARGE/RUN cycle toward left: huge leftward stride with opposite arm swinging; bent-knee passing position; opposite leg and arm stride; opposite passing position. Clearly alternate legs, chest tilted forward throughout.
ROW 4: hurt recoil leaning back with rune dim; recovery guarded stance; defeated kneeling peacefully with hands on ground; defeated seated bowed head and softly glowing rune, child-friendly.
Usable transparent game animation data, exactly16 poses, consistent body proportions and baseline, no cropped arms, no text.
```
