# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A-FIX-01`  
**Status:** `APPROVED_FOR_WORKER`  
**Start commit:** `d9b21c2`  
**Goal:** Replace the vacuous Draft workflow tests and complete only the
New/Load/color/endpoint/Undo/Redo window actions.

## Why this correction is required

Commit `d9b21c2` added six green tests but did not modify production code.
Several tests construct `FlowLevelDraft`, commands, mapper, or repository
directly, so they do not execute the Draft panel/window actions they claim to
verify. The original packet required 12 window-workflow tests and actual
callback or internal-handler coverage.

This packet deliberately excludes Save/Save As and generated-level handoff.
Those remain in `FLOW-RECOVERY-02-DRAFT-EDITOR-A-FIX-02`.

## Execution boundary

Execute this packet only. Use RED/GREEN. Create one local commit, report, and
stop. Do not begin Save, Board stroke, Completion, Solver, or reload-state work.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs.meta`

Every other tracked file is forbidden.

The currently untracked, empty test residue `Assets/Temp/` and
`Assets/Temp.meta` must be removed after verification. Do not delete
`Assets/Temp` if it contains any file not created by this fixture.

## Required RED tests

Rewrite or add these exact tests. Each test must invoke the real window callback
or a production internal action handler; directly reproducing production logic
inside the test is forbidden.

1. `NewDraft_UsesCurrentParametersAndClearsSourceAndHistory`
2. `LoadAsset_DeepCopiesDisplaysAndClearsHistory`
3. `AddColor_WindowActionCreatesOneUndoEntry`
4. `RemoveSelectedColor_WindowActionCreatesOneUndoEntry`
5. `EndpointPlaceMoveRemove_WindowActionsUseHistory`
6. `EndpointFailure_ShowsDiagnosticWithoutHistoryEntry`
7. `UndoRedo_WindowActionsRestoreDraftAndRefreshButtons`
8. `CreateGUITwice_EachDraftEditActionExecutesOnce`

Record the exact RED failures before changing production code. Tests that pass
before implementation do not satisfy RED unless they expose and strengthen an
existing behavior through the actual window path.

## Required implementation

1. Add a `FlowLevelAsset` `ObjectField` to `FlowDraftPanel`; restrict its object
   type to `FlowLevelAsset` and disallow scene objects.
2. New Draft must use current level ID, width, height, color count, and seed
   controls. It must set `loadedAsset` to null, clear command history, refresh
   the board and Draft panel, and refresh button state.
3. Load must read the selected asset, deep-copy it through `FlowDraftMapper`,
   assign `loadedAsset`, clear command history, and refresh board, Draft panel,
   diagnostics, and button state. A missing selection must report a diagnostic
   and must not mutate the existing Draft.
4. Add Color and Remove Selected Color must each create exactly one undoable
   `FlowSnapshotCommand` through the window action. Failed mutations must not
   add history entries.
5. Subscribe exactly once to the existing `FlowBoardView.CellSelected` intent:
   - `PlaceEndpoint` places the selected A/B endpoint at the selected cell;
   - `MoveEndpoint` moves that endpoint;
   - `RemoveEndpoint` removes it only when it currently occupies the selected
     cell;
   - `Select`, `DrawConstraint`, and `Erase` do not mutate endpoints here.
6. Endpoint mutations must execute `MoveEndpointCommand` through
   `FlowEditorCommandHistory`. A failed command must show a structured error
   message and leave history unchanged.
7. Undo and Redo must refresh board data, Draft state, diagnostics as
   appropriate, and enabled button states.
8. Repeated `CreateGUI` must not leave duplicate callbacks. One user action
   must produce one mutation and one history entry.
9. Centralize only the small repeated “refresh current Draft UI” operation if
   needed. Do not introduce a new architecture or service.

## Test-quality requirements

- Operate on a real `FlowLevelGeneratorWindow` created through
  `ScriptableObject.CreateInstance`.
- Invoke actual button callbacks, `CellSelected`, or narrowly exposed internal
  production action handlers.
- Do not instantiate a separate Draft/history/repository to simulate what the
  window should have done.
- Assert complete relevant state: selected asset identity, Draft values,
  history `CanUndo`/`CanRedo`, endpoints, panel button state, and source asset
  immutability.
- The duplicate-callback test must detect both duplicate mutation and duplicate
  history entries.
- Track the generated test `.meta` in the commit.

## Non-goals

- Save, Overwrite, or Save As behavior.
- Generate-One-to-Draft handoff.
- Constraint strokes or constraint rendering.
- Pointer move/up/cancel handling.
- Domain-reload persistence.
- Solver, Completion, retry, diagnostics catalog, or model tools.
- Refactoring unrelated Editor code.

## Verification

Run:

1. `FlowDraftEditorWorkflowTests`
2. existing `FlowLevelGeneratorWindowTests`
3. full EditMode suite
4. `git diff --check`
5. `git status --short`

Acceptance:

- all eight exact tests appear in XML and pass;
- full suite has zero individual failures or skips;
- only whitelist files are committed;
- test `.meta` is tracked;
- `Assets/Temp/` and `Assets/Temp.meta` are absent after tests;
- `.claude/settings.local.json` remains uncommitted and untouched by the worker.

## Required report

Report:

- start and end commits;
- RED failures;
- GREEN XML totals and the eight exact test names;
- changed and untracked files;
- Temp cleanup result;
- per-criterion PASS/FAIL;
- blocker, if any.

## Commit

```text
fix: wire draft editing actions
```

If any required change needs a file outside the whitelist, output `BLOCKED`
without modifying that file.
