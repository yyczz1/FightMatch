# External DeepSeek Recovery Verification Packet

**Task ID:** `FLOW-RECOVERY-VERIFY-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `10`  
**Depends on:** `FLOW-RECOVERY-FINAL-TESTS-001`  
**Goal:** Perform read-only final verification and return evidence without
changing or committing files.

## Scope

**Allowed to modify/create:** `NONE`

## Required automated verification

1. Confirm no Unity process has this project open.
2. Run compile-only batch mode and record command, exit, and log.
3. Run complete EditMode suite and parse XML result, total, passed, failed,
   skipped, failed cases, and failed suites.
4. List exact test names for:
   - Draft Editor actions and pointer strokes;
   - exhaustive Solver, timeout, cancellation, and progress;
   - Completion core and Editor completion;
   - diagnostics and retries;
   - tool adapter;
   - final acceptance/performance matrices.
5. Inspect logs for `error CS`, failed tests, and unexpected exceptions.
6. Record generation and solver matrix outputs.
7. Run:

```powershell
rg -n "IFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Generation
rg -n "void OnGUI\s*\(" Assets/Scripts/FlowPuzzle/Editor
rg -n "Task\.Run" Assets/Scripts/FlowPuzzle/Editor
rg -n "UnityEngine|UnityEditor|AssetDatabase" Assets/Scripts/FlowPuzzle/Solving
rg -n "HttpClient|UnityWebRequest|api.?key|OpenAI|DeepSeek|Claude" `
  Assets/Scripts/FlowPuzzle/Solving/Tools
rg -n "results\.Count >= 1|if \(r\.status != FlowSolveStatus\.Cancelled\)" `
  Assets/Scripts/FlowPuzzle/Solving `
  Assets/Tests/EditMode/Solving
git diff --check <RECOVERY-START-COMMIT>..HEAD
git status --short
Test-Path Assets/Temp
Test-Path Assets/Temp.meta
```

Expected: prohibited searches have no matches; status contains only
`.claude/settings.local.json`; no temp artifacts.

## Manual Editor acceptance

If an interactive Editor is available, perform and record:

1. Open and resize the UI Toolkit window.
2. Generate, preview, validate, save SO, and export JSON.
3. Create Draft, place/move/remove endpoints, add/remove color, Undo/Redo.
4. Draw and erase a multi-cell fixed constraint; one-stroke Undo/Redo.
5. Complete endpoint-only and constrained Drafts.
6. Observe progress and cancel an active completion.
7. Save, Save As, reload, and verify source immutability.
8. Apply a safe suggestion; retry same and new seed.
9. Run batch with one failure and continuation.
10. Trigger script reload and verify Draft/tool/selection restoration.

If unavailable, mark each item `NOT RUN` and return overall `PARTIAL`, not
`COMPLETED`.

## Required response

```text
STATUS: COMPLETED | PARTIAL | BLOCKED
COMPILE: <evidence>
EDITMODE: <XML evidence>
REQUIRED TEST NAMES: <evidence>
MATRICES: <evidence>
STATIC: <evidence>
MANUAL: <10 item table>
WORKTREE: <exact status and temp checks>
COMMIT CREATED: NO
BLOCKER:
```
