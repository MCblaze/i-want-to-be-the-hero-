# Client-ready demo audit

**Audit date:** 16 September 2026
**Baseline:** `41aece5` plus delivery production changes
**Status:** Functional six-zone prototype; final production art, sound and client qualification remain incomplete.

## Verified foundation

### Correction and repair checkpoint — 16 September 2026

The earlier ability results applied to the movement lab. Main did not reference HeroTuning, so its double-jump/backflip/wand paths were unavailable. This has now been connected and tested in Main for wand switch/fire, backflip and air-jump availability. Dedicated animation artwork is still missing. See `PLAYTEST-2026-09-16.md` and its test evidence for the current scope.

Platform walk lines were aligned to existing collider tops; small ledges now use a detail crop instead of compressing a full terrain illustration. Five thorn skins replace hazard placeholders, and camera-filling zone backgrounds replace exposed rectangular card edges. Main's viewport-dependent camera bounds are covered by regression checks. These repairs improve the prototype; they do not qualify it as client-ready or establish the requested 10–15 minute duration.

Follow-up: added pause/resume/restart controls, cleared paused input, accurate weapon/evade HUD, touch weapon swap, nine-slice UI framing, and transparent checkpoint/Seed artwork. Removed the unreachable duplicate Spark preview and repaired ambient particles that rendered as opaque squares. Standalone start, weapon swap, pause/resume and focus-loss checks now pass. HUD/menu layout repairs and six underpass fixes are saved; physical-device qualification and final presentation review remain open. See the playtest record.

- One editable Main scene with Trailhead, Broken Steps, Canopy Lift, Spark Shrine, Gate Return and Guardian Court.
- Movement, double jump, backflip/evade, sword/wand choice, Spark, checkpoint, gate, Guardian, victory and restart loops.
- Movement 10/10, lifecycle 4/4, accessibility 3/3, graphical animation audit 6/6 and manifest/browser animation regression 10/10.
- Windows x64 build succeeds and reached a responsive running state twice.
- F1 reduced effects and F2 mute retain visible status cues.

## Client-facing production gaps

### Character animation — release blocker

- Author weapon-consistent sword idle/run/air/land/dash/hurt frames.
- Author wand idle/run/air/fire/recovery frames; current cast reuses a swordless rise pose.
- Author dedicated double-jump, multi-frame backflip and defeat/respawn sequences.
- Separate and pad Logan atlas frames 18 and 22 to restore clipped boot/sword pixels.
- Add final landing, hurt and victory transition polish and perform an art-direction review at gameplay scale.

### Terrain, platforms and landmarks — release blocker

- Mossy-stone skins are integrated and six low ledges have been raised for clearance. Remaining work: visual consistency, variation and complete route accessibility review.
- Add readable caps, corners, undersides, broken variants, slopes, spikes and a distinct teal moving-platform family.
- Give every zone one authored landmark: canopy threshold, broken masonry rhythm, lift structure, Spark shrine, gate/cache frame and Guardian court.
- Preserve bright platform tops, unobscured hazards and the existing measured jump contract.

### Background, lighting and world motion — release blocker

- Six background bands and coverage/crossfades are implemented across x=0–51. Expand coverage with the route and finish per-zone depth/art review.
- Add foreground foliage exclusions, fog/depth cards, contact shadows and restrained painted-light overlays using the current built-in renderer.
- Add grass/canopy/water loops, leaf drift, landing/air-jump/weapon impacts, checkpoint/Spark/gate/boss effects, all respecting reduced effects.
- Do not migrate to URP until a separate renderer proof has matching sprite/normal assets and measured benefit.

### Audio and presentation — release blocker

- A 13-clip procedural audio foundation and audio director are integrated. Final listening/mix, footsteps and victory treatment remain open.
- Add original/licensed exploration, danger and victory loops plus jump, air jump, backflip, switch, sword, wand, damage, checkpoint, Spark, gate, boss and victory cues.
- Add mixer routing, Music/SFX volume controls and muted-audio visual equivalents.
- Replace rectangular runtime UI styling with a coherent panel/icon kit; add pause, display and input guidance.

### Release engineering and evidence

- Replace `DefaultCompany` / `unity`, add version/icon/splash decisions and client-facing build information.
- Add credits, licenses/provenance, delivery README, version manifest and packaged Windows build.
- Record CPU/GPU frame time, memory, draw calls and scene-load evidence on a named Windows machine.
- Qualify a physical controller, focus loss/device switching, graceful standalone shutdown and accessibility readability.
- Run five fresh-player sessions. Target 10–15 minutes first success, 6–9 learned route and 15–20 thorough route.

## Execution order

1. Lock the visual target with Trailhead, Canopy Lift and Spark Shrine captures.
2. Integrate and approve the modular terrain/prop kit; then skin all six zones without changing colliders.
3. Complete Logan weapon, ability and defeat animation packs and rerun all animation/movement/lifecycle checks.
4. Add full-route background bands, landmarks, built-in lighting overlays, VFX and environment motion.
5. Implement sound/music, mixer/settings, UI polish and boss/ending payoff.
6. Apply release identity, performance instrumentation, client packaging and documentation.
7. Complete controller, human timing/accessibility and client acceptance gates.

## Generated concept sources

Two AI-assisted concept sheets were generated during this audit: a mossy stone terrain/platform kit and a Sunleaf foliage/ruin prop kit. They are visual direction sources only until their backgrounds are converted to true alpha, elements are sliced, imports are normalized, gameplay-scale readability is reviewed and provenance is accepted. Do not claim them as integrated production assets.

## Definition of client-ready

The demo is ready to send only when every required visual and audio asset is integrated, all automated checks pass after integration, the Windows package carries correct identity and documentation, all third-party or generated assets have accepted provenance, and the controller/five-player/client checks have recorded evidence. Missing human evidence must remain marked **Not run**.


## Active delivery plan

See [Client delivery execution](CLIENT-DELIVERY-EXECUTION-20260916.md). Bezi owns scoped proposal/specification work; Codex generates missing sources, integrates and verifies. Bezi's connected generation capability is unavailable; this does not block Codex image generation. Required human playtest and client acceptance evidence remains open.
