# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-BATCH-001`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-04`

**Order in group:** `3`

**Depends on:** `FLOW-GROUP-03`

**Goal:** Add deterministic pure-C# batch generation with stable derived seeds,
per-level results, aggregate counts, and continue-on-failure behavior.

## Scope whitelist

**Files allowed to read:**

- project AI rules and Group 4 documents
- approved Flow Puzzle spec and implementation plan
- all Core, Validation, Difficulty, and Generation source
- all existing EditMode tests

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Generation/FlowBatchRequest.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowBatchReport.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowBatchGenerator.cs`
- `Assets/Tests/EditMode/Generation/FlowBatchGeneratorTests.cs`
- generated `.meta` files for these files

**Files forbidden to modify:**

- every existing source, test, asmdef, and `.meta`
- all Persistence and Editor files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

Do not write `.meta` files manually.

## Required data contracts

Create:

```csharp
[Serializable]
public sealed class FlowBatchRequest
{
    public int startLevelId;
    public int count;
    public int baseSeed;
    public FlowGenerationConfig config;
}
```

Create in `FlowBatchReport.cs`:

```csharp
[Serializable]
public sealed class FlowBatchItemResult
{
    public int levelId;
    public int usedSeed;
    public bool success;
    public string message;
    public FlowGenerationResult generationResult;
}

[Serializable]
public sealed class FlowBatchReport
{
    public int requestedCount;
    public int successfulCount;
    public int failedCount;
    public List<FlowBatchItemResult> items;
}
```

Initialize reference fields per instance.

## Generator API and behavior

```csharp
public sealed class FlowBatchGenerator
{
    public FlowBatchGenerator(FlowSolutionGenerator generator);

    public FlowBatchReport Generate(FlowBatchRequest request);
}
```

Rules:

1. Throw `ArgumentNullException` for null generator, request, or request
   config.
2. Throw `ArgumentOutOfRangeException` when `count <= 0`.
3. Iterate exactly `count` items in ascending level ID from
   `startLevelId`.
4. Derive each fixed seed using:

   ```text
   levelSeed = unchecked(baseSeed + levelId * 9973)
   ```

5. Deep-copy the request config for every level, force
   `useRandomSeed = false`, and set the derived seed.
6. Do not mutate the request or its config.
7. Call the injected `FlowSolutionGenerator` once per requested level.
8. Continue after every individual structured generation failure.
9. Preserve item order.
10. Every item records level ID, derived seed, success, generation result, and
    a concise message:
    - success: a stable generated message;
    - failure: diagnostic error code and message when available.
11. Aggregate requested, successful, and failed counts exactly.
12. Do not save assets or JSON and do not reference Persistence or Editor.

Keep config copying private inside `FlowBatchGenerator`. Do not add a new
interface solely for testing.

## Tests

Create `FlowBatchGeneratorTests.cs` and cover:

- null constructor/request/config and non-positive count;
- exact item count, order, and ascending level IDs;
- exact derived seeds including a representative negative/base case;
- successful count and failed count invariants;
- successful item result seed equals derived item seed;
- same request called twice produces deeply equal per-level generation data;
- request and original config remain unchanged;
- impossible configuration still attempts and reports every requested level;
- failure items contain diagnostic code/message;
- report and item lists are instance-owned;
- Generation remains free of UnityEngine, UnityEditor, Persistence, and
  AssetDatabase references.

Use real `FlowSolutionGenerator` dependencies and deterministic practical
fixtures. For continue-on-failure, an impossible config may make every item
fail; asserting all requested items are present proves continuation without a
new fake abstraction.

## Non-goals

- No persistence, asset save, JSON export, partial retry policy, parallelism,
  Task, cancellation, progress, UI, solver, or dependency injection framework.
- No changes to `FlowSolutionGenerator` or existing config fields.

## Maximum change scope

- New files including generated metas: maximum `8`
- Approximate source/test diff: `800` lines

## Acceptance criteria

- [ ] Seed derivation is exact and deterministic.
- [ ] Request config is copied and never mutated.
- [ ] Every requested level gets one ordered item even after failures.
- [ ] Aggregate counts are exact.
- [ ] Repeated batches are deeply reproducible.
- [ ] Generation remains pure C#.
- [ ] Tests pass with all previous tests.
- [ ] Only allowed files change.

## Verification steps

Tests first, then implementation. Run all EditMode tests once.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|AssetDatabase|FlowPuzzle.Persistence" `
  Assets/Scripts/FlowPuzzle/Generation
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add deterministic batch generation`

Never include `.claude/settings.local.json` or push.

## Expected output format

Use the repository task packet response contract.

## What to do if blocked

Return `BLOCKED` and make no commit.

## Review record

- Worker commit: `793f22d`
- Verification: Unity EditMode `215/215` passed.
- Production implementation: provisionally retained.
- Codex verdict: `NEEDS_FIX`
- Corrective task: `FLOW-GROUP-04-FIX-02`
- Remaining gaps are test-contract coverage:
  - repeatability does not compare generated layouts or difficulty data;
  - overflow/negative seed, complete input immutability, failure messages, and
    instance ownership are unverified.

### Final resolution

- Corrective commits: `ce2d896`, `f07d8aa`, `ef054f8`
- Final Codex verdict: `ACCEPT`
- Final integration verification: Unity EditMode `238/238` passed.
