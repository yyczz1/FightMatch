# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A2-FIX-01`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A2`, commit `620060e`  
**Start commit:** `620060e`

**Goal:** Finish A2 by fixing the failing generated-handoff Save As test,
removing test asset pollution, and completing the missing Save/Save As state
refresh.

## Background

`FLOW-RECOVERY-02-DRAFT-EDITOR-A2` is close but not acceptable:

- EditMode result was `342/343`, with
  `GeneratedLevel_BecomesSourceLessEditableDraftAndCanSaveAs` failing.
- The failing test mutates the active Draft endpoint to `(9, 9)` and then calls
  Save As. On a `5x5` board this is correctly rejected as
  `EndpointOutOfBounds`; the test is invalid.
- The run left untracked assets under `Assets/FlowPuzzleGenerated/`, meaning at
  least one test used the UI default output folder instead of `TestFolder`.
- `DoSaveDraft` and `DoSaveDraftAs` do not refresh Draft button/state after a
  successful save, even though A2 required that.

This is a surgical fix packet. Do not redesign Mapper, Repository, Validator,
or persistence.

## Current context

- Unity version: `2022.3.18f1`.
- Current partial A2 commit: `620060e`.
- Accepted A1 baseline before A2: `bb5adc1`.
- `FlowDraftMapper.FromGeneratedLevel` already deep-copies generated data into
  Draft.
