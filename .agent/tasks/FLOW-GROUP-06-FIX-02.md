# External DeepSeek Corrective Task Packet

## Metadata

**Task ID:** `FLOW-GROUP-06-FIX-02`

**Group:** `FLOW-GROUP-06-REPAIR`

**Order:** `2`

**Depends on:** `FLOW-GROUP-06-FIX-01`

**Goal:** Connect the existing Draft panel, Draft model, commands, board,
persistence actions, and reload state to the UI Toolkit window.

## Allowed files

**Modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

Every other file is forbidden.

## Required corrections

1. Build `FlowDraftPanel` into the visual tree exactly once.
2. Wire New Draft, Load Asset, Add/Remove Color, endpoint
   place/move/remove, Undo/Redo, Save, and Save As.
3. Successful Generate One creates a deep Draft copy that can be edited.
4. BoardView emits cell intents only. Window translates them into commands.
5. BoardView can display incomplete Draft endpoints without mutating Draft.
6. Save uses SaveNew for a source-less valid Draft and Overwrite for a loaded
   source asset. Save As works for all valid Draft origins.
7. Test the existing source-less Repository SaveAs overload completely,
   including normalization, deep ownership, duplicate rejection, and input
   immutability.
8. Dirty, incomplete, unvalidated, or solution-less Drafts cannot save.
9. `Complete` remains visible and disabled with the exact required tooltip.
10. Serialize and restore Draft, selected color, edit tool, and source asset
    across `CreateGUI` rebuild/domain reload. History may reset.
11. Repeated `CreateGUI` must not duplicate callbacks or controls.
12. Preserve every existing automatic-generation action and regression test.
13. Add non-vacuous action tests. Merely constructing `FlowDraftPanel` is not
    evidence of Editor integration.

## Verification

Run all EditMode tests. Test count must increase and failed count must be zero.

Static BoardView check:

```powershell
rg -n "AssetDatabase|FlowLevelGenerationService|FlowLevelAssetRepository" `
  Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs
```

Expected: no matches.

## Commit

`fix: connect draft editor workflow`
