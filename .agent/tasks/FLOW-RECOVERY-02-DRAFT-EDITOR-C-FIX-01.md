# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-01`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C`, commit `9f01f9d`  
**Start commit:** `9f01f9d`

**Goal:** Finish Draft editor window-state restore by fixing the remaining
Undo/Redo history failure and replacing weak C tests with exact assertions.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-C` is not accepted. It changed only whitelisted
files, but full EditMode verification still reports:

- `360/361`, `1 failed`
- failing test: `WindowState_ReloadClearsUndoRedoHistory`

Review also found weak C-test assertions that would allow broken behavior to
pass:

- `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI` uses
  `Assert.IsTrue(kind >= 0)` instead of proving the restored cell has the exact
  constraint/filled visual state.
- The same test repeats `Assert.IsNotNull(cd2)` and includes the comment
  `history state may vary with JSON round-trip`; history state must not vary.
- C implementation does not yet capture state from `OnDisable`, even though the
  original C packet required it.
- C implementation does not consistently refresh captured state after Draft
  actions, so later `CreateGUI` may restore stale state.

This fix must be surgical. Do not redesign the Editor window.

## Current context

- Unity version: `2022.3.18f1`.
- Current commit: `9f01f9d`.
- Last reported result: `360/361`, `1 failed`.
- Accepted baseline before C: `0974782`, `355/355` passed.
- C added:
  - `FlowLevelGeneratorWindow.CaptureDraftWindowState()`
  - `FlowLevelGeneratorWindow.RestoreDraftWindowState()`
  - `[SerializeField] private FlowDraftWindowState draftWindowState`
  - `FlowDraftWindowState.saveAsName`
  - 6 C tests in `FlowDraftEditorWorkflowTests.cs`
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C.md`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftWindowState.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Logs/FlowDraftCTests.xml`
- `Logs/FlowDraftCTests.log`

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
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftWindowState.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
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

## Required investigation

Before changing code, inspect the failing test and the current
`FlowLevelGeneratorWindow.RestoreDraftWindowState()` implementation.

Find the concrete reason `commandHistory.Clear()` is insufficient in the current
implementation or test setup. Likely acceptable fixes include one of these, but
you must verify the actual root cause:

1. Replace the window history with a fresh `new FlowEditorCommandHistory()` on
   restore, then update Draft UI state from that fresh history.
2. Clear history after all restore operations that could trigger callbacks.
3. Fix the test if it is holding an old history reference instead of the
   window's current field.

Do not guess. State the root cause in the returned report.

## Required implementation

1. Make `WindowState_ReloadClearsUndoRedoHistory` pass.
2. After restore/reload:
   - `commandHistory.CanUndo == false`;
   - `commandHistory.CanRedo == false`;
   - `draftPanel.undoBtn.enabledSelf == false`;
   - `draftPanel.redoBtn.enabledSelf == false`.
3. Add `OnDisable()` or equivalent Unity EditorWindow lifecycle capture:
   - It must call `CaptureDraftWindowState()` only when safe.
   - It must not throw when UI controls were never built.
4. Ensure Draft state capture is current after these window actions:
   - `DoNewDraft`
   - `DoLoadDraft`
   - `DoAddColor`
   - `DoRemoveColor`
   - `DoEndpointEdit`
   - `DoConstraintStroke`
   - `DoUndo`
   - `DoRedo`
   - `DoSaveDraft`
   - `DoSaveDraftAs`
   - `SetCurrentLevel`
   - `OnClearPreview`
5. Prefer a tiny helper if it reduces duplication, for example:

   ```csharp
   private void RefreshDraftViews(bool captureState)
   ```

   or:

   ```csharp
   private void RefreshDraftWindowState()
   ```

   But do not broadly rewrite unrelated generation/save/batch logic.
6. Restore invalid or malformed draft JSON without throwing. If this is already
   covered, keep it; otherwise add a focused assertion.
7. Do not change `FlowDraftWindowState` in this fix; `saveAsName` was already
   added by C.

## Required test fixes

Keep all 6 C test names and strengthen them:

1. `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`
   - Assert exact restored Draft fields:
     - width
     - height
     - levelId
     - seed
     - color count
     - endpoint coordinates
     - fixed constraint count
     - fixed constraint cells
   - Assert BoardView restored exact observable display state. Use the existing
     debug seam, but assert an exact expected visual kind/value, not `>= 0`.
   - Remove duplicate `Assert.IsNotNull(cd2)`.
   - Remove any comment or assertion implying history may vary.
   - Assert Undo/Redo disabled after restore.
2. `WindowState_ReloadClearsUndoRedoHistory`
   - Must prove there was history before capture.
   - Must prove both history and buttons are clear after `CreateGUI` restore.
3. Add or strengthen a test proving `OnDisable` captures current state:
   - Change selected tool/color/endpoint or Save As name.
   - Invoke `OnDisable` via reflection if needed.
   - Rebuild with `CreateGUI`.
   - Assert the changed values restored.
4. Existing C tests must not use weak assertions such as:
   - `kind >= 0`
   - `Count >= 0`
   - duplicate not-null checks as the main proof
   - `Assert.Pass`

Do not remove A/B tests. Do not weaken any existing exact tests.

## Non-goals

- No Solver.
- No Completion.
- No retry UI.
- No diagnostics-code redesign.
- No runtime-player state.
- No persistence format changes beyond the already-existing
  `FlowDraftWindowState.saveAsName`.
- No saving/restoring Undo/Redo command stacks.
- No new services, repositories, assets, UXML, USS, packages, or dependencies.
- No unrelated refactoring, formatting, renaming, comments, or optimization.

## Constraints

- Preserve all accepted A/B behavior and tests.
- Do not edit Draft Core, Undo command classes, BoardView, DraftPanel, or
  persistence.
- Do not use `EditorPrefs`.
- Do not use `OnGUI`, `Task.Run`, threads, QFramework, or solver types.
- If the required behavior cannot fit the whitelist, return `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | Internal test helpers only if already present |
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
**Approximate maximum diff:** `180 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: complete draft window state restore`

Commit only the allowed files after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] `WindowState_ReloadClearsUndoRedoHistory` passes.
- [ ] Full EditMode tests pass with at least `361` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] Restore clears command history and disables Undo/Redo buttons.
- [ ] `OnDisable` captures current Draft window state and does not throw when
      UI is not initialized.
- [ ] State capture is refreshed after all listed Draft-affecting actions.
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
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix01Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix01Tests.log'
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
git diff --name-only 9f01f9d..HEAD
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
- <one or two sentences explaining why history survived or why the test failed>

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
