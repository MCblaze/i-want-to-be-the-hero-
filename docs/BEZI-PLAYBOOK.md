# Bezi operating playbook

Version 1.0 · researched 14 September 2026 · reusable across Unity projects

[Notion edition](https://app.notion.com/p/3dba2b7d211481e988f1cf97697a6ef6) · [Copyable templates](BEZI-TEMPLATES.md) · [Sunleaf example](BEZI-DEMO-WORKFLOW.md)

Use Bezi to complete a clearly defined piece of work, inspect the result in Unity, then preserve what worked as a reusable procedure. The aim is more accepted work per hour and fewer correction rounds. This is a recommended method based on current Bezi documentation and project experience, not a measured claim of optimal performance.

Start with the quick start below. Use `BEZI-TEMPLATES.md` for copyable instructions. Keep project names, paths, art direction and controls in a separate project context document; the method itself should travel unchanged between projects.

## Quick start: one successful task

1. Open the intended Unity project and Bezi. Confirm both connection indicators are green, then identify the exact project path and scene. Record the starting revision and existing local changes.
2. Create or refresh a short Project Context Page in Bezi. Include the target outcome, actual technology, authoritative references, working folders and acceptance checks.
3. Start a new Bezi thread with the correct target project. Pin the context Page and the few assets needed for this task. Activate suggested pins explicitly.
4. Choose Ask for investigation, Plan for a dependent sequence, or Agent for a small, already specified change. Give one deliverable, allowed paths and observable completion checks.
5. Review the proposed approach when planning is useful, then execute that bounded assignment. Use Actions to operate on the real scene/assets and scripts when code is needed.
6. Inspect the changed files and the actual Unity result. Require evidence for each acceptance check; identify anything untested. Resolve Bezi's Keep/Undo decisions promptly.
7. Save a reviewed local version-control milestone through your usual Git client or integrator. Record the result and next task. A local commit and an uploaded commit are separate states.

The connection indicators and target-project setup are described in [Bezi Quickstart](https://docs.bezi.com/get-started/quickstart). The review cycle is documented in [Agent Mode](https://docs.bezi.com/fundamentals/agent-mode).

## 1. Give each kind of information a home

| Information | Where it belongs | Example |
| --- | --- | --- |
| Durable project facts and decisions | Project Context Page | Unity version, renderer, movement contract, naming examples |
| A few persistent working instructions | Workspace Rules | Use the target project's context; keep reports evidence-based |
| One execution objective | Task prompt or Plan | Build one room prefab with named entry/exit markers |
| The exact objects relevant now | Active @ pins | Room prefab, controller, layout reference, context Page |
| A repeatable procedure | Installed Skill | Audit a room, collect evidence, return a fixed report |
| Revision history and accepted source | Version control | Reviewed code, scenes, metadata and documentation |
| Project decisions and production status | Team documentation, such as Notion | Accepted scope, task owner, completion evidence |

Pages hold reference material across threads. Workspace Rules apply across that workspace's projects, so avoid placing one project's coordinates, names or tuning values in universal Rules. Reference project-specific Pages instead. Shared Pages require a suitable Team workspace; Rules and Connections remain personal. Each colleague needs their own connections and Rules setup. [Pages](https://docs.bezi.com/context/pages), [Rules](https://docs.bezi.com/context/rules), [Team Workspaces](https://docs.bezi.com/fundamentals/team-workspaces).

Keep actual state and intended behavior distinct: Unity/source show what exists; the current task defines the requested change; the project specification provides constraints. Record a conflict instead of silently converting an old assumption into a new requirement. Add an owner, version and review date to each context document.

A Notion URL is a reference, not proof that Bezi has read it. Verify access through Bezi's connector, or copy the relevant current section into a Bezi Page / connected local document. Include its source URL and revision date. Refresh that copy when a decision changes.

## 2. Prepare context once, focus it for each task

Before starting, record the Unity and Bezi plugin versions, render pipeline, input system, target platform, active scene, project path and baseline revision. Inventory relevant existing assets before ordering new ones. Save open work and record pre-existing changes so later diffs can be attributed correctly.

Use a separate task thread for each deliverable and each target project. Bezi threads do not inherit another thread's conversation. Do not switch a used thread to another project; transfer a short handoff into a new thread instead. Watch the context indicator and restart when the thread accumulates obsolete attempts. Bezi suggests roughly ten prompts as a useful refresh point, not a completion rule. [Threads](https://docs.bezi.com/fundamentals/threads), [Prompting](https://docs.bezi.com/fundamentals/prompting).

Use @ search to select real references. A Unity selection may appear as a suggested pill; click it to make it an active pin. Remove irrelevant pins. Keep reference projects read-only and disable unrelated folder/project Connections for the task. [Pins](https://docs.bezi.com/context/pins), [Connections](https://docs.bezi.com/context/connections).

For visual work, attach a small set of annotated images and state what each demonstrates: composition, scale, palette or a defect. Current docs allow three images per prompt; an animated GIF is interpreted from its first frame. Use separate frame captures for animation feedback. [Image attachments](https://docs.bezi.com/context/image-attachments).

## 3. Pick the right mode and capability

| Need | First choice | Completion evidence |
| --- | --- | --- |
| Understand an unfamiliar system or investigate a fault | Ask | Located objects/files, observed state, a testable explanation |
| Resolve dependencies for a medium-sized task | Plan | One goal, affected paths, ordered steps and checks |
| Make a bounded change with clear requirements | Agent | Saved changes plus actual Unity inspection |
| Explore numerical or visual alternatives | Proof | Interactive comparison with explicit assumptions |
| Repeat a proven procedure | Skill | Consistent inputs, ordered checks and report |
| Invoke a project-specific Editor operation | Custom Action | Reviewed method, validated inputs and structured output |
| Read external specs, issues or revisions | Relevant MCP connector | Exact source returned through that connector |

Ask is for information. Plan prepares a single execution goal; its Build control creates an Agent thread. A tiny material adjustment does not need a large planning exercise. A complete game should be decomposed into independent deliverables before execution. [Ask Mode](https://docs.bezi.com/fundamentals/ask-mode), [Plan Mode](https://docs.bezi.com/fundamentals/plan-mode).

Bezi can work on gameplay scripts as well as scenes, prefabs, materials, importers, UI, animation configuration and performance investigations. Assign work by clear ownership and available verification, rather than treating Bezi as only a scene assistant. Prefer native Actions for supported object operations; use code where behavior requires it. [Agent Mode](https://docs.bezi.com/fundamentals/agent-mode).

Start routine work with the model category labelled Core in the current documentation. Use Basic for simple lookups and Frontier for genuinely difficult dependencies or diagnosis. Choose the lowest-cost option that passes the same acceptance checks; record actual usage. Names and availability may change, so inspect the app's current selector. [Model selection](https://docs.bezi.com/fundamentals/model-selection).

## 4. Write a task Bezi can finish

A useful brief contains: **purpose → present state → one output → exact scope → references → acceptance checks → evidence to return**. Include dimensions, units, naming examples and control rules where they affect the result. Say which assumptions may be chosen locally and which unresolved dependency prevents implementation.

Replace “make this level amazing” with a reviewable request: “Create one room prefab with a readable main route, a recoverable lower route and an optional upper route. Use the measured movement envelope in the pinned Page. Return a saved scene, a 16:9 camera capture and pass/fail for the listed layout checks.”

Bundle related edits to the same asset into one coherent task. Separate independent systems or high-impact changes: movement tuning, renderer migration, final art integration and boss behavior need different checks. Allow Bezi to complete routine steps within the brief without pausing after every object creation. Request a decision only when the needed change exceeds the assignment or a missing choice materially changes the outcome. This scope policy is our recommendation; it is not a mandatory Bezi approval feature.

## 5. Verify with evidence, then hand off

Use the smallest check that can detect the relevant failure:

- **Scene/layout:** reopen the saved scene in Edit Mode; inspect hierarchy, prefab links and intended camera view. A temporary Play Mode change is not a saved authoring result.
- **Gameplay:** perform the specified inputs and inspect resulting state. Include relevant retry, collision or input-timing cases. A screenshot alone cannot establish movement behavior.
- **Art/animation:** compare silhouettes at gameplay size; inspect frame alignment, facing, pivots and transitions. Separate placeholder presentation from final art approval.
- **Performance:** capture before/after on the same scene, device and settings. Record frame time and workload; visual smoothness alone is insufficient.
- **Regression:** rerun checks for systems actually affected. Preserve targeted tests for reproducible bugs; avoid creating tests merely to mirror a trivial edit.

Every result report names the baseline, changed files including Unity metadata, checks performed, evidence locations and remaining gaps. Mark each check Passed, Failed or Not run. An empty Console or a successful import is useful evidence, but does not prove fun, controller comfort or a standalone build.

Resolve pending Bezi suggestions before unrelated work begins. Review the full Git diff too, including unexpected settings or asset changes. Bezi checkpoints are useful during its editing session; keep a recoverable Git baseline for the whole project. Coordinate restoration with other contributors so later work is not overwritten. [Agent review and checkpoints](https://docs.bezi.com/fundamentals/agent-mode).

Use **one writer per file or scene at a time**. For a solo developer, that means handing ownership between the human, Bezi and any coding assistant. Parallel changes require genuinely separate areas, or isolated checkouts with explicitly verified Unity/Bezi targets. A second chat does not isolate a shared Unity project. Record the handoff before another tool edits the same assets.

## 6. Turn successful repetitions into Skills

Choose a narrow procedure after it has worked in real tasks. Good candidates are room validation, prefab consistency checks, animation import inspection and release evidence collection. Define inputs, prerequisites, ordered steps, stop conditions, output format and two representative examples. Keep project-specific values in parameters or a referenced Page.

Current Bezi Skills use the Agent Skills format. Create one through Workspace Settings → Skills → Create with Bezi, or describe the workflow in chat. Installed skills can be called by slash command. They are local to each user, so distribute the skill files and documentation explicitly. Current docs say skill execution supports JavaScript, without direct filesystem, network or shell access; do not assume a shell-dependent skill from another agent will work unchanged. [Skills](https://docs.bezi.com/fundamentals/skills).

Test a new skill on a small fixture, then on a deliberately failing fixture. Record its version and available dependencies. Keep a normal prompt fallback. The companion templates include a skill-authoring brief; this playbook does not install a skill in anyone's Bezi account.

## 7. Use advanced features where they save real work

**Proofs:** build an interactive movement-envelope explorer, weapon comparison sheet, dialogue editor or composition study before making engine changes. Label estimated inputs and measured data separately. A Proof transfers into Unity through an interpreted, one-time operation; it is not a live two-way link. Verify resulting assets and track the Proof revision used. Save/export valuable versions before editing. [Proofs](https://docs.bezi.com/fundamentals/proofs).

**Custom Actions:** expose a repeated project-specific Editor operation when built-in Actions do not cover it. Prefer existing validated project utilities. Specify typed inputs, allowed scope, read/write behavior, repeat-run behavior and returned evidence. Current docs describe public static C# methods in an Editor assembly marked with `BeziAction`; read-only methods can be made available in Ask/Plan. Custom Actions run with full Editor permissions and have no automatic checkpoint/recovery. Review their code and use version control before running; destructive operations need deliberate authorization. Return structured data instead of relying only on Console logs. [Custom Actions](https://docs.bezi.com/fundamentals/custom-actions).

**External connectors:** use the official service integration where available. Confirm a real read of the intended repository, document or issue; a configured endpoint does not establish access. Enable only useful tools. Bezi currently documents a ceiling of 100 active MCP tools; pause unused connectors. Keep credentials out of project documentation and commits. Unity editing itself uses Bezi's Editor integration; external MCP access is a separate connection. [MCP](https://docs.bezi.com/context/mcp).

**IDE integration:** Bezi documents an ACP beta bridge for compatible clients. It acts on the workspace/project open in Bezi, so verify that target explicitly. Some features still require the Bezi app, including checkpoint decisions; IDE game-object pinning is unavailable. Treat support in a particular coding assistant as unverified until tested. ACP is optional, not required for this method. [ACP](https://docs.bezi.com/acp).

## 8. Recover without wasting prompts

| Symptom | Useful next step |
| --- | --- |
| Wrong project, missing objects or lost connection | Verify exact target, both connection indicators and compile/import completion; run a small read-only query before retrying work |
| Task drifts into unrelated changes | Restate the acceptance failure and allowed paths; resolve pending edits and transfer a concise handoff to a fresh thread |
| Same attempt fails twice | Stop repeating it; capture the exact failure, classify connection / task / code / test-fixture cause and change the diagnosis |
| Report says “done,” but evidence is missing | Keep status Needs validation; request only the missing checks |
| Graph, animation or shader editing is uncertain | Inspect the actual asset and test a small example; use supported operations or a reviewed Custom Action |
| Unexpected local changes | Pause overlapping writers; compare with the recorded baseline before deciding which changes to keep |

The two-attempt rule is a proposed workflow safeguard, not a Bezi limit. Save work before restarting applications. Do not treat deleting caches, reinstalling or changing project identity as the first diagnosis. See [Bezi troubleshooting](https://docs.bezi.com/get-started/troubleshooting).

Some official context pages describe graph limitations more broadly than the current Actions documentation. Verify the specific installed operation instead of promising unrestricted graph editing. Bezi's visual context also uses captures, not continuous observation. [Project context](https://docs.bezi.com/context/index-project), [Agent Mode](https://docs.bezi.com/fundamentals/agent-mode).

## 9. Measure efficiency and keep the method current

Pilot the method on three representative tasks: a small scene edit, a behavior change and a repeatable audit. Use the same acceptance standards throughout. Record preparation time, execution time, review time, correction rounds, task outcome and credits shown in the account dashboard. Include failures and abandoned attempts.

Calculate **accepted tasks per total hour**, **credits per accepted task**, and **first-pass acceptance rate**. Compare similar task sizes; a harder task is not evidence that a prompt was worse. A three-task pilot is directional feedback, not statistical proof. Credit costs depend on model and work performed; use observed usage instead of invented estimates. [Plans and usage](https://docs.bezi.com/account/plans).

At the end of each milestone, remove stale Rules, update project context, preserve successful examples, and revise only the procedures that caused repeated friction. Recheck feature availability after an app/plugin update. Keep the generic playbook and project-specific brief versioned separately.

## Portability checklist

- Replace all template variables; use local working paths appropriate to the new project.
- Verify installed versions, account features and connectors; record unavailable features and prompt/manual fallbacks.
- Supply the new project's references and a known-good asset example.
- Distribute the playbook, templates and any tested skill files; each user installs/configures their own copy.
- Run one small task and check the evidence before adopting the method broadly.

This package is documentation. It neither connects accounts nor dispatches jobs. The implementation loop applies to Unity; the briefing and review principles can also inform other tools, but Bezi engine support should not be assumed beyond its documented capabilities.

## Lessons from the first parallel run

See [Parallel execution trial](BEZI-PARALLEL-EXECUTION.md) for the reusable procedure exercised on 14 September 2026. Separate file ownership from editor ownership; provide routine geometry/prefab/dimension/label defaults; keep short specification work to one response; send one consolidated review; and save detailed test logs outside chat. Bezi ACP was researched as an optional direct connection and was not enabled or installed.

Validate saved assets after reopening. Loaded Editor objects can differ from serialized files, and named marker inventories can hide overlapping labels. Require readable captures, check parent/local transforms and persistent references, and include failed action batches plus integration repairs in the acceptance record. The [B1 review](B1-CANOPY-REVIEW.md) records a real example. An agent's completion report does not replace the final saved-state check.
