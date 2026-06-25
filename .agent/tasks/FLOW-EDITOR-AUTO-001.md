# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-EDITOR-AUTO-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-05`

**Order in group:** `5`

**Depends on:** `FLOW-EDITOR-WINDOW-001`

**Goal:** Wire the complete synchronous automatic-generation workflow into the
UI Toolkit window.

## Scope whitelist

**Files allowed to read:**

- project rules and all Group 5 documents
- all accepted FlowPuzzle source and tests

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowParameterPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowResultPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBatchReportPanel.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

**Files allowed to create:** `NONE`

**Files forbidden to modify:** every other file.

## Required actions

Compose once per window:

- `FlowLevelGenerationService`;
- `FlowLevelAssetRepository`;
- `FlowLevelJsonExporter`.

Implement:

1. Apply Preset: copy selected values to editable controls.
2. Generate One: create in-memory result only; update board, labels, result,
   diagnostics, and button availability.
3. Validate Current: validate current recommendation and display exact result.
4. Save Current Asset: save only a valid current result; refuse invalid/empty;
   use selected output folder.
5. Export Current JSON: export only current complete result and display paths.
6. Generate Batch: generate requested levels and save every success as an
   asset; continue after generation/save failures; report every item.
7. Clear Preview: clear current result, board, diagnostics, and labels.

Rules:

- fixed seed uses entered seed;
- batch uses entered base seed and level IDs through accepted batch service;
- Generate One never writes an asset;
- batch saves successful generated levels;
- no overwrite without an explicit existing-asset action (not present yet);
- no unique-solution or full-board checks;
- no solver;
- synchronous Editor main-thread implementation in this phase;
- catch expected repository/export exceptions and show actionable HelpBox
  messages without swallowing unexpected exceptions broadly;
- disable Save/Export/Validate when no valid current recommendation;
- Clear leaves editable parameters unchanged;
- avoid duplicate callbacks after domain/visual rebuild.

## Tests

Using deterministic small configs and temporary
`Assets/Temp/FlowPuzzleEditorTests`:

- Apply Preset copies values while controls remain editable;
- Generate One creates preview but no asset;
- generated recommendation validates and board receives data;
- Save Current creates one complete asset;
- Export creates level and solution JSON;
- Clear removes current result without changing config;
- invalid config disables generation and does no work;
- batch processes all requested items and saves every success;
- generation/save failure appears in diagnostics/list without stopping later
  items;
- repeated CreateGUI/click executes each action once;
- teardown removes only exact test folders and temp JSON paths.

If direct button clicks are brittle, test private action handlers through
reflection; do not add public APIs solely for tests.

## Manual smoke

After automated tests:

1. Open `Tools/Flow Puzzle/Level Generator`.
2. Apply Normal.
3. Set 6×6, fixed seed 202.
4. Generate One.
5. Validate.
6. Save to `Assets/FlowPuzzleGenerated/Levels`.
7. Export JSON to a chosen temporary folder.
8. Reload saved asset and confirm preview.

Report each step. Do not leave smoke-test assets unless the packet explicitly
names and cleans them.

## Non-goals

- No manual Draft editing, loading/editing assets, Save As UI, overwrite UI,
  solver/completion, async/progress search, Undo/Redo, runtime gameplay, or
  QFramework.

## Verification

Run all EditMode tests and compile. Static:

```powershell
rg -n "IFlowPuzzleSolver|Task\.Run|Thread|void OnGUI\s*\(" `
  Assets/Scripts/FlowPuzzle/Editor
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: wire automatic level generation workflow`

## What to do if blocked

Return `BLOCKED` with exact reason and no commit.
