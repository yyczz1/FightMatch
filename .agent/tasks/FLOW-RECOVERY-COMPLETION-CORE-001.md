# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-COMPLETION-CORE-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `4`  
**Depends on:** `FLOW-RECOVERY-SOLVER-001`  
**Goal:** Complete deeply owned asynchronous local completion and validated,
scored Application output.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowLevelCompletionProvider.cs`
- `Assets/Scripts/FlowPuzzle/Solving/LocalExactCompletionProvider.cs`
- `Assets/Scripts/FlowPuzzle/Application/FlowLevelCompletionService.cs`
- `Assets/Scripts/FlowPuzzle/Application/FlowPuzzle.Application.asmdef`

**Allowed to create:**

- `Assets/Tests/EditMode/Solving/LocalExactCompletionProviderTests.cs`
- `Assets/Tests/EditMode/Application/FlowLevelCompletionServiceTests.cs`
- generated `.meta` for those tests only

Every other file is forbidden.

## Required behavior

1. Completion request contains level endpoints, fixed constraints, seed,
   timeout, node budget, and progress interval. It never treats old
   `currentSolution` as a fixed constraint.
2. Request constructor/factory owns complete deep copies.
3. Provider receives an injected `IFlowPuzzleSolver`; do not construct the
   concrete solver inside each call.
4. `Task.Run` wraps only the pure synchronous solver invocation.
5. Pre-cancelled and mid-run cancellation produce a structured Cancelled
   result rather than leaking `TaskCanceledException`.
6. Progress propagates without touching Unity synchronization objects.
7. Provider maps every solver status exactly and owns a deep result copy.
8. Application service validates Solved output, evaluates difficulty,
   computes coverage, synchronizes level difficulty fields, preserves seed,
   and returns a complete deeply owned `FlowGeneratedLevel`.
9. Invalid solver output becomes structured ValidationFailed/Error and never
   becomes saveable.
10. Inputs, provider output, service output, solution paths, and reports share
    no mutable references.
11. No UnityEngine, UnityEditor, AssetDatabase, ScriptableObject, or GUI type.

## Required tests

Use fake injected solvers/providers to prove:

- execution leaves the caller thread asynchronously;
- request deep ownership;
- fixed constraints, not old solution, reach the solver;
- every status mapping;
- pre/mid cancellation;
- progress;
- timeout preservation;
- validator rejection;
- difficulty/coverage/seed synchronization;
- complete mutation isolation.

## Verification and commit

Focused provider/service fixtures, then full EditMode suite.

Commit exactly:

```text
fix: complete async completion core
```
