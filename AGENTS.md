# AI Collaboration Rules

This file is the authoritative entry point for AI-assisted work in this repository.
Detailed procedures and reusable templates live under [`.agent/`](.agent/README.md).

## 1. Repository context

- Project type: Unity project.
- Unity version: `2022.3.18f1`.
- Current repository content is minimal: one sample scene and approved Flow Puzzle design/implementation documents.
- Unity Test Framework `1.1.33` is present through package dependencies, but no project test assembly exists yet.
- There is currently no verified CI, lint command, or Player build script.
- Git is initialized on `master` with remote `origin`; do not commit, push, create branches, or create worktrees unless the user authorizes that action.
- `CLAUDE.md` contains existing Claude-specific architecture guidance and must be read when work is performed through Claude/Claude Code.
- `.claude/settings.local.json` is a user-managed local permission file. The
  user may approve new Claude CLI permissions during grouped work. Its local
  modification is not worker scope contamination when it is excluded from
  packet commits. Do not stage, commit, reset, or push it unless the user
  explicitly requests that exact action.
- Approved product documents:
  - `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
  - `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

Read [`.agent/PROJECT_CONTEXT.md`](.agent/PROJECT_CONTEXT.md) before planning or changing code.

## 2. Current commands

Set the Unity executable for this machine:

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
```

Open the project:

```powershell
& $env:UNITY_EXE -projectPath 'D:\Unity\UnityProj\FightMatch'
```

Compile/import check after source files exist:

```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\AICompile.log'
```

EditMode tests after a project test assembly exists:

```powershell
& $env:UNITY_EXE -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\TestResults.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\AITests.log'
```

- Lint/formatter: not currently defined.
- Player build: not currently defined.
- CI: not currently defined.

Do not invent a missing command. See [`.agent/VALIDATION.md`](.agent/VALIDATION.md) for evidence and process-safety requirements.

## 3. Instruction priority

Follow instructions in this order:

1. The user's current explicit request.
2. This `AGENTS.md`.
3. Project-specific approved specs and plans.
4. Detailed rules under `.agent/`.
5. Existing local code conventions.

If two instructions conflict, stop and report the conflict. Do not silently choose.

### Existing CLAUDE.md compatibility

`CLAUDE.md` is preserved as tool-specific guidance. For the approved Flow Puzzle work:

- its generic mention of future “unique solution validation” is superseded by the approved requirement to **not** implement unique-solution detection;
- its generic `Assets/Editor/` location guidance is superseded by the approved assembly-separated path in the Flow Puzzle design;
- its simplicity, verification, rollback, SOLID, Runtime/Editor separation, and pattern-selection rules remain compatible and apply.

## 4. Core engineering principles

### Think before coding

- Inspect relevant files before proposing changes.
- State assumptions and uncertainties.
- If requirements permit materially different implementations, present the choice instead of silently selecting one.
- If required information is missing, stop with `BLOCKED` rather than guessing.

### Simplicity first

- Implement the minimum code needed for the approved goal.
- Do not add speculative flexibility, configuration, dependencies, or abstractions.
- Do not create an interface for a single stable implementation unless the approved architecture identifies a real variation point.

### Surgical changes

- Modify only files directly required by the task.
- Do not refactor, rename, reformat, reorder, or rewrite unrelated code.
- Match existing style.
- Remove only unused code introduced by the current change.

### Goal-driven execution

- Complex work requires an approved plan before implementation.
- Every implementation task requires explicit acceptance criteria and verification steps.
- Completion claims require fresh verification evidence.

## 5. Project-specific constraints

- Do not edit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj`, or generated `.sln` files.
- Do not change `Packages/`, `ProjectSettings/`, package locks, CI, build scripts, dependencies, public APIs, serialized formats, assets, or `.meta` files unless the task packet explicitly allows it.
- Preserve Unity `.meta` files whenever moving or renaming assets.
- Do not run a Unity batch-mode command against this project while it is open in another Unity process.
- For the approved Flow Puzzle work, follow its design and implementation plan. In particular:
  - automatic generation remains solution-first;
  - no unique-solution detection;
  - no full-board requirement;
  - core algorithms remain separate from Editor UI;
  - Editor UI uses UI Toolkit as specified.

See [`.agent/CODING_RULES.md`](.agent/CODING_RULES.md) for detailed rules.

## 6. Codex responsibilities

Codex acts as project analyst, planner, architect, task splitter, and reviewer.

Codex must:

- inspect the repository and approved documents;
- clarify scope and risks;
- create small external-worker task packets;
- define allowed and forbidden files;
- review returned patches independently;
- run or inspect applicable validation;
- issue exactly one review verdict: `ACCEPT`, `NEEDS_FIX`, or `REJECT`;
- create a smaller corrective task packet after `NEEDS_FIX`.

Codex does **not** directly invoke DeepSeek, Claude, Claude Code, or another external model. The user manually transfers task packets and patches between environments.

## 7. External DeepSeek worker boundary

DeepSeek is an external, low-cost implementation worker. It receives either
one approved task packet or one approved ordered task group.

DeepSeek may:

- read only the permitted context;
- perform the single requested local implementation;
- modify or create only explicitly allowed files;
- run only explicitly permitted verification;
- return a concise patch and evidence.

DeepSeek may not:

- redesign architecture or project direction;
- expand requirements;
- perform unrelated cleanup or optimization;
- modify forbidden files;
- add dependencies or change build/configuration/CI unless explicitly authorized;
- handle multiple unrelated tasks in one packet;
- claim completion without verification;
- guess when blocked.

For an approved grouped delivery, the user may send DeepSeek one task-group
manifest containing multiple ordered task packets. In that mode:

- each packet remains an independent scope, verification, review, and rollback unit;
- DeepSeek must execute packets in the declared order;
- DeepSeek may create one local commit per accepted-for-execution packet only
  when the group manifest explicitly permits local commits;
- DeepSeek must never push, merge, rebase, amend, reset history, or combine
  multiple packets into one commit;
- a blocking failure stops all dependent packets in that group.
- user-approved `.claude/settings.local.json` permission changes may remain
  outside commits and do not make an otherwise clean group invalid.

The required external-worker prompt is [`.agent/DEEPSEEK_WORKER_PROMPT.md`](.agent/DEEPSEEK_WORKER_PROMPT.md).

## 8. Task and review requirements

- One task packet must produce one coherent result.
- One task group may contain multiple ordered task packets, but cannot weaken
  any packet's whitelist, acceptance criteria, verification, or output contract.
- New files must be explicitly listed.
- Changes to public APIs, dependencies, configuration, assets, generated files, persistence, or serialization require explicit permission in the packet.
- DeepSeek must list changed files and report each acceptance criterion.
- Ambiguity or required out-of-scope work must produce `BLOCKED`.
- Codex reviews every patch with [`.agent/REVIEW_CHECKLIST.md`](.agent/REVIEW_CHECKLIST.md).
- Validation follows [`.agent/VALIDATION.md`](.agent/VALIDATION.md).
- Planning and task lifecycle follow [`.agent/PLANS.md`](.agent/PLANS.md).

## 9. Prohibited behavior

- No unrelated refactoring.
- No task-scope expansion.
- No speculative architecture “for flexibility.”
- No silent dependency, lockfile, configuration, CI, build, or migration changes.
- No broad formatting or comment rewrites.
- No fabricated build, test, lint, or verification results.
- No accepting an external worker's claims without inspecting the actual patch and evidence.
