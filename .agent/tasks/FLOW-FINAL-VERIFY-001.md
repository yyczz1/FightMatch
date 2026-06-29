# External DeepSeek Verification Packet

## Metadata

**Task ID:** `FLOW-FINAL-VERIFY-001`

**Group:** `FLOW-GROUP-09`

**Order:** `2`

**Goal:** Perform final verification only and return an evidence table.

## File scope

**Files allowed to modify or create:** `NONE`

Do not create a commit.

## Required verification

1. Confirm no Unity process currently has this project open.
2. Run compile-only batch mode and record exit code/log path.
3. Run the complete EditMode suite and parse XML totals.
4. Inspect logs for compiler errors, failures, and unexpected exceptions.
5. Run/inspect deterministic generation matrix evidence.
6. Run/inspect solver performance matrix status, elapsed time, and node counts.
7. Run static checks:

```powershell
rg -n "UnityEngine\.Random|unique solution|fill entire board" `
  Assets/Scripts/FlowPuzzle

rg -n "IFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Generation

rg -n "AssetDatabase" Assets/Scripts/FlowPuzzle `
  -g "*.cs"

rg -n "void OnGUI\s*\(" Assets/Scripts/FlowPuzzle/Editor

rg -n "HttpClient|UnityWebRequest|api.?key" `
  Assets/Scripts/FlowPuzzle/Solving/Tools

git diff --check <MASTER-START-COMMIT>..HEAD

git status --short
```

Interpret `AssetDatabase` matches: they are allowed only under the Editor
assembly.

8. Verify `Assets/Temp` and `Assets/Temp.meta` are absent.
9. If an interactive Unity Editor is available, perform:
   - open menu/window and resize;
   - preset/custom generation, preview, validation, SO save, JSON export;
   - new Draft, endpoint editing, exact completion;
   - constrained completion;
   - endpoint and whole-stroke Undo/Redo;
   - Save As and reload;
   - batch continuation after one failure;
   - script reload and Draft/UI state restoration.
10. If interactive Editor is unavailable, mark every manual step `NOT RUN`;
    do not substitute static inspection or automated tests.

## Required output

```text
STATUS: COMPLETED | PARTIAL | BLOCKED

COMPILE:
- command:
- exit:
- evidence:

EDITMODE:
- total:
- passed:
- failed:
- skipped:
- XML:

MATRICES:
- generation:
- solver:
- cancellation/timeout:

STATIC CHECKS:
- result:

MANUAL ACCEPTANCE:
- each step RUN/PASS or NOT RUN

WORKTREE:
- exact git status
- temp artifacts

BLOCKER:
```

`COMPLETED` requires every manual step to be run. Otherwise return `PARTIAL`.
