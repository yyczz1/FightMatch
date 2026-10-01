# AI Collaboration Rules

This file is the authoritative entry point for AI-assisted work in this repository.
Detailed procedures and reusable templates live under [`.agent/`](.agent/README.md).

**New chats, device migration, and resumed FightMatch work:** read
[`START_HERE.md`](START_HERE.md) before choosing the next task. It identifies the
current delivery, approved decisions, role context, and remaining work without
requiring the original Windows chat history. Reading it does not itself authorize
resuming paused work or creating new chats; use the user's actual request.

## 1. Repository context

- Project type: Unity project.
- Unity version: `2022.3.18f1`.
- The repository contains FlowPuzzle and FightMatch source, assembly definitions, EditMode tests, and design/delivery records. Current acceptance and unfinished WIP are indexed in `START_HERE.md`.
- Unity Test Framework `1.1.33` is present through package dependencies; project test assemblies exist under `Assets/Tests/EditMode/`.
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

## 2. Windows command reference

These are Windows reference commands, not Mac commands or permission to rerun
accepted validation. Current task packets specify the permitted validation tool,
evidence paths, and sequence. Read `START_HERE.md` before adapting them on Mac.

Set the Unity executable for this machine:

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
```

Open the project:

```powershell
& $env:UNITY_EXE -projectPath 'D:\Unity\UnityProj\FightMatch'
```

Compile/import reference:

```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\AICompile.log'
```

EditMode test reference:

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
- FightMatch player/runtime UI uses **uGUI** (`UnityEngine.UI`, Canvas); UI Toolkit is for Editor tooling. This is the user's explicit correction on 2026-09-29.
- Before changing runtime UI, scenes, UI tests, or art integration, read [the uGUI correction plan](docs/system-design/2026-09-17/runtime-ui-ugui-correction.md). Existing runtime UI Toolkit views require migration; their historical acceptance does not authorize extending that choice. Keep gameplay, Application, and save semantics independent of the UI framework.
- Player UI supports English and Simplified Chinese initially. First launch uses Simplified Chinese for a Chinese system language and English otherwise; settings allow a manual choice that is remembered. Player-visible prefab text must default to an obvious diagnostic placeholder, such as `【if you see this, it is a bug.】`, and be replaced at runtime through localization bindings. Do not hide missing bindings behind finished copy in prefabs. The user selected Luban text tables with project-owned localization bindings and YooAsset resource loading; do not introduce Unity Localization/Addressables as an assumed alternative. See the correction plan for the remaining implementation design.
- Excel/CSV tables are the user's primary configuration authoring source, including localization text. Use Luban as authoring-time tooling and feed its output into the existing configuration validation/publication flow. Generated runtime/intermediate JSON may remain, but must not become a second independently edited authority. Exact tool versions and adapters still require a scoped implementation plan.
- The approved engine direction retains Unity 2022.3 and evaluates 2022.3.62f3 as the release-preparation patch candidate; it is not yet an accepted project upgrade. Preserve development on both Mac and Windows, with the same project and Unity/package versions. The user selected the Intel Mac editor. Host CPU choice does not replace platform-specific tooling validation or change the Android target. See packet465 and the version research before planning an upgrade.
- The user approved Android 7 (API 24) as the first-release minimum OS in packet465/Q29. Apply it through the scoped Android release plan; the existing development/build settings are not evidence that this change has been implemented or validated.
- For the approved Flow Puzzle work, follow its design and implementation plan. In particular:
  - automatic generation remains solution-first;
  - no unique-solution detection;
  - no full-board requirement;
  - core algorithms remain separate from Editor UI;
  - Editor UI uses UI Toolkit as specified.

See [`.agent/CODING_RULES.md`](.agent/CODING_RULES.md) for detailed rules.

## 6. Codex responsibilities

**Approved standing team workflow (2026-09-30):** the user confirmed
“以后就这么执行”. Future FightMatch work must follow the
[five-role organization](docs/architecture/2026-09-30-ai-team-organization.md)
and [execution agreement](.agent/TEAM_WORKFLOW.md).

- Keep five distinct lead chats: central coordinator（中央AI）, planning lead
  （主策）, engineering lead（主程）, art lead（主美）, and testing lead（主测试）.
  The central coordinator maintains the project overview and handles dispatch,
  dependencies, progress and receipt; specialist leads own detailed decisions.
- Engineering delegates architecture → system design → implementation.
  Planning, art and testing delegate through their documented specialist roles.
  Keep design and production in separate role chats; code review runs through
  GitHub PR Code Review under the rule below.
- For user-authorized FightMatch tasks, reuse suitable chats or create necessary
  role/execution chats, dispatch scoped work and exchange task receipts within
  this team. This approved coordination does not require repeated per-packet
  permission. Record actual thread/host/turn identities and the user's authority
  in handoffs; it does not authorize unrelated work or external human messaging.
- Every task needs one delivery owner, fixed input versions, exact file scope,
  relevant approved decisions, observable acceptance criteria and a return path.
  Implementers report proposed changes to approved choices to their lead before
  proceeding; changes to user decisions go through the central coordinator.
- The testing lead coordinates cases, existing evidence, dots/cloud and device
  execution, and receipt of independent GitHub PR Code Review. Authors cannot
  approve their own delivery. Preserve existing R verdicts as historical evidence.
- **User update (2026-10-01):** delegate new code reviews to GitHub PR Code
  Review, not local `/review`, R chats or duplicate reviewer agents. Follow
  [the PR review procedure](.agent/TEAM_WORKFLOW.md#7-github-pr-code-review).
  Track the reviewed head and findings separately from test and device acceptance.
  An unavailable reviewer blocks that gate; it does not authorize an automatic
  fallback that spends general Codex usage. dots remains the preferred route
  for supported automated tests; local Unity and device exceptions need the
  existing scoped execution contract.
- Use completion events or available wait tools to receive actual results,
  resolve required fixes and dispatch the next ready packet. While tests run,
  continue independent work with non-conflicting write scopes. Do not end
  coordination merely after dispatch or restart five-minute polling tasks.
- Keep detailed evidence in the responsible role's files and send concise
  receipts upstream. Reuse valid evidence; new runs must address relevant
  changes or unresolved failures, with explicit scope and resource limits.
- **Model policy (user update, 2026-10-01):** central and specialist lead,
  architecture, system-design and implementation chats default to
  `gpt-6-astra` / `xhigh` (极高), including new chats and subsequent dispatches.
  Use `gpt-6-luna` for clearly specified, repetitive low-complexity work;
  escalate ambiguous or substantive design/code decisions to Astra. Set the
  model and effort explicitly when dispatching. This supersedes old C/R
  `max` defaults; GitHub review uses its service configuration.

Existing SD00/C/R assignments are the transition baseline, not a reason to
revert to a single all-purpose chat. Record responsibility transfers before new
owners write shared files. Until that transfer, SD00 remains design/dispatch/
receipt only and C remains the serial local Unity executor. New code review
ownership is transferred to GitHub; existing R chats receive no new code reviews.
This workflow approval does not resume the paused product goal or expand Git,
paid-service, deployment or release permissions. Actual new chats and cloud
test readiness must be verified rather than inferred from this policy.

Codex acts as project analyst, planner, architect, task splitter, and reviewer.

Codex must:

- inspect the repository and approved documents;
- clarify scope and risks;
- create small external-worker task packets;
- define allowed and forbidden files;
- obtain independent GitHub PR review for code changes and inspect its findings;
- run or inspect applicable validation;
- record exactly one integration verdict: `ACCEPT`, `NEEDS_FIX`, or `REJECT`,
  citing the independent review and required validation rather than inventing
  a bot approval or conducting a duplicate local code review;
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
- Code changes follow the GitHub PR review procedure; its review context and
  delivery receipt use [`.agent/REVIEW_CHECKLIST.md`](.agent/REVIEW_CHECKLIST.md).
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

## Code Review Rules

For a GitHub review, inspect the PR diff and its scoped approved design. Do not
dispatch workers, resume development, or run the historical startup commands.

- Flag changes that break shared Demo/Android gameplay or save semantics,
  duplicate rewards, or discard protected ad benefits. A UI migration must
  preserve the existing Application and persistence contracts.
- Player UI must use uGUI, with project-owned English/Simplified Chinese
  bindings and diagnostic prefab placeholders. UI Toolkit is allowed for
  Editor tools; missing runtime bindings must remain visible and testable.
- Flag weakened acceptance assertions, lost lifecycle cleanup, unowned global
  state, or evidence attributed to a different input. A blocked or timed-out
  test is not a pass, and code review cannot establish device/visual acceptance.
