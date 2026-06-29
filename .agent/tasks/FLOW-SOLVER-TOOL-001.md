# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-SOLVER-TOOL-001`

**Group:** `FLOW-GROUP-08`

**Order:** `3`

**Depends on:** `FLOW-DIAGNOSTICS-001`

**Goal:** Expose local solve, validate, difficulty, and failure-analysis
capabilities through pure DTO tool contracts for future model use.

## Allowed files

**Create:**

- `Assets/Scripts/FlowPuzzle/Solving/Tools/FlowSolverToolContracts.cs`
- `Assets/Scripts/FlowPuzzle/Solving/Tools/FlowSolverToolAdapter.cs`
- `Assets/Tests/EditMode/Solving/FlowSolverToolAdapterTests.cs`
- generated `.meta` files and Tools folder meta

**Modify:**

- `Assets/Scripts/FlowPuzzle/Solving/FlowPuzzle.Solving.asmdef`

Every other file is forbidden.

## Required behavior

Provide pure DTO methods corresponding to:

```text
solve_board
solve_with_constraints
validate_solution
evaluate_difficulty
analyze_failure
```

1. Adapter composes accepted Solver, Validator, and Difficulty evaluator.
2. Constructors use explicit dependency injection and reject nulls.
3. Requests and responses deep-copy all mutable data.
4. Solve status, diagnostics, seed, visited nodes, elapsed time, validation,
   and difficulty remain structured fields.
5. `analyze_failure` consumes structured diagnostics; it does not infer from
   UI text.
6. Every solved response is Validator-checked.
7. No Unity API, HTTP, API keys, model SDK, prompt, provider selection, file
   system, or Editor dependency.
8. No unique-solution search.

## Tests

Test all five methods, constrained solution preservation, invalid input,
timeout/no-solution distinction, deep ownership, deterministic repeated
requests, and no network/Unity references.

## Verification

Run adapter tests, then all EditMode tests.

## Commit

`feat: add local solver tool adapter`
