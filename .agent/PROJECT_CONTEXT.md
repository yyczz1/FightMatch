# Project Context

Last context refresh: 2026-09-28 (repository and delivery records; not a new Unity or Mac validation).

This file separates observed repository facts from approved future design and unresolved information.
Do not convert assumptions into facts.

For a new chat or device, start with [`START_HERE.md`](../START_HERE.md). It links
the portable system-design, architecture, and planning context and the current
handoff. The old description of an empty project no longer applies.

## Observed facts

### Project and tooling

- Repository path used during verification: `D:\Unity\UnityProj\FightMatch`.
- Project type: Unity project.
- Unity Editor version: `2022.3.18f1`.
- Product name in `ProjectSettings/ProjectSettings.asset`: `FightMatch`.
- Historical Windows Unity executable (not a Mac path):
  `D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe`.
- Unity Test Framework `1.1.33` is present through `Packages/packages-lock.json`.
- Code coverage package `1.2.5` is present through package dependencies.

### Current repository contents

- FlowPuzzle and FightMatch source and assembly definitions exist under `Assets/Scripts/`.
- Project test assemblies exist under `Assets/Tests/EditMode/`, including FlowPuzzle, FightMatch.Core.Tests, and FightMatch.QFramework.Tests.
- QFramework is present under `Assets/ThirdParty/QFramework/`.
- Design, task, delivery, review, and selected migration evidence are versioned under `docs/`.
- CONT-C-RCV1 is unfinished WIP: the latest product snapshot is commit `4be170fd65051eeac5a523817f534e725526c4c6`, with 5 source changes not compiled or tested. Follow `START_HERE.md` for exact receipts and the CONT-C → 028 → 029 continuation.
- The accepted CONT-B-CODE-C1 Windows evidence reports 3872/3872 tests passing, with no failures or skips. It does not validate the later WIP or Mac.
- There is no verified CI configuration.
- There is no verified lint or formatting command.
- There is no verified Player build script.
- `START_HERE.md` is the root continuation entry; `docs/migration/2026-09-28/README.md` documents Git retrieval and the evidence archive.

### Version control

- Git is initialized and currently valid.
- Current branch: `master`.
- Current remote: `origin` at `https://github.com/yyczz1/FightMatch.git`.
- At the time of verification, `master` tracked `origin/master`.
- Product work was frozen for device migration and pushed through `4be170fd65051eeac5a523817f534e725526c4c6`; later context-only commits do not mean product work resumed.
- Preserve `.claude/settings.local.json` as user-managed local permissions. Its current local modification is not part of the migration commits.
- `.gitattributes` disables text conversion to preserve hash-bound source/evidence bytes across platforms; do not normalize line endings as incidental cleanup.
- Do not commit, push, branch, merge, or create a worktree unless the user authorizes the action.

### Existing rules and workflow files

- `CLAUDE.md` is tracked and contains Claude-specific architecture guidance.
- Its simplicity, verification, rollback, SOLID, Runtime/Editor separation, and design-pattern rules are compatible with this workflow.
- Its generic mention of future unique-solution validation conflicts with the approved Flow Puzzle specification; the approved “no unique-solution detection” rule wins.
- Its generic `Assets/Editor/` location guidance is overridden where an approved project plan specifies an assembly-separated Editor path.
- `.gitignore` excludes Unity caches, local settings, raw TestArtifacts, ExternalWork, and migration output. The selected continuation evidence archive is explicitly versioned under `docs/migration/2026-09-28/`.
- `.spec-workflow/` contains generic ignored templates. They are not evidence of current project architecture or product requirements.
- Repository workflow documents are in the tracked `.agent/` directory.
- Current SD00/C/R responsibilities and the latest user-approved same-product Demo scope are recorded in the continuation entry and task packets. Historical manual external-worker rules do not require recreating all old chats.

### Existing approved product documents

- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

These remain foundational FlowPuzzle specifications. Current implementation and
acceptance must be read from later delivery records, not inferred from their age.
For FightMatch product rules, architecture, and current tasks, use the three role
context links in `START_HERE.md` and their original sources.

## Approved technical direction

The Flow Puzzle documents currently approve:

- deterministic solution-first generation;
- pure C# domain/algorithm assemblies;
- Unity persistence and Editor layers separated from core algorithms;
- UI Toolkit Editor tooling;
- ScriptableObject primary storage with optional JSON export;
- EditMode tests;
- an exact local solver for manual layout completion;
- no unique-solution requirement.

FightMatch's Demo is an intermediate version of the same Android product, with
less published content and the same business rules, normal flow, and save
semantics. Platform adapters may differ. Do not create a second Demo business
implementation or reset player data merely to transition to a later version.

External workers must read only the portions relevant to their assigned task.

## Unknown or unresolved

- Mac import/compilation and Player/Android build/device acceptance are not established by the Windows evidence or Git migration. Android is the product direction; the first Demo's exact acceptance is in the current packets.
- `Tools/Invoke-FM025P2Validation.ps1` has fixed Windows paths. Mac adaptation needs a precise continuation supplement after inspecting the actual environment, without rewriting frozen packets or replaying every historical test stage.
- CI provider and CI commands.
- Repository-wide code style beyond rules defined here.
- Whether a formatter or analyzer will be adopted.
- New Mac host/thread/turn mappings are not known until the user resumes there. Old IDs remain provenance, not proof that those chats are callable on the new device.
- Full historical raw test artifacts, local caches, credentials, real player saves, and native Codex chat databases are not included in the selected migration archive.

## Updating this file

Codex may update this file only when:

1. the repository provides fresh evidence;
2. the user makes an explicit project decision; or
3. an approved implementation establishes a new stable command or structure.

Record unverified claims under “Unknown or unresolved,” not under “Observed facts.”
