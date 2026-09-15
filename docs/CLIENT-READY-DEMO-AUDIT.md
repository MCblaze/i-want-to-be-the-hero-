# Client-ready demo audit

**Audit date:** 15 September 2026  
**Baseline:** `49255b9`  
**Status:** Functional six-zone prototype; final production art, sound and client qualification remain incomplete.

## Verified foundation

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

- Replace flat colored platform bodies with a modular mossy-stone skin while retaining the accepted colliders.
- Add readable caps, corners, undersides, broken variants, slopes, spikes and a distinct teal moving-platform family.
- Give every zone one authored landmark: canopy threshold, broken masonry rhythm, lift structure, Spark shrine, gate/cache frame and Guardian court.
- Preserve bright platform tops, unobscured hazards and the existing measured jump contract.

### Background, lighting and world motion — release blocker

- Replace the single flattened backdrop plus tinted rectangles with authored six-band coverage across the full x=0–51 route.
- Add foreground foliage exclusions, fog/depth cards, contact shadows and restrained painted-light overlays using the current built-in renderer.
- Add grass/canopy/water loops, leaf drift, landing/air-jump/weapon impacts, checkpoint/Spark/gate/boss effects, all respecting reduced effects.
- Do not migrate to URP until a separate renderer proof has matching sprite/normal assets and measured benefit.

### Audio and presentation — release blocker

- Unity currently contains no project music or SFX assets; its scene AudioSource has no clip.
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
