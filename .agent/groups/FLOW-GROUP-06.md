# External DeepSeek Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-06`

**Goal:** Deliver the final manual Draft editing workflow, command-based
Undo/Redo, and endpoint-anchored fixed-path constraint editing without adding
any solver implementation.

**Start commit:** The clean repository `HEAD` containing this manifest. Codex
supplies the exact hash in the handoff message.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `FLOW-DRAFT-001` | `FLOW-GROUP-05` | `feat: add mutable flow level draft` |
| 2 | `FLOW-UNDO-001` | `FLOW-DRAFT-001` | `feat: add flow editor command history` |
| 3 | `FLOW-EDITOR-DRAFT-001` | `FLOW-UNDO-001` | `feat: add draft editing workflow` |
| 4 | `FLOW-CONSTRAINT-001` | `FLOW-EDITOR-DRAFT-001` | `feat: add fixed path constraint editing` |

## Group rules

1. Read this manifest and all four packets before changing files.
2. Begin only when `HEAD` equals the supplied start commit.
3. The only permitted pre-existing working-tree change is user-managed
   `.claude/settings.local.json`.
4. Execute packets in order and keep exactly one commit per packet.
5. Run each packet's required tests before its commit.
6. Never stage or commit `.claude/settings.local.json`.
7. Never push, merge, rebase, amend, squash, reset, or rewrite history.
8. Stop all dependent packets after `BLOCKED` or failed verification.
9. A later packet may not silently repair an earlier packet.
10. Do not write `.meta` files manually. Unity may generate only metas
    explicitly permitted by the active packet.
11. Before every Unity process, verify no Unity process has this project open.
    Do not kill an existing process.
12. Draft is the only mutable Editor model. Board views emit input intents and
    never mutate Draft, call repositories, or access `AssetDatabase`.
13. Do not implement a temporary or placeholder solver. `Complete` remains
    visible and disabled with the required explanatory message.
14. Do not add Runtime gameplay, QFramework, model APIs, IMGUI `OnGUI()`,
    background tasks, package changes, or serialized asset format changes.
15. After returning the group report, stop all work.

## Group integration verification

Run all EditMode tests using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Test total is greater than 278.
```

Run static checks:

```powershell
rg -n "IFlowPuzzleSolver|BacktrackingFlowPuzzleSolver|Task\.Run|System\.Threading|QFramework" `
  Assets/Scripts/FlowPuzzle/Editor

rg -n "void OnGUI\s*\(" Assets/Scripts/FlowPuzzle/Editor

git diff --check <START-COMMIT>..HEAD

git status --short

Test-Path Assets/Temp
Test-Path Assets/Temp.meta
```

Expected:

- both `rg` commands return no matches;
- `git diff --check` returns no errors;
- only `.claude/settings.local.json` may remain modified;
- `Assets/Temp` and `Assets/Temp.meta` are absent;
- exactly four packet commits exist after the supplied start commit.

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-GROUP-06
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- FLOW-DRAFT-001: COMPLETED | BLOCKED
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-UNDO-001: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-EDITOR-DRAFT-001: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-CONSTRAINT-001: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>

GROUP VERIFICATION:
- Result: <Unity exit, total, passed, failed>
- Static checks: <summary>
- Temp artifacts: <summary>

CHANGED FILES BY PACKET:
- <task id>
  - <path>

MANUAL SMOKE:
- RUN | NOT RUN
- Evidence: <Draft/edit/undo/constraint/save summary>

BLOCKER:
<required for PARTIAL or BLOCKED>
```
