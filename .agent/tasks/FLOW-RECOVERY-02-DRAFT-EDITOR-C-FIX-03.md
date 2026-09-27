# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-03`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-02`, commit `6a06c9e`  
**Start commit:** `6a06c9e`

**Goal:** Close the remaining C review gap by strengthening window-state tests
only. Production code is already passing and must not be touched.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-02` independently verifies green:

```text
Unity EditMode: total=362 passed=362 failed=0 skipped=0
Temp artifacts: absent
Forbidden static patterns: no matches
```

The production fix is acceptable, but the test-strengthening acceptance criteria
from C-FIX-02 were only partially completed. The main board-restore test still
does not assert all exact restored Draft data requested by the packet:

- `levelId`
- `seed`
- endpoint coordinates
- exact fixed-constraint cells
- Redo history/button disabled

This is a test-only cleanup packet. Do not modify production code.

## Current context

- Unity version: `2022.3.18f1`.
- Current commit: `6a06c9e`.
- Current full EditMode result: `362/362`, `0 failed`.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-02.md`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`

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

## Required changes

Modify only `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`.

Add exact assertions for the restored draft:

1. `width == 5`
2. `height == 5`
3. `levelId == 4001`
4. `seed == 42`
5. `colorCount == 1`
6. `pairs.Count == 1`
7. pair `colorId == 0`
8. endpoint A is `(0, 0)`
9. endpoint B is `(3, 0)`
10. `fixedConstraints.Count == 1`
11. fixed constraint `colorId == 0`
12. fixed constraint cell sequence exactly:

    ```text
    (0,0), (1,0), (2,0)
    ```

Add exact assertions for restored history/UI state:

13. `commandHistory.CanUndo == false`
14. `commandHistory.CanRedo == false`
15. `draftPanel.undoBtn.enabledSelf == false`
16. `draftPanel.redoBtn.enabledSelf == false`

Keep the current exact BoardView visual-kind assertion, but make it explicit:

17. `GetDebugCellVisualKind(new FlowPos(1,0))` must equal the enum value for
    `Constraint`, not the integer `1`, because the restored cell is part of the
    fixed constraint overlay and constraint should take precedence over solution.

Use the enum by reflection or a cast if accessible from the test assembly. Do
not change `FlowBoardView` visibility.

## Non-goals

- No production changes.
- No new tests unless you need a tiny helper assertion method.
- No rename/reformat of unrelated tests.
- No changes to behavior.
- No assets, `.meta`, package, project setting, or dependency changes.

## Constraints

- Do not weaken any existing assertion.
- Do not remove any C test.
- Do not use:
  - `Assert.Pass`
  - `Count >= 0`
  - `kind >= 0`
  - `history state may vary`
- If exact assertions require more than this one test method, keep changes
  local and small.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | None |
| Production code changes | `NO` | None |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `0`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `45 changed lines`

If the task cannot fit this budget, return `BLOCKED`.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `test: strengthen draft state restore assertions`

Commit only the allowed test file after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] Only `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
      changed.
- [ ] `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI` asserts all
      exact draft fields listed above.
- [ ] The same test asserts Undo and Redo history/buttons are disabled.
- [ ] The same test asserts exact BoardView visual kind for the constraint cell.
- [ ] Full EditMode tests pass with at least `362` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No `Assert.Pass`, `Count >= 0`, `kind >= 0`, or
      `history state may vary` remains in the test file.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix03Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix03Tests.log'
```

Expected:

```text
At least 362 total tests, 0 failed, 0 skipped/inconclusive.
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
git diff --name-only 6a06c9e..HEAD
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
