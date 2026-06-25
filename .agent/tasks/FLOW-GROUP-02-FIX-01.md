# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-GROUP-02-FIX-01`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-02`

**Depends on:** `FLOW-VALIDATION-001`, `FLOW-DIFFICULTY-001`

**Goal:**

Make validator first-error selection fully deterministic and replace weak
difficulty tests with exact bottleneck and tier-boundary verification.

**Current context:**

- Start from the clean repository `HEAD` containing this corrective packet.
- Existing worker commits are `8c7ed5b` and `d85f24a`.
- Existing Unity verification reports `127/127` passing, but review found
  uncovered specification gaps.
- `.claude/settings.local.json` is user-managed local state. Never stage,
  commit, reset, or report it as a task change.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-02.md`
- `.agent/tasks/FLOW-GROUP-02-FIX-01.md`
- `Assets/Scripts/FlowPuzzle/Core/**`
- `Assets/Scripts/FlowPuzzle/Validation/**`
- `Assets/Scripts/FlowPuzzle/Difficulty/**`
- `Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs`
- `Assets/Tests/EditMode/Difficulty/FlowDifficultyEvaluatorTests.cs`

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs`
- `Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs`
- `Assets/Scripts/FlowPuzzle/Difficulty/FlowDifficultyEvaluator.cs`
- `Assets/Tests/EditMode/Difficulty/FlowDifficultyEvaluatorTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- all Core files and tests
- all asmdef and `.meta` files
- `AGENTS.md`
- `.agent/**`
- `.claude/settings.local.json`
- `Packages/**`
- `ProjectSettings/**`
- `docs/**`
- every file not explicitly listed under “Files allowed to modify”

## Required corrections

### 1. Deterministic extra-path selection

In `FlowSolutionValidator`, do not enumerate `pathColorSet` when deciding the
first `ExtraPathColor`.

Iterate `solution.paths` in list order and return `ExtraPathColor` for the first
path whose color ID has no pair.

Add a test with two unknown path colors and assert the error message identifies
the first unknown path in list order.

### 2. Per-color error ordering

After global pair/path cardinality checks, validate each color completely in
`level.pairs` list order:

```text
PathTooShort
EndpointMismatch
CellOutOfBounds
InvalidAdjacency
SelfIntersection
ForeignEndpointTraversal
PathOverlap
```

Do not validate every path's geometry first and defer foreign endpoint/overlap
checks until after all colors.

Build endpoint lookup and global occupied-cell lookup before the pair loop, but
apply foreign-endpoint and overlap checks for the current pair before moving to
the next pair.

Within a cell, check `InvalidAdjacency` before `SelfIntersection` so the listed
priority remains stable when both are true.

Add tests proving:

1. an earlier color's `ForeignEndpointTraversal` is returned before a later
   color's `InvalidAdjacency`;
2. a repeated cell reached through an invalid step returns
   `InvalidAdjacency`, not `SelfIntersection`.

Remove the unused `occupiedCells` local.

### 3. Exact bottleneck tests

Replace the three assertions of `bottleneckCount >= 0` with exact cases:

- `2x1` board whose only occupied cells are both endpoints:
  `bottleneckCount == 0`;
- `3x1` straight path with endpoints at both ends and one occupied middle
  candidate: `bottleneckCount == 1`;
- `2x2` board with the bottom row occupied by endpoint cells and two empty top
  cells: `bottleneckCount == 2`.

Keep the weight test asserting:

```text
bottleneckScore == bottleneckCount * 1.5
```

### 4. Exact tier boundaries

Extract the existing tier selection into a `private static` method:

```csharp
private static FlowDifficultyTier ClassifyScore(float totalScore)
```

`Evaluate(...)` must use this method. Do not add a public or internal API.

In tests, use a small private reflection helper to invoke this private method
and assert:

```text
59.999f  => Easy
60f      => Normal
119.999f => Normal
120f     => Hard
199.999f => Hard
200f     => Expert
```

Existing public-evaluation tier smoke tests may remain, but rename misleading
test names and comments so they do not claim an exact boundary unless they
actually test one.

### 5. Diff hygiene

- Remove the unused `pathByColor` dictionary from
  `FlowDifficultyEvaluator`.
- Correct the stale “3 turns” comment for the path that has four direction
  changes.
- Do not otherwise refactor or reformat the files.

## Non-goals

- Do not change validator error codes, public APIs, scoring formulas,
  bottleneck semantics, weights, or tier thresholds.
- Do not modify asmdefs, Core contracts, metadata, packages, settings, or docs.
- Do not add dependencies or new files.
- Do not change behavior merely to make an existing weak test pass.

## Maximum change scope

**Maximum modified production files:** `2`

**Maximum modified test files:** `2`

**Maximum new files:** `0`

**Approximate maximum diff:** `260 changed lines`

## Acceptance criteria

- [ ] `ExtraPathColor` follows path list order.
- [ ] Per-color first-error priority matches the required order.
- [ ] New validator precedence tests pass.
- [ ] Bottleneck tests assert exact counts.
- [ ] Exact `60`, `120`, and `200` boundaries are tested.
- [ ] No public API is added.
- [ ] Unused locals are removed.
- [ ] All existing tests plus new tests pass.
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
Test total is greater than 127.
```

Static checks:

```powershell
rg -n "bottleneckCount >= 0|foreach \(var colorId in pathColorSet\)|occupiedCells|pathByColor" `
  Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs `
  Assets/Scripts/FlowPuzzle/Difficulty/FlowDifficultyEvaluator.cs `
  Assets/Tests/EditMode/Difficulty/FlowDifficultyEvaluatorTests.cs
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

`fix: enforce validation order and difficulty boundaries`

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

Return `BLOCKED` with the exact reason. Do not make partial fixes or a commit.

## Completion record

- Worker status: `COMPLETED`
- Commit: `009b0abc006bfcc4acaae8b3561d041def90b1ae`
- Codex verdict: `ACCEPT`
- Changed files: exactly the four allowed files
- Verification:
  - XML result `Passed`
  - total `136`
  - passed `136`
  - failed `0`
  - no prohibited static-check matches
