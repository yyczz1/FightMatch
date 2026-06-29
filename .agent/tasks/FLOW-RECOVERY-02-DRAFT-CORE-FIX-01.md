# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-CORE-FIX-01`  
**Status:** `APPROVED_FOR_WORKER`  
**Depends on:** commit `98db94e`  
**Goal:** Make Stroke Redo use immutable stored state, make erase use the
earliest touched chain index, and strengthen complete ownership assertions.

## Execution boundary

Execute this packet only, create one commit, report, and stop.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`

**Files allowed to create:** `NONE`

Every other file is forbidden.

## Required RED tests

1. `StrokeRedo_UsesStoredAfterSnapshot`
   - Execute and Undo a valid stroke.
   - Mutate the Draft state without executing a new history command.
   - Redo through `FlowEditorCommandHistory`.
   - Assert the exact originally captured post-command snapshot is restored;
     Redo must not recalculate the stroke against current Draft state.
2. `EraseStroke_UsesEarliestTouchedChainCell`
   - Supply touched cells in an order where the first touched cell is farther
     from the anchor than a later touched cell.
   - Assert erase trims from the minimum chain index among all touched cells.
3. Strengthen `StrokeUndo_RestoresCompletePreCommandState` to compare:
   coverage, dirty/validated flags, pairs, every constraint cell, solution
   level/path/cells, and every difficulty field.
4. Strengthen `Mapper_PreservesCoverageAndOwnsAllData` to compare:
   all level fields, ordered pair endpoints, solution level ID, ordered paths
   and cells, every difficulty-report field, seed, and coverage; mutate nested
   values in both directions to prove isolation.

Record observed RED failures before production changes.

## Required implementation

1. First Execute captures immutable before and after Draft snapshots.
2. Subsequent Execute used by Redo restores the stored after snapshot directly.
   It must not replace either snapshot or rerun Draft constraint mutation.
3. Invalid first Execute stores no usable after snapshot.
4. Erase finds every touched cell present in the existing chain and trims from
   the minimum chain index. If none are present, return false without mutation.
5. Undo restores the complete immutable before snapshot.
6. Do not change Draft mutation APIs, Editor UI, Solver, or any other file.

## Verification

Run the Draft/command/constraint fixtures and complete EditMode suite.

Expected:

- all required tests pass;
- test total greater than 325;
- zero failed/skipped;
- only three whitelist files changed;
- `git diff --check` clean except Unity-generated `.meta` whitespace;
- no temp artifacts.

## Commit

```text
fix: make draft stroke snapshots deterministic
```

Never continue to the Draft Editor packet afterward.
