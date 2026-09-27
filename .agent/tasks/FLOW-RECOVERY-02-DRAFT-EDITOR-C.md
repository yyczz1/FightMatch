# External DeepSeek Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-C`  
**Status:** `APPROVED_FOR_WORKER`  
**Group ID:** `NONE`  
**Order in group:** `N/A`  
**Depends on:** `FLOW-RECOVERY-02-DRAFT-EDITOR-B`, commit `0974782`  
**Start commit:** `0974782`

**Goal:** Preserve and restore Draft editor window state across `CreateGUI` /
domain-reload style rebuilds.

## Background

Draft Editor A and B are accepted. Manual editing, Save/Save As, generated
handoff, board pointer strokes, and constraint drawing are now covered.

There is already a `FlowDraftWindowState` DTO, but the window does not yet use
it. This packet makes the EditorWindow recover useful Draft editing state after
UI Toolkit rebuild/domain reload without pretending Undo/Redo history can safely
survive reload.

## Current context

- Unity version: `2022.3.18f1`.
- Current accepted commit: `0974782`.
- Full EditMode baseline: `355/355` passed.
- `FlowLevelGeneratorWindow.CreateGUI` rebuilds UI controls from scratch.
- `FlowDraftWindowState` currently contains:
  - `selectedColorId`
  - `selectedTool`
  - `isEndpointA`
  - `draftJson`
  - `loadedAssetGuid`
- `FlowLevelDraft` is `[Serializable]` and can be serialized by
  `JsonUtility.ToJson`.
- `.claude/settings.local.json` is user-managed and must remain outside commits.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-A2.md`
- `.agent/tasks/FLOW-RECOVERY-02-DRAFT-EDITOR-B.md`
- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftWindowState.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftWindowState.cs`
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

## Required RED tests

Add these tests before production changes. Use exact names:

1. `WindowState_RestoresDraftControlsAndBoardAfterCreateGUI`
2. `WindowState_RestoresLoadedAssetGuidAndAssetField`
3. `WindowState_InvalidAssetGuidDoesNotThrowOrLoadAsset`
4. `WindowState_ReloadClearsUndoRedoHistory`
5. `WindowState_SaveAsNameSurvivesCreateGUI`
6. `WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled`

Run the fixture before implementation and report the failing test names. If any
test unexpectedly passes, strengthen it to prove the missing observable
behavior.

## Required implementation

### A. State model

1. Use `FlowDraftWindowState` as the single serializable DTO for Draft editor
   window state.
2. Add fields only if needed for this packet. At minimum add:

   ```csharp
   public string saveAsName;
   ```

3. Do not add Undo/Redo command history to the state. History is intentionally
   cleared on reload.
4. Do not add generated-level or batch-report persistence in this packet.

### B. Window serialized state

5. Add a serialized field to `FlowLevelGeneratorWindow`, for example:

   ```csharp
   [SerializeField] private FlowDraftWindowState draftWindowState = new FlowDraftWindowState();
   ```

6. Add small internal methods for tests and lifecycle:

   ```csharp
   internal void CaptureDraftWindowState()
   internal void RestoreDraftWindowState()
   ```

   Names may differ only if there is a strong reason; tests must use the real
   methods, not duplicate the logic.
7. `CreateGUI` must call restore after UI controls are built and wired.
8. `OnDisable` should capture state if controls have been built. It must not
   throw if the window is partially initialized.
9. Capture must read:
   - `currentDraft` as JSON, if non-null;
   - selected color field;
   - selected tool;
   - endpoint A/B toggle;
   - Save As name;
   - `loadedAsset` GUID if loadedAsset exists and has an AssetDatabase path.
10. Restore must write:
    - Draft controls;
    - `currentDraft`;
    - `loadedAsset` and `draftPanel.assetField` when GUID resolves;
    - board view display data when Draft exists;
    - draft button state and normal action button state.

### C. Restore semantics

11. Restoring valid draft JSON must deep-own the restored Draft. Mutating the
    restored Draft after restore must not mutate the state JSON object by
    reference.
12. If `loadedAssetGuid` is invalid, missing, or resolves to a non-asset:
    - do not throw;
    - set `loadedAsset = null`;
    - set `draftPanel.assetField.value = null`;
    - still restore the Draft if `draftJson` is valid.
13. If there is no `draftJson`, restore leaves `currentDraft == null`, clears
    board data, and keeps Draft Save/Save As disabled.
14. Restored Undo/Redo history must be empty:
    - `commandHistory.CanUndo == false`;
    - `commandHistory.CanRedo == false`;
    - Undo/Redo buttons disabled.
