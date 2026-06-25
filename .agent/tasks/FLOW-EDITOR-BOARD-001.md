# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-EDITOR-BOARD-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-05`

**Order in group:** `3`

**Depends on:** `FLOW-EDITOR-PRESET-001`

**Goal:** Add deterministic board geometry and a single-element UI Toolkit
preview for generated Flow levels.

## Scope whitelist

**Files allowed to read:**

- project rules and Group 5 documents
- Core DTOs and accepted generated data
- current Editor source and tests

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- generated `.meta` files for new folders/files

**Files forbidden to modify:** every existing file.

## Geometry API

Create a static geometry helper with:

```csharp
Rect CalculateBoardRect(Rect contentRect, int width, int height, float padding);
Rect GetCellRect(Rect boardRect, int width, int height, FlowPos cell);
bool TryGetCell(Rect boardRect, int width, int height, Vector2 point, out FlowPos cell);
```

Rules:

- throw for non-positive dimensions and negative padding;
- aspect-fit the board inside content after padding;
- logical `(0,0)` renders at bottom-left while UI Y increases downward;
- cell rectangles exactly tile the board;
- right/bottom maximum edge is outside;
- points outside return false without throwing.

## FlowBoardView

Derive from `VisualElement`.

Required public surface:

```csharp
public event Action<FlowPos> CellHovered;
public event Action<FlowPos> CellSelected;

public void SetData(FlowLevelData level, FlowSolutionData solution);
public void ClearData();
```

Rules:

- use one custom element with `generateVisualContent` and `Painter2D`;
- draw background, empty cells, grid, colored recommendation paths, and larger
  endpoint markers;
- use a deterministic built-in palette for at least 12 colors;
- deep-copy or treat inputs read-only; never mutate them;
- call `MarkDirtyRepaint()` after data/state changes;
- register pointer move/down and emit hover/selection intents only;
- do not call services, save assets, generate, validate, solve, or edit data;
- no child VisualElement per cell;
- unregister callbacks in detach/dispose lifecycle as appropriate.

## Tests

Geometry tests cover aspect fit, padding, cell rectangles, all four corners,
Y-axis inversion, outside points, max-edge rejection, invalid inputs, and
round-trip cell-center mapping.

Static/source checks prove one custom element, Painter2D usage, and no service,
AssetDatabase, solver, or per-cell child creation.

## Non-goals

- No Draft drawing, runtime controls, path editing, animation, zoom/pan,
  context menu, or tooltips.

## Verification

Run all EditMode tests and compile. Static:

```powershell
rg -n "AssetDatabase|FlowLevelGenerationService|IFlowPuzzleSolver|new VisualElement" `
  Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add ui toolkit flow board preview`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
