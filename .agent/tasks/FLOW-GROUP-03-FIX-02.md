# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-03-FIX-02`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-03`

**Depends on:** `FLOW-GROUP-03-FIX-01`

**Goal:**

Make the Generation determinism tests actually prove the metadata, ordering,
and layout properties required by `FLOW-GROUP-03-FIX-01`.

**Current context:**

- Start from the clean repository `HEAD` containing this packet.
- Corrective implementation commit `07300ce` is retained.
- Codex independently parsed `Logs/FLOW-GROUP-03-FIX.xml`: result `Passed`,
  total `192`, passed `192`, failed `0`.
- The production corrections in `07300ce` passed review.
- Review found that the test comparison helpers are weaker than their test
  names and required assertions.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-03.md`
- `.agent/tasks/FLOW-GROUP-03-FIX-01.md`
- `.agent/tasks/FLOW-GROUP-03-FIX-02.md`
- all Generation source and Generation EditMode tests
- Core DTO source needed to inspect compared fields

**Files allowed to modify:**

- `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`

**Files allowed to create:** `NONE`

**Files forbidden to modify:**

- all production code
- every other test
- all asmdef and `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

## Required corrections

### 1. Make deep equality exact and ordered

Replace the current weak `DeepEquals(FlowGeneratedLevel, FlowGeneratedLevel)`
implementation.

It must return false unless both generated levels match in all of these
required properties:

- `usedSeed`;
- `coverageRatio`;
- `levelData.levelId`;
- `solutionData.levelId`;
- width and height;
- `levelData.difficulty`;
- `levelData.difficultyScore`;
- `difficultyReport.difficulty`;
- `difficultyReport.totalScore`;
- equal pair counts and pair equality at every corresponding list index;
- equal path counts and path equality at every corresponding list index.

Do not use `All(... Any(...))`; it ignores list order and can ignore extra
elements. Compare counts and corresponding indexes explicitly.

The existing `Generate_SameSeed_3x_DeeplyEqual` test must continue using this
exact helper.

### 2. Make the different-seed test compare layouts only

The current `Generate_DifferentSeeds_DifferentLayouts` assertion uses a helper
that includes `usedSeed`. It therefore passes merely because `42 != 123`,
even if both generated layouts are identical.

Add a small private `LayoutsEqual` helper, or equivalent direct assertions,
that compares only actual layout content:

- board width and height;
- ordered pairs, including endpoints;
- ordered solution paths, including every cell.

It must ignore `usedSeed`, coverage metadata, and difficulty metadata.

Update `Generate_DifferentSeeds_DifferentLayouts` to assert both generations
succeed and this layout-only comparison is false.

### 3. Preserve scope

- Do not change production code.
- Do not change generation fixtures or chosen seeds unless the corrected
  layout-only assertion demonstrates that the existing representative seeds
  really generate the same layout.
- Do not broadly reorganize, reformat, or rewrite other tests.

## Non-goals

- No new production behavior.
- No new APIs, dependencies, files, fixtures, or architecture.
- No changes to random, allocator, DFS, validator, difficulty, or generator
  behavior.

## Maximum change scope

**Maximum modified files:** `1`

**Maximum new files:** `0`

**Approximate maximum diff:** `100 changed lines`

## Acceptance criteria

- [ ] Fixed-seed deep equality includes every required metadata field.
- [ ] Fixed-seed deep equality compares pair/path counts and list order.
- [ ] Different-seed layout comparison cannot pass merely because seeds differ.
- [ ] Existing representative different seeds produce different actual layouts.
- [ ] All EditMode tests pass.
- [ ] Only `FlowSolutionGeneratorTests.cs` changes.

## Verification steps

Before Unity:

```powershell
Get-Process Unity -ErrorAction SilentlyContinue
```

Expected: no Unity process. If any exists, return `BLOCKED`; do not launch
another process and do not kill it.

Run all EditMode tests once using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Total=192 or greater.
```

Static inspection:

```powershell
rg -n "pairs\.All|paths\.All|usedSeed.*DifferentSeeds" `
  Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs
```

Expected: no weak unordered deep-comparison implementation and no seed-only
different-layout comparison.

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: only the single allowed test file before commit.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:**

`test: strengthen generation determinism checks`

Create one local commit after verification. Never push, amend, rebase, squash,
merge, reset, or include `.claude/settings.local.json`.

## Expected output format

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <Unity exit / total / passed / failed>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO

PATCH:
<concise diff summary>

LOCAL COMMIT:
- Created: YES/NO
- Hash: <hash or N/A>
- Message: <message or N/A>

BLOCKER:
<required only when STATUS is BLOCKED>
```

## What to do if blocked

Return `BLOCKED` with the exact reason. Do not create a partial commit.

## Completion record

- Worker commit: `c211081`
- Changed files: exactly
  `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`
- Codex verdict: `ACCEPT`
- Verification XML: `Logs/FLOW-FIX-02.xml`
  - result: `Passed`
  - total: `192`
  - passed: `192`
  - failed: `0`
  - failed test cases: `0`
  - failed suites: `0`
- Static weak-comparison check: no matches.
- Exact ordered generated-level comparison and layout-only different-seed
  comparison passed review.
