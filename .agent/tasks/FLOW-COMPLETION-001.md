# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-COMPLETION-001`

**Group:** `FLOW-GROUP-07`

**Order:** `2`

**Depends on:** `FLOW-SOLVER-001`

**Goal:** Add the async local completion provider and pure Application service
that validates and scores solved recommendations.

## Allowed files

**Create:**

- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowLevelCompletionProvider.cs`
- `Assets/Scripts/FlowPuzzle/Solving/LocalExactCompletionProvider.cs`
- `Assets/Scripts/FlowPuzzle/Application/FlowLevelCompletionService.cs`
- `Assets/Tests/EditMode/Solving/LocalExactCompletionProviderTests.cs`
- `Assets/Tests/EditMode/Application/FlowLevelCompletionServiceTests.cs`
- corresponding generated `.meta`

**Modify:**

- `Assets/Scripts/FlowPuzzle/Application/FlowPuzzle.Application.asmdef`

Every other file is forbidden.

## Required behavior

1. Provider interface matches the approved asynchronous contract.
2. Request owns deep pure-data copies and never references Draft or Unity.
3. Local provider wraps only solver work in `Task.Run`.
4. Cancellation and progress propagate without status translation errors.
5. Timeout remains Timeout; Cancelled remains Cancelled.
6. Completion service depends on provider, Validator, and Difficulty evaluator
   through constructor injection.
7. On Solved, service validates the returned solution, evaluates difficulty,
   computes coverage, and builds a complete deeply owned `FlowGeneratedLevel`.
8. A solver result that fails Validator becomes Error/ValidationFailed, not
   Solved.
9. Inputs, provider results, and returned completion results share no mutable
   lists.
10. No UnityEngine, UnityEditor, ScriptableObject, AssetDatabase, or GUI types.

## Tests

Prove work is asynchronous, cancellation, timeout, progress, validation,
difficulty, coverage, deep ownership, deterministic completion, provider
display name, and null/input failures.

## Verification

Run provider/service tests, then all EditMode tests.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|AssetDatabase|ScriptableObject|GUIContent" `
  Assets/Scripts/FlowPuzzle/Solving `
  Assets/Scripts/FlowPuzzle/Application
```

Expected: no matches.

## Commit

`feat: add async local completion service`
