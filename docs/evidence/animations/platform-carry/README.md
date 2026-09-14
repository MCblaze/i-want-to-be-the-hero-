# Moving-platform animation follow-up (AN-10)

14 September 2026. Follow-up to animation milestone `263dced`, reported by the user while playing station **3 Platforms** in `MovementAndFeel_Test`.

## Reproduction and cause

Stand on the teal solid moving platform and release movement. Logan remains at the same point on the platform but plays the run cycle. The live capture measured hero and platform horizontal velocities both at **-1.34 units/s**, grounded, with no movement input and clip `run`.

The physics controller already carries Logan correctly. The animation presenter used world horizontal speed, which includes platform motion. It now uses the magnitude of **hero velocity minus support velocity** for both idle/run selection and run playback rate. Jump, landing, attack, damage and evade presentation retain their existing priority.

## Evidence

- [Before: running while carried](before.png)
- [After: standing idle while carried](after.png)
- The final live after capture records hero/platform speed **+1.20 units/s**, grounded, clip `idle`; it is taken during travel, above the old run threshold.
- `before-results.xml`: the new animation regression fails; the existing one-way/carry/ceiling regression passes.
- `before-measurements.txt`: 3,002 of 3,181 idle samples played a non-idle clip, despite zero relative drift to four decimals. Both carry directions were observed.
- `after-results.xml`: **2 passed, 0 failed**, including both test methods below.
- `after-measurements.txt`: **0 non-idle frames out of 3,122 samples**, both carry directions observed, maximum relative drift **0.0000** to four decimals.

## Verification

Both targeted tests passed after the correction. The new test observes more than two complete platform periods, including both travel directions and reversals. It then checks actual walking in both directions, stopping, jumping off, and normal idle/run transitions on static ground. The existing platform test covers one-way passage/landing, physical carry and low-ceiling collision.

Repeat with Unity stopped: **Tools → Hero → Run Platform Regression Tests**. These targeted checks supplement the earlier animation audit; they do not establish final artwork quality or constitute a rerun of the whole game suite.

Screenshots are actual Unity camera renders. Source art and level geometry are unchanged. All files are local to the Documents/GitHub project; no OneDrive.
