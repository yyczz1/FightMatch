# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-CORE`  
**Status:** `APPROVED_FOR_WORKER`  
**Goal:** Close the remaining atomic Draft/constraint/command ownership gaps
before any further Editor, Solver, or Completion work.

## Execution boundary

Execute this packet only. After its commit and report, stop all work and return
to Codex review.

Begin only from the exact start commit supplied after this document is
committed.

Permitted pre-existing working-tree changes:

- user-managed `.claude/settings.local.json`;
- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs.meta`, which must
  be tracked in this packet commit.

## Scope whitelist

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs`
- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs.meta`

**Files allowed to create:** `NONE`

Every other file is forbidden.

## Required red tests

Before production changes, add tests that fail against the start commit:

1. `PlaceEndpoint_UnknownColor_FailsWithoutMutation`
   - pair count, colorCount, flags, solution, difficulty, and constraints remain
     exactly unchanged.
2. `Constraint_SecondOwnEndpointInMiddle_Rejected`
   - a chain cannot pass through the second own endpoint and continue.
3. `Erase_FirstNonEndpoint_ClearsConstraint`
   - trimming at index `1` cannot leave an anchor-only invalid constraint.
4. `StrokeUndo_RestoresCompletePreCommandState`
   - restore exact constraint, dirty/validated flags, solution, and difficulty.
5. `StrokeRedo_RestoresCompletePostCommandState`
   - Redo does not recalculate against a changed Draft or overwrite snapshots.
6. `RemoveMiddleColor_ClearsStaleSolutionAndDifficulty`
   - compacting IDs cannot retain a stale solution with old color IDs.
7. `Mapper_PreservesCoverageAndOwnsAllData`
   - generated -> Draft -> generated preserves coverage and every DTO field,
     with mutation isolation in both directions.

Record the observed failing test names in the response.

## Required implementation

1. Remove `EnsurePair` behavior that creates unknown colors. Endpoint mutations
   operate only on configured colors.
2. Constraint validation rejects the second own endpoint at any index after
   the anchor, not only when it is the final cell.
3. Erase removes the constraint whenever the remaining prefix would contain
   fewer than two cells.
4. `DrawConstraintStrokeCommand` owns complete immutable before/after Draft
   snapshots. Execute, Undo, and Redo restore exact snapshots.
5. Invalid Execute does not change dirty/validated state or any list.
6. Removing/reindexing a color clears stale current solution and difficulty.
7. Add Draft coverage state and include it in Clone, Restore, FromAsset,
   FromGeneratedLevel, and ToGeneratedLevel.
8. Mapper deep-copies every pair, path, cell, report field, seed, level ID, and
   coverage value.
9. Do not change UI, Solver, Completion, Diagnostics, Retry, or Tool code.

## Verification

Run:

- the three Draft/command/constraint fixtures;
- the complete EditMode suite;
- `git diff --check`;
- `git diff --name-only <START>..HEAD`.

Expected:

- every required red test is now green;
- total test count is greater than 318;
- zero failed/skipped tests;
- only whitelist files changed;
- no temp artifacts.

## Commit

Create exactly one commit:

```text
fix: close draft core recovery gaps
```

Never create an empty commit, merge another packet, or continue afterward.
