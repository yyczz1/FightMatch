# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-DRAFT-EDITOR-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `2`  
**Depends on:** `FLOW-RECOVERY-DRAFT-CORE-001`  
**Goal:** Complete the interactive UI Toolkit Draft workflow, pointer strokes,
save operations, Undo/Redo, and reload state.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

**Allowed to create:**

- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- generated `.meta` for that test only

Every other file is forbidden.

## Required behavior

1. Wire every Draft control exactly once:
   New, Load, Add Color, Remove Color, tool selection, selected color,
   endpoint place/move/remove, Draw, Erase, Undo, Redo, Save, Save As.
2. Generate One creates a deeply owned editable Draft.
3. Load Asset creates a Draft and never mutates the source before explicit
   Overwrite.
4. BoardView owns a display copy and supports zero/one/two endpoints without
   synthesizing missing endpoints at `(0,0)`.
5. BoardView emits neutral down/move/up/cancel intents only.
6. PointerDown captures; PointerMove accumulates each distinct orthogonally
   adjacent cell; PointerUp releases and commits one command; PointerCancel
   releases and commits none.
7. Draw/Erase strokes route through command history and Draft validation.
8. Fixed constraints render distinctly from recommendation paths with
   `Painter2D`; no per-cell `VisualElement`.
9. Save uses SaveNew for source-less Draft and Overwrite for loaded asset.
   Save As works for all valid origins.
10. Save/Save As require complete endpoints, non-dirty validated solution, and
    difficulty data.
11. Every manual edit immediately updates board/button/diagnostic state.
12. Undo/Redo buttons reflect history and operate endpoints, resize/color
    snapshots, solution application, and complete strokes.
13. Serialize and restore Draft, loaded asset, selected color, selected tool,
    and visible state across `CreateGUI`/domain reload. History may reset.
14. Repeated `CreateGUI` creates one control tree and one callback per action.
15. Preserve all accepted automatic generation, batch, JSON, and persistence
    behavior.

## Required tests

Invoke actual control callbacks or action handlers and prove every operation.
Include complete save/reload, source immutability, pointer draw/erase/cancel,
one-stroke Undo, duplicate `CreateGUI`, partial endpoint display, and reload
state tests.

Static BoardView check must find no service, repository, or `AssetDatabase`
reference.

## Verification and commit

Focused Editor/Persistence fixtures, then full EditMode suite.

Commit exactly:

```text
fix: complete draft editor workflow
```
