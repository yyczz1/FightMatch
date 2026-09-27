# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-03-SOLVER-A`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** accepted Draft Editor C line, latest accepted candidate commit `7b0c4ef`  
**Start commit:** `7b0c4ef`

**Goal:** Replace the current Solver skeleton with a deterministic exhaustive
first-solution local solver whose `Solved`, `NoSolution`, `Timeout`, and
`Cancelled` semantics are real and test-covered.

## Background

Draft Editor recovery is now accepted. The next unresolved recovery area is the
local exact solver.

Current solver issues visible in
`Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs`:

- `DfsPaths` stops after the first valid path:

  ```csharp
  if (results.Count >= 1) return; // first valid path only
  ```

  This means `NoSolution` is not trustworthy: a later path for the same color
  might allow another color to solve.
- Request timeout/node budget/progress interval are hardcoded in solver context,
  not supplied by request.
- `BuildSolution` reconstructs paths by scanning board coordinates instead of
  preserving actual traversal order.
- Fixed prefix validation is weak.
- Cancellation test is vacuous:

  ```csharp
  if (r.status != FlowSolveStatus.Cancelled)
      ; // Acceptable
  ```

This packet must fix Solver core only. Do not implement Completion provider,
Editor completion UI, diagnostics retry, or tool adapter here.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-SOLVER-001.md`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`
- `Assets/Scripts/FlowPuzzle/Core/FlowBoard.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowLevelData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPairData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPathData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowSolutionData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs`
- `Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowPuzzleSolver.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveStatus.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs`
- `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`
- `Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowPuzzleSolver.cs`
- `Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs`
- `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/FlowPuzzle/Core/**`
- `Assets/Scripts/FlowPuzzle/Validation/**`
- `Assets/Scripts/FlowPuzzle/Difficulty/**`
- `Assets/Scripts/FlowPuzzle/Generation/**`
- `Assets/Scripts/FlowPuzzle/Application/**`
- `Assets/Scripts/FlowPuzzle/Editor/**`
- `Assets/Scripts/FlowPuzzle/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Solving/LocalExactCompletionProvider.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionRequest.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionProgress.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/IFlowLevelCompletionProvider.cs`
- `Assets/Scripts/FlowPuzzle/Solving/Tools/**`
- every test file except
  `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`
- all `.meta` files

Anything not explicitly allowed to modify is forbidden.

## Required behavior

### A. Request/result/progress contracts

1. `FlowSolveRequest` must contain:
   - `FlowLevelData levelData`
   - `List<FlowPathData> fixedPrefixes`
   - `long nodeBudget`
   - `int timeoutMs`
   - `int progressIntervalNodes`
2. Provide safe defaults:
   - node budget: large but finite, e.g. `10_000_000`
   - timeout: e.g. `10000`
   - progress interval: positive, e.g. `1000`
3. `FlowSolveRequest.Clone()` must deep-copy:
   - level data;
   - pairs;
   - fixed prefixes;
   - budget/timeout/progress settings.
4. Solver must not mutate the request or its nested lists.
5. `FlowSolveResult` must preserve:
   - status;
   - solution;
   - visitedNodes;
   - elapsedMs.
6. `FlowSolveProgress` must report meaningful:
   - phase;
   - currentColorId;
   - visitedNodes;
   - elapsedMs.

### B. Input validation

Return `InvalidInput` for:

1. null request;
2. null level;
3. non-positive width/height;
4. null/empty pairs;
5. duplicate color IDs;
6. endpoint out of bounds;
7. duplicate endpoint cells across all pairs;
8. fixed prefix with unknown color;
9. fixed prefix with null cells or fewer than 2 cells;
10. fixed prefix out-of-bounds cell;
11. fixed prefix duplicate cell;
12. fixed prefix non-adjacent cells;
13. fixed prefix whose first cell is not one endpoint of its color;
14. fixed prefix that passes through its opposite endpoint before the last cell;
15. fixed prefix that touches any foreign endpoint;
16. overlapping fixed prefixes between different colors;
17. non-positive timeout or node budget.

Validation must happen before search mutates a board.

### C. Fixed prefixes

1. Accept a prefix anchored at endpoint A and continue from its last cell toward
   endpoint B.
