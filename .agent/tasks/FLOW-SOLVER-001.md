# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-SOLVER-001`

**Group:** `FLOW-GROUP-07`

**Order:** `1`

**Goal:** Implement deterministic synchronous exact first-solution search with
fixed-prefix support and strict result semantics.

## Allowed files

**Create:**

- `Assets/Scripts/FlowPuzzle/Solving/FlowPuzzle.Solving.asmdef`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveStatus.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowPuzzleSolver.cs`
- `Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs`
- `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`
- generated `.meta` files and new Solving/test folder metas

**Modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

Every other file is forbidden.

## Required behavior

1. Solving assembly references Core only and has no engine references.
2. Request owns a deep copy of level endpoints and fixed prefixes represented
   as pure Core path data.
3. Interface accepts request, `IProgress<FlowSolveProgress>`, and
   `CancellationToken`.
4. Result status is exactly Solved, NoSolution, Timeout, Cancelled,
   InvalidInput, or Error.
5. Validate dimensions, unique pairs, endpoint bounds/overlap, and all fixed
   prefix rules before search.
6. BFS precheck each unfinished color.
7. Deterministically order colors by fixed-prefix presence, reachable area,
   Manhattan distance, then color ID.
8. Enumerate simple candidate paths through DFS with deterministic neighbor
   ordering.
9. After committing a path, BFS-check remaining colors and backtrack on
   disconnection.
10. Preserve every fixed-prefix cell and continue only from its open end.
11. Stop after the first complete valid solution; do not check uniqueness.
12. Do not require full-board coverage.
13. Enforce cancellation, timeout milliseconds, and visited-node budget at
    bounded intervals.
14. Budget/time exhaustion returns Timeout, never NoSolution.
15. NoSolution only follows exhaustive elimination.
16. Progress reports phase, visited nodes, elapsed time, and current color
    without affecting determinism.
17. Repeated identical requests return deeply identical status and solution.

## Tests

Include deterministic 3x3, 4x4, and 5x5 known solutions, provable no-solution,
invalid input, fixed prefixes, empty-cell solutions, cancellation, node
timeout, wall timeout, progress, input immutability, and repeated equality.

Validate every solved result with the accepted Validator from the test
assembly; do not add a Validation dependency to the solver assembly.

## Verification

Run solver tests repeatedly, then all EditMode tests.

## Commit

`feat: add exact local flow solver`
