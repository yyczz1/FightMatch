# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-DIAGNOSTICS-001`

**Group:** `FLOW-GROUP-08`

**Order:** `1`

**Goal:** Define stable structured diagnostic codes and actionable parameter
suggestions across generation and completion.

## Allowed files

**Create:**

- `Assets/Scripts/FlowPuzzle/Core/FlowDiagnosticCodes.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowDiagnosticSuggestion.cs`
- `Assets/Tests/EditMode/Core/FlowDiagnosticTests.cs`
- corresponding generated `.meta`

**Modify:**

- `Assets/Scripts/FlowPuzzle/Core/FlowFailureDiagnostic.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowGenerationResult.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowSolutionGenerator.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs`
- `Assets/Scripts/FlowPuzzle/Application/FlowLevelCompletionService.cs`
- `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`
- `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`
- `Assets/Tests/EditMode/Application/FlowLevelCompletionServiceTests.cs`

Every other file is forbidden.

## Required behavior

Define stable constants for at least:

```text
InvalidDimensions
ImpossibleMinimumOccupancy
ImpossibleCoverageRange
PathGenerationFailed
CoverageOutOfRange
ValidationFailed
DifficultyOutOfRange
MaxLevelAttemptsReached
InvalidFixedConstraint
SolverTimeout
SolverCancelled
NoSolution
AssetAlreadyExists
InvalidOutputFolder
```

1. Diagnostic includes code, message, parameter name, current value, suggested
   direction, optional safe scalar value, used seed, and attempt count.
2. Preserve existing result factory compatibility while populating new fields.
3. Suggestions use stable parameter names matching config/UI fields.
4. Impossible minimum occupancy suggests reducing color/path minimum or
   increasing board size.
5. coverage/path failures suggest appropriate coverage/path/board changes.
6. difficulty failures identify the target range and safe direction.
7. timeout suggests budget increase, fewer colors, or fewer constraints.
8. cancellation gives no fake parameter cause.
9. NoSolution is distinct from Timeout and InvalidFixedConstraint.
10. Probabilistic failures may provide candidates but no automatic scalar
    application unless exactly one safe change is known.
11. Fixed seed and attempt metadata survive every failure path.

## Protected change permission

The exact structured additions to `FlowFailureDiagnostic` are authorized.
Do not change `FlowLevelAsset` serialization.

## Tests

Assert exact codes and actionable parameter fields for representative
impossible generation, validation, difficulty, invalid constraint, timeout,
cancellation, and no-solution cases.

## Verification

Run all affected tests, then all EditMode tests.

## Commit

`feat: add stable flow diagnostics`
