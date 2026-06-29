# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-FINAL-TESTS-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `9`  
**Depends on:** packets 1 through 8  
**Goal:** Add the previously omitted cross-module acceptance and performance
matrices without modifying production code.

## Scope

**Allowed to create:**

- `Assets/Tests/EditMode/Integration/FlowPuzzleFinalAcceptanceTests.cs`
- `Assets/Tests/EditMode/Integration/FlowPuzzlePerformanceMatrixTests.cs`
- generated `.meta` files and Integration folder meta

**Allowed to modify:** `NONE`

Every other file is forbidden.

## Required acceptance tests

1. Generate twice and deeply compare:
   - 5x5 Easy, seed 101;
   - 6x6 Normal, seed 202;
   - 7x7 Hard, seed 303.
2. Validate every generated recommendation and assert configured coverage,
   path length, and difficulty filters.
3. Round-trip generated -> Draft -> generated with full deep ownership.
4. Exercise Draft endpoint/color/resize/constraint Undo/Redo.
5. Solve and Validator-check deterministic 3x3, 4x4, and 5x5 fixtures.
6. Exercise bounded 6x6 and 7x7 solver fixtures; record nodes/time and verify
   status semantics without brittle machine-speed limits.
7. Complete endpoint-only and fixed-constraint Draft requests through provider
   and service.
8. Prove cancellation and timeout never become NoSolution.
9. Exercise SaveNew, Overwrite with Unity Undo, SaveAs, reload, and dual JSON
   using test-only paths.
10. Exercise diagnostic suggestions and all three retry actions.
11. Exercise all five tool-adapter operations.
12. Prove automatic Generation has no Solver dependency.
13. Remove only test-created assets and leave no `Assets/Temp` artifacts.

## Test quality rules

- Exact ordered comparisons for pairs, paths, and cells.
- No self-comparison, empty branch, conditional non-assertion, `>= 0`,
  name-only asset check, or test that passes when the target behavior is
  absent.
- Every matrix case must appear by exact test name in XML.

## Verification and commit

Run both new fixtures, then the full EditMode suite.

Commit exactly:

```text
test: add final flow acceptance matrices
```
