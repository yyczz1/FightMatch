# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-TOOL-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `8`  
**Depends on:** `FLOW-RECOVERY-DIAGNOSTICS-001`  
**Goal:** Implement the actual pure local solver tool adapter and all five
future model-orchestration operations.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Solving/Tools/FlowSolverToolContracts.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowPuzzle.Solving.asmdef`

**Allowed to create:**

- `Assets/Scripts/FlowPuzzle/Solving/Tools/FlowSolverToolAdapter.cs`
- `Assets/Tests/EditMode/Solving/FlowSolverToolAdapterTests.cs`
- generated `.meta` for those files only

Every other file is forbidden.

## Required operations

```text
solve_board
solve_with_constraints
validate_solution
evaluate_difficulty
analyze_failure
```

## Required behavior

1. Provide explicit pure request/response DTOs for all five operations.
2. Adapter constructor injects Solver, Validator, and Difficulty evaluator and
   rejects nulls.
3. Requests and responses deep-copy every mutable DTO/list/path/cell/report.
4. Both solve methods preserve structured status, diagnostics, nodes, elapsed
   time, and constraints.
5. Every Solved response is Validator-checked.
6. Validate and evaluate operations return complete structured results.
7. Analyze Failure consumes structured diagnostics and suggestions; it never
   parses UI/free text.
8. No Unity API, Editor API, network, filesystem, environment variable, API
   key, model SDK, prompt, provider selection, or unique-solution search.

## Required tests

Test all five operations, constrained prefix preservation, invalid input,
timeout/NoSolution distinction, constructor nulls, source/result ownership,
deterministic repeated calls, and static absence of Unity/network references.

## Verification and commit

Focused adapter tests, then full EditMode suite.

Commit exactly:

```text
fix: implement solver tool adapter
```
