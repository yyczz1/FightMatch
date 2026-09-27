# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-04`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-03`, blocked on commit `6a06c9e` with an uncommitted test change  
**Start commit:** `6a06c9e`

**Goal:** Fix Draft constraint JSON round-trip by making
`FlowDraftConstraintData` Unity-serializable, then finish the C-FIX-03 test.

## Background

C-FIX-03 correctly strengthened
`WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`, but the strengthened
assertion exposed a real production bug:

```text
Assert.AreEqual(2, kind, "Constraint cell (1,0) must be Constraint(2)")
Expected: 2
But was: 1
```

`FlowBoardView.GetDebugCellVisualKind(new FlowPos(1,0))` returns:

- `1` = `Solution`
- `2` = `Constraint`

Cell `(1,0)` is in both the solution path and the fixed constraint. Since
`FlowBoardView` gives constraints priority over solution cells, the expected
restored visual kind is `Constraint(2)`. Returning `Solution(1)` means
`fixedConstraints` did not survive `JsonUtility.ToJson/FromJson`.

Codex review found the likely root cause:

- `FlowDraftConstraintData.colorId` is `int?`.
- Unity `JsonUtility` does not serialize nullable fields reliably.
- `FlowPos` is `[Serializable]` with plain `int x/y`, and `List<FlowPos>` is
  not the suspicious part.

Do not work around this inside `FlowLevelGeneratorWindow` or `FlowBoardView`.
Fix the data object so Draft state round-trips correctly.

## Important current workspace note

Although the previous worker response said `No changes made`, the working tree
currently has an uncommitted modification in:

```text
Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

That modification contains the C-FIX-03 strengthened test. Do **not** revert it.
Use it as the failing regression test for this packet.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-03.md`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowConstraintEditingTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
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
- `Assets/Scripts/FlowPuzzle/Editor/UI/**`
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

## Required implementation

1. Change `FlowDraftConstraintData` so Unity `JsonUtility` can serialize and
   deserialize valid constraints.
2. Preferred minimal implementation:

   ```csharp
   public int colorId;
   public List<FlowPos> cells;

   public FlowDraftConstraintData Clone()
   {
       var c = new FlowDraftConstraintData { colorId = colorId, cells = new List<FlowPos>() };
       if (cells != null)
           foreach (var cell in cells)
               c.cells.Add(new FlowPos(cell.x, cell.y));
       return c;
   }
   ```

3. Do not introduce custom JSON, wrapper DTOs, `ISerializationCallbackReceiver`,
   or special BoardView reconstruction unless the preferred minimal fix cannot
   compile.
4. Do not modify `FlowLevelGeneratorWindow` for this fix.
5. Do not modify `FlowBoardView` for this fix.
6. Do not preserve nullable `colorId` unless you can prove JsonUtility
   round-trips it and all tests pass. Current evidence says it does not.
7. Make sure all existing code still compiles after changing `colorId` from
   `int?` to `int`.

## Required tests

Use the existing uncommitted C-FIX-03 test assertion as the primary regression:

```csharp
Assert.AreEqual(2, kind, "Constraint cell (1,0) must be Constraint(2) after restore");
```

Also ensure `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI` asserts:

- exact levelId and seed;
- exact endpoint coordinates;
- exact fixed constraint cell sequence;
- Undo and Redo history/buttons disabled;
- exact BoardView visual kind for `(1,0)` is `Constraint(2)`.

If the exact fixed-constraint cell assertions are not already present in the
working tree, add them.

Do not add weak fallbacks such as accepting `Solution(1)`.

## Non-goals

- No changes to window restore logic.
- No changes to BoardView.
- No custom persistence system.
- No runtime-player changes.
- No Solver or Completion work.
- No unrelated cleanup, formatting, renaming, or refactoring.

## Constraints

- Keep changes surgical.
- Preserve all accepted A/B/C behavior.
- Do not weaken any test.
- Do not use:
  - `Assert.Pass`
  - `Count >= 0`
  - `kind >= 0`
  - `history state may vary`
  - fallback acceptance of `Solution(1)` for the constraint cell

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `LIMITED` | `FlowDraftConstraintData.colorId` nullable-to-int only |
| Production code changes | `LIMITED` | `FlowDraftConstraintData.cs` only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `LIMITED` | Editor Draft constraint state only |
| Unity asset or `.meta` changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `1`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `70 changed lines`

If the task cannot fit this budget, return `BLOCKED`.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: serialize draft constraints in window state`

Commit only the allowed files after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] Only these tracked files changed:
  - `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
  - `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- [ ] `FlowDraftConstraintData` no longer uses nullable `int?` for `colorId`,
      unless you provide a passing, explicit JsonUtility round-trip proof.
- [ ] Draft fixed constraints survive `JsonUtility.ToJson/FromJson`.
- [ ] `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI` asserts exact
      fixed-constraint cells and exact `Constraint(2)` BoardView visual kind.
- [ ] Full EditMode tests pass with at least `362` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No `Assert.Pass`, `Count >= 0`, `kind >= 0`,
      `history state may vary`, or fallback acceptance of `Solution(1)` remains
      in `FlowDraftEditorWorkflowTests.cs`.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix04Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix04Tests.log'
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
rg -n "int\?|Assert\.Pass|Count\s*>=\s*0|kind\s*>=\s*0|history state may vary|Solution\(1\)" Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
No forbidden matches. If `int?` appears outside FlowDraftConstraintData context,
explain why.
```

Run:

```powershell
git diff --name-only 6a06c9e..HEAD
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
- <one or two sentences>

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
