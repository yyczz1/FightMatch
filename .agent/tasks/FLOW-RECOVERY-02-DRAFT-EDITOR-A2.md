# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A2`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A1`, commit `bb5adc1`  
**Start commit:** `bb5adc1`

**Goal:** Complete and verify Draft Save, Save As, and automatic-generation
handoff into a source-less editable Draft.

## Background

Draft Core and the A1 editing actions are accepted. The window currently has
untested anonymous Save/Save As callbacks, source-less Save does not retain the
created asset, and `SetCurrentLevel` does not fully reset and refresh Draft
state after automatic generation.

This packet closes only those persistence and generated-Draft handoff gaps.
Board strokes, reload state, Solver, and Completion are separate packets.

## Current context

- Unity version: `2022.3.18f1`.
- Accepted baseline: `bb5adc1`.
- Full EditMode baseline: `334/334` passed.
- `FlowDraftMapper` already owns deep conversion between generated levels,
  assets, and Drafts.
- `FlowLevelAssetRepository` already implements `SaveNew`, `Overwrite`, and
  both loaded/source-less `SaveAs` overloads.
- A1 established internal window action handlers as the test seam.
- `FlowDraftEditorWorkflowTests` owns safe `Assets/Temp` cleanup and must leave
  both `Assets/Temp/` and `Assets/Temp.meta` absent.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Scripts/FlowPuzzle/Persistence/FlowLevelAsset.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`
- `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/**`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/**`
- `Assets/Scripts/FlowPuzzle/Persistence/**`
- every test file except
  `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- all `.meta` files

Anything not explicitly allowed to modify is forbidden.

## Required RED tests

Add these exact tests before changing production code:

1. `DraftSaveButtons_RequireCompleteSolutionDifficultyCleanAndValidated`
2. `DraftSaveHandlers_InvalidStatesCreateNoAssetAndShowDiagnostic`
3. `SaveDraft_SourceLessCreatesAndTracksAsset`
4. `SaveDraft_LoadedOverwritesSameAssetWithoutCreatingAnother`
5. `SaveAs_SourceLessCreatesNamedAssetAndTracksIt`
6. `SaveAs_LoadedCreatesNewAssetAndPreservesSource`
7. `GeneratedLevel_BecomesSourceLessEditableDraftAndCanSaveAs`
8. `GenerateOne_AfterLoadedDraftClearsSourceHistoryAndRefreshesDraft`
9. `CreateGUITwice_DraftSaveAsCallbackExecutesOnce`

Run the fixture before implementation and report the exact failing test names.
Do not weaken or omit a test because existing partial behavior makes it pass.
If a test passes before implementation, strengthen it to prove the missing
observable behavior through the real window action.

## Required implementation

### A. Save eligibility

1. Draft Save and Save As are enabled only when all conditions are true:
   - Draft exists;
   - every color has both endpoints;
   - `currentSolution` is non-null;
   - `currentDifficulty` is non-null;
   - `isSolutionDirty == false`;
   - `isValidated == true`.
2. `FlowDraftPanel.UpdateDraftState` must include all six conditions.
3. The handlers must enforce the same conditions independently; disabled UI is
   not a substitute for handler validation.
4. Invalid Save/Save As:
   - creates or overwrites no asset;
   - does not change `loadedAsset`;
   - shows a visible Error diagnostic that identifies why the Draft cannot be
     saved.
5. Do not add validation or solving behavior. A dirty or incomplete Draft
   remains unsavable until a later Completion packet supplies valid data.

### B. Save

6. Extract the current Save callback into one small internal handler named
   `DoSaveDraft`, and wire `saveBtn.clicked` to that method.
7. Convert the Draft using `FlowDraftMapper.ToGeneratedLevel`; do not duplicate
   mapping logic in the window.
8. When `loadedAsset == null`:
   - call `repository.SaveNew`;
   - retain the returned asset in `loadedAsset`;
   - update `draftPanel.assetField` to that asset without invoking Load.
9. When `loadedAsset != null`:
   - call `repository.Overwrite`;
   - preserve the existing asset path and object identity;
   - create no additional `FlowLevelAsset`.
10. Successful Save shows an Info diagnostic and refreshes Draft button state.
11. Failed repository operations keep the previous source/state and show an
    Error diagnostic. Catch only the existing expected argument/operation
    exceptions; do not add `catch (Exception)`.

### C. Save As

12. Extract the current Save As callback into one small internal handler named
    `DoSaveDraftAs`, and wire `saveAsBtn.clicked` to that method.
13. Use `saveAsNameField`; when blank, use `Level_{levelId}`.
14. Source-less or generated Draft:
    - call the source-less `repository.SaveAs` overload.
15. Loaded Draft:
    - call the overload that receives the current `loadedAsset`;
    - leave the source asset and its path/data unchanged.
16. On success:
    - set `loadedAsset` to the newly created asset;
    - update `draftPanel.assetField` without invoking Load;
    - show an Info diagnostic;
    - refresh Draft button state.
17. On failure:
    - keep the prior `loadedAsset`;
    - do not mutate the source asset;
    - show an Error diagnostic.

### D. Generated result becomes a Draft

18. `SetCurrentLevel` remains the single handoff used by `OnGenerateOne`.
19. For a non-null generated result it must:
    - assign `currentLevel`;
    - create `currentDraft` only through
      `FlowDraftMapper.FromGeneratedLevel`;
    - set `loadedAsset = null`;
    - clear `draftPanel.assetField` without invoking Load;
    - clear command history;
    - refresh BoardView, Result panel, Draft panel, Draft Save state, and normal
      window button state.
20. The generated Draft must deeply own:
    - pairs and endpoint coordinates;
    - solution paths and cell coordinates;
    - difficulty report;
    - seed and coverage.
21. It must remain:
    - `isSolutionDirty == false`;
    - `isValidated == true`;
    - immediately eligible for Save and Save As.
22. Mutating the Draft after handoff must not mutate the original generated
    result.
23. Generate One itself must not create an asset. It produces a source-less
    Draft even when a loaded Draft existed immediately before generation.

## Test-quality requirements

- Use a real `FlowLevelGeneratorWindow` created with
  `ScriptableObject.CreateInstance` and `CreateGUI`.
- Invoke `DoSaveDraft`, `DoSaveDraftAs`, `SetCurrentLevel`, or
  `OnGenerateOne`; do not reproduce their logic inside tests.
- Persistence tests must inspect exact asset paths, object identity, complete
  nested values, source immutability, and asset counts inside `TestFolder`.
- For loaded overwrite, snapshot all relevant source fields before the action
  and prove only the intended asset is updated.
- For loaded Save As, prove the old source is byte-for-byte/value-for-value
  unchanged and the new asset contains the Draft data.
- For generated handoff, assert deep ownership in both directions: mutating the
  Draft does not mutate the generated source, and source mutation does not
  mutate the Draft.
- The duplicate-callback test must call the actual current
  `saveAsBtn.clickable` delegate after three `CreateGUI` calls using the
  accepted synchronous `Clickable.Invoke(EventBase)` reflection helper.
  Calling `DoSaveDraftAs` directly in that one test is forbidden.
- The duplicate-callback test must detect a second invocation: one click
  creates one asset and leaves an Info diagnostic. A duplicate second Save As
  would hit the existing path and leave an Error diagnostic.
- Do not use global asset searches. Restrict `AssetDatabase.FindAssets` to
  `TestFolder`.
- Keep the accepted fixture cleanup. Do not manually delete Temp after tests to
  fabricate a clean result.

## Non-goals

- Board pointer down/move/up/cancel.
- Constraint rendering, Draw, or Erase.
- Domain reload or serialized window-state restoration.
- Solver, Completion, Diagnostics catalog, Retry, or model-tool work.
- Changes to automatic batch generation, JSON export, validator, difficulty,
  mapper, repository, persistence formats, or runtime APIs.
- New services, interfaces, controllers, repositories, abstractions, or
  dependencies.
- Unrelated refactoring, formatting, renaming, comments, or optimization.

## Constraints

- Preserve all accepted A1 behavior and all existing automatic-generation
  actions.
- Match existing formatting; keep the window diff localized to Save handlers,
  callback delegation, and `SetCurrentLevel`.
- Do not compress or reorder unrelated methods.
- Do not modify public APIs.
- If the required behavior cannot fit the whitelist, return `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | Internal handlers only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `NO` | Temporary test assets must be cleaned |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `2`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `450 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: complete draft save and generated handoff`

Commit only the three whitelisted files that actually changed. Never include
`.claude/settings.local.json`, `.agent/**`, Temp artifacts, logs, or result XML.
Never push, amend, rebase, squash, reset, or start another packet.

## Acceptance criteria

- [ ] All nine exact RED tests exist and exercise real window action paths.
- [ ] Save buttons require complete endpoints, solution, difficulty, clean
      solution, and validated state.
- [ ] Invalid Save/Save As creates no asset and shows an Error diagnostic.
- [ ] Source-less Save creates exactly one asset and tracks it as the source.
- [ ] Loaded Save overwrites the same asset without creating another.
- [ ] Source-less Save As creates the requested asset and tracks it.
- [ ] Loaded Save As creates a new asset and preserves the old source.
- [ ] A generated result becomes a deep-owned, source-less, immediately
      savable Draft.
- [ ] Generate One clears a prior loaded source and history without creating an
      asset.
- [ ] Repeated `CreateGUI` leaves one effective Save As callback.
- [ ] Existing A1 workflow tests remain unchanged and pass.
- [ ] Full EditMode suite has zero failed, skipped, or inconclusive tests.
- [ ] `Assets/Temp/` and `Assets/Temp.meta` are absent immediately after tests.
- [ ] No files outside the whitelist changed.
- [ ] No unapproved protected change was made.

## Verification steps

Run the new workflow fixture first, then the full suite using the repository
Unity command.

Expected:

```text
All nine new test names appear in XML.
Total test count is greater than 334.
failed=0, skipped=0, inconclusive=0.
```

Then run:

```powershell
git diff bb5adc1..HEAD --check
git diff --stat bb5adc1..HEAD
git status --short
Test-Path Assets/Temp
Test-Path Assets/Temp.meta
```

Expected:

```text
git diff --check: no errors.
Tracked changes: only whitelisted files.
Both Test-Path commands: False immediately after the final test run.
.claude/settings.local.json may remain user-modified but must not be committed.
```

Static checks:

```powershell
rg -n "DoSaveDraft|DoSaveDraftAs|SetCurrentLevel|saveBtn.clicked|saveAsBtn.clicked" `
  Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs

rg -n "catch\\s*\\(Exception\\)" `
  Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs
```

Expected:

```text
Save buttons delegate to the two internal handlers.
No broad catch(Exception) exists.
```

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

PACKET: FLOW-RECOVERY-02-DRAFT-EDITOR-A2
START COMMIT: bb5adc1
END COMMIT: <hash>

CHANGED FILES:
- <path>

RED:
- <exact failing test names and pre-fix XML totals>

GREEN:
- <final XML totals>
- <nine exact test names>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <exit code / concise evidence>

TEMP:
- Assets/Temp: True | False
- Assets/Temp.meta: True | False
- Checked immediately after tests: YES | NO

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO
- Protected changes made: YES/NO

LOCAL COMMIT:
- Created: YES/NO
- Hash: <hash or N/A>
- Message: fix: complete draft save and generated handoff

BLOCKER:
<required only when STATUS is BLOCKED>
```

## What to do if blocked

Return `BLOCKED` with the exact missing symbol, required file, or technical
constraint. Do not modify an out-of-scope file, weaken a test, or deliver a
partial implementation as completed.