2. Accept a prefix anchored at endpoint B and continue from its last cell toward
   endpoint A.
3. Preserve prefix cells at the beginning of the returned path in the same
   orientation the prefix was supplied.
4. Do not clear prefix cells during backtracking.

### D. Exhaustive deterministic search

1. Solver must use a per-call search context. No shared mutable search state on
   the solver instance.
2. Determine color order deterministically. Acceptable order:
   - colors with fixed prefixes first;
   - smaller reachable area / tighter colors first if implemented;
   - shorter Manhattan distance first;
   - colorId ascending as final tie-breaker.
3. Before searching a color, run a BFS reachability precheck from open start to
   target through empty cells plus that color's target/prefix cells.
4. After committing a candidate path, run a reachability precheck for all
   remaining colors before deeper recursion.
5. DFS must enumerate all viable simple candidate paths for the current color
   until a complete solution is found or the search space is exhausted.
6. It must not stop after the first path for a color if that path causes a later
   color to fail.
7. Candidate path order must be deterministic. Use `FlowBoard.GetNeighbors`
   order unless there is a documented deterministic heuristic.
8. Backtracking must restore every non-prefix board cell exactly.
9. Stop at the first complete valid solution.
10. Do not test uniqueness.
11. Do not require all board cells to be occupied.

### E. Returned solution

1. Build each returned `FlowPathData.cells` from stored traversal paths, not by
   scanning board coordinates.
2. Each solved path must:
   - start at one endpoint and end at the paired endpoint;
   - be orthogonally adjacent cell-to-cell;
   - have no duplicate cells;
   - not overlap other paths except there should be no overlap at all in valid
     output.
3. Validate every solved result in tests with `FlowSolutionValidator`.

### F. Budget, timeout, cancellation, progress

1. Increment `visitedNodes` consistently at candidate expansion/DFS node visits.
2. If `visitedNodes >= nodeBudget`, return `Timeout`.
3. If elapsed milliseconds exceeds `timeoutMs`, return `Timeout`.
4. If cancellation is requested before or during search, return `Cancelled`.
5. Check cancellation/time/budget at bounded intervals inside DFS and recursion,
   not only at the top-level.
6. Report progress at least:
   - start/search phase;
   - per-color phase;
   - periodically by node count according to `progressIntervalNodes`;
   - final status phase is optional but useful.
7. Progress `visitedNodes` and `elapsedMs` must be nondecreasing.

## Required tests

Rewrite/strengthen `BacktrackingFlowPuzzleSolverTests.cs`. Remove vacuous tests.

Required exact test names:

1. `Solve_SimpleStraightPath_ReturnsValidatedSolution`
2. `Solve_FirstCandidateBlocksLaterColor_BacktracksToLaterPath`
3. `Solve_ExhaustedSearch_ReturnsNoSolution`
4. `Solve_DoesNotRequireFullBoardOccupancy`
5. `FixedPrefix_FromEndpointA_ContinuesToEndpointB`
6. `FixedPrefix_FromEndpointB_ContinuesToEndpointA`
7. `FixedPrefix_InvalidCases_ReturnInvalidInput`
8. `BudgetExceeded_ReturnsTimeout`
9. `PreCancelled_ReturnsCancelled`
10. `MidSearchCancellation_ReturnsCancelled`
11. `Progress_IsReportedWithNondecreasingNodesAndTime`
12. `SameRequest_DeeplyEqualAndDoesNotMutateRequest`
13. `SameSolverInstance_ConcurrentCalls_AreIsolated`
14. `SolvedPaths_AreInTraversalOrder_NotBoardScanOrder`
15. `NoUnityEngineReferencesInSolvingCore`

Test requirements:

- Every `Solved` result must be validated through `FlowSolutionValidator`.
- No conditional empty statements.
- No `Assert.Pass`.
- No `Assert.IsTrue(true)` or equivalent vacuous assertions.
- No weak assertions such as only checking `solution != null`.
- For the first-candidate-backtracks test, use a fixture where the earliest
  deterministic path for one color blocks another color, but a later candidate
  solves the puzzle. Assert final solution is valid and specifically not the
  blocking first path.
