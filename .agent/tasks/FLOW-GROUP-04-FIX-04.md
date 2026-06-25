# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-04-FIX-04`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-04`

**Depends on:** `FLOW-GROUP-04-FIX-03`

**Goal:** Replace the remaining false-positive test helpers and restore
regression coverage removed during the final persistence/batch cleanup.

**Current context:**

- Start from the clean repository `HEAD` containing this packet.
- Production corrections through commit `f07d8aa` are retained.
- Codex independently parsed `Logs/FLOW-FIX-03b.xml`: result `Passed`, total
  `229`, passed `229`, failed `0`.
- `Assets/Temp.meta` and `Assets/Temp/` are now absent.
- Test count dropped from `230` to `229` because 25 old tests were replaced by
  24 tests. Some were consolidated, but several independent contracts were
  removed.
- `SnapshotLevel` serializes an anonymous object with `JsonUtility`. Unity
  serializes fields, while anonymous objects expose properties rather than
  supported public serialized fields; this can reduce the snapshot to an
  empty object and make mutation assertions vacuous.
- `RequestConfigUnchanged_Complete` snapshots all config values but asserts
  only a small subset.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- project AI rules and all Group 4 documents
- Core, Generation, Persistence, Editor persistence, and relevant EditMode
  tests

**Files allowed to modify:**

- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`
- `Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs`

**Files allowed to create:** `NONE`

**Files forbidden to modify:**

- all production code
- every other test, asmdef, and `.meta` file
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

## Required corrections

### 1. Replace the anonymous JsonUtility snapshot

Remove:

```csharp
JsonUtility.ToJson(new { ... })
```

Use a private test-only snapshot type or equivalent explicit helper that
stores:

- `usedSeed`;
- `coverageRatio`;
- `JsonUtility.ToJson(levelData)`;
- `JsonUtility.ToJson(solutionData)`;
- `JsonUtility.ToJson(difficultyReport)`.

Compare every stored member explicitly. Do not serialize the snapshot wrapper
with JsonUtility.

Use this non-vacuous snapshot for SaveNew, Overwrite, SaveAs, and JSON export
input-immutability tests.

For SaveAs, also snapshot the source asset's complete Level, Solution,
Difficulty, seed, and coverage before the call and compare them afterward.

### 2. Restore removed persistence regressions

Add or restore direct tests for:

- assigning complete generated data to an in-memory `FlowLevelAsset` preserves
  Level, Solution, Difficulty, seed, and coverage;
- syntactically invalid JSON output path returns `InvalidOutputPath`;
- `SaveAs` refuses an existing destination;
- both constructor dependencies reject null;
- required null arguments:
  - `SaveNew` level;
  - `Overwrite` asset and level;
  - `SaveAs` source and level;
- invalid SaveNew recommendation creates no asset path or test folder;
- failed Overwrite throws `InvalidOperationException` and the reloaded asset
  remains unchanged.

Do not replace explicit exception assertions with `try/catch { }`.

Keep the exact cleanup behavior from `f07d8aa` and prove after the suite that
no `Assets/Temp` artifact remains.

### 3. Assert every config field

In `RequestConfigUnchanged_Complete`, compare every field stored in the
snapshot:

- all 25 current `FlowGenerationConfig` fields;
- request `startLevelId`, `count`, `baseSeed`;
- original config reference via `Assert.AreSame`.

Do not merely store fields without asserting them.

Prefer a private `AssertConfigEqual` helper to keep the test readable.

### 4. Complete batch deep equality

In `SameRequest_DeeplyEqual`, additionally compare:

- `failedCount`;
- `levelData.difficultyScore`.

Keep existing diagnostics symmetry, solution level ID, report tier/score,
ordered pairs, path counts, cell counts, and cells.

### 5. Preserve final coverage

Do not remove or weaken any currently passing test from commit `f07d8aa`.
The final suite total must be greater than `229`.

## Non-goals

- No production changes, new files, public APIs, dependencies, architecture,
  JSON/schema changes, asset behavior changes, generation changes, UI, solver,
  async, or broad test reformatting.

## Maximum change scope

- Modified files: `2`
- New files: `0`
- Approximate maximum diff: `260` changed lines

## Acceptance criteria

- [ ] Persistence mutation snapshots cannot serialize to an empty anonymous
      wrapper.
- [ ] SaveNew, Overwrite, SaveAs, and Export prove complete input immutability.
- [ ] SaveAs proves complete source-asset immutability.
- [ ] Removed JSON, asset, existing-destination, null-argument, and failed
      overwrite regressions are restored.
- [ ] Every one of the 25 config fields is asserted unchanged.
- [ ] Batch deep equality compares failed count and level difficulty score.
- [ ] Final test total is greater than 229 with zero failures.
- [ ] No `Assets/Temp.meta` or `Assets/Temp/` remains.
- [ ] Only the two allowed test files change.

## Verification steps

Before Unity:

```powershell
Get-Process Unity -ErrorAction SilentlyContinue
```

Expected: no Unity process. If one exists, return `BLOCKED`; do not launch or
kill another process.

Run all EditMode tests once using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Total > 229.
```

Static:

```powershell
rg -n "JsonUtility\.ToJson\(new|Assert\.AreSame\(req\.config, req\.config\)|try \{ repo\.Overwrite" `
  Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs `
  Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs
```

Expected: no matches.

```powershell
Test-Path Assets/Temp.meta
Test-Path Assets/Temp
```

Expected: both `False`.

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected before commit: only the two allowed test files. After commit: no
output.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `test: restore persistence and batch regressions`

Create one local commit after verification. Never push, amend, rebase, squash,
merge, reset, or include `.claude/settings.local.json`.

## Expected output format

Use the repository task packet response contract and include the final test
total plus cleanup evidence.

## What to do if blocked

Return `BLOCKED` with the exact reason and no commit.
