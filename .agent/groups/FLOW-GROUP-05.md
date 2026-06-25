# External DeepSeek Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-05`

**Goal:** Deliver the UI Toolkit automatic-generation Editor workflow:
application facade, editable presets, deterministic board geometry and
preview, window shell, generation, validation, persistence, JSON export, and
batch actions.

**Start commit:** The clean repository `HEAD` containing this manifest. Codex
supplies the exact hash in the handoff message.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `FLOW-APPLICATION-001` | `FLOW-GROUP-04` | `feat: add flow level generation service` |
| 2 | `FLOW-EDITOR-PRESET-001` | `FLOW-APPLICATION-001` | `feat: add editable difficulty presets` |
| 3 | `FLOW-EDITOR-BOARD-001` | `FLOW-EDITOR-PRESET-001` | `feat: add ui toolkit flow board preview` |
| 4 | `FLOW-EDITOR-WINDOW-001` | `FLOW-EDITOR-BOARD-001` | `feat: add flow puzzle editor window shell` |
| 5 | `FLOW-EDITOR-AUTO-001` | `FLOW-EDITOR-WINDOW-001` | `feat: wire automatic level generation workflow` |

## Group rules

1. Read this manifest and all five packets before changing files.
2. Begin only when `HEAD` equals the supplied start commit.
3. The only permitted pre-existing working-tree change is user-managed
   `.claude/settings.local.json`.
4. Execute packets in order and keep exactly one commit per packet.
5. Run each packet's required tests before its commit.
6. Never stage or commit `.claude/settings.local.json`.
7. Never push, merge, rebase, amend, squash, reset, or rewrite history.
8. Stop all dependent packets after `BLOCKED` or failed verification.
9. A later packet may not silently repair an earlier packet.
10. Do not write `.meta` files manually.
11. Before every Unity process, verify no Unity process currently has this
    project open. Do not kill an existing process.
12. UI files remain under the existing Editor assembly; Application remains
    pure C#.
13. Do not add IMGUI `OnGUI()` to the main window.
14. Do not add Runtime player interaction, Draft editing, Solver, async,
    Undo/Redo command history, QFramework, or LLM integration.
15. After the group report, stop all work.

## Group integration verification

Run all EditMode tests using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Test total is greater than 238.
```

Run static checks:

```powershell
rg -n "UnityEngine|UnityEditor" Assets/Scripts/FlowPuzzle/Application

rg -n "void OnGUI\s*\(" Assets/Scripts/FlowPuzzle/Editor

rg -n "IFlowPuzzleSolver|QFramework|Task\.Run|Thread" `
  Assets/Scripts/FlowPuzzle/Application `
  Assets/Scripts/FlowPuzzle/Editor
```

Expected: no matches.

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-GROUP-05
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- <task id>: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>

GROUP VERIFICATION:
- Result: <Unity exit, total, passed, failed>

CHANGED FILES BY PACKET:
- <task id>
  - <path>

MANUAL SMOKE:
- RUN | NOT RUN
- Evidence: <menu/window/generate/validate/save/reload summary>

BLOCKER:
<required for PARTIAL or BLOCKED>
```
