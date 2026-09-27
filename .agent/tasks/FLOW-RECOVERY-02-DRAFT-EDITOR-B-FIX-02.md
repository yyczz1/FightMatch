# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-02`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-01`, commit `1baa45f`  
**Start commit:** `1baa45f`

**Goal:** Make Draft Editor B verification real by replacing weak BoardView
gesture assertions and proving duplicate callback behavior through the actual
BoardView stroke event path.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-01` made the full test suite green, but the
verification is still too weak for acceptance:

- `PointerStroke_DownMoveUp_EmitsOrderedUniqueCells` uses
  `Assert.IsTrue(collected.Count >= 0)`, which is always true.
- `PointerStroke_RepeatedSameCell_Deduplicates` uses only `cells.Count <= 3`,
  not an exact ordered sequence.
- `PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells` uses
  `Assert.IsTrue(cells.Count >= 0)`, which is always true.
- `CreateGUITwice_BoardStrokeCallbackExecutesOnce` directly invokes
  `DoConstraintStroke`, so it does not prove the `boardView.CellStrokeCompleted`
  subscription is not duplicated after repeated `CreateGUI`.
- `EndpointTools_StillUseSingleCellSelectionNotStroke` directly invokes
  `DoEndpointEdit`, so it does not prove the `CellSelected` path still works.

This packet is test-quality focused with only minimal production test seams if
needed. Do not redesign BoardView or Window.

## Current context

- Unity version: `2022.3.18f1`.
- Current commit: `1baa45f`.
- Last known full EditMode result: `355/355`, `0 failed`, but verification is
  weak.
- `FlowBoardView` currently has internal `DoPointerDown/Move/Up/Cancel`
  methods, but tests using raw coordinates may not be reliable if `contentRect`
  is zero outside a live panel.
- It is acceptable to add a tiny internal debug content-rect override if needed,
  as long as actual pointer callbacks and tests share the same coordinate-based
  implementation path.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-B.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-01.md`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftEditTool.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/**`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/**`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Persistence/**`
- every test file except the two allowed test files above
- all `.meta` files

Anything not explicitly allowed to modify is forbidden.

## Required behavior

### A. Make BoardView gesture tests exact

1. Replace weak assertions in these tests with exact expectations:
   - `PointerStroke_DownMoveUp_EmitsOrderedUniqueCells`
   - `PointerStroke_RepeatedSameCell_Deduplicates`
   - `PointerStroke_MoveOutsideThenBack_IgnoresOutsideCells`
2. Each of those tests must assert the exact ordered `FlowPos` sequence emitted
   by `CellStrokeCompleted`.
3. `PointerStroke_Cancel_DoesNotEmitCompletedStroke` must assert both:
   - no `CellStrokeCompleted`;
   - no `CellSelected`.
4. `PointerStroke_WithoutData_EmitsNothing` must remain no-emits for both
   events.
5. Do not use assertions that are always true, such as:
   - `Count >= 0`;
   - broad upper bounds without exact sequence;
   - comments as proof.

If coordinate tests are unreliable because `contentRect` is zero, add a minimal
internal test seam in `FlowBoardView`, for example:

```csharp
internal void SetDebugContentRect(Rect rect)
```

Then all pointer logic must call a single internal helper that uses either the
debug rect or `contentRect`. Actual Unity pointer handlers and tests must share
the same coordinate-to-cell path.

### B. Prove duplicate callback behavior through actual BoardView stroke path

6. `CreateGUITwice_BoardStrokeCallbackExecutesOnce` must not call
   `DoConstraintStroke` directly.
7. It must:
   - call `CreateGUI` three times total;
   - configure the current Draft and DrawConstraint tool;
   - drive the current `boardView` through its pointer/debug pointer path;
   - assert exactly one constraint is created;
   - assert one Undo clears the constraint;
   - assert `CanUndo == false` after that one Undo.
8. This test must prove no duplicate `CellStrokeCompleted` subscription remains
   after repeated `CreateGUI`.

### C. Prove endpoint tools through actual CellSelected path

9. `EndpointTools_StillUseSingleCellSelectionNotStroke` must not call
   `DoEndpointEdit` directly.
10. It must:
    - create a Draft with the selected color present;
    - select an endpoint tool;
    - drive a one-cell click through current `boardView` pointer/debug pointer
      path so `CellSelected` fires;
    - assert the endpoint changed and exactly one Undo entry exists;
    - then drive a multi-cell stroke while the endpoint tool is selected;
    - assert no fixed constraint is created by that stroke.

### D. Keep production narrow

11. Do not modify `FlowLevelGeneratorWindow.cs`; the existing
    `DoConstraintStroke` and method-group subscription from `1baa45f` are
    sufficient.
12. Do not modify `FlowLevelDraft` or `DrawConstraintStrokeCommand`.
13. Do not add new files.
14. Do not broaden public API. Any new seam must be `internal`.
15. Preserve `PointerCancelEvent` handling and outside-move no-hover behavior.

## Non-goals

- No feature behavior changes beyond minimal test seams.
- No UI layout UXML/USS changes.
- No solver/completion/retry/diagnostics work.
- No unrelated formatting, comment cleanup, or file reorganization.

## Constraints

- Preserve all accepted tests and all 12 B test names.
- Full suite must remain green.
- Do not use `Assert.Pass` placeholder tests.
- Do not use direct command construction in tests.
- Do not use `GetRaiseMethod` event hacks.
- Do not edit `.claude/settings.local.json`.
- If the required proof cannot be done without modifying forbidden files,
  return `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | Internal test seam only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `NO` | None |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `1`  
**Maximum changed test files:** `2`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `140 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `test: strengthen draft board gesture verification`

Commit only this packet's allowed files after verification passes. Never push,
merge, rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] All 12 B test names still exist.
- [ ] `FlowBoardViewGestureTests.cs` contains no `Assert.Pass`.
- [ ] Gesture tests assert exact emitted `FlowPos` sequences.
- [ ] No gesture assertion uses always-true conditions such as `Count >= 0`.
- [ ] `CreateGUITwice_BoardStrokeCallbackExecutesOnce` uses actual BoardView
      pointer/debug pointer path, not direct `DoConstraintStroke`.
- [ ] `EndpointTools_StillUseSingleCellSelectionNotStroke` uses actual BoardView
      one-cell click path, not direct `DoEndpointEdit`.
- [ ] Tests do not directly instantiate `DrawConstraintStrokeCommand`.
- [ ] Tests do not use `GetRaiseMethod`.
- [ ] Full EditMode tests pass with at least `355` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No files outside the whitelist changed.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBFix02Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBFix02Tests.log'
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
rg -n "Assert\.Pass|Count\s*>=\s*0|new DrawConstraintStrokeCommand|GetRaiseMethod|DoConstraintStroke|DoEndpointEdit" Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
No matches, except references to method names are allowed only if they are
reflection setup outside the two tests that must prove actual BoardView paths.
Prefer no matches.
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
git diff --name-only 1baa45f..HEAD
git status --short
git diff --check
```

Expected:

```text
Only allowed files changed.
No untracked Temp/FlowPuzzleGenerated artifacts.
No whitespace errors.
```

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ARTIFACTS DELETED:
- <path or NONE>

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
