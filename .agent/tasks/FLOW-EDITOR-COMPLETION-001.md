# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-EDITOR-COMPLETION-001`

**Group:** `FLOW-GROUP-07`

**Order:** `3`

**Depends on:** `FLOW-COMPLETION-001`

**Goal:** Enable Local Exact Solver completion, progress, cancellation, and
main-thread Draft application in the Editor window.

## Allowed files

**Modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowResultPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

Every other file is forbidden.

## Required behavior

1. Enable Complete only for endpoint-complete Drafts not already solving.
2. Show the only actual provider as `Local Exact Solver`; do not show an LLM
   option.
3. Copy Draft into a pure completion request before starting.
4. Start completion through `FlowLevelCompletionService`.
5. Poll task state from `EditorApplication.update`.
6. Apply progress, diagnostics, solved data, board repaint, and button state
   only on the main thread.
7. Cancel uses `CancellationTokenSource`; close/disable cancels and disposes it.
8. Applying a solved result is one snapshot command so Undo restores the
   pre-completion Draft and Redo restores the solution.
9. Solved application sets current solution/difficulty, clean state, validated
   state, and enables Save.
10. Timeout, cancellation, invalid input, no solution, and error leave the
    Draft unsaveable and show exact status.
11. Ignore stale completion results after a new Draft, new completion, clear,
    load, or window disable.
12. Repeated `CreateGUI` or enable/disable does not duplicate update callbacks.
13. Editor code must not call `Task.Run`.

## Tests

Cover Complete enablement, solved apply, Undo/Redo apply, progress display,
cancel, timeout, stale result rejection, disable cleanup, no duplicate update
callbacks, main-thread UI application, and Save enablement.

## Verification

Run Editor completion tests, then all EditMode tests.

## Commit

`feat: connect editor completion workflow`
