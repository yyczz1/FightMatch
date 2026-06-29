# External DeepSeek Corrective Task Packet

## Metadata

**Task ID:** `FLOW-GROUP-06-FIX-01`

**Group:** `FLOW-GROUP-06-REPAIR`

**Order:** `1`

**Goal:** Correct Draft mutation, mapping, command atomicity, deep ownership,
and tests before any UI integration.

## Allowed files

**Read:** all Group 6 source, tests, documents, accepted Core/Persistence DTOs.

**Modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftPairData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMutationResult.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowSnapshotCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/MoveEndpointCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/ResizeBoardCommand.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`
- the two matching currently-untracked test `.meta` files

Every other file is forbidden.

## Required corrections

1. Replace the single global `constraint` with a per-color
   `fixedConstraints` collection containing at most one entry per color.
2. Add atomic Draft APIs for add/remove color, place/move/remove endpoint, and
   resize. Callers and tests must not edit public lists to simulate operations.
3. Validate dimensions, bounds, unique colors, endpoint occupancy, endpoint
   count, and duplicate endpoint cells.
4. Keep `colorCount`, configured pairs, and color removal deterministic.
5. Every successful structural mutation invalidates the old solution.
6. Every failed mutation leaves all fields, dirty state, validation state,
   lists, solution, and difficulty unchanged.
7. `HasCompleteEndpoints` validates every configured pair rather than assuming
   IDs are exactly `0..colorCount-1`.
8. Resize deterministically removes out-of-bounds endpoints and clears affected
   constraints through the Draft API.
9. All mapper directions own complete deep copies. Mutating a returned
   generated level must not mutate Draft, and vice versa.
10. `ToGeneratedLevel` preserves level ID, seed, solution level ID, difficulty,
    coverage metadata, ordered pairs, ordered paths, and every cell.
11. Endpoint command failure must not call `MarkDirty` or mutate Draft.
12. Undo restores the exact pre-command dirty/validated/solution/difficulty
    state; Redo restores the exact post-command state.
13. Tests must prove every rule above, including mutation snapshots before and
    after failure.

## Verification

Run all EditMode tests. Expected total greater than 295 and zero failures.

## Commit

`fix: complete draft and command contracts`

If scope is insufficient, return `BLOCKED`; do not modify another file.
