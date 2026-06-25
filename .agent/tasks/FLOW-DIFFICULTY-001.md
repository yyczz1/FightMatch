# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-DIFFICULTY-001`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-02`

**Order in group:** `2`

**Depends on:** `FLOW-VALIDATION-001`

**Goal:**

Implement deterministic difficulty metrics, weighted scoring, and tier
classification for an already-valid Flow Puzzle recommendation.

**Background:**

Core data and path utilities are accepted. Validation is completed in the
previous packet. Difficulty remains a separate pure-C# assembly that depends
only on Core and evaluates the supplied recommendation without searching for
other solutions.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-02.md`
- `.agent/tasks/FLOW-DIFFICULTY-001.md`
- `Assets/Scripts/FlowPuzzle/Core/**`
- `Assets/Scripts/FlowPuzzle/Validation/**`
- existing EditMode tests
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

**Files allowed to modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Difficulty/FlowPuzzle.Difficulty.asmdef`
- `Assets/Scripts/FlowPuzzle/Difficulty/FlowDifficultyEvaluator.cs`
- `Assets/Tests/EditMode/Difficulty/FlowDifficultyEvaluatorTests.cs`
- Unity-generated `.meta` files for the new Difficulty and test folders
- Unity-generated `.meta` files corresponding to the three new files

**Files forbidden to modify:**

- `Assets/Scripts/FlowPuzzle/Core/**`
- `Assets/Scripts/FlowPuzzle/Validation/**`
- existing tests
- existing `.meta` files
- `AGENTS.md`
- `.agent/**`
- `Packages/**`
- `ProjectSettings/**`
- `docs/**`
- every file not explicitly allowed above

Do not write `.meta` files manually.

## Required behavior

### Assembly

Create `FlowPuzzle.Difficulty`:

- root namespace `FlowPuzzle.Difficulty`;
- references only `FlowPuzzle.Core`;
- `noEngineReferences: true`;
- must not reference `FlowPuzzle.Validation`.

Add `FlowPuzzle.Difficulty` to the existing test asmdef while preserving the
Core and Validation references.

### Evaluator API

```csharp
public sealed class FlowDifficultyEvaluator
{
    public FlowDifficultyReport Evaluate(
        FlowLevelData level,
        FlowSolutionData solution);
}
```

The evaluator assumes a recommendation has already passed validation.
It must:

- throw `ArgumentNullException` for null level or solution;
- throw `ArgumentOutOfRangeException` for non-positive dimensions;
- throw `ArgumentException` when `level.pairs`, `solution.paths`, or any
  `path.cells` collection is null;
- not call or depend on `FlowSolutionValidator`.

### Metrics

Build one `FlowBoard` from the supplied paths.

Compute:

```text
boardCellCount = width * height
colorCount = level.pairs.Count
usedCellCount = number of distinct path cells
coverageRatio = usedCellCount / boardCellCount
totalTurnCount = sum FlowPathUtility.CountTurns(path.cells)
totalDetour = sum FlowPathUtility.GetDetour(path.cells)
totalEndpointManhattanDistance =
    sum distance(endpointA, endpointB) for all pairs
```

Interaction:

- inspect only right and up neighbors to count each undirected pair once;
- both cells must be occupied;
- colors must differ;
- each qualifying neighboring cell pair increments
  `differentColorAdjacentCount` once.

Bottleneck semantics:

- endpoint coordinates are excluded from bottleneck candidates;
- inspect every other board coordinate exactly once;
- for an empty candidate cell, a traversable neighbor is an in-bounds empty
  cell;
- for an occupied candidate cell of color `C`, a traversable neighbor is an
  in-bounds empty cell or a cell occupied by color `C`;
- a candidate is a bottleneck when it has at most two traversable orthogonal
  neighbors;
- count empty and occupied candidates;
- use deterministic right, left, up, down neighbor inspection;
- `bottleneckCount` is a metric only and does not invalidate a solution.

### Scores

Use exactly:

```text
boardSizeScore = width * height * 0.25
colorCountScore = colorCount * 6
coverageScore = coverageRatio * 45
turnScore = totalTurnCount * 2.5
detourScore = totalDetour * 2
interactionScore = differentColorAdjacentCount * 1.5
endpointDistanceScore = totalEndpointManhattanDistance * 1.2
bottleneckScore = bottleneckCount * 1.5

totalScore =
    boardSizeScore
  + colorCountScore
  + coverageScore
  + turnScore
  + detourScore
  + interactionScore
  + endpointDistanceScore
  + bottleneckScore
```

Populate every corresponding field already present in
`FlowDifficultyReport`.

Tier boundaries:

```text
totalScore < 60       => Easy
totalScore < 120      => Normal
totalScore < 200      => Hard
otherwise             => Expert
```

Do not round stored scores. Tests may compare floats with tolerance.

### Tests

Use hand-built valid levels and solutions. Cover:

1. exact board-size and color-count scores;
2. exact coverage ratio contribution;
3. straight path turn count and score;
4. L and multi-turn paths;
5. detour metric and score;
6. endpoint Manhattan metric and score;
7. different-color adjacency counted once per undirected pair;
8. bottleneck semantics for empty, occupied, and endpoint cells;
9. exact total equals the sum of all components;
10. boundaries immediately below/at `60`, `120`, and `200`;
11. null and malformed gross input exceptions;
12. repeat evaluation returns deeply equal report values;
13. evaluator assembly has no Validation reference.

For threshold tests, use a small internal test helper that constructs report
inputs or a private tier-classification scenario through public evaluation.
Do not add production APIs solely to make threshold tests easy.

## Non-goals

- No unique-solution analysis.
- No search, solver, validation call, generation, persistence, presets, JSON,
  Application service, or Editor UI.
- No score normalization, clamping, rounding, configurable weights, Scriptable
  Objects, or interfaces.
- Do not change Core report fields or validator behavior.
- No unrelated formatting, cleanup, comments, or renaming.

## Constraints

- Production code may use only `System`, `System.Collections.Generic`,
  `FlowPuzzle.Core`, and its own namespace.
- No `UnityEngine`, `UnityEditor`, or `FlowPuzzle.Validation` reference.
- Deterministic loops only.
- Do not use dictionary/hash-set enumeration order to affect results.
- Reuse `FlowBoard` and `FlowPathUtility`; do not duplicate their logic.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `YES` | Create only `FlowDifficultyEvaluator.Evaluate` |
| New dependency/package | `NO` | None |
| Build/configuration changes | `YES` | Create Difficulty asmdef and add its reference to test asmdef |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `NO` | Use existing `FlowDifficultyReport` only |
| Unity asset or `.meta` changes | `YES` | Listed source, asmdef, test, folders, and generated metas |
| Generated-file changes | `NO` | No Library, Temp, Logs, csproj, or sln files committed |

## Maximum change scope

**Maximum changed production files:** `0`

**Maximum changed test/config files:** `1`

**Maximum new files:** `10` including generated folder/file metas

**Approximate maximum diff:** `800 changed lines` excluding `.meta`

## Acceptance criteria

- [ ] Difficulty assembly references only Core and has no engine references.
- [ ] Every metric follows the exact specified semantics.
- [ ] Every weighted score field is populated.
- [ ] Tier boundaries are exact.
- [ ] Evaluation is deterministic.
- [ ] Evaluator does not call or reference Validation.
- [ ] Focused difficulty tests pass.
- [ ] All earlier tests remain passing.
- [ ] Only the allowed test asmdef is modified.
- [ ] No file outside the whitelist changes.

## Verification steps

### Red verification

Create the test file and test asmdef reference before production
implementation.

Expected: compile failure naming missing Difficulty types. Record a short
excerpt.

### Green verification

```powershell
rg -n "UnityEngine|UnityEditor|FlowPuzzle.Validation" `
  Assets/Scripts/FlowPuzzle/Difficulty
```

Expected: no matches.

Run all EditMode tests with the single-process command in
`.agent/groups/FLOW-GROUP-02.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
All prior Core and Validation tests remain passing.
```

Before commit:

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: only packet-approved files.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: score flow puzzle difficulty`

Commit only this packet after green verification. Never include
`.claude/settings.local.json`, push, amend, rebase, squash, merge, or reset.

## Expected output format

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RED RUN — <command and expected missing-type evidence>
- GREEN RUN — <command>
- Result: <Unity exit / total / passed / failed>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO
- Protected changes made: YES/NO

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

Return `BLOCKED` and make no local commit.

## Completion record

- Primary commit: `d85f24a9c7cc91c803011bd028bf50e2ccc22ff8`
- Corrective commit:
  `009b0abc006bfcc4acaae8b3561d041def90b1ae`
- Final Codex verdict: `ACCEPT`
- Group verification: Unity EditMode `136/136` passed
