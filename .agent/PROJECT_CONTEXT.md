# Project Context

Last context refresh: 2026-09-29 (CONT-C accepted; 028 independently ACCEPTed in442 on full0064010/4011 plus corrected focused1/1; latest overnight scope extension recorded in §438).

This file separates observed repository facts from approved future design and unresolved information.

Current user boundary: the goal remains active. The latest user instruction relayed through the Codex side conversation (§438) extends work beyond the first Demo: preserve a playable candidate/accepted version with source, APK identity, normal push, version marker, review, acceptance evidence and guide, then continue the existing approved Android plan. Keep useful work moving over the next approximately seven hours. Preserve any physical-device or approval gaps truthfully and continue independent work; do not silently mark those gates passed. SD00 decides ordinary reversible technical choices and necessary version isolation. Current 028 section434 and original runs001–006 remain immutable. Section440 completed fresh compilation and the corrected test1/1 with unchanged product and verified source/DLL identities; independent R accepted the whole028 patch and this combined evidence in section442; do not rerun unchanged CONT-C. Android Demo acceptance is still incomplete.
Do not convert assumptions into facts.

For a new chat or device, start with [`START_HERE.md`](../START_HERE.md). It links
the portable system-design, architecture, and planning context and the current
handoff. The old description of an empty project no longer applies.

## Observed facts

### Project and tooling

- Historical Windows repository path: `D:\Unity\UnityProj\FightMatch`.
- Current Mac repository: `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`.
- Current Mac Unity executable: `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity` (x86_64/Rosetta).
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
- CONT-C-MAC-R1 is accepted after C formal completion and independent R ACCEPT (§430–431). Final Compile013 and unfiltered EditMode015 passed: 3956/3956, no failures or skips. The original 3872 named occurrences remain.
- `4be170fd65051eeac5a523817f534e725526c4c6` is the historical pre-resumption WIP, not the current delivery. 028 is now independently accepted (§442). The remaining first-Demo stage is029; later Android work continues under the newer §438 instruction; a runnable Android APK and physical/emulator gameplay acceptance are still pending.
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
- The migration starting HEAD was `adaffdc4d7e8329fb4e8ee663ed8918c78ad8f0c`. Accepted CONT-C source commit `7464efd10e4d2ed5341bbc1e2d179364dd94489c` was pushed normally to origin/master and independently verified by ls-remote at 2026-09-28T11:15:07Z. A following documentation-only checkpoint does not change that accepted source. See task packet §432; read actual HEAD when resuming.
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

- Mac compilation/EditMode for final CONT-C are established by this round's actual evidence. Player/Android build and iQOO Neo5/BlueStacks normal flows and cold restarts remain unverified; toolchain/emulator preparation is not gameplay evidence.
- `Tools/Invoke-FM025P2Validation.ps1` now has an accepted fixed CONT-C-MAC-R1 stage with serial process, script SHA and same-version checks. The canonical external runtime directory is `Applications`, not the former `application`. 028 has its signed §434 packet, §440 focused supplement and §442 independent acceptance; 029 still needs its own explicit packet. Reuse valid evidence and preserve old failure/input records.
- CI provider and CI commands.
- Repository-wide code style beyond rules defined here.
- Whether a formatter or analyzer will be adopted.
- Current authorized local C thread: `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`; R thread: `01a0e404-e8ee-7310-8388-9260babd53f1`, both gpt-6-astra/max. Actual completed author/review turns and formal messages are recorded in §430–431. Old Windows IDs remain historical provenance.
- Full historical raw test artifacts, local caches, credentials, real player saves, and native Codex chat databases are not included in the selected migration archive.

## Updating this file

Codex may update this file only when:

1. the repository provides fresh evidence;
2. the user makes an explicit project decision; or
3. an approved implementation establishes a new stable command or structure.

Record unverified claims under “Unknown or unresolved,” not under “Observed facts.”
