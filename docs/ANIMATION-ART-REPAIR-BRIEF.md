# Animation art repair: next bounded assignment

Status: prepared, not dispatched to Bezi. Codex handled the verified playback/crop fixes directly. This brief can be given to Bezi or the in-house artist once they own the relevant art files.

## Objective and context

Resolve the open animation findings in ANIMATION-AUDIT.md before more Canopy construction. Use the exact local Unity project and current commit, not OneDrive. Read only the audit, contact sheets and C1-F source contract; do not search the whole project or rebuild the game.

## First output: a consistent weapon set

- Decide a visible sword carry/holster rule and apply it to run4–7, jump8–10, land11, dash16–18 and hurt21. Existing idle0–3, attack12–15, dash19, hurt20 and victory22–23 contain the sword. Avoid arbitrary weapon disappearance.
- Produce wand idle/run/air/fire/recovery frames or a tested separable weapon layer. The current sheet has baked sword pixels; putting a wand on top is not sufficient. Remove the inappropriate sword while preserving Logan's silhouette and outfit.
- Ground and air casts must read as ranged attacks. Seed release is immediate on accepted input; recovery is 0.20 seconds, cooldown 0.45 seconds. Do not retime damage or reset cooldowns through swapping.
- Preview only the affected poses first in both directions, at actual gameplay size. Review the small set before expanding to every pose.

## Following outputs

- Dedicated second-jump pose and a 0.40-second backflip tuck/recovery; preserve facing, foot alignment and physics collider. The dodge window stays 0.08–0.22 seconds. Remove the provisional procedural rotation only when its replacement is integrated.
- Separate Logan18/22 into padded cells. Their boot and sword-tip pixels share source rows1250–1252 at different X positions. Simple crop expansion contaminates both slices. Return clean transparent exports and updated crop/pivot data, not global scale changes.
- Logan defeat is currently absent. Agree its short retry presentation without delaying input outside the existing death/respawn contract.

## Scope, ownership and evidence

Work on copied/proposed source exports first. Original PNGs, canonical `web/assets/animations/manifest.json`, generated browser data and Unity copies have one assigned writer. No gameplay, scene, package, renderer or input changes belong in the art task. Treat 32 PPU as unproven; current Unity PPU comes from sheet cell height / unityHeight.

Return changed paths, before/after gameplay-scale contact sheets, exact clip/frame names, pivot/scale data, transparent margins, and Passed/Failed/Not run evidence. The integrator synchronizes the manifest, verifies both facings, repeats the animation audit and runs movement/quest regressions. A saved Bezi Page or proposed art list is not finished animation.

Use one bounded task for the first reviewed weapon set. Supply routine defaults in the brief and ask only about actual art choices. Require a readable saved-state capture, not only a successful action report. Record corrections and credits; no claimed token saving without measurements.
