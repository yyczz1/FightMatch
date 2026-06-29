# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-FINAL-TESTS-001`

**Group:** `FLOW-GROUP-09`

**Order:** `1`

**Goal:** Add final cross-module generation, solving, Draft, completion,
persistence, and tool-contract regression matrices without changing
production code.

## Allowed files

**Create:**

- `Assets/Tests/EditMode/Integration/FlowPuzzleFinalAcceptanceTests.cs`
- `Assets/Tests/EditMode/Integration/FlowPuzzlePerformanceMatrixTests.cs`
- generated `.meta` files and Integration folder meta

**Modify:** `NONE`

Every production and existing test file is forbidden.

## Required tests

1. Generate each twice and deeply compare all level/solution fields:
   - 5x5 Easy, seed 101
   - 6x6 Normal, seed 202
   - 7x7 Hard, seed 303
2. Validate every generated recommendation.
3. Round-trip generated result -> Draft -> generated result with deep ownership.
4. Solve known deterministic 3x3 through 5x5 layouts and validate results.
5. Exercise bounded 6x6 and 7x7 fixtures without brittle machine-speed
   assertions; record status/nodes/time and require correct timeout semantics.
6. Complete endpoint-only and constrained Drafts through the provider/service.
7. Verify cancellation and tiny-budget timeout never become NoSolution.
8. Verify SaveNew, Overwrite, SaveAs, and dual JSON through temporary paths.
9. Verify tool adapter solve, validate, difficulty, and failure analysis.
10. Verify automatic generator has no solver dependency by behavior and static
    source inspection.
11. Clean only test-created assets and leave no `Assets/Temp` artifacts.

No test may use weak unordered comparisons, self-comparisons, vacuous
assertions, or name-only checks where exact data can be asserted.

## Verification

Run the two new fixtures, then all EditMode tests.

## Commit

`test: add final flow puzzle acceptance matrix`