- `FlowLevelAssetRepository` correctly rejects out-of-bounds endpoints.
- A generated Draft must remain source-less and saveable without mutating the
  original generated level.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-A2.md`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

- `NONE`

**Files allowed to delete as untracked test artifacts only:**

- `Assets/FlowPuzzleGenerated/`
- `Assets/FlowPuzzleGenerated.meta`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/**`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- every test file except
  `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- all `.meta` files, except deleting untracked
  `Assets/FlowPuzzleGenerated.meta` if it exists

Anything not explicitly allowed to modify, create, or delete as a test artifact
is forbidden.

## Required behavior

### A. Fix generated-handoff test without weakening it

1. Fix `GeneratedLevel_BecomesSourceLessEditableDraftAndCanSaveAs` so it does
   not save an intentionally invalid Draft.
2. Keep the deep-ownership assertion, but do it without leaving the Draft
   invalid before Save As. Acceptable approaches:
   - mutate the Draft, assert the generated source is unchanged, then restore
     the Draft endpoint before Save As; and separately mutate the generated
     source, assert the Draft is unchanged; or
   - create a second generated level/draft pair dedicated to deep-ownership
     checks, while the Save As path uses a valid draft.
3. The test must still prove:
   - `SetCurrentLevel` makes `loadedAsset == null`;
   - command history is cleared;
   - Draft has valid width/height/endpoints/solution/difficulty;
   - Draft is `isSolutionDirty == false` and `isValidated == true`;
   - mutating Draft data does not mutate the original generated result;
   - mutating original generated data after handoff does not mutate the Draft;
   - Save As creates exactly one asset under `TestFolder`.

### B. Prevent asset pollution

4. No A2 test may write to the UI default output folder
   `Assets/FlowPuzzleGenerated/Levels`.
5. Any test invoking `DoSaveDraft`, `DoSaveDraftAs`, or a clickable that calls
   Save As must set `PP(w).outputFolderField.value = TestFolder` before the
   action.
6. Remove any existing untracked test artifacts under:
   - `Assets/FlowPuzzleGenerated/`
   - `Assets/FlowPuzzleGenerated.meta`
7. After the full EditMode run, these must be absent:
   - `Assets/Temp/`
   - `Assets/Temp.meta`
   - `Assets/FlowPuzzleGenerated/`
   - `Assets/FlowPuzzleGenerated.meta`

### C. Complete Save/Save As state refresh

8. After successful `DoSaveDraft`, call
   `draftPanel.UpdateDraftState(currentDraft, commandHistory)` and
   `UpdateButtonStates()`.
9. After successful `DoSaveDraftAs`, call
   `draftPanel.UpdateDraftState(currentDraft, commandHistory)` and
   `UpdateButtonStates()`.
10. Do not add solver, validation, mapper, repository, or persistence changes.

### D. Strengthen weak A2 assertions

11. In `CreateGUITwice_DraftSaveAsCallbackExecutesOnce`, assert the first click
    leaves a visible Info diagnostic, then the second click leaves a visible
    Error diagnostic for the duplicate path, and the asset count remains one.
12. Keep using the actual `saveAsBtn.clickable` delegate via reflection in that
    duplicate-callback test. Do not call `DoSaveDraftAs` directly in that test.
13. All nine A2 tests from the original packet must still exist with the exact
    names.

## Non-goals

- Do not modify `FlowDraftMapper`.
- Do not modify `FlowLevelAssetRepository`.
- Do not modify `FlowDraftPanel`.
- Do not change validation semantics.
- Do not add new tests outside `FlowDraftEditorWorkflowTests.cs`.
- Do not add cleanup code that hides production-created assets; fix the tests
  so they write only to `TestFolder`.
- No unrelated refactoring, formatting, renaming, comments, or optimization.

## Constraints

- Preserve existing A1 and A2 behavior outside this fix.
- Keep production diff tiny and localized to Save/Save As success paths.
- Do not add `catch (Exception)` or broaden exception handling.
- Do not change public APIs.
- Do not commit untracked task packet files or user-managed Claude settings.
- If the fix appears to require forbidden files, return `BLOCKED`.

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
| Unity asset or `.meta` changes | `NO` | Only delete untracked `Assets/FlowPuzzleGenerated*` test artifacts |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `1`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `120 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: finish draft save handoff verification`

Commit only the two allowed source/test files if verification passes. Do not
commit `.agent/**`, `.claude/settings.local.json`, `Assets/FlowPuzzleGenerated*`,
or any other artifact.

## Acceptance criteria

- [ ] `GeneratedLevel_BecomesSourceLessEditableDraftAndCanSaveAs` passes for a
      valid generated Draft and still proves deep ownership both directions.
- [ ] `CreateGUITwice_DraftSaveAsCallbackExecutesOnce` proves first click Info,
      second click Error, and exactly one asset under `TestFolder`.
- [ ] All nine original A2 test names still exist.
- [ ] No test writes to `Assets/FlowPuzzleGenerated/Levels`.
- [ ] `DoSaveDraft` refreshes Draft state and normal button state after success.
- [ ] `DoSaveDraftAs` refreshes Draft state and normal button state after
      success.
- [ ] Full EditMode tests pass with at least `343/343`, `0 failed`, `0 skipped`
      or inconclusive.
- [ ] `Assets/Temp/`, `Assets/Temp.meta`, `Assets/FlowPuzzleGenerated/`, and
      `Assets/FlowPuzzleGenerated.meta` are absent immediately after tests.
- [ ] `git status --short` shows only allowed tracked code/test changes plus
      user-managed `.claude/settings.local.json` and untracked `.agent/tasks`
      files, if they already existed before this packet.
- [ ] No files outside this packet's whitelist changed.

## Verification steps

Run:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftA2Fix01Tests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftA2Fix01Tests.log'
```

Expected:

```text
At least 343 tests, 0 failed, 0 skipped/inconclusive.
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
rg -n "GeneratedLevel_BecomesSourceLessEditableDraftAndCanSaveAs|CreateGUITwice_DraftSaveAsCallbackExecutesOnce|DraftSaveButtons_RequireCompleteSolutionDifficultyCleanAndValidated|DraftSaveHandlers_InvalidStatesCreateNoAssetAndShowDiagnostic|SaveDraft_SourceLessCreatesAndTracksAsset|SaveDraft_LoadedOverwritesSameAssetWithoutCreatingAnother|SaveAs_SourceLessCreatesNamedAssetAndTracksIt|SaveAs_LoadedCreatesNewAssetAndPreservesSource|GenerateOne_AfterLoadedDraftClearsSourceHistoryAndRefreshesDraft" Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
All nine test names are present.
```

Run:

```powershell
rg -n "FlowPuzzleGenerated|catch\s*\(\s*Exception\s+ex\s*\)" Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
No test path writes to FlowPuzzleGenerated. No broad catch(Exception) was added.
Filtered catches may remain only if already using `when` for expected exceptions.
```

Run:

```powershell
git diff --name-only 620060e..HEAD
git status --short
git diff --check
```

Expected:

```text
Only allowed tracked files changed.
No whitespace errors except Unity-generated .meta trailing whitespace if any
pre-existing/generated .meta appears, but this packet should not create .meta.
No untracked Assets/FlowPuzzleGenerated* artifacts.
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
