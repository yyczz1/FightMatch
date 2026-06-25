# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-04-FIX-01`

**Status:** `NEEDS_FIX`

**Group ID:** `FLOW-GROUP-04`

**Depends on:** `FLOW-PERSISTENCE-001`, `FLOW-PERSISTENCE-002`

**Goal:** Make JSON export tests prove the serialized contract and make the
Editor asset repository save canonical deep copies without mutating caller
data.

**Current context:**

- Start from the clean repository `HEAD` containing this corrective packet.
- Group 4 worker commits are `d492954`, `d045abc`, and `793f22d`.
- Codex independently parsed `Logs/FLOW-GROUP-04-final.xml`: result `Passed`,
  total `215`, passed `215`, failed `0`.
- `FLOW-PERSISTENCE-002` added no repository tests.
- `FlowLevelAssetRepository.ValidateAndPrepare` currently changes
  `level.levelData`, `level.difficultyReport`, and `level.coverageRatio`,
  violating the explicit no-input-mutation contract.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- project AI rules and all Group 4 documents
- approved Flow Puzzle spec and implementation plan
- all Core, Validation, Difficulty, Generation, Persistence, and Editor
  persistence source
- all existing EditMode tests and test asmdef

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Persistence/FlowLevelJsonExporter.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

**Files allowed to create:** `NONE`

**Files forbidden to modify:**

- every file not explicitly listed above
- all asmdef and `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`

## Required corrections

### 1. JSON exporter input and path contract

`FlowLevelJsonExporter.Export` must:

- treat a null `difficultyReport` as `IncompleteLevel`, in addition to missing
  Level or Solution;
- reject null, empty, or whitespace-only output folders as
  `InvalidOutputPath`;
- convert invalid path syntax exceptions such as `ArgumentException`,
  `NotSupportedException`, and `PathTooLongException` into
  `InvalidOutputPath`;
- continue returning `JsonExportFailed` for other expected serialization,
  directory, and IO failures;
- preserve the current exact filenames and dual-file separation;
- not mutate the supplied generated level.

Do not add an importer, schema version, wrapper JSON object, or public API.

### 2. Real JSON round-trip tests

Replace the current string-only “round trip” assertion with deserialization:

```csharp
JsonUtility.FromJson<FlowLevelData>(levelJson)
JsonUtility.FromJson<FlowSolutionData>(solutionJson)
```

Assert exact:

- both level IDs;
- width and height;
- pair count, color ID, and both endpoints;
- path count, color ID, cell count, and every corresponding cell.

Keep separate assertions proving the level JSON has no `paths` and solution
JSON does.

Add tests for:

- missing `difficultyReport`;
- whitespace-only output folder;
- a syntactically invalid path;
- full input JSON snapshot unchanged before and after export.

JSON tests must use one unique folder created from `Path.GetTempPath()` and a
GUID, outside `Assets`. Teardown deletes only the exact folder created by the
current test.

### 3. Repository canonical-copy behavior

Replace mutating `ValidateAndPrepare` behavior with a private operation that:

1. validates the supplied Level + Solution;
2. evaluates difficulty without modifying the input;
3. calculates coverage without modifying the input;
4. builds a complete deep-copied canonical value;
5. synchronizes difficulty and score only on the canonical copy.

`SaveNew`, `Overwrite`, and `SaveAs` must populate assets from the canonical
copy. They must never change any field or list in the supplied
`FlowGeneratedLevel`.

No new public mapper, repository interface, or source file.

### 4. Folder and filename safety

Normalize directory separators before validation.

An accepted asset folder must be exactly `Assets` or begin with `Assets/`.
Reject:

- `AssetsOutside`;
- absolute paths;
- folders containing `.` or `..` path segments;
- empty path segments after normalization.

`SaveAs` name must be a filename only:

- reject directory separators and traversal;
- reject invalid filename characters;
- normalize to exactly one `.asset` extension;
- reject an empty base name.

Do not allow a destination to escape the requested project-relative folder.

### 5. Repository tests

Extend `FlowLevelPersistenceTests.cs` using exactly:

```text
Assets/Temp/FlowPuzzleTests/
```

for asset tests. Setup/teardown may delete only that exact folder through
AssetDatabase.

Add tests covering:

- constructor null dependencies;
- `SaveNew` creates `Level_<id>.asset` and reload preserves all nested values,
  recalculated report, seed, and recalculated coverage;
- saved pair/path/cell lists do not share references with caller data;
- `SaveNew` leaves the complete input object unchanged;
- `SaveNew` refuses an existing destination;
- invalid recommendation creates no asset;
- `Overwrite` preserves the asset path, updates data, and leaves input
  unchanged;
- failed `Overwrite` leaves the existing asset content unchanged;
- `SaveAs` creates the requested asset, leaves source unchanged, and leaves
  input unchanged;
- missing nested Assets folders are created;
- `AssetsOutside`, `Assets/../Outside`, absolute paths, invalid names, and
  names containing separators are rejected;
- null required arguments throw;
- teardown removes the exact test asset folder.

Code inspection must still show `Undo.RecordObject` only in `Overwrite`.

## Non-goals

- No changes to asset serialized fields, JSON filenames, Core DTOs, batch
  generation, Application, UI, solver, async, draft, or Undo history tests.
- No new dependencies, files, public APIs, or architecture layers.

## Maximum change scope

- Modified production files: `2`
- Modified test files: `1`
- New files: `0`
- Approximate maximum diff: `650` changed lines

## Acceptance criteria

- [ ] JSON tests perform actual DTO round-trip comparisons.
- [ ] Invalid/incomplete JSON inputs return the required structured codes.
- [ ] All repository operations leave caller data unchanged.
- [ ] Repository assets contain recalculated canonical deep copies.
- [ ] Folder and filename traversal are rejected.
- [ ] SaveNew, Overwrite, and SaveAs have direct tests.
- [ ] Failed overwrite is asset-atomic.
- [ ] Tests pass with all previous tests.
- [ ] Only the three allowed files change.

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
Test total is greater than 215.
```

Static:

```powershell
rg -n "Undo.RecordObject|EditorUtility.SetDirty|AssetDatabase.SaveAssets|AssetDatabase.Refresh" `
  Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs
```

```powershell
rg -n "UnityEditor" Assets/Scripts/FlowPuzzle/Persistence
```

Expected: required Editor calls are present only in the Editor repository and
Persistence has no UnityEditor references.

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: only the three allowed files before commit.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `fix: preserve persistence input contracts`

Create one local commit after verification. Never push, amend, rebase, squash,
merge, reset, or include `.claude/settings.local.json`.

## Expected output format

Use the repository task packet response contract.

## What to do if blocked

Return `BLOCKED` with the exact reason. Do not make a partial commit and do not
begin `FLOW-GROUP-04-FIX-02`.

## Review record

- Worker commit: `83d08c9`
- Verification XML: `230/230` passed.
- Codex verdict: `NEEDS_FIX`
- Follow-up task: `FLOW-GROUP-04-FIX-03`
- Production canonical-copy correction is retained.
- Remaining issues:
  - normalized folder value is not used downstream;
  - repeated/case-varied `.asset` suffixes are not normalized;
  - JSON round-trip does not compare complete endpoints or cells;
  - complete input immutability and exact recalculated values are not proven;
  - tests leave untracked `Assets/Temp.meta`.
