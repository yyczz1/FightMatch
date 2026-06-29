# External DeepSeek Remaining Work Execution

**Status:** `APPROVED_FOR_WORKER`

## Goal

Execute every remaining Editor-tool task without intermediate Codex review,
while preserving group-level test gates, packet commits, rollback boundaries,
and a final group-by-group Codex audit.

## Required order

| Order | Group | Goal |
|---:|---|---|
| 1 | `FLOW-GROUP-06-REPAIR` | Complete and correct Draft editing |
| 2 | `FLOW-GROUP-07` | Exact solver, async completion, Editor completion |
| 3 | `FLOW-GROUP-08` | Diagnostics, retry actions, solver tool contract |
| 4 | `FLOW-GROUP-09` | Final integration tests and acceptance verification |

## Master rules

1. Read every group manifest and packet before starting.
2. Start only from the exact hash supplied by Codex after these documents are
   committed.
3. The only permitted initial working-tree changes are:
   - user-managed `.claude/settings.local.json`;
   - the two untracked Group 6 test `.meta` files explicitly assigned to
     `FLOW-GROUP-06-FIX-01`.
4. Execute all groups and packets strictly in declared order.
5. Keep one local commit per authorized packet. Never squash or combine them.
6. Run each packet verification and each group integration gate.
7. A failed test, compile failure, scope violation, or `BLOCKED` result stops
   the current group and every later group.
8. A later packet or group must not silently repair an earlier packet. Stop
   and report the earlier contract failure.
9. Never push, merge, rebase, amend, reset, clean, or rewrite history.
10. Never stage or commit `.claude/settings.local.json`.
11. Do not claim manual Editor smoke steps that were not actually run.
12. Return one consolidated report containing every packet commit, each group
    test result, final static checks, and all `NOT RUN` manual steps.

## Final response

```text
EXECUTION STATUS: COMPLETED | PARTIAL | BLOCKED
START COMMIT: <hash>
END COMMIT: <hash or N/A>

GROUP RESULTS:
- FLOW-GROUP-06-REPAIR: <status, commits, test count>
- FLOW-GROUP-07: <status, commits, test count>
- FLOW-GROUP-08: <status, commits, test count>
- FLOW-GROUP-09: <status, commits, test count>

FINAL VERIFICATION:
- Compile:
- EditMode:
- Static checks:
- Manual acceptance:

UNCOMMITTED FILES:
- <exact git status>

BLOCKER:
<required unless COMPLETED>
```
