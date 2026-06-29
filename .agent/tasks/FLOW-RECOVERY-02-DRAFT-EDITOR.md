# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR`  
**Status:** `APPROVED_FOR_WORKER`  
**Depends on:** accepted Draft Core commit `237d128`  
**Goal:** Replace the placeholder Draft panel wiring with a complete tested
UI Toolkit manual-editing workflow.

## Execution boundary

Execute this packet only. Create one commit, return evidence, and stop before
Solver or Completion work.

Begin from the exact start commit supplied after this document is committed.
Only `.claude/settings.local.json` may be dirty before work.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftWindowState.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- generated `.meta` files for those two files only

Every other file is forbidden.

## Required RED tests

Add tests first and record observed failures:

1. `NewDraft_UsesCurrentParameters`
2. `LoadAsset_DeepCopiesAndDisplaysDraft`
3. `AddRemoveColor_UseSnapshotHistory`
4. `EndpointCellIntent_UsesCommandHistory`
5. `IncompleteEndpoint_DoesNotRenderPhantomMarker`
6. `DrawStroke_CommitsOneCommand`
7. `EraseStroke_CommitsOneCommand`
8. `CancelledStroke_CommitsNoCommand`
9. `SaveDraft_NewAndLoadedUseCorrectRepositoryAction`
10. `SaveAs_WorksForEveryDraftOrigin`
11. `DirtyIncompleteOrUnvalidatedDraft_CannotSave`
12. `WindowState_RestoresDraftAssetColorAndTool`
13. `CreateGUITwice_DraftCallbacksExecuteOnce`

Tests may invoke internal handlers through reflection when unattached pointer
dispatch is unavailable. Do not add public APIs solely for tests.

## Required implementation

### Draft panel

1. Provide controls for:
   - source `FlowLevelAsset` selection;
   - selected color ID;
   - selected endpoint A/B;
   - edit tool;
   - Save As name;
   - New, Load, Add/Remove Color, Undo/Redo, Save, Save As, Complete.
2. Build controls once and expose intents/events rather than business logic.
3. Complete remains visible and disabled with:
   `Available after local solver is installed.`

### Window coordination

4. New Draft uses current Level ID, width, height, color count, and seed UI
   values; no hardcoded `5x5`, two colors, level `1`, or seed `42`.
5. Load maps the selected asset through `FlowDraftMapper`, sets `loadedAsset`,
   clears history, and refreshes board/panels.
6. Add/Remove selected color execute one `FlowSnapshotCommand`; never clear
   history as a substitute for Undo.
7. Board cell endpoint intents execute `MoveEndpointCommand` for the selected
   color/endpoint/tool.
8. Every successful edit refreshes board, diagnostics, Save state, and
   Undo/Redo state.
9. Save uses SaveNew for source-less Draft and Overwrite for loaded Draft.
   Save As works for new, generated, and loaded Drafts and updates loaded
   source to the new asset.
10. Saving requires complete endpoints, non-null solution and difficulty,
    `isSolutionDirty=false`, and `isValidated=true`.
11. Existing Generate One produces an editable source-less Draft.
12. Existing automatic generation, batch, validation, JSON, and clear actions
    remain compatible.

### Board and pointer strokes

13. Board owns a deep display copy of partial endpoints and constraints.
    Missing endpoints are not represented by `(0,0)` or any sentinel marker.
14. Render fixed constraints distinctly from recommendation paths using one
    `Painter2D` board element.
15. Emit neutral cell down/move/up/cancel intents only; do not call Draft,
    history, services, repository, or `AssetDatabase`.
16. Down captures the pointer and starts a stroke. Move accumulates distinct
    orthogonally adjacent cells. Up releases and commits one
    `DrawConstraintStrokeCommand`. Cancel releases and commits none.
17. Draw and Erase both use one history item per gesture.

### Reload state

18. Preserve current Draft, loaded asset, selected color, endpoint selection,
    and edit tool across `CreateGUI`/domain reload.
19. Do not rely on Unity serializing nullable `FlowPos?` fields directly.
    Use `FlowDraftWindowState` with explicit endpoint-presence flags and
    complete deep conversion to/from Draft, stored through serialized window
    state.
20. History may reset after reload.
21. Repeated `CreateGUI`, enable, and disable do not duplicate callbacks.

## Verification

Run the new Draft Editor fixture, affected existing Editor fixtures, then the
complete EditMode suite.

Static:

```powershell
rg -n "AssetDatabase|FlowLevelGenerationService|FlowLevelAssetRepository" `
  Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs
```

Expected:

- all required tests appear in XML and pass;
- total tests greater than 326;
- zero failed/skipped;
- only whitelist files changed;
- no temp assets;
- `git diff --check` has no source errors.

## Commit

```text
fix: complete manual draft editor workflow
```

Never continue to Solver afterward.
