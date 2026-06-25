# External DeepSeek Task Group

Use this manifest to send multiple ordered, already-approved task packets to
one external DeepSeek session.

## Group metadata

**Group ID:** `<GROUP-ID>`

**Goal:** `<One coherent group outcome>`

**Start commit:** `<required repository HEAD>`

**Execution mode:** `SEQUENTIAL`

**Local commits allowed:** `YES | NO`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Required commit message |
|---:|---|---|---|
| 1 | `<TASK-ID>` | `NONE` | `<message>` |
| 2 | `<TASK-ID>` | `<TASK-ID>` | `<message>` |

## Group rules

1. Read the manifest and every packet before changing files.
2. Begin only when `HEAD` equals the required start commit and the working tree
   is clean.
3. Execute packets in order.
4. Apply each packet's whitelist independently.
5. Run packet verification before creating its local commit.
6. Create exactly one local commit per packet when authorized.
7. Never push, merge, rebase, amend, squash, reset, or rewrite history.
8. If a packet is blocked or fails verification, stop every dependent packet.
9. Do not use a later packet to repair an earlier packet.
10. Run group integration verification only after all packets complete.

## Group integration verification

```text
<exact commands>
```

Expected:

```text
<specific pass condition>
```

## Required group response

```text
GROUP STATUS: COMPLETED | PARTIAL | BLOCKED
GROUP ID: <id>
START COMMIT: <hash>
END COMMIT: <hash or N/A>

PACKET RESULTS:
- <task id>: COMPLETED | BLOCKED
  Commit: <hash or N/A>
  Verification: <concise result>

GROUP VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <evidence>

CHANGED FILES BY PACKET:
- <task id>
  - <path>

BLOCKER:
<required for PARTIAL or BLOCKED>
```
