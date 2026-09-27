# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-01`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B`, commit `8f40c17`  
**Start commit:** `8f40c17`

**Goal:** Finish Draft Editor B by replacing fake gesture/window tests with real
event-path tests, fixing the two failing tests, and ensuring `.meta` tracking.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-B` is not acceptable:

- Unity EditMode result was `353/355`, `2 failed`.
- `FlowBoardViewGestureTests` contains placeholder tests such as
  `Assert.Pass("Stroke lifecycle implemented in BoardView")`. These do not
  verify PointerDown/Move/Up/Cancel behavior.
- Several window tests directly execute `DrawConstraintStrokeCommand` as a
  fallback instead of proving the BoardView → Window wiring.
- `EndpointTools_StillUseSingleCellSelectionNotStroke` fails because it uses a
  one-color draft and then reads `pairs[1]`.
- `EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry` fails because
  its setup attempts to apply a constraint that traverses the second own
  endpoint; Draft Core correctly rejects that constraint, so erase has nothing
  to erase.
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs.meta` exists but
  is untracked. It must be included in the local commit if the test file remains.

Do not claim B is complete until all tests are real and the full suite is green.

## Current context

- Unity version: `2022.3.18f1`.
- Current partial B commit: `8f40c17`.
- Last known full EditMode result: `355 total`, `353 passed`, `2 failed`.
- Failure names:
  - `FlowDraftEditorWorkflowTests.EndpointTools_StillUseSingleCellSelectionNotStroke`
  - `FlowDraftEditorWorkflowTests.EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry`
- `FlowLevelDraft.ApplyConstraint` requires chains to start at the selected
  color's endpoint and rejects traversal through the second own endpoint.
- `DrawConstraintStrokeCommand` is still forbidden to modify for this fix.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-B.md`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftEditTool.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Logs/FLOW-RECOV-B.xml`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

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
- every test file except the two allowed test files above
- all `.meta` files except `FlowBoardViewGestureTests.cs.meta`

Anything not explicitly allowed to modify or create is forbidden.

## Required behavior

### A. Replace fake BoardView gesture tests

1. Remove all placeholder `Assert.Pass(...)` gesture tests.
2. The following tests must exercise real BoardView stroke logic, not comments
   or implementation assumptions:
   - `PointerStroke_DownMoveUp_EmitsOrderedUniqueCells`
   - `PointerStroke_RepeatedSameCell_Deduplicates`
   - `PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells`
   - `PointerStroke_Cancel_DoesNotEmitCompletedStroke`
   - `PointerStroke_WithoutData_EmitsNothing`
3. If Unity pointer event dispatch is unreliable in EditMode, add minimal
   `internal` BoardView test seams. The seams must share the exact same private
   implementation path as registered pointer callbacks.
4. Preferred seam shape:

   ```csharp
   internal void DebugPointerDown(Vector2 localPosition, int pointerId = 1)
   internal void DebugPointerMove(Vector2 localPosition)
   internal void DebugPointerUp(Vector2 localPosition, int pointerId = 1)
   internal void DebugPointerCancel()
   ```

   The actual `PointerDownEvent`, `PointerMoveEvent`, `PointerUpEvent`, and
   `PointerCancelEvent` handlers must delegate into the same logic.
5. Tests must assert exact emitted cell sequences.
6. Tests must assert no event is emitted for cancel or no-data cases.
7. Repeated same-cell moves must not duplicate the cell.
8. Moving outside then back must ignore outside cells and preserve order.

### B. Fix BoardView pointer implementation gaps

9. Register and handle `PointerCancelEvent`, not only
   `PointerCaptureOutEvent`. You may also handle capture-out if useful, but
   PointerCancel must exist.
10. PointerUp should evaluate the PointerUp local position. If the pointer is
    released over a new valid cell not already appended by move, append that
    cell before completing.
11. One-cell pointer click should emit `CellSelected`.
12. Multi-cell pointer stroke should emit `CellStrokeCompleted`.
13. Pointer cancel should clear active stroke state and emit neither
    `CellSelected` nor `CellStrokeCompleted`.
14. Keep `CellHovered` behavior, but do not emit a fake `default` cell on
    outside move. Outside move should simply not call `CellHovered`.

### C. Replace fake Window wiring tests

15. Window tests must prove actual `FlowBoardView.CellStrokeCompleted` wiring.
    Do not directly instantiate and execute `DrawConstraintStrokeCommand` in the
    window wiring tests.
16. Add a minimal `internal` window handler only if needed, for example:

    ```csharp
    internal void DoConstraintStroke(IReadOnlyList<FlowPos> cells)
    ```

    `WireDraftPanel` must subscribe `boardView.CellStrokeCompleted +=
    DoConstraintStroke;`, and tests may call `DoConstraintStroke` only when the
    test name is not specifically about duplicate callback execution.
17. `CreateGUITwice_BoardStrokeCallbackExecutesOnce` must use the actual
    BoardView stroke event/callback path after three `CreateGUI` calls, not a
    direct command or direct `DoConstraintStroke` call. Use BoardView debug
    pointer seams if needed.
