# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A-TEMP-CLEANUP`  
**Status:** `APPROVED_FOR_WORKER`  
**Start commit:** `a2c561d`  
**Goal:** Make the Draft workflow fixture clean its own temporary parent folder.

## Independent reproduction

Codex independently ran the full EditMode suite:

- `334` passed;
- `0` failed/skipped/inconclusive;
- after Unity exited, `Assets/Temp/` and `Assets/Temp.meta` existed again;
- `Assets/Temp/` was empty.

The fixture deletes only
`Assets/Temp/FlowPuzzleDraftEditorTests`, leaving Unity's generated parent folder
and `.meta`.

## Scope

**Only file allowed to modify:**

- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

Every other tracked file is forbidden. Do not modify its `.meta` or production
code.

The currently untracked empty `Assets/Temp/` and `Assets/Temp.meta` may be
removed after confirming the directory is empty.

## Required behavior

1. Introduce a `TempParent = "Assets/Temp"` constant.
2. Before the fixture runs, record whether `TempParent` existed before this
   fixture.
3. Continue deleting only the known `TestFolder`.
4. During teardown, if and only if:
   - `TempParent` did not exist before this fixture; and
   - the known `TestFolder` has been deleted; and
   - `TempParent` contains no remaining files or subdirectories,
   
   delete `TempParent` through `AssetDatabase.DeleteAsset`.
5. Never delete a pre-existing `Assets/Temp` folder or any unrelated content.
6. Do not manually clean Temp after the final verification run. The fixture
   itself must leave it absent.

Use `System.IO.Directory.EnumerateFileSystemEntries` or an equivalently direct
empty-directory check. Do not infer emptiness only from the known test folder.

## Verification

1. Confirm the current `Assets/Temp` directory is empty, then remove the current
   test-created residue before running tests.
2. Run `FlowDraftEditorWorkflowTests`.
3. Immediately check:

```powershell
Test-Path Assets/Temp
Test-Path Assets/Temp.meta
```

Both must return `False` without any intervening cleanup command.
4. Run the full EditMode suite.
5. Immediately repeat both `Test-Path` checks. Both must still return `False`.
6. Run `git diff --check` and `git status --short`.

Acceptance:

- all `334` tests pass with zero failed/skipped/inconclusive;
- the fixture preserves a pre-existing non-empty parent, proven by a focused
  test or a safe setup/teardown verification;
- no Temp residue after either test run;
- only the whitelisted test file is committed;
- `.claude/settings.local.json` remains untouched and uncommitted.

## Commit

```text
test: clean draft fixture temp parent
```

Report the two immediate post-test `Test-Path` results, not a status obtained
after manual cleanup.
