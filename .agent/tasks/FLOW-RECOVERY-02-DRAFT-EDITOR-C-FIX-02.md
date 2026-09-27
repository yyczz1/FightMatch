# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-02`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-01`, commit `d73d954`  
**Start commit:** `d73d954`

**Goal:** Finish C by fixing the actual remaining NoDraft restore failure and
completing the previously missed C test-strengthening requirements.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-01` is not accepted.

Worker report said the remaining failure was:

- `WindowState_ReloadClearsUndoRedoHistory`

Independent Codex verification on commit `d73d954` found a different actual
failure:

```text
Unity exit code: 2
total=361 passed=360 failed=1 inconclusive=0 skipped=0 result=Failed(Child)
Failed: FlowPuzzle.Tests.Editor.FlowDraftEditorWorkflowTests.WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled
Message: Expected: False But was: True
Stack: FlowDraftEditorWorkflowTests.cs:616
```

The same independent run showed:

- `WindowState_ReloadClearsUndoRedoHistory` passed.

Likely root cause for the actual failure:

- `RestoreDraftWindowState()` handles `currentDraft == null` by calling
  `boardView.ClearData()`, but it does not refresh `draftPanel` button state
  from `null` draft + empty history.
- As a result, Save or Save As can remain enabled from prior UI state.

Also, C-FIX-01 did not modify tests, so required test-quality issues remain:

- `kind >= 0` still exists.
- duplicate `Assert.IsNotNull(cd2)` still exists.
- `history state may vary with JSON round-trip` still exists.
- there is still no test proving `OnDisable` captures state.

This packet is intentionally narrow. Do not chase the already-passing history
test unless it fails in your fresh run.

## Current context

- Unity version: `2022.3.18f1`.
- Current commit: `d73d954`.
- Current independent result: `361 total`, `360 passed`, `1 failed`.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-01.md`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Logs/FlowDraftCFix01Review.xml`
- `Logs/FlowDraftCFix01Review.log`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/FlowPuzzle/Editor/UI/**`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/**`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/**`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Generation/**`
- `Assets/Scripts/FlowPuzzle/Validation/**`
- `Assets/Scripts/FlowPuzzle/Difficulty/**`
- `Assets/Scripts/FlowPuzzle/Application/**`
- every test file except
  `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- all `.meta` files

Anything not explicitly allowed to modify is forbidden.

## Required production fix

1. Fix `RestoreDraftWindowState()` so the no-draft branch updates Draft panel
   state from the restored state:

   - `currentDraft == null`
   - board data cleared
   - Save disabled
   - Save As disabled
   - Undo disabled
   - Redo disabled

2. The minimal expected shape is:

   ```csharp
   if (currentDraft != null)
   {
       boardView.SetData(currentDraft);
       draftPanel.UpdateDraftState(currentDraft, commandHistory);
   }
   else
   {
       boardView.ClearData();
       draftPanel.UpdateDraftState(null, commandHistory);
   }
   UpdateButtonStates();
   ```

   Equivalent small code is acceptable.

3. Keep `commandHistory = new FlowEditorCommandHistory()` at restore start if it
   is now passing the history test.

4. Keep `OnDisable()` capture, but make sure it cannot throw if controls are not
   built. If current `CaptureDraftWindowState()` already returns when
   `draftPanel == null`, that is acceptable.

5. Do not modify `FlowDraftPanel`, `FlowBoardView`, Draft Core, Undo commands,
   persistence, or generation code.

## Required test fixes

Modify only `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`.

1. Strengthen `WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled`:
   - Set up a complete draft first and call `draftPanel.UpdateDraftState(...)`
     so Save and Save As are definitely enabled before restore.
   - Then restore an empty `FlowDraftWindowState`.
   - Assert:
     - `currentDraft == null`
     - Save disabled
     - Save As disabled
     - Undo disabled
     - Redo disabled
   - This prevents the test from passing accidentally from default button state.

2. Strengthen `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`:
   - Assert exact restored Draft fields:
     - width
     - height
     - levelId
     - seed
     - color count
     - endpoint coordinates
     - fixed constraint count
     - exact fixed constraint cells
   - Assert BoardView restored an exact observable cell state.
     Use `FlowBoardView.GetDebugCellVisualKind` if needed, but assert an exact
     expected value. Do not use `kind >= 0`.
   - Remove duplicate `Assert.IsNotNull(cd2)`.
   - Remove `history state may vary with JSON round-trip`.
   - Assert Undo and Redo buttons are disabled after restore.

3. Add or strengthen a test proving `OnDisable` captures current state:
   - Create a real window with `CreateGUI`.
   - Change at least Save As name and one of selected tool/color/endpoint.
   - Invoke `OnDisable` via reflection.
   - Call `CreateGUI` again.
   - Assert the changed values restored.

4. Keep all 6 C test names:
   - `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`
   - `WindowState_RestoresLoadedAssetGuidAndAssetField`
   - `WindowState_InvalidAssetGuidDoesNotThrowOrLoadAsset`
   - `WindowState_ReloadClearsUndoRedoHistory`
   - `WindowState_SaveAsNameSurvivesCreateGUI`
   - `WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled`

5. Do not delete A/B tests.
6. Do not weaken existing exact assertions.

## Non-goals

- No Solver.
- No Completion.
- No diagnostics redesign.
- No retry UI.
- No runtime-player state.
- No persistence format changes.
- No saving/restoring Undo/Redo command stacks.
- No new services, repositories, assets, UXML, USS, packages, or dependencies.
- No unrelated refactoring, formatting, renaming, comments, or optimization.

## Constraints

- Preserve all accepted A/B behavior and tests.
- Do not edit any file outside the whitelist.
- Do not use `EditorPrefs`.
- Do not use `OnGUI`, `Task.Run`, threads, QFramework, or solver types.
- If the required behavior cannot fit the whitelist, return `BLOCKED`.

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

**Maximum changed production files:** `1`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `140 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: finish draft window state tests`

Commit only the allowed files after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] `WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled` passes and
      proves Save/SaveAs/Undo/Redo are disabled after restoring no draft.
- [ ] `WindowState_ReloadClearsUndoRedoHistory` still passes.
- [ ] Full EditMode tests pass with at least `361` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `OnDisable` capture is tested.
- [ ] Board restore test uses exact assertions, not `kind >= 0`.
- [ ] All 6 C test names still exist.
- [ ] No `Assert.Pass`, `Count >= 0`, `kind >= 0`, or
      `history state may vary` remains in C tests.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No files outside the whitelist changed.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix02Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix02Tests.log'
```

Expected:

```text
At least 361 total tests, 0 failed, 0 skipped/inconclusive.
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
rg -n "Assert\.Pass|Count\s*>=\s*0|kind\s*>=\s*0|history state may vary" Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
No matches.
```

Run:

```powershell
rg -n "EditorPrefs|OnGUI|Task\.Run|Thread|QFramework|IFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Editor
```

Expected:

```text
No matches.
```

Run:

```powershell
git diff --name-only d73d954..HEAD
git status --short
git diff --check
```

Expected:

```text
Only allowed tracked files changed.
No untracked Temp/FlowPuzzleGenerated artifacts.
No whitespace errors.
```

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ROOT CAUSE:
- <one or two sentences explaining the actual failing assertion and fix>

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
