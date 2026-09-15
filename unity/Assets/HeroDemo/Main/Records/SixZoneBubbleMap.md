# QP1 Six-Zone Bubble Map

Linked records: [Critical Path](CriticalPath.md) · [Optional Recovery Overlay](OptionalRecoveryOverlay.md) · [Mechanic Resonance Table](MechanicResonanceTable.md)

Legend: green = safe, amber = test, red = danger, blue = recovery/checkpoint, gold = reward, purple = boss threshold.

```
[GREEN] 1 Trailhead  x0–8
  spawn x1.2 · first landing · safe runway ≥2 wide
       |
[AMBER] 2 Broken Steps  x8–17
  required jump gaps ≤2.75 · spikes/hazard · blue recovery shelf
       |
[AMBER/BLUE] 3 Canopy Lift  x17–27
  teal cross-platform preview · checkpoint x≈24.55
       |
[GOLD/AMBER] 4 Spark Shrine  x27–34
  Spark x≈31 · unreachable preview before Spark · safe double-jump teach/test
       |
[AMBER] 5 Gate Return  x34–41
  sword-low + wand-high targets · backflip cache optional · gate at x≈40.25
       |
[PURPLE] 6 Guardian Court  x41–51
  boss threshold · clear floor ≥9 units · recovery is respawn/checkpoint
```

Geometry contract: pre-Spark step height ≤1.8; post-Spark step height ≤3.0; safe landing surfaces ≥2 wide; required gaps ≤2.75; demonstrated optional stretch ≤3.5 with recovery; backflip never required.

Authoring roots in `Main.unity`: `Collision - traversal surfaces`, `Decoration - zone readability`, `Triggers - gameplay volumes`. Existing platform, hazard, checkpoint, Spark and gate objects remain editable under `Main Game Session`.