15. Restore must not call `DoLoadDraft`; it should set the object field/source
    directly so no accidental overwrite/load side effects occur.
16. Restore must not create assets, temp folders, or generated output folders.
17. Restore must tolerate malformed/empty JSON by leaving draft null and showing
    no exception. A diagnostic warning is optional, but do not add noisy errors
    unless tested.

### D. Keep existing workflows updating state

18. After these actions, capture or update state so a later `CreateGUI` restore
    is current:
    - New Draft;
    - Load Draft;
    - Add/Remove color;
    - endpoint edit;
    - constraint stroke;
    - Undo/Redo;
    - Save;
    - Save As;
    - generated handoff via `SetCurrentLevel`;
    - Clear Preview.
19. You may centralize with a small helper such as
    `RefreshDraftViewsAndState()` only if it reduces duplication without broad
    refactoring. Do not rewrite unrelated window methods.
20. Normal UI field edits for selected color/tool/endpoint/saveAsName should be
    captured on `OnDisable`; live value-change capture is optional.

## Test-quality requirements

- Tests must use a real `FlowLevelGeneratorWindow` created with
  `ScriptableObject.CreateInstance` and `CreateGUI`.
- Tests must use `CaptureDraftWindowState` and `RestoreDraftWindowState`, not
  reimplement serialization logic.
- For loaded asset GUID tests, create a real `FlowLevelAsset` under the existing
  test `TestFolder`, then verify the restored object field references that
  asset.
- For board restore, inspect BoardView's internal displayed level snapshot or
  visual-kind seam to prove board data was restored, not just `currentDraft`.
- Do not use global asset searches; restrict to `TestFolder`.
- Keep existing fixture cleanup so `Assets/Temp/` and `Assets/Temp.meta` are
  absent after tests when they were not present before.
- Avoid weak assertions such as `not null` when exact values are knowable.

## Non-goals

- No Solver or Completion.
- No runtime-player state.
- No persistence format changes for `FlowLevelAsset`, JSON export, or generated
  level data.
- No saving/restoring Undo/Redo command stacks.
- No new services, repositories, ScriptableObjects, settings assets, packages,
  UXML, USS, or dependencies.
- No unrelated refactoring, formatting, renaming, comments, or optimization.

## Constraints

- Preserve all accepted A/B behavior and tests.
- Keep state DTO simple and serializable.
- Do not edit Draft Core or command classes.
- Do not use `EditorPrefs` for this packet. Use the EditorWindow serialized
  field because this is window-local state.
- Do not use `OnGUI`, `Task.Run`, threads, QFramework, or solver types.
- If the required behavior cannot fit the whitelist, return `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | Internal test methods only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `LIMITED` | `FlowDraftWindowState.saveAsName` only |
| Unity asset or `.meta` changes | `NO` | None |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `2`  
**Maximum changed test files:** `1`  
**Maximum new files:** `0`  
**Approximate maximum diff:** `300 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES`  
**Required commit message:** `fix: restore draft editor window state`

Commit only this packet's allowed files after verification passes. Never push,
merge, rebase, amend, reset history, or include `.agent/**` task files.

## Acceptance criteria

- [ ] All 6 required C test names exist.
- [ ] Draft controls restore exact selected color, selected tool, endpoint
      toggle, and Save As name.
- [ ] Draft data restores exact width/height/levelId/seed/pairs/constraints/
      dirty/validated state.
- [ ] BoardView display data refreshes from restored Draft.
- [ ] Loaded asset GUID restores `loadedAsset` and `draftPanel.assetField`.
- [ ] Invalid asset GUID does not throw and does not load an asset.
- [ ] Reload/restore clears command history and disables Undo/Redo.
- [ ] No Draft restore leaves Draft null and Save/Save As disabled.
- [ ] No assets or temp folders are created by restore itself.
- [ ] Full EditMode tests pass with at least `361` tests, `0 failed`,
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
  -testResults 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCTests.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowDraftCTests.log'
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
rg -n "WindowState_RestoresDraftControlsAndBoardAfterCreateGUI|WindowState_RestoresLoadedAssetGuidAndAssetField|WindowState_InvalidAssetGuidDoesNotThrowOrLoadAsset|WindowState_ReloadClearsUndoRedoHistory|WindowState_SaveAsNameSurvivesCreateGUI|WindowState_NoDraft_RestoreLeavesDraftNullAndButtonsDisabled" Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

Expected:

```text
All 6 test names are present.
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
git diff --name-only 0974782..HEAD
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
