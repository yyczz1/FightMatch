# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-03`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-02`, commit `0cf982d`  
**Start commit:** `0cf982d`

**Goal:** Fix the two remaining Draft Editor B window tests by correctly
initializing BoardView display data before driving the pointer path.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-02` still fails `2/355`:

- `CreateGUITwice_BoardStrokeCallbackExecutesOnce`
- `EndpointTools_StillUseSingleCellSelectionNotStroke`

Root cause from review:

- Both failing tests directly assign `currentDraft` via reflection.
- They do **not** call `boardView.SetData(currentDraft)` afterward.
- `FlowBoardView` therefore still has `levelData == null`.
- The pointer/debug pointer path correctly emits nothing when no board data is
  loaded.

This is a test setup problem, not a reason to change Draft Core or Window
business behavior.

## Current context

- Unity version: `2022.3.18f1`.
- Current commit: `0cf982d`.
- Last known full EditMode result: `355 total`, `353 passed`, `2 failed`.
- `FlowBoardView` already has debug pointer seams.
- `FlowLevelGeneratorWindow.WireDraftPanel` already subscribes:
  - `boardView.CellSelected += DoEndpointEdit`
  - `boardView.CellStrokeCompleted += DoConstraintStroke`
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-B-FIX-02.md`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowBoardViewGestureTests.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Logs/FLOW-RECOV-BFIX2.xml`

**Files allowed to modify:**

- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/**`
- every test file except
  `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- all `.meta` files

Anything not explicitly allowed to modify is forbidden.

## Required behavior

1. Fix `CreateGUITwice_BoardStrokeCallbackExecutesOnce` by ensuring the current
   `boardView` has display data before driving pointer/debug pointer events.
2. Fix `EndpointTools_StillUseSingleCellSelectionNotStroke` by ensuring the
   current `boardView` has display data before driving pointer/debug pointer
   events.
3. Use the real current `boardView` from the Window after the final `CreateGUI`
   call.
4. Do not call `DoConstraintStroke` directly in
   `CreateGUITwice_BoardStrokeCallbackExecutesOnce`.
5. Do not call `DoEndpointEdit` directly in
   `EndpointTools_StillUseSingleCellSelectionNotStroke`.
6. Do not instantiate `DrawConstraintStrokeCommand` in either test.
7. Do not use `GetRaiseMethod`.
8. Prefer test setup like:

   ```csharp
   curDraftF.SetValue(w, d);
   var bv = ... current boardView ...;
   bv.SetData(d);
   ```

   This mirrors production handlers such as `DoNewDraft`, `DoLoadDraft`,
   `SetCurrentLevel`, Undo, and Redo, which refresh BoardView whenever the Draft
   changes.
9. Keep exact assertions:
   - CreateGUI twice test: pointer stroke creates exactly one constraint; one
     Undo clears it; `CanUndo == false` after that one Undo.
   - Endpoint tools test: one-cell click via BoardView places endpoint for
     selected color; exactly one Undo entry exists; multi-cell stroke while
     endpoint tool is selected creates no constraint.
10. Do not weaken any assertions.
11. Do not edit production code for this fix.

## Non-goals

- No changes to `FlowBoardView`.
- No changes to `FlowLevelGeneratorWindow`.
- No changes to Draft Core or commands.
- No new test files.
- No UI/layout/assets changes.
- No unrelated cleanup, formatting, or refactoring.

## Constraints

- Preserve all 12 B test names.
- Keep BoardView gesture tests exact.
- Do not reintroduce weak assertions such as `Count >= 0`.
- Do not reintroduce `Assert.Pass` placeholders.
- Do not modify `.claude/settings.local.json`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | None |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `NO` | None |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `0`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `40 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `test: initialize board view for gesture wiring tests`

Commit only the allowed test file after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] `CreateGUITwice_BoardStrokeCallbackExecutesOnce` passes via actual
      BoardView pointer/debug pointer path.
- [ ] `EndpointTools_StillUseSingleCellSelectionNotStroke` passes via actual
      BoardView one-cell click path.
- [ ] Neither test calls `DoConstraintStroke` directly.
- [ ] Neither test calls `DoEndpointEdit` directly.
- [ ] Neither test instantiates `DrawConstraintStrokeCommand`.
- [ ] No `Assert.Pass`, `Count >= 0`, or `GetRaiseMethod` patterns are present
      in B tests.
- [ ] Full EditMode tests pass with at least `355` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] Only `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
      changed.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBFix03Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftBFix03Tests.log'
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
No new forbidden usage. Existing reflection setup for older A1 tests may remain,
but the two B tests must not directly invoke DoConstraintStroke or DoEndpointEdit.
```

Run:

```powershell
git diff --name-only 0cf982d..HEAD
git status --short
git diff --check
```

Expected:

```text
Only Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs changed.
No untracked Temp/FlowPuzzleGenerated artifacts.
No whitespace errors.
```

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

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
