# External DeepSeek Task Packet

Copy this template, fill every required field, and send it together with
[`DEEPSEEK_WORKER_PROMPT.md`](DEEPSEEK_WORKER_PROMPT.md).

One packet must perform one coherent task.

---

## Task metadata

**Task ID:** `<AREA-NNN>`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `<GROUP-ID or NONE>`

**Order in group:** `<number or N/A>`

**Depends on:** `<task IDs or NONE>`

**Goal:**

`<One sentence describing the single result to produce.>`

**Background:**

`<Why this task exists. Keep it short and task-specific.>`

**Current context:**

`<Verified facts needed to do this task. Do not paste the full project history.>`

## Scope whitelist

**Files allowed to read:**

- `<path>`

**Files allowed to modify:**

- `<existing path>`

**Files allowed to create:**

- `<new path, or NONE>`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `Packages/**`
- `ProjectSettings/**`
- `<task-specific forbidden paths>`

Anything not explicitly allowed to modify or create is forbidden.

## Required behavior

1. `<Required behavior>`
2. `<Required behavior>`

## Non-goals

- `<Explicitly excluded work>`
- No unrelated refactoring, cleanup, renaming, formatting, comments, or optimization.

## Constraints

- Preserve existing behavior outside the required change.
- Match existing repository style.
- Do not introduce speculative abstractions.
- Do not add dependencies.
- Do not modify task-external files.
- If required work exceeds this packet, return `BLOCKED`.

## Project-specific constraints

- `<Relevant Unity/framework/runtime/serialization/threading constraint>`
- `<Reference an approved spec section if needed>`

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `NO` | `<NONE or exact symbol>` |
| New dependency/package | `NO` | `<NONE or exact dependency>` |
| Build/configuration changes | `NO` | `<NONE or exact file/key>` |
| Lockfile changes | `NO` | `<NONE or exact file>` |
| CI changes | `NO` | `<NONE or exact file>` |
| Database/schema migration | `NO` | `<NONE or exact migration>` |
| Serialized format changes | `NO` | `<NONE or exact field/type>` |
| Unity asset or `.meta` changes | `NO` | `<NONE or exact asset>` |
| Generated-file changes | `NO` | `<NONE or exact file>` |

`NO` means stop with `BLOCKED` if the task appears to require that change.

## Maximum change scope

**Maximum changed production files:** `<number>`

**Maximum changed test files:** `<number>`

**Maximum new files:** `<number>`

**Approximate maximum diff:** `<number> changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Git checkpoint permission

**Local commit allowed:** `YES | NO`

**Required commit message:** `<exact message or N/A>`

When `YES`, commit only this packet after its verification passes. Never push,
merge, rebase, amend, squash, reset history, or include another packet's work.

## Acceptance criteria

- [ ] `<Observable criterion>`
- [ ] `<Observable criterion>`
- [ ] No files outside the whitelist changed.
- [ ] No unapproved protected change was made.

## Verification steps

Run only applicable commands:

```text
<exact command>
```

Expected result:

```text
<specific pass condition>
```

If a command cannot run, report `NOT RUN` and the concrete reason. Do not replace it with a claim.

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <exit code / test count / concise evidence>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO
- Protected changes made: YES/NO

PATCH:
<unified diff preferred; if edits were applied directly, provide a concise diff summary>

LOCAL COMMIT:
- Created: YES/NO
- Hash: <hash or N/A>
- Message: <message or N/A>

BLOCKER:
<required only when STATUS is BLOCKED>
```

Do not include architecture essays or unrelated suggestions.

## What to do if blocked

Return:

```text
BLOCKED
Reason:
Missing information:
Required decision:
Files inspected:
No changes made: Yes/No
```

Do not guess. Do not create a partial out-of-scope solution.
