# External DeepSeek Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-07`

**Goal:** Add the exact deterministic local solver, asynchronous local
completion provider, completion application service, and responsive UI
Toolkit completion workflow.

**Depends on:** accepted `FLOW-GROUP-06-REPAIR`

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Commit |
|---:|---|---|---|
| 1 | `FLOW-SOLVER-001` | Group 6 Repair | `feat: add exact local flow solver` |
| 2 | `FLOW-COMPLETION-001` | `FLOW-SOLVER-001` | `feat: add async local completion service` |
| 3 | `FLOW-EDITOR-COMPLETION-001` | `FLOW-COMPLETION-001` | `feat: connect editor completion workflow` |

## Group rules

1. Solver and contracts remain pure C# with no Unity API.
2. The automatic solution-first generator must not call the Solver.
3. Stop after the first valid solution. Do not search for uniqueness.
4. Empty board cells are allowed.
5. `NoSolution` is permitted only after exhaustive search.
6. Node/time budget exhaustion is `Timeout`; cancellation is `Cancelled`.
7. Only pure-data solving runs on a worker thread.
8. Unity objects, Editor state, UI, assets, and Draft application remain on the
   main thread.
9. Do not add model APIs, HTTP, QFramework, Runtime gameplay, or SAT/ILP.

## Integration verification

Expected:

```text
All EditMode tests Passed.
Failed=0.
Solver known-solution, no-solution, prefix, timeout, cancellation, progress,
provider, service, and Editor action tests all execute.
```

Static:

```powershell
rg -n "UnityEngine|UnityEditor|AssetDatabase" Assets/Scripts/FlowPuzzle/Solving

rg -n "IFlowPuzzleSolver" Assets/Scripts/FlowPuzzle/Generation

rg -n "Task\.Run" Assets/Scripts/FlowPuzzle/Editor
```

Expected: no matches.

## Required response

Use the standard group response with elapsed time and visited-node evidence for
the solver fixtures.
