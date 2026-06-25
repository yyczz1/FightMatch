# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-PERSISTENCE-001`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-04`

**Order in group:** `1`

**Depends on:** `FLOW-GROUP-03`

**Goal:** Add the runtime-safe single ScriptableObject level asset and optional
dual-JSON export contract without any UnityEditor dependency.

## Scope whitelist

**Files allowed to read:**

- project AI rules and Group 4 documents
- approved Flow Puzzle spec and implementation plan
- all Core, Validation, Difficulty, and Generation source
- all existing EditMode tests and test asmdef

**Files allowed to modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Persistence/FlowPuzzle.Persistence.asmdef`
- `Assets/Scripts/FlowPuzzle/Persistence/FlowLevelAsset.cs`
- `Assets/Scripts/FlowPuzzle/Persistence/FlowJsonExportResult.cs`
- `Assets/Scripts/FlowPuzzle/Persistence/FlowLevelJsonExporter.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`
- generated `.meta` files for the new folders and files

**Files forbidden to modify:**

- every existing production file
- every existing test file
- existing `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

Do not write `.meta` files manually.

## Required behavior

### Assembly

Create `FlowPuzzle.Persistence`:

- references `FlowPuzzle.Core` and `FlowPuzzle.Difficulty`;
- may reference `UnityEngine`;
- must not reference `UnityEditor`;
- is runtime-safe and not Editor-platform restricted.

Add `FlowPuzzle.Persistence` to the EditMode test asmdef.

### FlowLevelAsset

Create:

```csharp
public sealed class FlowLevelAsset : ScriptableObject
{
    public FlowLevelData levelData;
    public FlowSolutionData solutionData;
    public FlowDifficultyReport difficultyReport;
    public int generationSeed;
    public float coverageRatio;
}
```

Initialize reference fields per asset instance. One asset physically contains
the level, recommendation, difficulty report, seed, and coverage. Do not split
the recommendation into another asset.

`CreateAssetMenu` is permitted but not required.

### JSON result

Create:

```csharp
[Serializable]
public sealed class FlowJsonExportResult
{
    public bool success;
    public string levelFilePath;
    public string solutionFilePath;
    public FlowFailureDiagnostic diagnostic;
}
```

Initialize strings to empty values. Provide small `Success(...)` and
`Failure(...)` factories. Failure must include a non-null diagnostic with a
stable error code and human-readable message.

### JSON exporter

Create:

```csharp
public sealed class FlowLevelJsonExporter
{
    public FlowJsonExportResult Export(
        FlowGeneratedLevel level,
        string outputFolder);
}
```

Rules:

1. Return structured failure, not an exception, for null/incomplete level or
   null/empty/invalid output path.
2. Create the output directory when valid and missing.
3. Use `JsonUtility.ToJson(..., true)`.
4. Write exactly:
   - `level_<levelId>.json` from `FlowLevelData`;
   - `solution_<levelId>.json` from `FlowSolutionData`.
5. Level JSON contains pairs and no full `paths`.
6. Solution JSON contains paths.
7. On success, return the final full paths of both files.
8. Convert expected path, directory, serialization, and IO failures into
   structured `JsonExportFailed` or `InvalidOutputPath` diagnostics.
9. Do not call `AssetDatabase`, `EditorUtility`, or any UnityEditor API.
10. Do not mutate the supplied level.

## Tests

In `FlowLevelPersistenceTests.cs`, cover:

- each `FlowLevelAsset` instance owns initialized DTO instances;
- assigning complete generated data preserves level, solution, report, seed,
  and coverage in memory;
- successful export creates both exact filenames;
- level JSON contains `pairs` and does not contain `"paths"`;
- solution JSON contains `"paths"`;
- JSON round-trips the level ID, endpoints, color IDs, and solution cells;
- missing valid output directory is created;
- null level, incomplete level, and empty/invalid output path return structured
  failure;
- exporter does not mutate its input;
- no UnityEditor reference exists in Persistence.

Use a unique temporary folder outside `Assets/` for JSON tests and delete only
that test-created folder in teardown.

## Non-goals

- No AssetDatabase repository, Editor asmdef, asset save/reload, overwrite,
  Save As, Undo, batch generation, Application service, UI, solver, or draft.
- No custom JSON library, schema version, compression, encryption, or import.
- No changes to Core DTOs or existing APIs.

## Maximum change scope

- Existing files modified: `1`
- New files including generated metas: maximum `13`
- Approximate source/test diff: `800` lines

## Acceptance criteria

- [ ] One runtime-safe asset contains Level, Solution, Difficulty, seed, and
      coverage.
- [ ] Dual JSON files have exact names and separated content.
- [ ] JSON failures are structured.
- [ ] Persistence has no UnityEditor dependency.
- [ ] Tests pass with all previous tests.
- [ ] Only allowed files change.

## Verification steps

Tests first, then implementation. Run all EditMode tests once.

Static:

```powershell
rg -n "UnityEditor|AssetDatabase|EditorUtility" `
  Assets/Scripts/FlowPuzzle/Persistence
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow level asset and json export`

Never include `.claude/settings.local.json` or push.

## Expected output format

Use the repository task packet response contract with test totals and static
check evidence.

## What to do if blocked

Return `BLOCKED`, make no commit, and do not begin `FLOW-PERSISTENCE-002`.

## Review record

- Worker commit: `d492954`
- Verification: Unity EditMode suite later reached `215/215` passed.
- Codex verdict: `NEEDS_FIX`
- Corrective task: `FLOW-GROUP-04-FIX-01`
- Remaining gaps:
  - JSON round-trip test does not deserialize or compare endpoints/cells;
  - incomplete level and invalid/whitespace path cases are incomplete;
  - test output folder is fixed rather than unique.

### Final resolution

- Corrective commits: `83d08c9`, `f07d8aa`, `ef054f8`
- Final Codex verdict: `ACCEPT`
- Final integration verification: Unity EditMode `238/238` passed.
