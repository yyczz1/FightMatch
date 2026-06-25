# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-GENERATION-003`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-03`

**Order in group:** `3`

**Depends on:** `FLOW-GENERATION-002`

**Goal:** Implement the final deterministic solution-first single-level
generator that returns validated, scored Core data or structured failure.

## Scope whitelist

**Files allowed to read:**

- project AI rules and all Group 3 documents
- Core, Validation, Difficulty, Generation source
- all existing EditMode tests and test asmdef
- approved Flow Puzzle spec and implementation plan

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Generation/FlowSolutionGenerator.cs`
- `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`
- generated `.meta` files for these files

**Files forbidden to modify:**

- every existing source, test, asmdef, and `.meta`
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- packages, settings, docs, generated project files

## Required API

```csharp
public sealed class FlowSolutionGenerator
{
    public FlowSolutionGenerator(
        FlowPathLengthAllocator allocator,
        IFlowPathGenerationStrategy pathStrategy,
        FlowSolutionValidator validator,
        FlowDifficultyEvaluator difficultyEvaluator);

    public FlowGenerationResult Generate(
        int levelId,
        FlowGenerationConfig config);
}
```

Throw `ArgumentNullException` for null constructor dependencies or null config.

## Required generation behavior

### Configuration checks

Return `FlowGenerationResult.Failure` without attempts for:

- non-positive width, height, color count: `InvalidDimensions`;
- coverage outside `[0,1]` or min greater than max:
  `ImpossibleCoverageRange`;
- invalid path range: `InvalidPathLengthRange`;
- non-positive path/level attempt budget: `InvalidAttemptBudget`.

### Seed

- fixed mode uses `config.seed`;
- random mode resolves one seed for the call using
  `Environment.TickCount` combined with `levelId`;
- record the resolved seed in every result;
- create one fresh `SystemFlowRandom` per Generate call;
- same level ID, fixed config, and fixed seed must deeply reproduce.

### Attempts

For attempt `1..maxLevelAttempt`:

1. Create an empty board.
2. Sample target coverage from configured range.
3. Compute rounded target used cells.
4. Allocate path lengths.
5. If allocation is an arithmetic failure, return immediately with its
   diagnostic and current attempt count.
6. Generate colors in allocator generation order.
7. For each color, call the path strategy with its allocated target length,
   `maxPathAttempt`, and soft preferences.
8. A failed path abandons the whole current level attempt.
9. Construct one pair from each path's first/last cells and one solution path
   per color ID. Store pairs and paths in color-ID ascending order.
10. Compute actual coverage from distinct occupied cells.
11. Reject and retry when actual coverage lies outside the hard configured
    range.
12. Validate with `FlowSolutionValidator`.
13. Evaluate difficulty.
14. If enabled, enforce target tier and/or inclusive score range; retry on
    mismatch.
15. Return success containing level, solution, difficulty, seed, coverage, and
    attempt count.

Do not call a solver. Do not require all cells occupied. Do not check unique
solutions.

### Failure

After exhausting attempts, return non-null failure:

- `MaxLevelAttemptsReached`;
- resolved seed;
- `attemptCount == maxLevelAttempt`;
- message states the final rejection reason when known.

Validation rejection may use `ValidationFailed`; path placement may use
`PathGenerationFailed`; difficulty mismatch may use
`DifficultyOutOfRange` internally/final message.

## Tests

Add deep comparison helpers in the test file only.

Cover:

- same fixed seed/config produces deeply equal result;
- fixed result records level ID and seed;
- different representative seeds produce different layouts without asserting
  universal difference;
- representative `5x5`, `6x6`, and `7x7` configs succeed;
- every successful recommendation passes validator;
- actual coverage is within hard range;
- every path length is within configured range;
- level pairs and solution paths are color-ID sorted;
- target difficulty mismatch exhausts exactly `maxLevelAttempt`;
- impossible minimum occupancy returns structured failure without null;
- invalid coverage/path/attempt configurations fail immediately;
- empty cells are allowed;
- generator source has no solver or Unity random reference;
- run the deterministic fixture at least three repeated invocations.

Use practical configs and budgets so the full suite remains fast. Do not weaken
hard assertions because generation is flaky; fix determinism or fixture
feasibility.

## Non-goals

- No batch generation, persistence, JSON, Application services, Editor UI,
  solver, async, unique-solution detection, or full-board requirement.
- No diagnostics suggestion engine yet.
- No changes to existing APIs.

## Constraints

- Pure C# and deterministic fixed-seed behavior.
- No Unity types, `UnityEngine.Random`, thread APIs, or solver references.
- Never depend on dictionary/hash-set enumeration order.
- Keep orchestration in this class; do not add extra factories or services.

## Maximum change scope

- New files including metas: maximum `4`
- Approximate diff: `900` lines

## Verification steps

Tests first, then implementation. Run all EditMode tests once.
Run generation tests three times if filtering is available; otherwise execute
the complete suite three times and report totals.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|UnityEngine.Random|IFlowPuzzleSolver" `
  Assets/Scripts/FlowPuzzle/Generation
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: generate deterministic flow puzzle levels`

## Expected output format

Use the repository task packet response contract and include the repeated test
evidence.

## What to do if blocked

Return `BLOCKED` and make no commit.

## Completion record

- Worker commit: `67d33fa`
- Corrective implementation commit: `07300ce`
- Corrective test commit: `c211081`
- Final Codex verdict: `ACCEPT`
- Final integration verification:
  - XML result: `Passed`
  - total: `192`
  - passed: `192`
  - failed: `0`
- Fixed-seed deep equality and different-seed layout comparison now verify
  their actual contracts.
