# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-SOLVER-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `3`  
**Depends on:** `FLOW-RECOVERY-DRAFT-EDITOR-001`  
**Goal:** Replace the first-path skeleton with a deterministic exhaustive
first-solution solver whose NoSolution, Timeout, and Cancelled semantics are
correct.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveStatus.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowPuzzleSolver.cs`
- `Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs`
- `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`

Every other file is forbidden.

## Required behavior

1. Request owns deep copies and includes timeout milliseconds, visited-node
   budget, and progress interval.
2. Solver uses per-call local search context; no mutable search fields shared
   across calls.
3. Validate dimensions, pair IDs, endpoints, fixed-prefix color membership,
   bounds, continuity, duplicates, own anchor orientation, endpoint traversal,
   and overlaps before touching the board.
4. Accept a prefix anchored at either endpoint and continue from its open end
   toward the opposite endpoint.
5. BFS precheck every unfinished color before search and after each committed
   candidate path.
6. Deterministically order colors by prefix presence, reachable area,
   Manhattan distance, and color ID.
7. DFS must enumerate every viable simple candidate path for a color until a
   complete solution is found or the search space is exhausted. It must not
   stop after the first path when later colors fail.
8. Backtracking restores every non-prefix board cell exactly.
9. Build each returned `FlowPathData.cells` in actual endpoint-to-endpoint
   traversal order; never reconstruct by scanning board coordinates.
10. Stop at the first valid complete solution. Do not test uniqueness or
    require full-board occupancy.
11. Count search nodes consistently at candidate expansion.
12. Check cancellation, elapsed timeout, and node budget at bounded intervals.
13. Exhaustion alone returns NoSolution. Budget/time returns Timeout.
    Cancellation returns Cancelled.
14. Report progress with nondecreasing nodes/time and meaningful phase/color.
15. Repeated and concurrent calls on one solver instance remain isolated and
    deterministic.

## Required tests

Tests must validate every Solved result with `FlowSolutionValidator`.

Include:

- a fixture where the first candidate path blocks another color but a later
  candidate solves the board;
- provable exhaustive NoSolution;
- both fixed-prefix endpoint orientations;
- every invalid-prefix category;
- tiny node budget and wall timeout;
- pre-cancelled and mid-search cancellation with exact assertions;
- observed progress;
- ordered path adjacency/endpoints;
- request immutability;
- deep deterministic equality;
- same-instance concurrent calls.

Remove the vacuous cancellation test. No conditional empty statements.

## Verification and commit

Run solver fixture at least three times, then full EditMode suite.

Commit exactly:

```text
fix: implement exhaustive flow solver
```
