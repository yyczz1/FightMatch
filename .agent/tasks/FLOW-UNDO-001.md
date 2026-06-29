# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-UNDO-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-06`

**Order in group:** `2`

**Depends on:** `FLOW-DRAFT-001`

**Goal:** Add deterministic command-based in-memory Undo/Redo for Draft edits.

## Scope whitelist

**Files allowed to read:**

- project rules and all Group 6 documents
- all files accepted from `FLOW-DRAFT-001`
- accepted Core and Editor tests

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/Undo/IFlowEditorCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowSnapshotCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/MoveEndpointCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/ResizeBoardCommand.cs`
- `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`
- generated `.meta` files for the listed files and new Undo folder

**Files forbidden to modify:** every existing file.

## Required behavior

1. `IFlowEditorCommand` provides a display name, `Execute`, and `Undo`.
2. `FlowEditorCommandHistory` supports Execute, Undo, Redo, Clear,
   `CanUndo`, and `CanRedo`.
3. Executing a new command after Undo clears the redo stack.
4. Failed commands do not enter history or partially mutate Draft.
5. `MoveEndpointCommand` supports endpoint placement, movement, and removal
   without Unity APIs.
6. Resize, add/remove color, and other multi-field operations use complete
   before/after deep snapshots through `FlowSnapshotCommand`.
7. `ResizeBoardCommand` preserves and restores every affected Draft field.
8. Snapshot commands own their snapshots and share no mutable lists with Draft.
9. Undo/Redo restores all Draft state, including constraints, solution,
   difficulty, seed, dirty state, and validation state.
10. This is Draft history only. Do not call Unity `Undo`.

## Tests

Cover:

- Execute/Undo/Redo ordering;
- empty history behavior;
- redo clearing after a new command;
- failed command history isolation;
- endpoint place/move/remove round trips;
- resize round trip including removed out-of-bounds data;
- add/remove color snapshot restoration;
- snapshot ownership and complete-state restoration.

## Non-goals

- No Stroke Command, UI, keyboard shortcuts, ScriptableObject changes, solver,
  async work, or Runtime code.

## Verification

Run the command-history tests, then all EditMode tests and compile.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|Undo\.|AssetDatabase|IFlowPuzzleSolver" `
  Assets/Scripts/FlowPuzzle/Editor/Undo
```

Expected: no matches.

## Maximum change scope

- production files: 5
- test files: 1
- approximate maximum diff: 900 changed lines

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow editor command history`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
