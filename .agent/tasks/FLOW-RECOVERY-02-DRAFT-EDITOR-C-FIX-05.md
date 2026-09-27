# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-05`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-04`, commit `5d23423`  
**Start commit:** `5d23423`

**Goal:** Finish the C-FIX-04 test assertions without touching production code.

## Background

C-FIX-04 fixed the production serialization issue by changing
`FlowDraftConstraintData.colorId` from `int?` to `int`, and reported
`362/362` tests passing.

However, Codex review found the test file still violates C-FIX-04 acceptance
criteria:

1. Forbidden fallback text remains:

   ```csharp
   // If round-trip strips constraint data, fallback: the solution path at (1,0) shows as Solution(1).
   ```

2. `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI` still does not
   assert exact endpoint coordinates.
3. The same test still does not assert exact fixed-constraint cell sequence.
4. The same test contains duplicated `Assert.AreEqual(0, cd2.pairs[0].colorId);`.

This is a test-only cleanup. Production code is already fixed and must not be
modified.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-04.md`
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

1. Remove the forbidden fallback comment mentioning `Solution(1)`.
2. Remove the duplicate `Assert.AreEqual(0, cd2.pairs[0].colorId);`.
3. Add exact endpoint assertions:

   ```csharp
   Assert.IsTrue(cd2.pairs[0].endpointA.HasValue);
   Assert.IsTrue(cd2.pairs[0].endpointB.HasValue);
   Assert.AreEqual(0, cd2.pairs[0].endpointA.Value.x);
   Assert.AreEqual(0, cd2.pairs[0].endpointA.Value.y);
   Assert.AreEqual(3, cd2.pairs[0].endpointB.Value.x);
   Assert.AreEqual(0, cd2.pairs[0].endpointB.Value.y);
   ```

4. Add exact fixed-constraint assertions:

   ```csharp
   Assert.AreEqual(1, cd2.fixedConstraints.Count);
   Assert.AreEqual(0, cd2.fixedConstraints[0].colorId);
   Assert.AreEqual(3, cd2.fixedConstraints[0].cells.Count);
   Assert.AreEqual(new FlowPos(0, 0), cd2.fixedConstraints[0].cells[0]);
   Assert.AreEqual(new FlowPos(1, 0), cd2.fixedConstraints[0].cells[1]);
   Assert.AreEqual(new FlowPos(2, 0), cd2.fixedConstraints[0].cells[2]);
   ```

5. Keep existing exact BoardView assertion:

   ```csharp
   Assert.AreEqual(2, kind, "Constraint cell (1,0) must be Constraint(2) after restore");
   ```

6. Keep existing Undo/Redo history and button disabled assertions.
7. Do not add any fallback acceptance of Solution visual state.

## Non-goals

- No production code changes.
- No new tests.
- No new files.
- No refactoring or formatting unrelated tests.
- No changes to `FlowDraftConstraintData.cs`.

## Constraints

- Keep the diff tiny and localized.
- Do not weaken any assertion.
- Do not use:
  - `Assert.Pass`
  - `Count >= 0`
  - `kind >= 0`
  - `history state may vary`
  - `Solution(1)`

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
**Approximate maximum diff:** `35 changed lines`

If the task cannot fit this budget, return `BLOCKED`.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `test: complete draft state restore assertions`

Commit only the allowed test file after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] Only `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
      changed.
- [ ] Duplicate `colorId` assertion removed.
- [ ] Endpoint A and endpoint B coordinates are asserted exactly.
- [ ] Fixed constraint color and exact cell sequence are asserted.
- [ ] Constraint BoardView visual kind remains asserted as `2`.
- [ ] Full EditMode tests pass with at least `362` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No `Assert.Pass`, `Count >= 0`, `kind >= 0`,
      `history state may vary`, or `Solution(1)` remains in the test file.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix05Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix05Tests.log'
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
rg -n "Assert\.Pass|Count\s*>=\s*0|kind\s*>=\s*0|history state may vary|Solution\(1\)" Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
No matches.
```

Run:

```powershell
git diff --name-only 5d23423..HEAD
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
