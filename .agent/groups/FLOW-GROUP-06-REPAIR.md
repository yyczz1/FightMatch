# External DeepSeek Corrective Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-06-REPAIR`

**Goal:** Repair the rejected Group 6 delivery so Draft mutation, command
history, UI integration, fixed constraints, rendering, persistence, and tests
meet the approved Phase 6 contract.

**Start commit:** Supplied by Codex after all remaining-work documents are
committed.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Commit |
|---:|---|---|---|
| 1 | `FLOW-GROUP-06-FIX-01` | rejected Group 6 | `fix: complete draft and command contracts` |
| 2 | `FLOW-GROUP-06-FIX-02` | `FLOW-GROUP-06-FIX-01` | `fix: connect draft editor workflow` |
| 3 | `FLOW-GROUP-06-FIX-03` | `FLOW-GROUP-06-FIX-02` | `fix: complete fixed constraint editing` |

## Group rules

1. Existing Group 6 code is salvageable but not accepted.
2. The two untracked test `.meta` files are explicitly assigned to Fix 01 and
   must be tracked in that commit.
3. Keep fixes surgical. Do not rewrite accepted automatic generation code.
4. Do not add a Solver, async work, QFramework, Runtime interaction, or model
   API.
5. Run all EditMode tests after every packet.
6. Stop before Group 7 if any requirement remains unverified.

## Integration verification

Expected:

```text
All EditMode tests Passed.
Failed=0.
Test total greater than 295.
No unexpected working-tree files.
Assets/Temp and Assets/Temp.meta absent.
```

Static:

```powershell
rg -n "IFlowPuzzleSolver|Task\.Run|System\.Threading|QFramework|void OnGUI\s*\(" `
  Assets/Scripts/FlowPuzzle/Editor

git diff --check <START-COMMIT>..HEAD

git status --short
```

## Required response

Use the standard group response and list exact evidence for every previously
missing Editor and constraint behavior.
