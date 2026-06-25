# External DeepSeek Task Group

**Status:** `ACCEPTED`

## Group metadata

**Group ID:** `FLOW-GROUP-03`

**Goal:** Implement the deterministic solution-first generation pipeline from
random source and length allocation through complete validated level
generation.

**Start commit:** The clean repository `HEAD` containing this manifest. Codex
supplies the exact hash in the handoff message.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `FLOW-GENERATION-001` | `FLOW-GROUP-02` | `feat: add deterministic path length allocation` |
| 2 | `FLOW-GENERATION-002` | `FLOW-GENERATION-001` | `feat: add randomized dfs path generation` |
| 3 | `FLOW-GENERATION-003` | `FLOW-GENERATION-002` | `feat: generate deterministic flow puzzle levels` |

## Group rules

1. Read this manifest and all three packets before changing files.
2. Begin only when `HEAD` equals the supplied start commit.
3. The only permitted pre-existing working-tree change is user-managed
   `.claude/settings.local.json`.
4. Execute packets in order and keep one commit per packet.
5. Follow every packet's red-green verification.
6. Never stage or commit `.claude/settings.local.json`.
7. Never push, merge, rebase, amend, squash, reset, or rewrite history.
8. Stop dependent packets after any `BLOCKED` or failed verification.
9. Do not use a later packet to repair an earlier packet.
10. Do not launch multiple Unity processes. Before every Unity run, verify no
    Unity process currently has this project open.
11. Do not delete `Library`, `Temp`, package caches, or project settings.
12. After returning the group report, stop all work.

## Group integration verification

Run one complete EditMode test process using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Test total is greater than 136.
```

Run:

```powershell
rg -n "UnityEngine|UnityEditor|UnityEngine.Random" `
  Assets/Scripts/FlowPuzzle/Generation

rg -n "IFlowPuzzleSolver" `
  Assets/Scripts/FlowPuzzle/Generation

git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: no prohibited references and no unexpected changes.

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-GROUP-03
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- FLOW-GENERATION-001: COMPLETED | BLOCKED
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-GENERATION-002: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-GENERATION-003: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>

GROUP VERIFICATION:
- Result: <Unity exit, total, passed, failed>

CHANGED FILES BY PACKET:
- <task id>
  - <path>

BLOCKER:
<required for PARTIAL or BLOCKED>
```

## Review record

- `FLOW-GENERATION-001` commit:
  `2981fa9`
- `FLOW-GENERATION-002` commit:
  `b82d8f2`
- `FLOW-GENERATION-003` commit:
  `67d33fa`
- Worker verification: Unity EditMode `181/181` passed.
- Codex independently parsed `Logs/FLOW-GROUP-03-p3.xml`:
  - result: `Passed`
  - total: `181`
  - passed: `181`
  - failed: `0`
- Codex verdict: `NEEDS_FIX`
- Corrective task: `FLOW-GROUP-03-FIX-01`
- Findings:
  - `SystemFlowRandom.NextFloat` accepts an empty range although packet 1
    requires empty ranges to throw;
  - immediate generator configuration failures record seed `0` instead of
    the resolved seed required in every result;
  - generated `FlowLevelData.difficulty` and `difficultyScore` remain default
    values instead of matching the evaluated report;
  - packet-required `6x6`, `7x7`, representative different-seed, exact
    immediate-failure, list-copy ownership, inclusive score-range, and
    three-run deep-equality coverage is absent or weaker than required.
- Scope, commit boundaries, deterministic collection usage, DFS rollback, and
  prohibited-reference checks otherwise passed review.

### Corrective review 1

- Corrective commit: `07300ce`
- Verification: Unity EditMode `192/192` passed.
- Production fixes: accepted.
- Corrective task verdict: `NEEDS_FIX`
- Follow-up task: `FLOW-GROUP-03-FIX-02`
- Remaining issue is test-only:
  - fixed-seed deep equality does not compare all required metadata or ordered
    lists;
  - different-seed layout comparison can pass solely because seeds differ.

### Final corrective completion

- Test corrective commit: `c211081`
- `FLOW-GENERATION-001` final verdict: `ACCEPT`
- `FLOW-GENERATION-002` final verdict: `ACCEPT`
- `FLOW-GENERATION-003` final verdict: `ACCEPT`
- `FLOW-GROUP-03-FIX-01` final verdict: `ACCEPT`
- `FLOW-GROUP-03-FIX-02` final verdict: `ACCEPT`
- Group final verdict: `ACCEPT`
- Final verification XML: `Logs/FLOW-FIX-02.xml`
  - result: `Passed`
  - total: `192`
  - passed: `192`
  - failed: `0`
  - skipped: `0`
  - failed test cases: `0`
  - failed suites: `0`
- Only the user-managed `.claude/settings.local.json` remains modified outside
  accepted commits.
