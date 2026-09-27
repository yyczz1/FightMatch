# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A-FIX-01-TEST-REPAIR-02`  
**Status:** `APPROVED_FOR_WORKER`  
**Start commit:** `74dc600`  
**Comparison baseline:** `216122b`  
**Goal:** Repair the remaining workflow-test gaps and restore the window file
to a surgical net diff.

## Review findings

Commit `74dc600` is not accepted:

1. `CreateGUITwice_EachDraftEditActionExecutesOnce` calls `DoAddColor`
   directly. It never invokes `addColorBtn`, so it cannot detect duplicated UI
   subscriptions.
2. Its “one history entry” assertion converts `CanUndo` to `1` or `0`; it
   cannot distinguish one entry from multiple entries.
3. `EndpointPlaceMoveRemove_WindowActionsUseHistory` tests Place and Remove but
   never selects or verifies `MoveEndpoint`.
4. `UndoRedo_WindowActionsRestoreDraftAndRefreshButtons` does not assert either
   button state.
5. Load does not verify board or Draft-panel refresh.
6. The production file was broadly compressed and reordered: relative to
   `216122b`, it has 81 additions and 214 deletions. This violates surgical
   change rules and changes unrelated Save/generation formatting and behavior.
7. `Assets/Temp/` and `Assets/Temp.meta` exist after the reported clean run.

## Execution boundary

Create one new corrective commit on top of `74dc600`. Do not amend, reset,
rebase, revert the whole commit, or start another packet. Stop after reporting.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

Every other tracked file is forbidden.

The untracked empty `Assets/Temp/` and `Assets/Temp.meta` may be removed after
confirming they contain no unrelated files.

## Production-file repair

Use `216122b` as the formatting and behavior baseline.

The final net diff from `216122b` may contain only:

- the seven small `internal` Draft action handlers;
- replacing the corresponding anonymous callback bodies in
  `WireDraftPanel` with method-group delegation;
- the minimum behavior correction demonstrated by a real failing test.

Restore every unrelated line to its `216122b` form, including:

- field order and existing comments;
- `CreateGUI` formatting and callback registration;
- configuration, generation, batch, save, export, validation, clear, and
  `SetCurrentLevel` method formatting and behavior;
- Save/Save As behavior, which belongs to the next packet.

Do not preserve the new whole-file line compression, section comments, renamed
locals, or mojibake comments. Do not add an interface, controller, service, or
dependency.

## Required test corrections

Keep all eight exact test names.

### Add and Remove

- Invoke `DoAddColor` / `DoRemoveColor`.
- After one Undo, assert the complete before snapshot and `CanUndo == false`.
- After one Redo, assert the complete after snapshot and `CanRedo == false`.
- This proves exactly one history entry for a single handler invocation.

### Endpoint Place, Move, Remove

Through `DoEndpointEdit`:

1. select `PlaceEndpoint`, place the endpoint, and verify Undo/Redo;
2. select `MoveEndpoint`, move it to a different cell, then verify Undo restores
   the old cell and Redo restores the new cell;
3. select `RemoveEndpoint`, remove it, then verify Undo restores it.

Do not call `MoveEndpointCommand` directly.

### Undo/Redo UI refresh

Invoke `DoUndo` and `DoRedo`, then assert:

- complete endpoint state;
- `undoBtn` enabled/disabled state;
- `redoBtn` enabled/disabled state;
- history `CanUndo` / `CanRedo`.

### Load refresh

After `DoLoadDraft`, assert:

- complete Draft data and source immutability;
- `loadedAsset` identity and cleared history;
- Draft-panel Undo/Redo state;
- the board's displayed level dimensions and endpoint data, read from the
  `FlowBoardView` snapshot if necessary.

### Actual duplicate-callback proof

`CreateGUITwice_EachDraftEditActionExecutesOnce` must:

1. call `CreateGUI` three times;
2. set the current window Draft;
3. invoke the **current `addColorBtn` click callback**, not `DoAddColor`;
4. assert exactly one color was added;
5. invoke the current Undo button callback once;
6. assert the original complete Draft is restored and `CanUndo == false`.

Use UI Toolkit event dispatch or a narrowly scoped reflection helper that
invokes the button's registered `clicked` delegate. Calling the action handler
directly is forbidden in this one test.

## Cleanup

Before verification:

- inspect `Assets/Temp`;
- delete only the empty/test-created `Assets/Temp/` and `Assets/Temp.meta`;
- update fixture teardown so its known test folder and an otherwise empty
  test-created parent do not leave residue;
- never delete unrelated assets under `Assets/Temp`.

## Static checks

Run and report:

```powershell
git diff --numstat 216122b..HEAD -- Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs
git diff 216122b..HEAD -- Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs
git diff --check
git status --short
```

The net production diff must be localized to the seven handlers and
`WireDraftPanel`. Any unrelated hunk fails the packet.

Also show that the duplicate-callback test contains a real reference to
`addColorBtn` and does not call `addColorM` or `DoAddColor`.

## Verification

Run:

1. `FlowDraftEditorWorkflowTests`;
2. existing `FlowLevelGeneratorWindowTests`;
3. full EditMode suite.

Acceptance:

- all eight exact tests pass with zero skipped;
- all behavioral proofs above are explicit;
- full suite has zero individual failures;
- production net diff is surgical relative to `216122b`;
- only the two whitelisted tracked files change;
- `Assets/Temp/` and `Assets/Temp.meta` are absent;
- `.claude/settings.local.json` remains untouched and uncommitted.

## Commit

```text
fix: make draft workflow verification surgical
```

If the real button callback cannot be invoked under EditMode without changing
another tracked file, output `BLOCKED` with the exact technical reason.
