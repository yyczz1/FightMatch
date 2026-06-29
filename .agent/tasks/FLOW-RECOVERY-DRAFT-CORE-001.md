# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-DRAFT-CORE-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `1`  
**Goal:** Complete the atomic Draft domain, fixed-constraint validation, and
command semantics before any UI work.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftPairData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMutationResult.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowSnapshotCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/MoveEndpointCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/ResizeBoardCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`

**Allowed to create:**

- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs`
- generated `.meta` for that test only

Every other file is forbidden.

## Required behavior

1. Configured colors have unique contiguous IDs `0..colorCount-1`.
2. Removing a selected color deterministically compacts higher IDs in pairs and
   constraints and invalidates/clears stale solved data.
3. Endpoint operations require an existing color and in-bounds destination.
   Endpoint A and B of the same color may not overlap, and no endpoint may
   overlap another color endpoint.
4. Every mutation is atomic and returns a stable diagnostic. Failure preserves
   a complete before-snapshot byte-for-byte by explicit field comparison.
5. Successful structural edits invalidate the recommendation and validation.
6. Resize removes out-of-bounds endpoints and affected constraints
   deterministically.
7. Draft has at most one fixed chain per color.
8. A valid chain:
   - starts at exactly one own-color endpoint;
   - includes at least one non-endpoint cell;
   - is orthogonally contiguous;
   - stays in bounds;
   - has no duplicate, self-intersection, branch, second own endpoint, foreign
     endpoint, or overlap with another constraint.
9. Reject floating, multi-segment, complete endpoint-to-endpoint, invalid, and
   overlapping constraints without mutation.
10. Erase trims from the earliest touched chain index through the open end.
    Touching the anchor clears the chain. It never leaves a floating segment.
11. `DrawConstraintStrokeCommand` computes and owns complete before/after
    snapshots, calls Draft validation, returns false on invalid strokes, and
    makes one Undo/Redo restore the whole stroke.
12. Endpoint/resize/snapshot commands restore exact dirty, validation,
    solution, difficulty, pairs, and constraints state.
13. Mapper directions deep-copy every list, path, cell, report field, seed,
    coverage, and level ID.

## Required tests

Add exact tests for every rule above. Explicitly include:

- remove middle color and ID compaction;
- same-color endpoint overlap rejection;
- unknown-color endpoint rejection;
- failed mutation complete snapshot equality;
- every fixed-chain rejection category;
- draw/erase full-chain Undo/Redo;
- mapper mutation isolation in both directions.

No direct list/field mutation may substitute for testing a public mutation API.

## Verification and commit

Focused Draft/command/constraint fixtures, then full EditMode suite.

Commit exactly:

```text
fix: complete draft constraint domain
```
