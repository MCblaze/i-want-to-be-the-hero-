# Sunleaf client delivery execution plan

Baseline: 41aece5, 16 September 2026. Status: production in progress, not client-ready.

## Shared contract

Canonical project: C:/Users/marvi/Documents/GitHub/i-want-to-be-the-hero-/unity. No OneDrive. Preserve original art and Unity metadata. Confirm the connected project before editing. Read PLAYTEST-2026-09-16.md before older audits: pause, HUD, six background bands, audio foundation, checkpoint/Seed and terrain repairs already exist. Main is editable. Six low ledges now have 1.41 units clearance for a 1.15-unit character capsule. Keep that clearance or validate an equivalent accessible route.

One writer per file/scene. Codex integrates, tests, builds, commits and updates Notion. Bezi owns only its assigned asset folder until an explicit handoff. Do not run overlapping Unity play tests, rebuild Main, migrate renderer or change movement during art production. Use concise result reports: paths, checks Passed/Failed/Not run, evidence and next dependency. Back up each accepted batch to GitHub; generated originals must be saved immediately locally. Never mark a visual as complete from a successful import alone.

## 1. Reconcile and lock visual direction

Inventory existing assets, animation manifests, in-house literature summaries and latest evidence. Produce current-game captures of Trailhead, Canopy and Shrine. Judge at gameplay scale: consistent side view, readable top surfaces, character separation, matching light direction, no stretched stone or opaque cutout backgrounds. Record concrete defects rather than ordering duplicate assets. Codex owns baseline and route validation; Bezi first owns the proposed art package below.

## 2. Bezi batch A: gate and encounter art proposals

Own Assets/Art/Sunleaf/DeliveryProps only. Create original transparent, side-on mossy-stone gate frame, dormant/active rune inset, sword target and wand target skins, plus a small shrine accent, consistent with current terrain/background palette. Inspect current target sizes and source art before authoring. Save source images and .meta files immediately. Use installed generation tools if they return available models; if unavailable, report the exact blocker once and prepare precise asset dimensions/prompts instead of claiming generated art. Do not spend retries polling unavailable generation. Do not integrate into Main yet. Return actual-size contact sheet, alpha/pivot/PPU details, source provenance and proposed target assignments. Codex reviews and integrates accepted assets into a separate checkpoint.

## 3. Bezi batch B: Logan final animation package

Start only after batch A is reviewed or handed off. Follow ANIMATION-ART-REPAIR-BRIEF.md and ANIMATION-AUDIT.md. Produce a small consistent sword/wand pose proof before expanding. Remove baked sword pixels from wand frames; an overlay does not qualify. Preserve outfit, silhouette, frame scale, feet/pivots and both facings. Then complete wand idle/run/air/fire/recovery, second jump, backflip tuck/recovery, defeat and identified sword/crop repairs. Preserve source originals and combat timing. Return frame-by-frame gameplay-size evidence. Codex owns manifest integration and controller regression until handed over explicitly.

## 4. Route expansion and encounter design

Codex owns layout design and implementation after art ownership is settled. Current approximately 51-unit route has an observed 3:26 completion; this is not a fresh-player timing study. Target 10–15 minutes first success through meaningful content, not waiting or repetition. Budget: Trailhead onboarding 1–2 min; Broken Steps movement/combat 2–3; Canopy moving-platform choices 2–3; Shrine ability discovery 1–2; return/gate weapon and ability combinations 2–3; Guardian/payoff 2–3. These are design estimates to tune with tests, not achieved timings. Map entry/exit, camera bounds, recovery floors, checkpoints, enemy arenas, optional rewards and ability gates before moving objects. Keep enemy approaches clear of low ceilings. Measure reachable jumps using real tuning. Save an editable candidate scene and preserve Main until the route is verified.

## 5. World integration and motion

Apply approved caps/undersides/variants to avoid repetitive stretched platforms; reinforce a distinct landmark per zone. Keep collision tops aligned to bright visible walklines. Add restrained foliage/water motion, depth layers, contact shadows, ambient light and combat/progression feedback. Keep hazards recognizable and foreground away from actionable silhouettes. Test both directions, ledge undersides, moving support idle, 16:9 and wide/narrow views. Reduced effects must suppress optional movement/particles and retain readable feedback. No renderer migration without an independent proven need.

## 6. Audio, interface and encounter finish

Audit existing 13-clip audio foundation before adding anything. Add missing footsteps and victory treatment, tune levels and repetition, expose Music/SFX preferences and keep mute/reduced effects persistent. Refine boss tells, damage feedback, fair recovery, ending and retry. Confirm current pause/focus and HUD fixes survive integration. UI must fit small windows and supported aspect ratios; controller navigation and physical touch checks remain separate gates.

## 7. Qualification and package

Run affected regression checks after each integrated batch and a combined acceptance suite at release candidate. Complete controlled route runs covering sword/wand, deaths, checkpoint/restart, moving platforms, jumping/evading, gate and boss. Save screenshots and record failures honestly. Build Windows with zero errors and attributable warnings resolved. Measure frame time/memory on the actual test machine; document resolution/settings. Verify input, focus loss, audio settings and graceful exit. Package versioned executable, controls, credits/provenance, README and known issues; source/art/evidence backed up to GitHub.

Five fresh-player sessions must supply measured first-success times and comprehension feedback. Physical controller/touch and client visual/listening acceptance require real observations; never invent these results. Send a review candidate only with its remaining limitations disclosed. Declare client-ready only when required art and audio are integrated, critical gameplay defects are resolved, qualification evidence is recorded and outstanding client decisions are accepted.

## Dispatch order and parallel work

Bezi A art proposals runs alongside Codex route audit and documentation. Bezi B animation proof follows A or uses a separate explicitly owned folder/thread. Main editing and play-mode tests are serialized by Codex. Integration follows review of each package; then world/audio polish; then complete qualification. Each handoff includes exact paths and releases ownership. No whole-project searches every turn, repeated Notion copies, or simultaneous scene writers.
