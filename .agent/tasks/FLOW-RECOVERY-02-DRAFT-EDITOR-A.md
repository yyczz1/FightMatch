# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A`  
**Status:** `APPROVED_FOR_WORKER`  
**Depends on:** commit `f904ea9`  
**Goal:** Complete and test Draft panel/window business actions only: New,
Load, colors, endpoint commands, Undo/Redo, Save, and Save As.

## Execution boundary

Execute this packet only, commit once, report, and stop. Board pointer strokes,
constraint rendering, and reload serialization are deferred to B and C.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

**Files allowed to create:**

- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- generated `.meta` for that test only

Every other file is forbidden.

## Required RED tests

Add and run these tests before implementation:

1. `NewDraft_UsesCurrentParameters`
2. `LoadAsset_DeepCopiesAndDisplaysDraft`
3. `AddColor_UsesSnapshotHistory`
4. `RemoveSelectedColor_UsesSnapshotHistory`
5. `EndpointPlaceMoveRemove_UseCommandHistory`
6. `UndoRedoButtons_TrackHistoryAndRefreshDraft`
7. `SaveDraft_SourceLessUsesSaveNew`
8. `SaveDraft_LoadedUsesOverwrite`
9. `SaveAs_AllOriginsUpdatesLoadedAsset`
10. `DirtyIncompleteUnvalidatedOrMissingData_CannotSave`
11. `GeneratedLevel_BecomesEditableSourceLessDraft`
12. `CreateGUITwice_DraftActionExecutesOnce`

Record exact failing names and XML totals.

## Required implementation

1. `FlowDraftPanel` includes an `ObjectField` restricted to
   `FlowLevelAsset`, selected color, endpoint A/B, edit tool, and Save As name.
2. New Draft uses current parameter-panel level ID, width, height, color count,
   and seed. It clears source asset and history.
3. Load reads the selected `FlowLevelAsset`, maps via `FlowDraftMapper`, sets
   `loadedAsset`, clears history, and refreshes board/panels.
4. Add Color and Remove Selected Color each execute exactly one
   `FlowSnapshotCommand`. Never clear history as a replacement for Undo.
5. Subscribe once to the existing BoardView `CellSelected` intent:
   - PlaceEndpoint places the selected A/B endpoint;
   - MoveEndpoint moves it;
   - RemoveEndpoint removes it only when the selected endpoint occupies the
     clicked cell;
   - Select/Draw/Erase do no endpoint mutation in this packet.
6. Endpoint changes execute `MoveEndpointCommand` through history and display
   structured failure diagnostics.
7. Undo/Redo refresh board, panel, button state, and diagnostics.
8. Save eligibility requires complete endpoints, non-null solution,
   non-null difficulty, clean solution, and validated state.
9. Save uses SaveNew when source-less and Overwrite when `loadedAsset` exists.
   After SaveNew, retain the returned asset as `loadedAsset`.
10. Save As works for new/generated/loaded Drafts and sets `loadedAsset` to the
    newly created asset.
11. Generate One produces an editable source-less Draft and refreshes Draft
    controls.
12. All action paths update `FlowDraftPanel.UpdateDraftState`.
13. Repeated `CreateGUI` has one callback per action.
14. Do not implement fake Load messages, hardcoded Draft parameters, pointer
    stroke behavior, Completion, Solver, or state persistence here.

## Test quality

- Tests must invoke actual callbacks or internal action handlers.
- Save tests use temporary asset folders and assert exact created/overwritten
  asset data and source immutability.
- Endpoint tests assert complete Draft snapshots before/after Undo/Redo.
- Teardown removes only test-created assets.

## Verification

Run the new workflow fixture and existing window tests, then full EditMode.

Expected:

- all 12 named tests appear in XML;
- total tests greater than 326;
- zero failed/skipped;
- only whitelist files changed;
- no `Assets/Temp` residue;
- `git diff --check` has no source errors.

## Commit

```text
fix: complete draft panel actions
```

Stop after reporting; do not start Board/Stroke work.