18. `DrawConstraintStroke_WindowActionCreatesConstraintAndUndoEntry` must
    exercise the window's real stroke handler/path and assert:
    - exactly one constraint;
    - exact constraint cell sequence;
    - `CanUndo == true`;
    - `isSolutionDirty == true`;
    - `isValidated == false`;
    - Save and Save As buttons disabled.
19. `EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry` must create
    a valid pre-existing constraint that Draft Core accepts. Do not include the
    second own endpoint in the fixed constraint chain. Then exercise the real
    erase stroke path and assert:
    - constraint is trimmed or cleared exactly as expected;
    - exactly one undo entry;
    - dirty/unvalidated flags;
    - Undo restores the previous constraint;
    - Redo reapplies the erase result.
20. `DrawConstraint_InvalidStrokeShowsDiagnosticAndNoHistory` must exercise the
    real window stroke handler/path. It must not directly execute the command.
21. `EndpointTools_StillUseSingleCellSelectionNotStroke` must use a draft with
    the selected color present. If selecting color `1`, create a two-color draft.
    It must prove:
    - single-cell click places/moves/removes endpoint through `CellSelected`;
    - a multi-cell stroke while an endpoint tool is selected does not create a
      constraint;
    - endpoint edit creates exactly one undo entry.

### D. Keep B production behavior narrow

22. `FlowBoardView` must not know about `FlowLevelDraft` mutation commands.
23. `FlowLevelGeneratorWindow` remains responsible for interpreting the current
    tool and executing commands through `commandHistory`.
24. Do not modify `FlowLevelDraft` or `DrawConstraintStrokeCommand`.
25. Do not add solver, completion, async, threading, QFramework, or OnGUI.

### E. Track the new `.meta`

26. Include `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs.meta` in
    the local commit if Unity created it.
27. Do not include unrelated `.meta`, `.agent/**`, or `.claude/**` changes.

## Non-goals

- No new Draft Core behavior.
- No changes to persistence, generation, validation, difficulty, or runtime
  APIs.
- No UI layout UXML/USS changes unless you return `BLOCKED` explaining why
  they are unavoidable.
- No broad refactoring or formatting cleanup.
- No attempt to fix unrelated encoding/mojibake text.

## Constraints

- Preserve accepted A1/A2 behavior.
- Preserve all 12 B test names from the original B packet.
- All B tests must be meaningful; no placeholder `Assert.Pass`, no direct
  command fallback in Window wiring tests.
- Keep changes inside the whitelist.
- If a required fix needs a forbidden file, return `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `LIMITED` | Internal BoardView/window test seams only; existing `CellStrokeCompleted` event may remain public |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `LIMITED` | Track `FlowBoardViewGestureTests.cs.meta` only |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `2`  
**Maximum changed test files:** `2`  
**Maximum new files:** `1` `.meta` only  
**Approximate maximum diff:** `260 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: verify draft board gesture wiring`

Commit only this packet's allowed files after verification passes. Never push,
merge, rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] All 12 B test names still exist.
- [ ] No `Assert.Pass` placeholder remains in `FlowBoardViewGestureTests.cs`.
- [ ] BoardView gesture tests assert exact emitted cell sequences and no-emits.
- [ ] Window wiring tests do not directly execute
      `DrawConstraintStrokeCommand` as a fallback.
- [ ] `EndpointTools_StillUseSingleCellSelectionNotStroke` passes and uses an
      existing selected color.
- [ ] `EraseConstraintStroke_WindowActionTrimsConstraintAndUndoEntry` passes
      using a valid accepted pre-existing constraint.
- [ ] PointerCancel is handled explicitly.
- [ ] Outside pointer move does not emit fake/default hover cells.
- [ ] Full EditMode tests pass with at least `355` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] `FlowBoardViewGestureTests.cs.meta` is tracked if it exists.
- [ ] No files outside the whitelist changed.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBFix01Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBFix01Tests.log'
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
rg -n "Assert\.Pass|new DrawConstraintStrokeCommand|GetRaiseMethod|fallback|Fallback" Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
No placeholder Assert.Pass.
No direct DrawConstraintStrokeCommand in window wiring tests.
No GetRaiseMethod/fallback event hacks.
```

Run:

```powershell
rg -n "PointerCancelEvent|CellStrokeCompleted|DebugPointerDown|DebugPointerMove|DebugPointerUp|DebugPointerCancel|DoConstraintStroke" Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs
```

Expected:

```text
PointerCancel handling exists.
Stroke event and real shared test seams/handler exist.
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
git ls-files --error-unmatch Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs.meta
git diff --name-only 8f40c17..HEAD
git status --short
git diff --check
```

Expected:

```text
The new test .meta is tracked.
Only allowed tracked files changed.
No untracked test .meta remains.
No whitespace errors except Unity-generated .meta trailing whitespace if any.
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
- <failing tests before fix, with messages>

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
