# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-EDITOR-WINDOW-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-05`

**Order in group:** `4`

**Depends on:** `FLOW-EDITOR-BOARD-001`

**Goal:** Build the UI Toolkit Editor window shell, stable visual hierarchy,
parameter binding, status panels, and domain-reload-safe UI state.

## Scope whitelist

**Files allowed to read:**

- project rules and Group 5 documents
- accepted Application, preset, board, Persistence and Editor source
- existing tests

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowParameterPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowResultPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBatchReportPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- generated `.meta` files

**Files forbidden to modify:** every existing file.

## Required shell

- Menu: `Tools/Flow Puzzle/Level Generator`.
- `FlowLevelGeneratorWindow : EditorWindow`.
- Use `CreateGUI()`; no main `OnGUI()`.
- Load UXML and USS by stable AssetDatabase paths.
- UXML uses `TwoPaneSplitView` or equivalent, parameter `ScrollView`, preview
  area containing one `FlowBoardView`, and result/status area.
- Panels use UI Toolkit fields and controls, not IMGUI.
- Base fields: level ID, width, height, colors, coverage min/max, path min/max,
  seed/random seed, path attempts, level attempts, batch count, output folder,
  preset, target tier toggle/tier, score-range toggle/min/max.
- Advanced Foldout: turn, interaction, endpoint range, detour range,
  bottleneck, solver timeout/node budget.
- Buttons with stable names:
  `apply-preset`, `generate-one`, `generate-batch`, `save-current`,
  `export-json`, `validate-current`, `clear-preview`.
- Status includes `HelpBox`, `ProgressBar`, difficulty/coverage/seed labels,
  and virtualized `ListView` for batch items.
- Impossible hard ranges disable Generate One/Batch and show a concise HelpBox.

## State and responsibilities

Serialize only window state: config values, selected preset, batch count,
output folder, foldout state, selected asset reference. Re-query controls and
re-register callbacks in every `CreateGUI()`.

This packet creates shell callbacks/events but does not execute generation,
save, export, or batch work. Buttons may emit internal events or call empty
private handlers that only update “not wired” state. The next packet wires
actions.

Panels own presentation/binding only. They do not compose domain dependencies.

## Tests

Instantiate the window, call/create GUI, and assert:

- menu/window type exists;
- required UXML controls and names exist exactly once;
- main window has no `OnGUI`;
- invalid ranges disable generation buttons and valid ranges enable them;
- applying panel values produces a copied `FlowGenerationConfig`;
- repeated `CreateGUI()` does not duplicate callbacks or visual trees;
- state values survive simulated visual tree rebuild;
- batch ListView uses fixed-height virtualization.

Tests must close/destroy created windows.

## Non-goals

- No business action wiring, asset writes, JSON writes, Draft, Solver,
  completion, async, Undo/Redo, runtime UI, or browser preview.

## Verification

Run all EditMode tests and compile. Static:

```powershell
rg -n "void OnGUI\s*\(" Assets/Scripts/FlowPuzzle/Editor
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow puzzle editor window shell`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
