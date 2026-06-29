# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-CONSTRAINT-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-06`

**Order in group:** `4`

**Depends on:** `FLOW-EDITOR-DRAFT-001`

**Goal:** Add endpoint-anchored fixed simple-chain constraints with atomic
Draft mutations, one command per pointer stroke, and distinct rendering.

## Scope whitelist

**Files allowed to read:**

- project rules and all Group 6 documents
- all files accepted from the first three Group 6 packets
- accepted Core and Editor tests

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- its generated `.meta`

**Files forbidden to modify:** every other file.

## Required behavior

1. Each color has at most one fixed constraint.
2. A valid constraint:
   - starts at exactly one endpoint of its own color;
   - contains that endpoint and at least one non-endpoint cell;
   - is orthogonally contiguous;
   - has no self-intersection or branch;
   - does not reach the second same-color endpoint;
   - does not cross another color endpoint;
   - does not overlap another color constraint.
3. Reject floating, multi-segment, complete endpoint-to-endpoint, out-of-bounds,
   overlapping, duplicate-cell, and non-contiguous constraints.
4. Constraint mutation is atomic: apply the whole legal result or leave Draft
   completely unchanged with a stable diagnostic.
5. Erase may only shorten or clear the endpoint-anchored chain. It must never
   leave a floating segment or branch.
6. One PointerDown/PointerMove*/PointerUp gesture creates exactly one
   `DrawConstraintStrokeCommand`.
7. PointerCancel abandons the uncommitted stroke and creates no history entry.
8. The view captures the pointer on down and releases it on up/cancel.
9. The view emits down/move/up/cancel intents only and never mutates Draft.
10. Draw and Erase use the same stroke grouping and history rules.
11. One Undo restores the entire pre-stroke state; one Redo reapplies it.
12. Draw constraints distinctly from solver/generated paths using `Painter2D`
    border, width, or pattern differences. Do not create one child element per
    cell.
13. Every successful constraint edit marks the solution dirty, clears
    validation, and disables saving.
14. `Complete` remains disabled. Do not add any temporary solver.

## Tests

Cover every accepted and rejected constraint rule with exact assertions.

Also cover:

- failed mutation leaves complete Draft snapshot unchanged;
- draw and erase multi-cell strokes create one history entry;
- Undo/Redo restores the whole stroke;
- cancel creates no command;
- the final item in a stroke is included;
- pointer lifecycle does not duplicate callbacks after `CreateGUI()` rebuild;
- board view has no Generator, Solver, Repository, or AssetDatabase dependency.

If GPU-independent pointer dispatch is unavailable in batch mode, test the
intent handlers and command boundary directly without adding public APIs only
for tests. Report any visual smoke step as `NOT RUN`; do not claim it passed.

## Non-goals

- No solver-generated continuation, async work, Runtime dragging, animation,
  zoom/pan, QFramework, LLM integration, or unique-solution search.

## Verification

Run all affected Draft, command, board, and window tests, then all EditMode
tests and compile.

Static:

```powershell
rg -n "IFlowPuzzleSolver|BacktrackingFlowPuzzleSolver|Task\.Run|System\.Threading|QFramework" `
  Assets/Scripts/FlowPuzzle/Editor

rg -n "AssetDatabase|FlowLevelGenerationService|FlowLevelAssetRepository" `
  Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs
```

Expected: no matches.

## Maximum change scope

- modified production files: 6
- new production files: 1
- test files: 4
- approximate maximum diff: 1400 changed lines

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add fixed path constraint editing`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
