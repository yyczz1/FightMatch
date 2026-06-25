# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-04-FIX-03`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-04`

**Depends on:** `FLOW-GROUP-04-FIX-01`, `FLOW-GROUP-04-FIX-02`

**Goal:** Close the remaining persistence and batch verification gaps, fix
path normalization edge cases, and leave no Unity test artifacts.

**Current context:**

- Start from the clean repository `HEAD` containing this packet.
- Corrective commits `83d08c9` and `ce2d896` are retained.
- Codex independently parsed `Logs/FLOW-FIX-01.xml`: result `Passed`, total
  `230`, passed `230`, failed `0`.
- The test process left untracked `Assets/Temp.meta` and an empty
  `Assets/Temp/` folder.
- Current JSON round-trip assertions do not compare both endpoint coordinates
  or every solution cell.
- The current overflow test starts at `int.MinValue` and adds positive values;
  it does not overflow.
- The current config immutability test checks only a few fields and includes
  the tautology `Assert.AreSame(req.config, req.config)`.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- project AI rules and all Group 4 documents
- approved Flow Puzzle spec and implementation plan
- all Core, Generation, Persistence, Editor persistence, and relevant EditMode
  test source

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`
- `Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs`

**Files allowed to delete as test-artifact cleanup:**

- untracked `Assets/Temp.meta`
- empty untracked `Assets/Temp/`

Delete those two paths only after verifying:

- `Assets/Temp.meta` is not tracked by Git;
- `Assets/Temp/` contains no files or subfolders;
- neither path existed in the Group 4 start commit.

If any check fails, return `BLOCKED` rather than deleting.

**Files allowed to create:** `NONE`

**Files forbidden to modify:**

- every file not explicitly listed above
- all other production files, tests, asmdefs, and `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`

## Required corrections

### 1. Use normalized asset folders downstream

Change folder validation to return the normalized project-relative folder.

`SaveNew` and `SaveAs` must use that normalized value for:

- `Path.Combine`;
- existing-asset lookup;
- `EnsureFolderExists`.

Add a test using backslash separators for a missing nested folder and prove the
saved AssetDatabase path is normalized under `Assets/Temp/FlowPuzzleTests`.

Do not accept absolute paths, `AssetsOutside`, traversal, empty segments, or
dot segments.

### 2. Normalize to exactly one asset extension

Create a private filename normalization operation.

Rules:

- input remains filename-only;
- trailing `.asset` matching is case-insensitive;
- remove all repeated trailing `.asset` suffixes;
- require a non-empty base name;
- append exactly one lowercase `.asset`;
- preserve the validated base name otherwise.

Tests must cover:

- `Name` -> `Name.asset`;
- `Name.asset` -> `Name.asset`;
- `Name.ASSET` -> `Name.asset`;
- `Name.asset.asset` -> `Name.asset`;
- `.asset` and repeated extension with no base are rejected.

Do not change the public repository API.

### 3. Exact JSON DTO round-trip

In `Export_RoundTrip_DeserializesLevelAndSolution`, compare exact:

- both endpoint `x` and `y` coordinates;
- every deserialized solution cell against the corresponding source cell.

Do not replace this with string search or count-only assertions.

### 4. Complete generated-level input snapshots

Add a private test helper that captures or compares the complete relevant
`FlowGeneratedLevel` state:

- `usedSeed`, `coverageRatio`;
- complete Level DTO including difficulty and ordered pairs;
- complete Solution DTO including all ordered cells;
- every `FlowDifficultyReport` field.

Use it to prove `SaveNew`, `Overwrite`, and `SaveAs` do not mutate input.

Also strengthen the canonical-save test to assert exact:

- recalculated `coverageRatio == 4f / 12f`;
- asset report tier and total score equal a fresh evaluator result;
- asset level difficulty fields match the report;
- saved solution and pair data remain complete after reload.

### 5. Test cleanup leaves no parent artifact

At setup, record whether `Assets/Temp` existed before this fixture.

At teardown:

1. delete only `Assets/Temp/FlowPuzzleTests` through AssetDatabase;
2. if `Assets/Temp` did not exist before setup and is now empty, delete that
   parent through AssetDatabase;
3. never delete a pre-existing or non-empty `Assets/Temp`.

After the final test run:

- `Assets/Temp.meta` must not exist as an untracked file;
- `Assets/Temp/` must not remain if the fixture created it;
- the JSON temp folder must not remain.

### 6. Real negative and overflow seed tests

Replace or extend `DerivedSeeds_NegativeAndOverflow` with:

- a representative negative base seed and/or negative level ID;
- a real overflow case, for example `baseSeed = int.MaxValue` and positive
  `levelId`, where the mathematical result exceeds `int.MaxValue`.

Expected values must use:

```csharp
unchecked(baseSeed + levelId * 9973)
```

Prove at least one result wrapped across the integer boundary.

### 7. Complete request/config immutability

Before batch generation:

- store the original config reference;
- snapshot every `FlowGenerationConfig` field;
- snapshot `startLevelId`, `count`, `baseSeed`, and request config reference.

After generation, compare every field and assert:

```csharp
Assert.AreSame(originalConfig, request.config);
```

Remove `Assert.AreSame(req.config, req.config)`.

### 8. Exact batch deep comparison

Strengthen `SameRequest_DeeplyEqual` to also assert:

- diagnostics are null on both sides or non-null on both sides;
- `solutionData.levelId`;
- `difficultyReport.difficulty`;
- corresponding solution cell counts before `SequenceEqual`.

Keep all existing ordered pair/path comparisons.

## Non-goals

- No changes to JSON exporter, batch production code, public APIs, serialized
  fields, asset structure, generation algorithm, Application, UI, solver,
  async, draft, dependencies, or assemblies.
- No new source files or helper architecture.
- No cleanup outside the explicitly listed untracked test artifacts.

## Maximum change scope

- Modified production files: `1`
- Modified test files: `2`
- New files: `0`
- Explicit untracked artifact deletions: `2`
- Approximate maximum diff: `360` changed lines

## Acceptance criteria

- [ ] Backslash asset folders are normalized and work for missing folders.
- [ ] SaveAs produces exactly one lowercase `.asset` extension.
- [ ] JSON round-trip compares complete endpoints and every solution cell.
- [ ] SaveNew, Overwrite, and SaveAs prove complete input immutability.
- [ ] Canonical asset coverage and difficulty are asserted exactly.
- [ ] Tests leave no `Assets/Temp.meta` or empty test-created parent.
- [ ] Batch seed tests include a real integer overflow and negative case.
- [ ] Every config/request field is proven unchanged.
- [ ] Batch deep equality includes diagnostics symmetry, solution level ID,
      report tier, and cell counts.
- [ ] All EditMode tests pass.
- [ ] Only the three allowed files change and allowed artifacts are removed.

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
Total is greater than or equal to 230.
```

Static:

```powershell
rg -n "Assert\.AreSame\(req\.config, req\.config\)|DeepllyEqual" `
  Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs
```

Expected: no matches.

```powershell
git ls-files --error-unmatch Assets/Temp.meta
```

Expected: command fails because the artifact is not tracked.

```powershell
Test-Path Assets/Temp.meta
Test-Path Assets/Temp
```

Expected after tests: both `False` when the fixture created the parent.

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected before commit: only the three allowed modified files. After commit:
no output.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `fix: close persistence and batch verification gaps`

Create one local commit after verification. Never push, amend, rebase, squash,
merge, reset, or include `.claude/settings.local.json`.

## Expected output format

Use the repository task packet response contract. Include explicit
`Assets/Temp` cleanup evidence.

## What to do if blocked

Return `BLOCKED` with the exact reason and no commit.
