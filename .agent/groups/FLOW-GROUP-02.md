# External DeepSeek Task Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-02`

**Goal:** Add deterministic recommendation validation and exact difficulty
scoring on top of the accepted Core foundation.

**Start commit:** The clean repository `HEAD` containing this manifest. Codex
supplies the exact hash in the handoff message.

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `FLOW-VALIDATION-001` | `FLOW-GROUP-01` | `feat: validate flow puzzle solutions` |
| 2 | `FLOW-DIFFICULTY-001` | `FLOW-VALIDATION-001` | `feat: score flow puzzle difficulty` |

The second packet depends on the first only for group ordering and integration
stability. `FlowPuzzle.Difficulty` must not reference
`FlowPuzzle.Validation`.

## Group rules

1. Read this manifest and both packets before changing files.
2. Begin only when `HEAD` equals the supplied start commit.
3. The working tree may contain only a user-approved
   `.claude/settings.local.json` modification.
4. Execute packets in order and keep their diffs separate.
5. Follow each packet's red-green sequence.
6. Run packet verification before its local commit.
7. Create exactly one local commit per packet using the required message.
8. Never stage or commit `.claude/settings.local.json`.
9. Unity may generate only `.meta` files explicitly permitted by the packet.
10. Never push, merge, rebase, amend, squash, reset, or rewrite history.
11. If validation is blocked or fails verification, do not begin difficulty.
12. A later packet may not repair or rewrite an earlier packet.
13. Never launch Unity batch mode while another Unity process has this project
    open. Do not launch several Unity verification processes concurrently.

## Group integration verification

Before running Unity:

```powershell
Get-Process Unity -ErrorAction SilentlyContinue |
  Select-Object Id, MainWindowTitle, StartTime, Path
```

Expected: no Unity process using this project. If any exist, stop and report
`BLOCKED`; do not kill them.

Run one test process and wait for it:

```powershell
$unity = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$result = 'D:\Unity\UnityProj\FightMatch\Logs\FLOW-GROUP-02-results.xml'
$log = 'D:\Unity\UnityProj\FightMatch\Logs\FLOW-GROUP-02.log'

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
rg -n "UnityEngine|UnityEditor" `
  Assets/Scripts/FlowPuzzle/Validation `
  Assets/Scripts/FlowPuzzle/Difficulty

git diff HEAD~2..HEAD --check

git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: no Unity references, no non-`.meta` diff-check errors, and no
unexpected working-tree changes.

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: FLOW-GROUP-02
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- FLOW-VALIDATION-001: COMPLETED | BLOCKED
  Commit: <hash or N/A>
  Verification: <red/green and final test evidence>
- FLOW-DIFFICULTY-001: COMPLETED | BLOCKED | NOT RUN
  Commit: <hash or N/A>
  Verification: <red/green and final test evidence>

GROUP VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <Unity exit, total, passed, failed>

CHANGED FILES BY PACKET:
- FLOW-VALIDATION-001
  - <path>
- FLOW-DIFFICULTY-001
  - <path>

BLOCKER:
<required for PARTIAL or BLOCKED>
```
