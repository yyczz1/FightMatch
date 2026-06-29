# External DeepSeek Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-08`

**Goal:** Complete stable diagnostics, actionable suggestions, retry actions,
and the pure local solver tool contract intended for future model orchestration.

**Depends on:** accepted `FLOW-GROUP-07`

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Commit |
|---:|---|---|---|
| 1 | `FLOW-DIAGNOSTICS-001` | Group 7 | `feat: add stable flow diagnostics` |
| 2 | `FLOW-EDITOR-RETRY-001` | `FLOW-DIAGNOSTICS-001` | `feat: add diagnostic retry actions` |
| 3 | `FLOW-SOLVER-TOOL-001` | `FLOW-DIAGNOSTICS-001` | `feat: add local solver tool adapter` |

## Group rules

1. Diagnostic codes are stable constants, not ad-hoc UI strings.
2. Suggestions identify a parameter and direction without claiming a
   probabilistic cause is certain.
3. Apply Suggestion is enabled only for one unambiguous safe scalar edit.
4. Retry actions do not change unrelated configuration.
5. Tool contracts are pure DTOs and local calls only.
6. Do not implement HTTP, API keys, model choice, prompt logic, or an LLM
   provider.

## Integration verification

Run all EditMode tests. Expected Passed, zero failed.

Static:

```powershell
rg -n "HttpClient|UnityWebRequest|api.?key|OpenAI|DeepSeek|Claude" `
  Assets/Scripts/FlowPuzzle/Solving/Tools

rg -n "UnityEngine|UnityEditor" Assets/Scripts/FlowPuzzle/Solving/Tools
```

Expected: no matches.

## Required response

Use the standard group response and list each stable diagnostic code exercised
by tests.
