# Bezi workflow for the Sunleaf Ruins demo

Version 1.0 · 14 September 2026 · methodology adopted in Demo Plan v0.2

[Notion edition and first assignment](https://app.notion.com/p/3dba2b7d2114815594f3fc1fb6e6b53f) · [Operating playbook](BEZI-PLAYBOOK.md) · [Task brief](BEZI-NEXT-TASK.md)

Reusable method: `BEZI-PLAYBOOK.md`. Copyable prompts: `BEZI-TEMPLATES.md`. First assignment: `BEZI-NEXT-TASK.md`.

## Current state

The game clone is `C:\Users\marvi\Documents\GitHub\i-want-to-be-the-hero-`; its Unity project is the `unity` subfolder. **Do not use OneDrive.** Unity 6000.3.6f1 and Bezi plugin 0.115.12 were verified in this project earlier in this session, with green Editor connections. Gameplay baseline is local commit `a564b5c`, following `70edf51` and `85f068d`. Recheck the current revision and local changes before a Bezi task starts.

The Bezi GitHub endpoint is configured, but a read of this repository through Bezi remains unverified. The current local GitHub account has READ access; recent local commits have not been uploaded. Skills, Proofs, Custom Actions and ACP are documented by Bezi; their availability and operation in this installed app have not been tested here. No Bezi task or Skill installation is performed by these documents.

## Context and ownership

Create a Bezi Project Context Page from the current [demo plan](https://app.notion.com/p/3dba2b7d211481e58e4ffda252533eaf), using the [asset register](https://app.notion.com/p/3dba2b7d2114813daf1bff41107fe80a) for relevant art tasks. Include exact project location, current built-in renderer, runtime-generated Main scene, measured versus proposed movement values, accepted control decisions and review date. Connect the repository's `docs` folder as reference context if needed; documents outside the Unity folder should not be assumed indexed automatically.

Codex currently leads gameplay implementation, integration, regression checks and milestone documentation. Bezi's first assignment is scene layout. This is a division of this project's work, not a limit on Bezi's ability to write gameplay code or build tools. Transfer ownership explicitly before either edits the other's files.

Notion holds the production plan and decisions. Bezi Pages provide usable task context. Git records accepted files. Keep the source date/version on copied Pages and refresh them after accepted decisions; these systems are not assumed to synchronize automatically.

## Execution sequence

| Step | Owner / mode | Output | Exit check |
| --- | --- | --- | --- |
| B0. Target and context | Bezi Ask | Exact project/scene read, current context, required references | Correct local path and real asset references; missing external access labelled |
| B1. Canopy layout | Bezi Plan → Agent | Separate authoring scene and reusable room prefab | Edit Mode persistence, route/readability checks, clean scoped diff and captures |
| C1. Movement contract | Codex | Measured jump, double-jump and dash envelopes; controller interfaces | Relevant input/collision/retry checks pass; measurements documented |
| B2. Layout fit | Bezi Plan → Agent after C1 | Geometry adjusted to measured movement; integration markers | Main route and optional branch fit supplied bounds; no speculative ability changes |
| C2. Gameplay integration | Codex | Controller, checkpoint, moving platform and camera behavior in the room | Actual traversal and retry evidence; scene loads consistently |
| B3. Presentation proof | Assigned implementer after C2 and renderer/art decision | One room with approved depth, lighting, animation and effects | Readable captures plus runtime/performance evidence |
| B4. Reuse | Bezi Skill after the review procedure works | Room Acceptance Review procedure with parameters | Passing and failing fixture produce correct reports |
| C3. Expansion and release | Codex coordinates bounded Bezi assignments | Remaining rooms and demo build | Full demo acceptance and fresh-player checks |

B1 may establish composition before C1 is ready, but gaps and reachability must remain explicitly provisional. B2 depends on measured movement. B3 depends on the renderer and sprite/normal-map pipeline proof; package migration must be a separate assignment. Limit active writing to one owner per scene/file.

## Recommended Bezi experiments

- **Movement Proof:** compare estimated and measured landing envelopes. Use it to discuss gaps, then validate actual controller behavior in Unity.
- **Weapon balance Proof:** compare sword/wand damage per opening, recovery, projectile limits and range. Preserve cooldowns through switching. Selected values require a separate verified Unity transfer.
- **Visual Actions:** author six named scenery bands and camera compositions from approved references; keep playability checks separate from visual polish.
- **Room Acceptance Review Skill:** reuse the same read-only layout and evidence procedure across rooms, with the room asset and checklist as inputs.
- **Custom Action only when justified:** wrap a repeated project-specific validator after the ordinary workflow reveals a real gap. Code review and a baseline precede execution.

These are queued opportunities, not implemented features. Use [Bezi's Proofs documentation](https://docs.bezi.com/fundamentals/proofs), [Skills](https://docs.bezi.com/fundamentals/skills) and [Custom Actions](https://docs.bezi.com/fundamentals/custom-actions) to verify current behavior before use.

## First three-task efficiency pilot

Measure B0 discovery, B1 layout and the later room review separately; do not compare them as equal-sized tasks. Record preparation/execution/review time, model, correction rounds, credits where visible, acceptance and evidence in the template log. Include failed attempts. After three tasks, identify repeated friction, update the context/template, then repeat with comparable room work. No speedup or credit saving is claimed yet.

## Completion and handoff rule

Each assignment returns the exact project and baseline, changed paths including `.meta` files, saved scene/prefab paths, relevant captures, Passed/Failed/Not run acceptance results, assumptions and the next owner. Codex reviews the local changes, performs integration checks and saves a local milestone. GitHub publication is recorded separately once repository write access is available.
