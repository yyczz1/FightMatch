# External DeepSeek Corrective Task Packet

## Metadata

**Task ID:** `FLOW-GROUP-06-FIX-03`

**Group:** `FLOW-GROUP-06-REPAIR`

**Order:** `3`

**Depends on:** `FLOW-GROUP-06-FIX-02`

**Goal:** Complete fixed-path legality, atomic draw/erase strokes, pointer
grouping, distinct rendering, Undo/Redo, and tests.

## Allowed files

**Modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

Every other file is forbidden.

## Required corrections

1. Validate one endpoint-anchored simple chain per color.
2. Require one own-color endpoint plus at least one non-endpoint cell,
   orthogonal continuity, bounds, no duplicates, no branches, no second own
   endpoint, no foreign endpoint, and no cross-color overlap.
3. Reject floating, multi-segment, complete endpoint-to-endpoint, invalid, and
   overlapping strokes atomically.
4. Erase shortens or clears only the endpoint-anchored suffix; never leave a
   floating segment.
5. Command stores complete before/after constraint snapshots and returns false
   without Draft mutation on invalid execution.
6. One down/move*/up sequence creates one history item. Cancel creates none.
7. Capture/release the pointer correctly and avoid duplicate callbacks.
8. BoardView emits pointer intents only and renders constraints through
   `Painter2D` with a visible style difference.
9. One Undo/Redo affects the complete multi-cell stroke.
10. Add exact positive/negative tests for every rule and integration tests for
    draw, erase, cancel, Undo, Redo, and save disabling.

## Verification

Run all EditMode tests; total must increase and failed count must be zero.

## Commit

`fix: complete fixed constraint editing`
