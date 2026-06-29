# External DeepSeek Final Verification Group

**Status:** `APPROVED_FOR_WORKER`

## Group metadata

**Group ID:** `FLOW-GROUP-09`

**Goal:** Add final cross-module regression matrices and perform complete
compile, test, static, performance, and Editor acceptance verification.

**Depends on:** accepted `FLOW-GROUP-08`

**Execution mode:** `SEQUENTIAL`

**Push allowed:** `NO`

## Ordered packets

| Order | Task ID | Depends on | Commit |
|---:|---|---|---|
| 1 | `FLOW-FINAL-TESTS-001` | Group 8 | `test: add final flow puzzle acceptance matrix` |
| 2 | `FLOW-FINAL-VERIFY-001` | `FLOW-FINAL-TESTS-001` | no commit |

## Group rules

1. Packet 1 may add tests only; it cannot repair production behavior.
2. A production failure returns `BLOCKED` for later Codex review.
3. Packet 2 is verification-only and must not change any file.
4. Do not claim manual Editor actions that were not actually performed.
5. Never weaken, delete, ignore, or skip an existing test to make the matrix
   pass.

## Final acceptance

Required:

- compile exit 0;
- all EditMode tests passed, zero failed/skipped;
- deterministic generation matrix deeply equal;
- solver matrix records status, elapsed time, and visited nodes;
- timeout/cancellation responsive;
- prohibited dependency checks clean;
- Editor smoke checklist either fully RUN or explicitly NOT RUN/PARTIAL.

## Required response

Return the standard group response plus the complete acceptance table from
`FLOW-FINAL-VERIFY-001`.
