# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A2`, commit `91882ae`  
**Start commit:** `91882ae`

**Goal:** Complete manual Board editing by adding BoardView pointer-stroke
gestures, distinct constraint rendering, and Window wiring for DrawConstraint
and EraseConstraint.

## Background

Draft Core and Draft Editor A are accepted. Existing state:

- `FlowLevelDraft` already has atomic constraint APIs:
  `ApplyConstraint`, `EraseConstraint`, and `ValidateConstraint`.
- `DrawConstraintStrokeCommand` already supports draw/erase, undo, redo, and
  deterministic snapshots.
- `FlowDraftEditTool` already contains `DrawConstraint` and `EraseConstraint`.
- `FlowDraftPanel.toolField` already lets users select those tools.
- `FlowBoardView` currently only emits single-cell hover/selected events and
  renders endpoints/solution cells. It does not support pointer drag strokes or
  fixed-constraint rendering.
- `FlowLevelGeneratorWindow` currently wires `CellSelected` only to endpoint
  editing.

This packet must connect existing pieces. Do not redesign Draft Core.

## Current context

- Unity version: `2022.3.18f1`.
- Current accepted commit: `91882ae`.
- Full EditMode baseline after A2: `343/343` passed.
- The intended boundary is:
  - `FlowBoardView`: visual display, coordinate mapping, pointer gesture
    tracking, and events.
  - `FlowLevelGeneratorWindow`: interprets selected Draft tool and creates
    editor commands.
  - `FlowLevelDraft` and `DrawConstraintStrokeCommand`: existing mutation and
    undo/redo semantics.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-A2.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-A2-FIX-01.md`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftEditTool.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs.meta`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/**`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/**`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Generation/**`
- `Assets/Scripts/FlowPuzzle/Validation/**`
- `Assets/Scripts/FlowPuzzle/Difficulty/**`
- `Assets/Scripts/FlowPuzzle/Application/**`
- every test file except the allowed test files above
- all `.meta` files except the new test `.meta`

Anything not explicitly allowed to modify or create is forbidden.

## Required RED tests

Add tests before production changes. Use exact names where listed.

### BoardView gesture tests

Create `FlowBoardViewGestureTests.cs` unless a cleaner existing fixture can hold
all tests without bloating it.

Required exact test names:

1. `PointerStroke_DownMoveUp_EmitsOrderedUniqueCells`
2. `PointerStroke_RepeatedSameCell_Deduplicates`
3. `PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells`
4. `PointerStroke_Cancel_DoesNotEmitCompletedStroke`
5. `PointerStroke_WithoutData_EmitsNothing`
6. `SetData_DraftDeepCopiesConstraintsAndSolutions`
7. `RenderSnapshot_DistinguishesSolutionCellsAndConstraintCells`

The gesture tests may use internal test seams if Unity event pooling makes
direct pointer dispatch unreliable in EditMode. If a seam is added, it must be
`internal`, minimal, and still exercise the same code path as
`PointerDownEvent`, `PointerMoveEvent`, `PointerUpEvent`, and
`PointerCancelEvent`.

### Window wiring tests

Add to `FlowDraftEditorWorkflowTests.cs`.

Required exact test names:

8. `DrawConstraintStroke_WindowActionCreatesConstraintAndUndoEntry`
9. `EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry`
10. `DrawConstraint_InvalidStrokeShowsDiagnosticAndNoHistory`
11. `CreateGUITwice_BoardStrokeCallbackExecutesOnce`
12. `EndpointTools_StillUseSingleCellSelectionNotStroke`

Run the affected fixtures before implementation and report the failing test
names. If any test passes before implementation, strengthen it so it proves the
missing observable behavior.

## Required implementation

### A. BoardView gesture model

1. `FlowBoardView` must support a complete pointer stroke lifecycle:
   - PointerDown starts an active stroke when board data exists and the pointer
     is inside a board cell.
   - PointerMove appends the cell when an active stroke is in progress and the
     pointer is inside the board.
   - PointerUp completes the active stroke and emits the ordered cells.
   - PointerCancel cancels the active stroke and emits no completed stroke.
2. Repeated events in the same cell must not duplicate that cell.
3. Moving outside the board while dragging must not append an invalid cell and
   must not cancel the stroke by itself.
4. A stroke must contain cells in the exact order first encountered.
5. A completed stroke of one valid cell is allowed to emit one cell, but Window
   may reject it for DrawConstraint because Draft Core requires at least two
   cells.
6. When no board data is loaded, pointer events emit nothing.
7. Existing `CellHovered` behavior must remain.
8. Existing endpoint click behavior must remain through `CellSelected` or a
   compatible replacement.
9. Do not let `FlowBoardView` directly mutate `FlowLevelDraft`.

Recommended event shape:

```csharp
public event Action<IReadOnlyList<FlowPos>> CellStrokeCompleted;
```

If using a different event shape, keep it equally small and explain why.

### B. BoardView data ownership and rendering inputs

10. `FlowBoardView.SetData(FlowLevelDraft draft)` must deep-copy:
    - level dimensions and endpoint data;
    - current solution paths;
    - fixed constraint chains.
11. Mutating the source Draft after `SetData` must not change BoardView's
    stored render data.
12. BoardView rendering must visually distinguish:
    - recommendation solution cells;
    - fixed constraint cells;
    - endpoints.
13. Constraint cells should render as path-like strokes or thinner overlays, not
    as full filled solution cells. Use `Painter2D` in `OnGenerateVisualContent`.
14. Draw endpoints after paths/constraints so endpoints remain visible.
15. Do not introduce UI Toolkit `OnGUI`.

Because EditMode tests cannot reliably inspect pixels, add a tiny internal
render snapshot/test seam if needed. It must expose only render classification
data, not production mutation behavior.

Recommended internal test seam:

```csharp
internal enum BoardCellVisualKind { Empty, Solution, Constraint, Endpoint }
internal BoardCellVisualKind GetDebugCellVisualKind(FlowPos cell)
```

If implemented, keep it inside `FlowBoardView` and do not create new production
files.

### C. Window wiring for Draw / Erase

16. `FlowLevelGeneratorWindow` must subscribe to BoardView stroke completion.
17. When `draftPanel.toolField.value == FlowDraftEditTool.DrawConstraint`:
    - use selected color from `selectedColorField`;
    - create `DrawConstraintStrokeCommand(currentDraft, colorId, cells, false)`;
    - execute it through `commandHistory.Execute`;
    - refresh `boardView`, `draftPanel`, normal button states, and diagnostics.
18. When `draftPanel.toolField.value == FlowDraftEditTool.EraseConstraint`:
    - use selected color from `selectedColorField`;
    - create `DrawConstraintStrokeCommand(currentDraft, colorId, cells, true)`;
    - execute it through `commandHistory.Execute`;
    - refresh `boardView`, `draftPanel`, normal button states, and diagnostics.
19. If command execution fails:
    - do not mutate history;
    - show a visible Error diagnostic;
    - leave existing Draft state unchanged.
20. Existing endpoint tools must still use single-cell selection:
    - `PlaceEndpoint`
    - `MoveEndpoint`
    - `RemoveEndpoint`
21. `Select` tool should not mutate the Draft on click or stroke.
22. The same user stroke must create exactly one history entry even after
    repeated `CreateGUI` calls.

### D. UX behavior

23. After a successful Draw or Erase:
    - `currentDraft.isSolutionDirty == true`;
    - `currentDraft.isValidated == false`;
    - Save/Save As disabled until Completion/Validation later makes the Draft
      clean and valid again.
24. Undo/Redo must restore constraint data and refresh the board.
25. Diagnostics should be cleared or replaced on successful command execution.
    Do not leave stale error messages visible after a later successful stroke.

## Test-quality requirements

- Test the actual window handlers or actual BoardView event/callback path; do
  not reproduce command logic in tests.
- For BoardView gestures, assert exact ordered cell sequences.
- For duplicate-callback tests, use actual callbacks/delegates after multiple
  `CreateGUI` calls. Do not directly invoke the final handler in that one test.
- For Draw/Erase window tests, assert:
  - constraint cells;
  - dirty/validated flags;
  - `CanUndo`/`CanRedo`;
  - Undo/Redo restoration;
  - diagnostics visibility/text for failure.
- Avoid weak assertions such as `Count > 0` when exact counts are knowable.
- Do not use global asset searches.
- Keep existing Temp cleanup behavior intact.

## Non-goals

- Do not implement local solver or Completion.
- Do not make constraints part of runtime player data format.
- Do not change `FlowLevelDraft` validation semantics.
- Do not change `DrawConstraintStrokeCommand` unless the task becomes
  impossible without it. If that happens, return `BLOCKED` and explain the exact
  missing behavior.
- Do not add new services, interfaces, repositories, or architecture layers.
- Do not change UI layout UXML/USS unless absolutely required; prefer code-only
  event wiring.
- Do not alter automatic generation, persistence, JSON export, validator,
  difficulty, or batch generation.
- No unrelated refactoring, formatting, renaming, comments, or optimization.

## Constraints

- Preserve all accepted A1/A2 tests and behavior.
- Keep `FlowBoardView` independent of Draft mutation commands.
- Keep all new public surface internal unless Unity UI Toolkit requires public.
- Do not add dependencies.
- Do not use `Task.Run`, threads, async, QFramework, or `OnGUI`.
- Do not edit `.claude/settings.local.json`.
- If the required work cannot fit the whitelist, return `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `LIMITED` | `FlowBoardView` event for completed cell strokes only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `LIMITED` | New test `.meta` only if Unity creates it |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `2`  
**Maximum changed test files:** `3`  
**Maximum new files:** `2` including `.meta`  
**Approximate maximum diff:** `450 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: wire draft board constraint editing`

Commit only this packet's allowed files after verification passes. Never push,
merge, rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] BoardView supports PointerDown/Move/Up/Cancel stroke lifecycle.
- [ ] BoardView emits ordered unique stroke cells and ignores outside cells.
- [ ] PointerCancel emits no completed stroke.
- [ ] Existing hover and single-cell endpoint selection behavior remains.
- [ ] BoardView deep-copies Draft constraints and solutions in `SetData`.
- [ ] BoardView render logic distinguishes solution cells, constraint cells,
      and endpoints.
- [ ] Window DrawConstraint tool creates a constraint through
      `DrawConstraintStrokeCommand` and one undo entry.
- [ ] Window EraseConstraint tool trims/removes a constraint through
      `DrawConstraintStrokeCommand` and one undo entry.
- [ ] Invalid strokes show diagnostics and create no history entry.
- [ ] Repeated `CreateGUI` does not duplicate BoardView stroke callbacks.
- [ ] Endpoint tools still work through single-cell selection, not stroke
      completion.
- [ ] Save/Save As become disabled after Draw/Erase because Draft is dirty and
      unvalidated.
- [ ] Full EditMode tests pass with at least `355` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No files outside the whitelist changed.
- [ ] No unapproved protected change was made.

## Verification steps

Run affected tests first if possible:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBTests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBTests.log'
```

Expected:

```text
At least 355 total tests, 0 failed, 0 skipped/inconclusive.
```

Run:

```powershell
Test-Path 'Assets/Temp'
Test-Path 'Assets/Temp.meta'
Test-Path 'Assets/FlowPuzzleGenerated'
Test-Path 'Assets/FlowPuzzleGenerated.meta'
```

Expected:

```text
False
False
False
False
```

Run:

```powershell
rg -n "OnGUI|Task\.Run|Thread|QFramework|IFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Editor
```

Expected:

```text
No matches.
```

Run:

```powershell
rg -n "PointerStroke_DownMoveUp_EmitsOrderedUniqueCells|PointerStroke_RepeatedSameCell_Deduplicates|PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells|PointerStroke_Cancel_DoesNotEmitCompletedStroke|PointerStroke_WithoutData_EmitsNothing|SetData_DraftDeepCopiesConstraintsAndSolutions|RenderSnapshot_DistinguishesSolutionCellsAndConstraintCells|DrawConstraintStroke_WindowActionCreatesConstraintAndUndoEntry|EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry|DrawConstraint_InvalidStrokeShowsDiagnosticAndNoHistory|CreateGUITwice_BoardStrokeCallbackExecutesOnce|EndpointTools_StillUseSingleCellSelectionNotStroke" Assets/Tests/EditMode/Editor
```

Expected:

```text
All 12 required test names are present.
```

Run:

```powershell
git diff --name-only 91882ae..HEAD
git status --short
git diff --check
```

Expected:

```text
Only allowed tracked files changed.
No whitespace errors except Unity-generated .meta trailing whitespace if any.
No untracked Temp or FlowPuzzleGenerated artifacts.
```

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ARTIFACTS DELETED:
- <path or NONE>

RED:
- <failing tests before implementation, or NOT RUN with reason>

GREEN:
- <test count, failed/skipped count>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <exit code / test count / concise evidence>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO
- Protected changes made: YES/NO

PATCH:
<unified diff preferred; if edits were applied directly, provide a concise diff summary>

LOCAL COMMIT:
- Created: YES/NO
- Hash: <hash or N/A>
- Message: <message or N/A>

BLOCKER:
<required only when STATUS is BLOCKED>
```

Do not include architecture essays or unrelated suggestions.

## What to do if blocked

Return:

```text
BLOCKED
Reason:
Missing information:
Required decision:
Files inspected:
No changes made: Yes/No
```

Do not guess. Do not expand scope.
