# External DeepSeek Recovery Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-RECOVERY-01`

**Goal:** Replace the incomplete Draft, Solver, Completion, Diagnostics, retry,
tool-adapter, and final-verification skeletons with the approved final Editor
automation implementation.

**Start commit:** The clean `HEAD` containing this manifest and all ten Recovery
packets. Codex supplies the exact hash after the documents are committed.

**Execution mode:** `STRICT_SEQUENTIAL`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Required commit |
|---:|---|---|
| 1 | `FLOW-RECOVERY-DRAFT-CORE-001` | `fix: complete draft constraint domain` |
| 2 | `FLOW-RECOVERY-DRAFT-EDITOR-001` | `fix: complete draft editor workflow` |
| 3 | `FLOW-RECOVERY-SOLVER-001` | `fix: implement exhaustive flow solver` |
| 4 | `FLOW-RECOVERY-COMPLETION-CORE-001` | `fix: complete async completion core` |
| 5 | `FLOW-RECOVERY-COMPLETION-EDITOR-001` | `fix: complete editor completion workflow` |
| 6 | `FLOW-RECOVERY-DIAGNOSTICS-001` | `fix: implement structured flow diagnostics` |
| 7 | `FLOW-RECOVERY-RETRY-001` | `fix: implement diagnostic retry actions` |
| 8 | `FLOW-RECOVERY-TOOL-001` | `fix: implement solver tool adapter` |
| 9 | `FLOW-RECOVERY-FINAL-TESTS-001` | `test: add final flow acceptance matrices` |
| 10 | `FLOW-RECOVERY-VERIFY-001` | no commit |

## Non-negotiable execution rules

1. Read this manifest and all ten packets before changing files.
2. Begin only when `HEAD` equals the supplied start hash.
3. The only permitted pre-existing change is
   `.claude/settings.local.json`.
4. Execute packets in order. Never merge, skip, reorder, or combine packets.
5. Each of packets 1 through 9 produces exactly one commit with the exact
   message above. Packet 10 is read-only and creates no commit.
6. Before each commit:
   - inspect `git diff --name-only`;
   - verify every path is in the active packet whitelist;
   - run the active packet's focused tests;
   - run the complete EditMode suite.
7. Every implementation packet must add or strengthen tests that fail against
   its pre-packet commit. A green unchanged suite is not acceptance evidence.
8. Never delete, weaken, ignore, skip, rename away, or replace a valid test to
   reduce failures.
9. A blocked requirement, compile error, test failure, scope violation, or
   missing red/green evidence stops this packet and every later packet.
10. A later packet may not repair an earlier packet.
11. Never push, merge, rebase, amend, squash, reset, clean, or rewrite history.
12. Never stage or commit `.claude/settings.local.json`.
13. Do not claim manual Editor verification unless it was performed
    interactively.
14. Do not implement unique-solution search, full-board requirements, Runtime
    gameplay, QFramework, model/network APIs, SAT, or ILP.
15. After the consolidated response, stop all work.

## Packet gate evidence

For every packet return:

```text
PACKET: <id>
START COMMIT: <hash>
END COMMIT: <hash>
CHANGED FILES: <complete list>
RED: <test names and observed failures before implementation>
GREEN: <focused and full-suite XML totals>
SCOPE CHECK: <git diff --name-only result>
COMMIT: <hash and exact message>
```

## Group integration gate

Run compile-only and complete EditMode tests using `.agent/VALIDATION.md`.

Also run:

```powershell
rg -n "IFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Generation
rg -n "void OnGUI\s*\(" Assets/Scripts/FlowPuzzle/Editor
rg -n "Task\.Run" Assets/Scripts/FlowPuzzle/Editor
rg -n "UnityEngine|UnityEditor|AssetDatabase" Assets/Scripts/FlowPuzzle/Solving
rg -n "HttpClient|UnityWebRequest|api.?key" Assets/Scripts/FlowPuzzle/Solving/Tools
git diff --check <START-COMMIT>..HEAD
git status --short
Test-Path Assets/Temp
Test-Path Assets/Temp.meta
```

Expected:

- automatic Generation has no Solver reference;
- Editor has no main-window `OnGUI` and no `Task.Run`;
- Solving and Tools contain no Unity or network dependency;
- diff check has no source errors;
- worktree contains only user-managed Claude settings;
- no temp assets remain.

## Required consolidated response

```text
RECOVERY STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-RECOVERY-01
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- <all ten packet evidence blocks>

FINAL VERIFICATION:
- compile:
- EditMode XML:
- static checks:
- generation matrix:
- solver matrix:
- manual acceptance:
- exact git status:

BLOCKER:
<required unless COMPLETED>
```
