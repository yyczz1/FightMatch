# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-DIAGNOSTICS-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `6`  
**Depends on:** `FLOW-RECOVERY-COMPLETION-EDITOR-001`  
**Goal:** Replace diagnostic-code constants with complete structured
diagnostics and safe suggestion metadata across generation and completion.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Core/FlowDiagnosticCodes.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowFailureDiagnostic.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowGenerationResult.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowSolutionGenerator.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs`
- `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs`
- `Assets/Scripts/FlowPuzzle/Application/FlowLevelCompletionService.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Tests/EditMode/Core/FlowCoreDataContractsTests.cs`
- `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`
- `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`
- `Assets/Tests/EditMode/Application/FlowLevelCompletionServiceTests.cs`

**Allowed to create:**

- `Assets/Scripts/FlowPuzzle/Core/FlowDiagnosticSuggestion.cs`
- `Assets/Tests/EditMode/Core/FlowDiagnosticTests.cs`
- generated `.meta` for those files only

Every other file is forbidden.

## Required behavior

1. Preserve all required stable codes:
   InvalidDimensions, ImpossibleMinimumOccupancy, ImpossibleCoverageRange,
   PathGenerationFailed, CoverageOutOfRange, ValidationFailed,
   DifficultyOutOfRange, MaxLevelAttemptsReached, InvalidFixedConstraint,
   SolverTimeout, SolverCancelled, NoSolution, AssetAlreadyExists, and
   InvalidOutputFolder.
2. Diagnostic contains code, message, parameter name, current value,
   suggestion direction, optional safe scalar value, used seed, attempts, and
   zero or more candidate suggestions.
3. Suggestions use exact config/UI field names.
4. Only one unambiguous safe scalar change may populate the automatic value.
5. Probabilistic generation failures provide candidates without claiming a
   unique cause.
6. Solver Timeout, Cancelled, NoSolution, InvalidInput, and validation failure
   map to distinct structured diagnostics.
7. Preserve used seed and attempt count on every generation failure,
   including immediate validation failures.
8. Existing result factories remain compatible and own suggestion collections.
9. Diagnostics panel renders structured fields and does not parse free text.

## Required tests

Assert exact code, parameter, direction, safe-value presence/absence, seed,
attempts, and deep ownership for representative generation and solving
failures. Every required stable code must be exercised or explicitly factory
contract-tested.

## Verification and commit

Focused diagnostic tests, then full EditMode suite.

Commit exactly:

```text
fix: implement structured flow diagnostics
```
