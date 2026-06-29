# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-EDITOR-DRAFT-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-06`

**Order in group:** `3`

**Depends on:** `FLOW-UNDO-001`

**Goal:** Integrate Draft and command history into the existing UI Toolkit
window for manual endpoint editing, loading, saving, and Save As.

## Scope whitelist

**Files allowed to read:**

- project rules and all Group 6 documents
- all accepted FlowPuzzle source and tests

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- its generated `.meta`

**Files forbidden to modify:** every other file.

## Required behavior

1. Add a focused `FlowDraftPanel`; do not put every new control into the
   EditorWindow class.
2. Support New Draft, Load Asset, Add Color, Remove Selected Color,
   Place/Move/Remove Endpoint, Undo, Redo, Save, and Save As.
3. Successful Generate One creates a deep-copied Draft so generated layouts
   can immediately be manually edited.
4. Loaded assets are never edited directly. All manual changes affect Draft.
5. `FlowBoardView` deep-copies display state and emits cell input intents only.
   It must not mutate Draft or call services, repositories, or `AssetDatabase`.
6. The window converts intents into commands and executes them through history.
7. Save overwrites through the repository when a source asset exists and uses
   SaveNew when no source asset exists.
8. Save As works for new, generated, and loaded Drafts.
9. The only permitted public API addition is:

```csharp
FlowLevelAsset SaveAs(
    FlowGeneratedLevel level,
    string folder,
    string name);
```

Keep the existing source-asset overload and its behavior.
10. Save and Save As are enabled only when endpoints are complete, a solution
    exists, `isSolutionDirty` is false, and `isValidated` is true.
11. Every manual edit immediately disables saving.
12. `Complete` is visible but disabled and shows:
    `Available after local solver is installed.`
13. Serialize and restore the necessary Draft, source asset, selected color,
    and edit-tool state across `CreateGUI()` rebuild/domain reload. Command
    history itself may reset.
14. Repeated `CreateGUI()` must not duplicate controls, callbacks, or board
    views.
15. Preserve all accepted automatic generation, batch, JSON, validation, and
    persistence behavior.

## Tests

Cover:

- New Draft and Load Asset deep-copy behavior;
- generated result becoming editable Draft;
- endpoint edits flowing through history;
- add/remove color and Undo/Redo controls;
- dirty/invalid/incomplete save blocking;
- Save, Overwrite, and Save As without source mutation;
- the new repository overload's normalization and atomicity;
- disabled Complete message;
- repeated `CreateGUI()` and state restoration;
- all existing automatic workflow regressions.

Use temporary project folders and remove only test-created assets.

## Non-goals

- No fixed-path stroke input, solver, completion, async work, progress,
  Runtime gameplay, QFramework, or serialization-format changes.

## Verification

Run the affected Editor and Persistence tests, then all EditMode tests and
compile.

Static:

```powershell
rg -n "IFlowPuzzleSolver|Task\.Run|Thread|QFramework|void OnGUI\s*\(" `
  Assets/Scripts/FlowPuzzle/Editor
```

Expected: no matches.

## Protected-change permission

- public API: only the exact Repository overload above;
- UXML/USS: only the listed existing files;
- dependencies, packages, asmdefs, asset data formats: not allowed.

## Maximum change scope

- modified production files: 5
- new production files: 1
- test files: 2
- approximate maximum diff: 1400 changed lines

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add draft editing workflow`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
