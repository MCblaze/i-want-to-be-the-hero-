# Bezi reusable templates

Version 1.0 · 14 September 2026

[Notion edition](https://app.notion.com/p/3dba2b7d2114814b8868f359bfe032f9) · [Operating playbook](BEZI-PLAYBOOK.md)

Copy only the template you need. Replace every `{VARIABLE}`. Names written as `@Project Context` are placeholders: use Bezi's @ picker to select the real Page or asset. A typed name or URL alone does not prove the source is available.

These are proposed working procedures, to accompany `BEZI-PLAYBOOK.md`. They are not installed Rules, Skills or connections.

## A. Project Context Page

```text
Project: {NAME}
Owner / reviewed date / context version: {VALUES}
Unity project absolute path: {PATH}
Repository / branch / starting revision: {VALUES}
Local changes already present and owner: {LIST}
Unity version / Bezi plugin version: {VALUES}
Renderer / input system / target devices: {VALUES}
Active authoring scene / runtime entry scene: {PATHS}

Desired player or product outcome: {ONE PARAGRAPH}
Current implementation: {OBSERVED FACTS}
Approved decisions: {RULES THE TASK MUST PRESERVE}
Proposals still under test: {ASSUMPTIONS}
Working folders and naming example: {VALUES}
Existing asset or code pattern to reuse: {REFERENCE}
Design and technical sources with version dates: {REFERENCES}
Verification methods currently available: {METHODS}
Who integrates, reviews, commits and publishes: {OWNERS}
Current task / next dependency: {VALUES}
```

## B. Compact Workspace Rules draft

Adapt these to the workspace; keep detailed design in Pages.

```text
Use the selected target project's Project Context Page for project-specific facts and constraints.
Inspect the relevant current scene/assets before changing them; identify the task's working paths.
Complete the authorized task within its stated scope and preserve unrelated work.
Reuse the project's established patterns and report any conflicting requirements.
Report changed paths, acceptance results and evidence; label assumptions and untested behavior explicitly.
Keep persistent documentation current after accepted changes, with source and revision dates.
```

For a team, place the shared standards in a Shared Page and have each member reference that Page from their own Workspace Rules. See [Bezi Rules](https://docs.bezi.com/context/rules).

## C. Read-only discovery / connection proof — Ask

```text
Inspect the target Unity project without changing it.
Expected project path: {PATH}. Expected scene or asset: {REFERENCE}.
Return the observed project path, Unity version, active scene, and relevant objects/components.
Explain how {SYSTEM} currently works, with exact asset/script references.
Separate observed facts from inferred behavior. Identify any missing access or stale context.
For each required external connector, read {EXACT DOCUMENT/REPOSITORY OBJECT}
and report its title/path and revision. Mark access unverified if the read cannot be performed.
Recommend the smallest next task that achieves {OUTCOME}; do not implement it yet.
```

## D. Bounded task — Plan or Agent

```text
Task ID / title: {ID / TITLE}
Context: @Project Context, @{RELEVANT ASSETS}, {DATED REFERENCE}
Target Unity path / baseline: {VALUES}
Purpose: {WHY THE RESULT MATTERS}
Current state: {OBSERVED BEHAVIOR}
Deliverable: {ONE REVIEWABLE OUTPUT}
Allowed paths and operations: {EXACT SCOPE, INCLUDING SUPPORT FILES}
Preserve: {IMPORTANT EXISTING BEHAVIOR / OTHER OWNERS' FILES}
Dependencies: {MEASURED INPUTS OR REQUIRED PREVIOUS TASK}
Reference example: {ASSET OR IMAGE; WHAT TO MATCH}

Acceptance:
1. {OBSERVABLE CHECK}
2. {OBSERVABLE CHECK}
3. {OBSERVABLE CHECK}

Return changed paths, evidence for each check, remaining assumptions and anything not run.
Use existing supported Actions and project patterns where suitable.
Complete routine steps within this scope. Surface a blocker before expanding the scope.
```

In Plan Mode append: “Produce an implementation-ready plan for this single deliverable, including affected assets, dependencies and verification. Leave implementation for Build.” For a small Agent task append: “Implement and verify this assignment now.” Select the actual mode in the app before submitting. [Plan Mode](https://docs.bezi.com/fundamentals/plan-mode).

## E. Visual review — Ask

```text
Review @{ROOM/UI/PREFAB} against @{STYLE PAGE} and the attached reference.
Image 1 shows {COMPOSITION}; image 2 shows {CURRENT RESULT}; image 3 shows {DETAIL}.
Inspect the result at {RESOLUTION / CAMERA / SCALE}.
Assess {READABILITY, ALIGNMENT, SILHOUETTE, CONTRAST, OCCLUSION}.
Return the three highest-impact discrepancies with the affected objects and proposed corrections.
Distinguish a visual preference from a functional defect. Make no changes in this review.
```

## F. Completion report / handoff

```text
Task ID and context version:
Target project / scene / baseline:
Result: Ready for review / Needs validation / Blocked
Changed paths, including .meta files:
Pre-existing changes preserved:
Acceptance result for each item: Passed / Failed / Not run
Evidence: exact capture, test result or reproducible steps for each claim
Known limitations / assumptions:
Bezi suggestions: resolved / still pending (list)
Local commit: {HASH OR NOT CREATED}
Remote publication: {VERIFIED REVISION OR NOT PUBLISHED}
Next task, required inputs and new owner:
```

## G. Correct one failed acceptance check

```text
Acceptance item {ID} failed.
Expected: {OBSERVABLE RESULT}.
Actual: {OBSERVED RESULT}; evidence: {PATH/ERROR/CAPTURE}.
Reproduction: {SHORT STEPS} on {PROJECT / REVISION / SCENE}.
The accepted parts are {LIST}; preserve them.
Investigate the cause first, including whether the test setup or connection failed.
Then correct only {ALLOWED SCOPE} and rerun the relevant check.
After two equivalent failed attempts, return the competing explanations and the next
discriminating check instead of repeating the same edit.
```

## H. Request an interactive Proof

```text
Create a Proof to compare {DECISION}, using @{DATA/CONTEXT}.
Inputs and units: {VALUES}; label measured and estimated values separately.
Interactive controls: {SLIDERS / FILTERS / CHOICES}.
Outputs: {CHART / COMPARISON / WARNINGS}.
Show the assumptions and data revision in the Proof.
Keep this as a design experiment. Return selected values and uncertainties for review.
Any later Unity transfer must name the target assets, then verify the actual stored values.
```

Proof transfer is a separate one-time translation. Do not assume ongoing synchronization. [Proofs](https://docs.bezi.com/fundamentals/proofs).

## I. Turn a proven process into a Skill

Use this after running and refining the normal task procedure. Creating a custom Skill in Bezi installs it locally; the text below is only a draft request.

```text
Create a reusable Bezi Skill called {NAME} for this proven workflow: {PURPOSE}.
Trigger it only for {SPECIFIC REQUEST TYPES}; exclude {ADJACENT WORK}.
Inputs: {PROJECT CONTEXT PAGE, TARGET ASSET, EXPECTED CHECKS}.
Prerequisites: {CONNECTED PROJECT / AVAILABLE ACTIONS / INSTALLED DEPENDENCIES}.
Steps:
1. Read the supplied context and confirm the target asset.
2. {PROVEN INSPECTION OR EXECUTION STEP}.
3. Evaluate each supplied acceptance condition against actual evidence.
4. Return the standard completion report, including Not run for unavailable checks.
Stop conditions: missing target, conflicting ownership, required operation outside scope.
Keep project paths, art style and tuning values in inputs or Pages.
Use supported Bezi operations; this skill must not depend on shell commands or direct
filesystem/network access. Identify any required MCP or Custom Action explicitly.
Include one passing fixture and one deliberately failing fixture in the usage instructions.
Return the skill's version and files so another user can reproduce the setup.
```

Suggested first skill: **Room Acceptance Review**, taking a room asset and a checklist, producing a read-only evidence report. Its review process is reusable; each project supplies different geometry and art requirements. Validate the resulting skill before treating it as standard. [Skills](https://docs.bezi.com/fundamentals/skills).

## J. Request a project-specific Custom Action

```text
Design a Custom Action for {REPEATED EDITOR OPERATION}.
Input types / allowed targets: {VALUES}.
Output: a structured {REPORT}, including exact affected objects and failures.
Read-only or modifying: {CHOICE AND SIDE EFFECTS}.
Validate the target and inputs before any mutation; define repeat-run behavior.
Use the project's existing utility {REFERENCE} where suitable.
For a modifying action, describe Undo support and recovery limitations.
Provide the code and a tiny fixture for review before the first execution.
Keep the operation within {PATH/OBJECT SCOPE}.
```

Current Custom Actions have full Editor permissions and no automatic recovery. Use `RequireApproval` for destructive or sensitive operations, and a saved project baseline before testing them. [Custom Actions](https://docs.bezi.com/fundamentals/custom-actions).

## K. Efficiency log

| Task / type | Context + skill version | Model | Prep minutes | Execution minutes | Review minutes | Corrections | Credits observed | Accepted? | Evidence / lesson |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| {ID} | {VERSIONS} | {TYPE} | {N} | {N} | {N} | {N} | {N OR UNAVAILABLE} | {YES/NO} | {REFERENCE} |

Count failed attempts in time and cost totals. Compare similar work, then update the brief or Skill only when the evidence suggests an improvement.
