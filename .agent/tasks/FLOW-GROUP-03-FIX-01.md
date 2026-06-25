# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-03-FIX-01`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-03`

**Depends on:** `FLOW-GENERATION-001`, `FLOW-GENERATION-002`,
`FLOW-GENERATION-003`

**Goal:**

Close the Generation contract gaps found during Codex review without changing
the accepted generation architecture.

**Current context:**

- Start from the clean repository `HEAD` containing this corrective packet.
- Existing Group 3 worker commits are `2981fa9`, `b82d8f2`, and `67d33fa`.
- Codex independently parsed `Logs/FLOW-GROUP-03-p3.xml`: result `Passed`,
  total `181`, passed `181`, failed `0`.
- Passing tests do not complete the packet contract because several required
  cases are absent and two result fields have incorrect semantics.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-03.md`
- `.agent/tasks/FLOW-GROUP-03-FIX-01.md`
- all existing Core, Validation, Difficulty, and Generation source
- all existing EditMode tests
- approved Flow Puzzle spec and implementation plan

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Generation/SystemFlowRandom.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowSolutionGenerator.cs`
- `Assets/Tests/EditMode/Generation/FlowPathLengthAllocatorTests.cs`
- `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`

**Files allowed to create:** `NONE`

**Files forbidden to modify:**

- every file not explicitly listed under “Files allowed to modify”
- all asmdef and `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`

## Required corrections

### 1. Reject an empty floating-point random range

`SystemFlowRandom.NextFloat(minInclusive, maxInclusive)` must throw
`ArgumentOutOfRangeException` when `minInclusive >= maxInclusive`, matching the
packet requirement to reject inverted and empty ranges.

Add an exact test for `NextFloat(3f, 3f)`.

Do not change the public API. `FlowSolutionGenerator` already bypasses
`NextFloat` when the two coverage bounds are equal.

### 2. Prove allocation-result list ownership

Extend the allocation-result success test to mutate the source list after
`Success(...)` returns and prove that:

- `pathLengthsByColorId` is a copy;
- `generationOrderColorIds` remains valid;
- neither result list is the same reference as the input list.

No production change is expected for this correction unless the new test
reveals an actual defect.

### 3. Preserve the resolved seed in immediate failures

Resolve the call seed before configuration validation in
`FlowSolutionGenerator.Generate`.

Every immediate failure must use that resolved seed and `attemptCount == 0`.
For fixed-seed mode, an invalid configuration must therefore report
`config.seed`, not `0`.

Add tests covering at least:

- invalid dimensions;
- invalid coverage range;
- invalid path-length range;
- invalid attempt budget.

Each test must assert the exact error code, fixed seed, and zero attempts.

### 4. Synchronize level difficulty summary

After difficulty evaluation succeeds, copy:

```csharp
levelData.difficulty = difficultyReport.difficulty;
levelData.difficultyScore = difficultyReport.totalScore;
```

The level summary, full difficulty report, asset, and future level-only JSON
must not disagree.

Add assertions that the two `FlowLevelData` fields exactly match the
`FlowDifficultyReport` returned in the same generated result.

### 5. Complete the required generator coverage

Strengthen `FlowSolutionGeneratorTests` with deterministic, practical cases
for the requirements omitted from the original patch:

1. representative `5x5`, `6x6`, and `7x7` configurations succeed;
2. two explicitly chosen representative fixed seeds produce different
   layouts; assert both calls succeed before comparing;
3. the same fixed fixture is generated at least three times and every result
   is deeply equal to the first, including:
   - level ID, seed, coverage;
   - ordered pairs and ordered paths;
   - difficulty tier and total score;
4. target score-range filtering is inclusive:
   - first generate a deterministic baseline;
   - using the same generation inputs, set min and max target score to the
     baseline score and assert success;
   - use a non-containing score range and assert exhaustion at exactly
     `maxLevelAttempt`;
5. keep existing validator, coverage, path-length, color-order, empty-cell,
   target-tier, and impossible-occupancy tests.

Use deterministic fixtures and practical attempt budgets. Do not weaken an
assertion or retry a test externally to hide flakiness.

### 6. Static contract verification

Do not add any solver or Unity random reference. The final static check must
have no matches for:

```text
UnityEngine
UnityEditor
UnityEngine.Random
IFlowPuzzleSolver
```

## Non-goals

- Do not redesign the allocator, DFS strategy, validator, or difficulty
  evaluator.
- Do not add endpoint-distance, detour, bottleneck, batch, persistence,
  diagnostics-advice, solver, async, Application, or Editor behavior.
- Do not change public APIs, serialized field names, assembly references, or
  dependencies.
- Do not refactor or reformat unrelated code.

## Maximum change scope

**Maximum modified production files:** `2`

**Maximum modified test files:** `2`

**Maximum new files:** `0`

**Approximate maximum diff:** `320 changed lines`

## Acceptance criteria

- [ ] `NextFloat` rejects equal bounds.
- [ ] Allocation result list-copy ownership is explicitly tested.
- [ ] Immediate failures preserve fixed seed and zero attempts.
- [ ] Generated `FlowLevelData` difficulty fields match the full report.
- [ ] Required `5x5`, `6x6`, and `7x7` success cases exist.
- [ ] Representative different-seed layout behavior is tested.
- [ ] Three repeated fixed-seed results are deeply equal.
- [ ] Inclusive and rejecting target-score range cases are tested.
- [ ] All existing and new EditMode tests pass.
- [ ] Static prohibited-reference check has no matches.
- [ ] Only the four allowed files change.

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
Test total is greater than 181.
```

Static checks:

```powershell
rg -n "UnityEngine|UnityEditor|UnityEngine.Random|IFlowPuzzleSolver" `
  Assets/Scripts/FlowPuzzle/Generation
```

Expected: no matches.

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: only the four allowed files before commit.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:**

`fix: complete generation result contracts`

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

Return `BLOCKED` with the exact reason. Do not make a partial correction or
create a commit.

## Review record

- Worker commit: `07300ce`
- Changed files: exactly the four allowed files
- Verification XML:
  - result: `Passed`
  - total: `192`
  - passed: `192`
  - failed: `0`
- Production corrections: `ACCEPT`
- Overall task verdict: `NEEDS_FIX`
- Follow-up task: `FLOW-GROUP-03-FIX-02`
- Remaining test-contract findings:
  - generated-level deep comparison omits level IDs, difficulty tier, score,
    pair/path counts, and ordered list comparison;
  - the different-seed “different layouts” test compares `usedSeed`, so it
    can pass even when the actual layouts are identical.

### Final resolution

- Follow-up commit: `c211081`
- Final task verdict: `ACCEPT`
- Final verification: Unity EditMode `192/192` passed.
- Production corrections from `07300ce` and strengthened determinism tests
  from `c211081` are retained.
