# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-PERSISTENCE-002`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-04`

**Order in group:** `2`

**Depends on:** `FLOW-PERSISTENCE-001`

**Goal:** Add the Editor-only repository that validates, recalculates, creates,
overwrites, and saves complete FlowLevelAsset instances safely.

## Scope whitelist

**Files allowed to read:**

- project AI rules and Group 4 documents
- approved Flow Puzzle spec and implementation plan
- all Core, Validation, Difficulty, Generation, and Persistence source
- all existing EditMode tests and test asmdef

**Files allowed to modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- generated `.meta` files for the new folders and files

**Files forbidden to modify:**

- every existing production file
- all other tests
- existing `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

Do not write `.meta` files manually.

## Required behavior

### Editor assembly

Create `FlowPuzzle.Editor`:

- `includePlatforms` contains only `Editor`;
- references `FlowPuzzle.Core`, `FlowPuzzle.Validation`,
  `FlowPuzzle.Difficulty`, and `FlowPuzzle.Persistence`;
- may reference UnityEditor and UnityEngine;
- does not reference Application or Solving yet because those assemblies do
  not exist.

Add `FlowPuzzle.Editor` to the EditMode test asmdef.

### Repository API

Create:

```csharp
public sealed class FlowLevelAssetRepository
{
    public FlowLevelAssetRepository(
        FlowSolutionValidator validator,
        FlowDifficultyEvaluator difficultyEvaluator);

    public FlowLevelAsset SaveNew(
        FlowGeneratedLevel level,
        string folder);

    public void Overwrite(
        FlowLevelAsset asset,
        FlowGeneratedLevel level);

    public FlowLevelAsset SaveAs(
        FlowLevelAsset source,
        FlowGeneratedLevel level,
        string folder,
        string name);
}
```

Throw `ArgumentNullException` for null constructor dependencies and required
objects. Throw `ArgumentException` for invalid asset folders/names. Throw
`InvalidOperationException` for invalid recommendations or an attempted
unapproved overwrite.

### Validation and canonical save data

Before any asset mutation or creation:

1. Require non-null Level, Solution, and Difficulty data.
2. Validate Level + Solution with `FlowSolutionValidator`.
3. Recalculate difficulty with `FlowDifficultyEvaluator`.
4. Recalculate coverage from distinct recommendation cells divided by board
   capacity.
5. Synchronize copied `FlowLevelData.difficulty` and `difficultyScore`.
6. Deep-copy all DTO data into the asset. Never keep mutable list references
   from the caller.
7. Do not mutate the supplied `FlowGeneratedLevel`.

The saved asset uses `level.usedSeed` as `generationSeed`.

### Asset path and save rules

- Accept only project-relative folders beginning with `Assets`.
- Create missing folders under `Assets` using AssetDatabase-safe operations.
- `SaveNew` uses `Level_<levelId>.asset`.
- `SaveNew` refuses to overwrite an existing same-path asset.
- `SaveAs` requires non-null source and a non-empty explicit name, normalizes
  one `.asset` extension, creates a new asset, and never mutates source.
- `SaveAs` refuses to overwrite an existing same-path asset.
- `Overwrite` modifies the same asset only after validation succeeds.
- Use `Undo.RecordObject` only for `Overwrite`.
- After a successful operation, use `EditorUtility.SetDirty` where applicable,
  `AssetDatabase.SaveAssets()`, and `AssetDatabase.Refresh()`.
- All AssetDatabase and Unity object access remains synchronous on the Editor
  main thread. Do not add Task/thread code.

Private cloning helpers are allowed inside the repository. Do not add a public
mapper or new architectural layer in this packet.

## Tests

Extend `FlowLevelPersistenceTests.cs`. Use only:

```text
Assets/Temp/FlowPuzzleTests/
```

for asset tests, with setup/teardown deleting only that exact folder.

Cover:

- SaveNew creates `Level_<id>.asset`, save/reload preserves complete nested
  data, seed, and coverage;
- saved lists do not share references with input lists;
- stale input difficulty and coverage are recalculated;
- invalid recommendation creates no asset;
- SaveNew refuses an existing path;
- Overwrite preserves the asset path and updates complete data;
- failed Overwrite leaves the original asset unchanged;
- SaveAs creates the requested new asset and leaves source unchanged;
- invalid/outside-Assets folder and invalid name are rejected;
- constructor and required null arguments throw;
- cleanup leaves no `Assets/Temp/FlowPuzzleTests` asset folder.

Do not assert Unity Undo history internals; code inspection verifies the
required `Undo.RecordObject` call.

## Non-goals

- No JSON changes, batch saving, automatic folder outside the explicit call,
  UI, Application service, draft, solver, async, import, or asset migration.
- No repository interface or dependency-injection framework.
- No changes to Persistence or Core serialized fields.

## Maximum change scope

- Existing files modified: `2`
- New files including generated metas: maximum `8`
- Approximate source/test diff: `950` lines

## Acceptance criteria

- [ ] SaveNew, Overwrite, and SaveAs follow the exact asset rules.
- [ ] Invalid recommendations are never persisted.
- [ ] Difficulty and coverage are recalculated before save.
- [ ] Saved DTOs do not share mutable lists with caller data.
- [ ] Editor APIs remain in the Editor assembly.
- [ ] Tests pass and clean their exact test folder.
- [ ] Only allowed files change.

## Verification steps

Tests first, then implementation. Run all EditMode tests once.

Static:

```powershell
rg -n "Undo.RecordObject|EditorUtility.SetDirty|AssetDatabase.SaveAssets|AssetDatabase.Refresh" `
  Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs
```

Expected: all required APIs are present; `Undo.RecordObject` occurs only in
the overwrite path.

```powershell
rg -n "UnityEditor" Assets/Scripts/FlowPuzzle/Persistence
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow level asset repository`

Never include `.claude/settings.local.json` or push.

## Expected output format

Use the repository task packet response contract and list any test-created
asset paths that were cleaned.

## What to do if blocked

Return `BLOCKED`, make no commit, and do not begin `FLOW-BATCH-001`.
