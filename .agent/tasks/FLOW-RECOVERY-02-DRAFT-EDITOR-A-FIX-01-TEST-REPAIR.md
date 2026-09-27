# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A-FIX-01-TEST-REPAIR`  
**Status:** `APPROVED_FOR_WORKER`  
**Start commit:** `216122b`  
**Goal:** Replace all eight bypass tests with tests that execute the real
`FlowLevelGeneratorWindow` Draft action paths.

## Review finding

Commit `216122b` added plausible Load and endpoint production wiring, but every
new workflow test still bypasses that wiring:

- tests directly construct or assign `FlowLevelDraft`;
- tests directly call `AddColor`, `RemoveColor`, `MoveEndpointCommand`,
  `FlowEditorCommandHistory`, or `FlowDraftMapper`;
- the duplicate-callback test counts controls but performs no action;
- the diagnostic test never inspects `FlowDiagnosticsPanel`.

Passing these tests therefore does not verify the window behavior named by the
tests.

## Execution boundary

Execute only this repair. Use one commit, report, and stop.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

Every other file is forbidden. Do not modify the tracked test `.meta`.

## Required production test seam

Extract the existing anonymous Draft action bodies into the smallest possible
`internal` window methods, and have the UI callbacks delegate to those methods:

- New Draft
- Load Draft asset
- Add Color
- Remove selected Color
- selected board cell / endpoint edit
- Draft Undo
- Draft Redo

This is a test seam, not a new architecture. Do not add services, interfaces,
controllers, or dependencies. Preserve the current behavior except where a
real test exposes a packet requirement failure.

## Required tests

Keep these exact eight names, but rewrite their bodies:

1. `NewDraft_UsesCurrentParametersAndClearsSourceAndHistory`
2. `LoadAsset_DeepCopiesDisplaysAndClearsHistory`
3. `AddColor_WindowActionCreatesOneUndoEntry`
4. `RemoveSelectedColor_WindowActionCreatesOneUndoEntry`
5. `EndpointPlaceMoveRemove_WindowActionsUseHistory`
6. `EndpointFailure_ShowsDiagnosticWithoutHistoryEntry`
7. `UndoRedo_WindowActionsRestoreDraftAndRefreshButtons`
8. `CreateGUITwice_EachDraftEditActionExecutesOnce`

### Mandatory test rules

- Create a real window with `ScriptableObject.CreateInstance` and `CreateGUI`.
- Invoke the production internal action handler, or the actual UI callback
  where callback multiplicity is under test.
- Do not duplicate the handler logic inside a test.
- Do not directly call `FlowLevelDraft.AddColor`, `RemoveColor`,
  `PlaceEndpoint`, `RemoveEndpoint`, `MoveEndpointCommand`,
  `FlowSnapshotCommand`, `FlowEditorCommandHistory.Execute/Undo/Redo`, or
  `FlowDraftMapper.FromAsset` to perform the action being tested.
- Setup may create input assets and pre-existing Draft state. Assertions must
  inspect the state owned by the window after the action.

### Exact behavioral proof

- New: seed a loaded asset and non-empty history first; after the production
  action assert all current parameter values, source cleared, and no Undo/Redo.
- Load: select the real `ObjectField` asset, seed non-empty history, invoke
  production Load, and assert `loadedAsset`, complete copied Draft data,
  source immutability, history cleared, and board/panel refreshed.
- Add/Remove: invoke each production action. One Undo must restore the complete
  before snapshot; after that `CanUndo` must be false. Redo must restore the
  complete after snapshot.
- Endpoint: set panel color/endpoint/tool values and invoke the production board
  action for Place, Move, and Remove. Assert Draft snapshots and history after
  each Undo/Redo.
- Failure: use an invalid color or occupied cell through the production board
  action; assert Draft unchanged, no history entry, and visible Error
  `HelpBox` containing a useful message.
- Undo/Redo: invoke the production window actions, not history directly.
  Assert endpoint state plus Undo/Redo button enabled states after each action.
- Repeated CreateGUI: call `CreateGUI` repeatedly, then trigger a real current
  button or board callback once. Assert exactly one mutation and one history
  entry; merely counting controls is insufficient.

## Forbidden test patterns

The implementation is incomplete if any workflow action is simulated with:

```csharp
draft.AddColor();
draft.RemoveColor(...);
history.Execute(...);
history.Undo();
history.Redo();
MoveEndpointCommand.Place(...);
MoveEndpointCommand.Move(...);
MoveEndpointCommand.Remove(...);
FlowDraftMapper.FromAsset(...);
```

Those APIs may be used only for fixture setup when they are not the action under
test. Add a static `rg` check and explain every remaining match.

Remove misleading comments such as “simulate button click” when no callback is
invoked, and remove unrelated calls such as `OnApplyPreset`.

## Verification

Run:

1. the workflow fixture;
2. existing window tests;
3. full EditMode suite;
4. `git diff --check`;
5. `git status --short`;
6. static search for the forbidden bypass calls.

Acceptance:

- all eight exact tests execute their production window paths and pass;
- the duplicate-callback test performs a real action;
- full suite has zero individual failures or skips;
- only the two whitelisted files are changed;
- `Assets/Temp/` and `Assets/Temp.meta` remain absent;
- `.claude/settings.local.json` remains uncommitted and untouched.

## Required report

Report:

- start/end commit;
- changed files;
- which production handler each test invokes;
- test XML totals and exact names;
- forbidden-pattern search with explanations for any remaining setup-only use;
- Temp status;
- blocker, if any.

## Commit

```text
test: verify real draft window actions
```

If real verification requires a file outside the whitelist, output `BLOCKED`
without changing that file.
