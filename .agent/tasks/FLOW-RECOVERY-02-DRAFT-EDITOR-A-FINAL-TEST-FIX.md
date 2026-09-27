# External DeepSeek Corrective Task Packet

**Task ID:** `FLOW-RECOVERY-02-DRAFT-EDITOR-A-FINAL-TEST-FIX`  
**Status:** `APPROVED_FOR_WORKER`  
**Start commit:** `62751f5`  
**Goal:** Close the final two Draft workflow test gaps without changing
production code.

## Scope

**Only file allowed to modify:**

- `Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs`

Every other tracked file is forbidden. Do not modify its `.meta`.

## Required corrections

### 1. Verify loaded board data

In `LoadAsset_DeepCopiesDisplaysAndClearsHistory`, replacing
`Assert.IsNotNull(boardView)` is required.

Read the `FlowBoardView` private display snapshot through reflection and assert:

- displayed width is `4`;
- displayed height is `4`;
- exactly one displayed pair exists;
- its color and both endpoint coordinates equal the loaded asset;
- Undo and Redo buttons are both disabled after history is cleared.

This is test inspection only. Do not expose new production APIs.

### 2. Invoke the real button callbacks

In `CreateGUITwice_EachDraftEditActionExecutesOnce`:

- calling `addColorM`, `DoAddColor`, `undoM`, or `DoUndo` is forbidden;
- after three `CreateGUI` calls, invoke the current
  `DP(w).addColorBtn.clickable` callback;
- then invoke the current `DP(w).undoBtn.clickable` callback once;
- assert exactly one color was added;
- assert the complete original Draft is restored after the one Undo click;
- assert `CanUndo == false` and `CanRedo == true`.

Unity 2022.3.18f1 provides the non-public synchronous method:

```csharp
Clickable.Invoke(EventBase)
```

Use a narrowly scoped test helper with reflection:

```csharp
typeof(Clickable).GetMethod(
    "Invoke",
    BindingFlags.Instance | BindingFlags.NonPublic)
```

Invoke it on `button.clickable` with a null `EventBase`. This executes the
actual delegates registered by `WireDraftPanel`.

Do not use `SimulateSingleClick`, because its delayed scheduler would make this
EditMode test timing-dependent.

## Cleanup

- Keep fixture cleanup limited to its known test assets.
- Confirm `Assets/Temp/` and `Assets/Temp.meta` remain absent.
- Do not touch `.claude/settings.local.json`.

## Verification

Run:

1. `FlowDraftEditorWorkflowTests`;
2. existing `FlowLevelGeneratorWindowTests`;
3. full EditMode suite;
4. `git diff --check`;
5. `git status --short`.

Static acceptance:

```powershell
rg -n -A25 "CreateGUITwice_EachDraftEditActionExecutesOnce" `
  Assets/Tests/EditMode/Editor/FlowDraftEditorWorkflowTests.cs
```

The displayed test body must reference `addColorBtn`, `undoBtn`, and the
`Clickable.Invoke` reflection helper. It must not reference `addColorM`,
`undoM`, `DoAddColor`, or `DoUndo`.

## Commit

```text
test: close draft callback verification
```

Report exact XML totals, changed files, static-search evidence, and Temp state,
then stop.
