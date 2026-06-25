# External DeepSeek Task Group

**Status:** `ACCEPTED`

## Group metadata

**Group ID:** `FLOW-GROUP-01`

**Goal:** Complete the final pure-C# Core foundation by adding all remaining
Core data contracts, `FlowBoard`, and `FlowPathUtility` with EditMode tests.

**Start commit:** The clean repository `HEAD` containing this manifest. Codex
supplies the exact hash in the handoff message.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `FLOW-CORE-002` | `FLOW-CORE-001` | `feat: add flow puzzle core data contracts` |
| 2 | `FLOW-CORE-003` | `FLOW-CORE-002` | `feat: add flow board and path utilities` |

## Group rules

1. Read this manifest and both task packets before changing files.
2. Begin only when `HEAD` equals the required start commit and the working tree
   has no changes except an optional user-approved
   `.claude/settings.local.json` modification.
3. Execute `FLOW-CORE-002` before `FLOW-CORE-003`.
4. Apply each packet's whitelist independently.
5. Follow the red-green test sequence required by each packet.
6. Run packet verification before creating its local commit.
7. Create exactly one local commit per packet using the exact message above.
8. Unity may generate only the `.meta` files explicitly allowed by the current
   packet. Do not write `.meta` files manually.
9. Never push, merge, rebase, amend, squash, reset, or rewrite history.
10. If `FLOW-CORE-002` is blocked or fails verification, do not begin
    `FLOW-CORE-003`.
11. Do not use `FLOW-CORE-003` to repair or rewrite `FLOW-CORE-002`.
12. Never stage or commit `.claude/settings.local.json`.

## Group integration verification

Before running Unity, verify no Unity Editor process has this project open.

```powershell
$unity = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$result = 'D:\Unity\UnityProj\FightMatch\Logs\FLOW-GROUP-01-results.xml'
$log = 'D:\Unity\UnityProj\FightMatch\Logs\FLOW-GROUP-01.log'

$process = Start-Process `
  -FilePath $unity `
  -ArgumentList @(
    '-batchmode',
    '-nographics',
    '-projectPath', 'D:\Unity\UnityProj\FightMatch',
    '-runTests',
    '-testPlatform', 'EditMode',
    '-testResults', $result,
    '-logFile', $log
  ) `
  -Wait `
  -PassThru `
  -WindowStyle Hidden

[xml]$xml = Get-Content -Raw -Encoding UTF8 $result
$run = $xml.'test-run'

"UnityExit=$($process.ExitCode)"
"Result=$($run.result)"
"Total=$($run.total)"
"Passed=$($run.passed)"
"Failed=$($run.failed)"
```

Expected:

```text
UnityExit=0
Result=Passed
Failed=0
```

Also run:

```powershell
rg -n "UnityEngine|UnityEditor" Assets/Scripts/FlowPuzzle/Core
git diff HEAD~2..HEAD --check
git status --short
```

Expected:

```text
No UnityEngine or UnityEditor matches.
No diff-check errors.
Clean working tree after the two packet commits.
```

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-GROUP-01
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- FLOW-CORE-002: COMPLETED | BLOCKED
  Commit: <hash or N/A>
  Verification: <red/green and final test evidence>
- FLOW-CORE-003: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <red/green and final test evidence>

GROUP VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <Unity exit, total, passed, failed>

CHANGED FILES BY PACKET:
- FLOW-CORE-002
  - <path>
- FLOW-CORE-003
  - <path>

BLOCKER:
<required for PARTIAL or BLOCKED>
```

## Completion record

- Start commit: `7a007d967ff959c1fa5e828082cbd674dea8b00d`
- `FLOW-CORE-002` commit: `865715e82cef7c996453453f7b1e221cb6b84011`
- `FLOW-CORE-003` commit: `994c1f143db2276b98fad5335b50d5fdf4a7ec10`
- Corrective test commit: `aca299c5e5f4e67d1540cbaf2414d5f88dd896cd`
- `FLOW-CORE-002` verdict: `ACCEPT`
- `FLOW-CORE-003` verdict: `ACCEPT`
- Group verdict: `ACCEPT`
- Independent verification:
  - Unity EditMode result: `Passed`
  - Total: `78`
  - Passed: `78`
  - Failed: `0`
  - C# compiler errors: `0`
  - Core Unity references: none
- `.claude/settings.local.json` remained uncommitted as user-approved local
  permission state.
- The worker's returned report omitted the later corrective commit. Codex
  independently reviewed it: the original L-shaped path had zero detour under
  `moves - endpoint Manhattan distance`; the corrected fixture has four moves,
  endpoint distance two, and expected detour two.
