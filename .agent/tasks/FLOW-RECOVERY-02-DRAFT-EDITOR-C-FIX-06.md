# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-06`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-05`, commit `33eec26`  
**Start commit:** `33eec26`

**Goal:** Fix Draft endpoint JSON round-trip while preserving the existing
nullable endpoint API.

## Background

C-FIX-05 correctly completed the test assertions, but independent Codex
verification failed:

```text
Unity EditMode exit=2
total=362 passed=361 failed=1 skipped=0
Failed: WindowState_RestoresDraftControlsAndBoardAfterCreateGUI
Message: Expected: True But was: False
Line: FlowDraftEditorWorkflowTests.cs:552
```

Line 552 is:

```csharp
Assert.IsTrue(cd2.pairs[0].endpointA.HasValue);
```

Root cause:

- `FlowDraftPairData.endpointA` and `endpointB` are `FlowPos?`.
- Unity `JsonUtility` does not reliably serialize nullable fields.
- C-FIX-04 fixed `FlowDraftConstraintData.colorId`, but pair endpoints still
  disappear during `JsonUtility.ToJson/FromJson`.

Do not weaken the test. Draft window state must restore endpoints.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-C-FIX-05.md`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftPairData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftPairData.cs`
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

Modify `FlowDraftPairData` so Unity `JsonUtility` round-trips endpoint presence
and endpoint coordinates, while preserving existing public API used throughout
Draft code:

```csharp
public FlowPos? endpointA;
public FlowPos? endpointB;
```

Preferred minimal implementation:

1. Keep `endpointA` and `endpointB` as public nullable fields so existing code
   does not need broad edits.
2. Add Unity-serializable backing fields:

   ```csharp
   [SerializeField] private bool hasEndpointA;
   [SerializeField] private FlowPos endpointAValue;
   [SerializeField] private bool hasEndpointB;
   [SerializeField] private FlowPos endpointBValue;
   ```

3. Implement `UnityEngine.ISerializationCallbackReceiver`:

   ```csharp
   public void OnBeforeSerialize()
   {
       hasEndpointA = endpointA.HasValue;
       if (hasEndpointA) endpointAValue = endpointA.Value;
       hasEndpointB = endpointB.HasValue;
       if (hasEndpointB) endpointBValue = endpointB.Value;
   }

   public void OnAfterDeserialize()
   {
       endpointA = hasEndpointA ? endpointAValue : (FlowPos?)null;
       endpointB = hasEndpointB ? endpointBValue : (FlowPos?)null;
   }
   ```

4. Add `using UnityEngine;` if needed.
5. Do not change `FlowLevelDraft`, `FlowDraftMapper`, `FlowLevelGeneratorWindow`,
   or `FlowBoardView`.
6. Do not replace the nullable API with a large bool/property rewrite in this
   packet.

If `JsonUtility` does not invoke `ISerializationCallbackReceiver` for this
plain serializable class in the current Unity version, return `BLOCKED` with
evidence rather than inventing a broader persistence system.

## Required tests

Use the existing failing test as the primary regression:

- `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`

It must pass with:

- endpoint A restored to `(0,0)`;
- endpoint B restored to `(3,0)`;
- fixed constraint cells restored;
- BoardView visual kind for `(1,0)` restored as `Constraint(2)`.

Optionally add a small focused assertion in the same test to ensure endpoint
presence survives the JSON restore. Do not add a new test unless necessary.

Do not weaken any C-FIX-05 assertion.

## Non-goals

- No window restore logic changes.
- No BoardView changes.
- No custom JSON system.
- No changes to Draft mapper or mutation semantics.
- No Solver or Completion work.
- No unrelated cleanup, formatting, renaming, or refactoring.

## Constraints

- Keep changes surgical.
- Preserve all accepted A/B/C behavior.
- Do not use:
  - `Assert.Pass`
  - `Count >= 0`
  - `kind >= 0`
  - `history state may vary`
  - `Solution(1)`

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | Preserve `FlowPos? endpointA/B` public fields |
| Production code changes | `LIMITED` | `FlowDraftPairData.cs` only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `LIMITED` | Editor Draft pair endpoint state only |
| Unity asset or `.meta` changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `1`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `90 changed lines`

If the task cannot fit this budget, return `BLOCKED`.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: serialize draft endpoints in window state`

Commit only the allowed files after verification passes. Never push, merge,
rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] Only allowed tracked files changed.
- [ ] `FlowDraftPairData` preserves existing public nullable endpoint fields.
- [ ] `FlowDraftPairData` uses Unity-serializable backing fields or an equally
      small proven mechanism for JsonUtility endpoint round-trip.
- [ ] `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI` passes with
      exact endpoint, constraint, and BoardView assertions intact.
- [ ] Full EditMode tests pass with at least `362` tests, `0 failed`,
      `0 skipped/inconclusive`.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] No `Assert.Pass`, `Count >= 0`, `kind >= 0`,
      `history state may vary`, or `Solution(1)` remains in
      `FlowDraftEditorWorkflowTests.cs`.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix06Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCFix06Tests.log'
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
git diff --name-only 33eec26..HEAD
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
