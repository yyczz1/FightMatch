# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-04-FIX-02`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-04`

**Depends on:** `FLOW-GROUP-04-FIX-01`, `FLOW-BATCH-001`

**Goal:** Strengthen batch-generation tests so they prove complete
reproducibility, exact overflow seed semantics, failure messages, and
instance-owned results.

**Current context:**

- Start after accepted-for-execution `FLOW-GROUP-04-FIX-01`.
- Existing batch implementation commit is `793f22d`.
- Current batch tests compare only level ID, seed, and success for repeated
  requests; they do not compare generated layouts or difficulty data.
- No production defect has been established. This packet is test-only unless
  the stronger tests expose a real defect; if that happens, return `BLOCKED`
  rather than modifying production outside scope.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- project AI rules and all Group 4 documents
- Core and Generation source
- all Generation EditMode tests

**Files allowed to modify:**

- `Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs`

**Files allowed to create:** `NONE`

**Files forbidden to modify:**

- all production code
- every other test, asmdef, and `.meta`
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

## Required corrections

### 1. Exact batch deep equality

Replace the weak `Generate_SameRequest_DeepllyEqual` assertions with an exact
ordered comparison of both reports.

Compare:

- requested, successful, and failed counts;
- item count and every corresponding item's level ID, used seed, success, and
  message;
- generation result success, level ID, used seed, attempt count, and
  diagnostic code/message;
- on success, generated level seed, coverage, both level IDs, dimensions,
  difficulty tier and total score, ordered pairs, and ordered paths including
  every cell.

Rename the misspelled test to `Generate_SameRequest_DeeplyEqual`.

### 2. Seed overflow and negative case

Add an exact derived-seed case using representative negative values and an
overflow boundary. Expected values must use the same explicit formula:

```csharp
unchecked(baseSeed + levelId * 9973)
```

Do not merely repeat the existing small positive case.

### 3. Full request immutability

Snapshot and compare every field of `FlowGenerationConfig`, plus request
`startLevelId`, `count`, `baseSeed`, and the original config reference.

After generation, all values and references must be unchanged.

### 4. Failure report contract

For an impossible configuration, assert every item:

- remains in ascending level order;
- has the exact derived seed;
- has a non-null generation result and diagnostic;
- has `success == false`;
- has a message containing both diagnostic error code and error message.

Assert aggregate counts exactly equal requested count and zero successes.

### 5. Instance ownership

Generate the same request twice and assert:

- report instances differ;
- `items` list instances differ;
- corresponding item instances differ;
- corresponding `generationResult` instances differ;
- mutating one report's items list does not affect the other report.

Do not mutate a generated level's nested content merely to test ownership.

## Non-goals

- No production code changes.
- No persistence, asset, JSON, parallelism, async, progress, UI, solver, or
  new abstraction.
- No broad reformatting or unrelated test rewrites.

## Maximum change scope

- Modified files: `1`
- New files: `0`
- Approximate maximum diff: `260` changed lines

## Acceptance criteria

- [ ] Repeated batches compare complete ordered generation data.
- [ ] Negative and overflowing seed derivation are tested exactly.
- [ ] Every request/config field is proven unchanged.
- [ ] Failure items prove message and count contracts.
- [ ] Reports, lists, items, and results are instance-owned.
- [ ] All EditMode tests pass.
- [ ] Only the single allowed test file changes.

## Verification steps

Before Unity:

```powershell
Get-Process Unity -ErrorAction SilentlyContinue
```

Expected: no Unity process. If one exists, return `BLOCKED`; do not launch or
kill another process.

Run all EditMode tests once using `.agent/VALIDATION.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
Total is not lower than the result after FLOW-GROUP-04-FIX-01.
```

Static:

```powershell
rg -n "DeepllyEqual" `
  Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs
```

Expected: no matches.

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: only the single allowed test file before commit.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `test: strengthen batch generation contracts`

Create one local commit after verification. Never push, amend, rebase, squash,
merge, reset, or include `.claude/settings.local.json`.

## Expected output format

Use the repository task packet response contract.

## What to do if blocked

Return `BLOCKED` with the exact reason and no commit.
