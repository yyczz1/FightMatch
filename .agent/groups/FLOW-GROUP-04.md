# External DeepSeek Task Group

**Status:** `NEEDS_FIX`

## Group metadata

**Group ID:** `FLOW-GROUP-04`

**Goal:** Implement final single-asset persistence, optional dual-JSON export,
Editor asset repository operations, and deterministic batch generation.

**Start commit:** The clean repository `HEAD` containing this manifest. Codex
supplies the exact hash in the handoff message.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `FLOW-PERSISTENCE-001` | `FLOW-GROUP-03` | `feat: add flow level asset and json export` |
| 2 | `FLOW-PERSISTENCE-002` | `FLOW-PERSISTENCE-001` | `feat: add flow level asset repository` |
| 3 | `FLOW-BATCH-001` | `FLOW-GROUP-03` | `feat: add deterministic batch generation` |

`FLOW-BATCH-001` is logically independent of persistence but executes third so
the worker and reviewer retain one unambiguous commit order.

## Group rules

1. Read this manifest and all three packets before changing files.
2. Begin only when `HEAD` equals the supplied start commit.
3. The only permitted pre-existing working-tree change is user-managed
   `.claude/settings.local.json`.
4. Execute packets in order and keep one commit per packet.
5. Run each packet's verification before its commit.
6. Never stage or commit `.claude/settings.local.json`.
7. Never push, merge, rebase, amend, squash, reset, or rewrite history.
8. Stop dependent packets after `BLOCKED` or failed verification.
9. Do not use a later packet to repair an earlier packet.
10. Do not manually create `.meta` files; Unity may generate only the metas
    explicitly permitted by the active packet.
11. Before every Unity process, verify no Unity process currently has this
    project open. Do not kill an existing process.
12. Tests may create `Assets/Temp/FlowPuzzleTests/` and must remove only that
    exact test-created folder during teardown.
13. Do not delete project `Library/`, root `Temp/`, package caches, settings,
    user assets, or unrelated generated files.
14. After returning the group report, stop all work.

## Group integration verification

Run one complete EditMode test process using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Test total is greater than 192.
```

Run:

```powershell
rg -n "UnityEditor" Assets/Scripts/FlowPuzzle/Persistence

rg -n "UnityEngine|UnityEditor|AssetDatabase|JsonUtility" `
  Assets/Scripts/FlowPuzzle/Generation

git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected:

- Persistence contains no `UnityEditor`;
- Generation remains pure C#;
- no unexpected working-tree changes remain.

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-GROUP-04
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- FLOW-PERSISTENCE-001: COMPLETED | BLOCKED
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-PERSISTENCE-002: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <summary>
- FLOW-BATCH-001: COMPLETED | BLOCKED | NOT RUN
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

- `FLOW-PERSISTENCE-001` commit: `d492954`
- `FLOW-PERSISTENCE-002` commit: `d045abc`
- `FLOW-BATCH-001` commit: `793f22d`
- Codex independently parsed `Logs/FLOW-GROUP-04-final.xml`:
  - result: `Passed`
  - total: `215`
  - passed: `215`
  - failed: `0`
  - failed test cases: `0`
  - failed suites: `0`
- Codex verdicts:
  - `FLOW-PERSISTENCE-001`: `NEEDS_FIX`
  - `FLOW-PERSISTENCE-002`: `NEEDS_FIX`
  - `FLOW-BATCH-001`: `NEEDS_FIX`
  - Group: `NEEDS_FIX`
- Corrective sequence:
  1. `FLOW-GROUP-04-FIX-01`
  2. `FLOW-GROUP-04-FIX-02`
- Findings:
  - repository preparation mutates the supplied generated level, despite the
    no-input-mutation requirement;
  - the repository packet added no SaveNew, Overwrite, or SaveAs tests;
  - folder prefix validation accepts paths such as `AssetsOutside` and does
    not reject traversal/name separators;
  - JSON “round-trip” tests only search text and do not deserialize endpoints
    or solution cells;
  - incomplete and invalid JSON input cases are not fully tested;
  - batch repeatability tests compare only item metadata, not generated level
    data;
  - batch overflow/negative seed, complete config immutability, failure
    message, and instance-ownership contracts are not tested.
- Assembly boundaries, commit ordering, pure-C# Generation references, and the
  basic single-asset/dual-JSON/batch architecture otherwise passed review.

### Corrective review 1

- `FLOW-GROUP-04-FIX-01` commit: `83d08c9`
- `FLOW-GROUP-04-FIX-02` commit: `ce2d896`
- Verification XML: `Logs/FLOW-FIX-01.xml`
  - result: `Passed`
  - total: `230`
  - passed: `230`
  - failed: `0`
- Codex verdict: `NEEDS_FIX`
- Final bounded corrective task: `FLOW-GROUP-04-FIX-03`
- Remaining findings:
  - repository validates normalized folders but continues using the original
    separators for creation and asset paths;
  - SaveAs does not normalize repeated or case-varied `.asset` suffixes to
    exactly one extension;
  - JSON and repository tests still do not prove complete DTO/caller-state
    contracts;
  - batch overflow and full config immutability tests do not perform what their
    names claim;
  - test execution leaves untracked `Assets/Temp.meta` and an empty parent
    folder.