- For `SolvedPaths_AreInTraversalOrder_NotBoardScanOrder`, use a path whose
  board-coordinate scan order would differ from traversal order, then assert
  adjacency and exact start/end order.
- For fixed prefixes, assert the returned path begins with the supplied prefix
  sequence exactly.
- For request immutability, snapshot request before solving and compare after.
- For concurrency, run at least two `Task.Run` calls on the same solver instance
  with different levels and assert both results are valid and deterministic.

## Non-goals

- No Completion provider changes.
- No Editor UI changes.
- No diagnostics panel changes.
- No tool adapter changes.
- No generation changes.
- No uniqueness checking.
- No full-board occupancy requirement.
- No new dependencies.

## Constraints

- Solving core must remain pure C#.
- Do not reference `UnityEngine`, `UnityEditor`, `AssetDatabase`, `Task.Run` in
  production solving code. `Task.Run` is allowed only in tests for concurrency.
- Automatic generation must not reference or call the solver.
- Keep implementation understandable; avoid speculative architecture.
- If exhaustive search needs helper private methods/classes, keep them inside
  `BacktrackingFlowPuzzleSolver.cs` unless a split is absolutely necessary.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `LIMITED` | Add budget/timeout/progress fields to `FlowSolveRequest` only |
| Production code changes | `LIMITED` | Solving files in whitelist only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `6`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `650 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: implement exhaustive flow solver`

Commit only the allowed files after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] All 15 required solver test names exist.
- [ ] Solver no longer has `if (results.Count >= 1) return` or equivalent
      first-path-only behavior.
- [ ] Solved results are validated with `FlowSolutionValidator`.
- [ ] NoSolution means search exhausted, not first candidate failed.
- [ ] Timeout is returned for node budget/time budget.
- [ ] Cancelled is returned for pre-cancelled and mid-search cancellation.
- [ ] Fixed prefixes work from either endpoint and preserve prefix order.
- [ ] Returned paths are in traversal order.
- [ ] Request is not mutated.
- [ ] Same solver instance supports concurrent calls without shared-state leaks.
- [ ] Full EditMode tests pass with at least `362` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No files outside the whitelist changed.

## Verification steps

Run the solver fixture three times:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowSolverA1.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowSolverA1.log'
```

Repeat as `FlowSolverA2.xml` and `FlowSolverA3.xml`.

Expected each run:

```text
At least 362 total tests, 0 failed, 0 skipped/inconclusive.
```

Run:

```powershell
Test-Path 'Assets/Temp'
Test-Path 'Assets/Temp.meta'
Test-Path 'Assets/FlowPuzzleGenerated'
Test-Path 'Assets/FlowPuzzleGenerated.meta'
```

Expected:

```text
False
False
False
False
```

Run:

```powershell
rg -n "results\.Count\s*>=\s*1|Acceptable|Assert\.Pass|Assert\.IsTrue\(true\)|;\\s*// Acceptable|UnityEngine|UnityEditor|AssetDatabase" Assets/Scripts/FlowPuzzle/Solving Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs
```

Expected:

```text
No forbidden matches. If `Task.Run` appears, it must be in tests only.
```

Run:

```powershell
rg -n "IFlowPuzzleSolver|BacktrackingFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Generation
```

Expected:

```text
No matches.
```

Run:

```powershell
git diff --name-only 7b0c4ef..HEAD
git status --short
git diff --check
```

Expected:

```text
Only allowed tracked files changed.
No untracked Temp/FlowPuzzleGenerated artifacts.
No whitespace errors.
```

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ROOT CAUSE:
- <one or two sentences describing the old solver flaw>

GREEN:
- <test count, failed/skipped count for each run>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <exit code / test count / concise evidence>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO
- Protected changes made: YES/NO

PATCH:
<unified diff preferred; if edits were applied directly, provide a concise diff summary>

LOCAL COMMIT:
- Created: YES/NO
- Hash: <hash or N/A>
- Message: <message or N/A>

BLOCKER:
<required only when STATUS is BLOCKED>
```

Do not include architecture essays or unrelated suggestions.

## What to do if blocked

Return:

```text
BLOCKED
Reason:
Missing information:
Required decision:
Files inspected:
No changes made: Yes/No
```

Do not guess. Do not expand scope.
