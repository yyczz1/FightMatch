# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-COMPLETION-EDITOR-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `5`  
**Depends on:** `FLOW-RECOVERY-COMPLETION-CORE-001`  
**Goal:** Implement the previously empty Editor completion packet with
main-thread polling, progress, cancellation, stale-result protection, and
snapshot application.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowResultPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

Every other file is forbidden.

## Required behavior

1. Compose provider/service once per visual lifecycle.
2. Complete enables only for an endpoint-complete Draft when no completion is
   running. Provider label is exactly `Local Exact Solver`.
3. Copy Draft endpoints and fixed constraints into a request before starting.
4. Poll completion and progress through one registered
   `EditorApplication.update` callback.
5. Apply all UI and Draft changes only from Editor main thread.
6. Cancel button cancels the active source and reflects Cancelled.
7. On disable/destroy/rebuild, unsubscribe update callback and cancel/dispose
   the source.
8. Use a monotonically increasing operation ID so stale results after New,
   Load, Clear, another Complete, or disable are ignored.
9. Solved application is one complete snapshot command; Undo restores the
   unsolved Draft and Redo restores solved solution/difficulty/validation.
10. Timeout, Cancelled, NoSolution, InvalidInput, and Error do not make Draft
    saveable and display exact structured status.
11. ProgressBar displays phase, nodes, and elapsed data.
12. Repeated `CreateGUI` does not duplicate callbacks, services, or controls.
13. Editor code contains no `Task.Run`.

## Required tests

Use controllable fake providers/tasks. Prove enablement, request copy,
constraints, progress, solved application, Undo/Redo, all failure statuses,
cancel, stale-result rejection, disable cleanup, main-thread update boundary,
and repeated `CreateGUI`.

## Verification and commit

Focused Editor completion tests, then full EditMode suite.

Commit exactly:

```text
fix: complete editor completion workflow
```
