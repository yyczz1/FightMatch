# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-GENERATION-001`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-03`

**Order in group:** `1`

**Depends on:** `FLOW-GROUP-02`

**Goal:** Implement the deterministic random abstraction and feasible
path-length allocation required by solution-first generation.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-03.md`
- `.agent/tasks/FLOW-GENERATION-001.md`
- `Assets/Scripts/FlowPuzzle/Core/**`
- existing EditMode tests and test asmdef
- approved Flow Puzzle spec and implementation plan

**Files allowed to modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Generation/FlowPuzzle.Generation.asmdef`
- `Assets/Scripts/FlowPuzzle/Generation/IFlowRandom.cs`
- `Assets/Scripts/FlowPuzzle/Generation/SystemFlowRandom.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowPathLengthAllocationResult.cs`
- `Assets/Scripts/FlowPuzzle/Generation/FlowPathLengthAllocator.cs`
- `Assets/Tests/EditMode/Generation/FlowPathLengthAllocatorTests.cs`
- generated `.meta` files for the new folders and files

**Files forbidden to modify:**

- all existing production files
- existing tests
- existing `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`
- every file not explicitly allowed above

Do not write `.meta` files manually.

## Required behavior

### Assembly

Create `FlowPuzzle.Generation` referencing:

- `FlowPuzzle.Core`
- `FlowPuzzle.Validation`
- `FlowPuzzle.Difficulty`

Set `noEngineReferences: true`.

Add `FlowPuzzle.Generation` to the test asmdef.

### Random API

```csharp
public interface IFlowRandom
{
    int NextInt(int minInclusive, int maxExclusive);
    float NextFloat(float minInclusive, float maxInclusive);
    void Shuffle<T>(IList<T> items);
}
```

`SystemFlowRandom`:

- has constructor `SystemFlowRandom(int seed)`;
- wraps one private `System.Random`;
- validates inverted/empty ranges with `ArgumentOutOfRangeException`;
- uses Fisher-Yates shuffle;
- same seed and same calls produce identical values and shuffle order;
- no static/global random state.

### Allocation result

In namespace `FlowPuzzle.Generation`:

```csharp
[Serializable]
public sealed class FlowPathLengthAllocationResult
{
    public bool success;
    public int requestedTargetUsedCellCount;
    public int allocatedUsedCellCount;
    public List<int> pathLengthsByColorId;
    public List<int> generationOrderColorIds;
    public FlowFailureDiagnostic diagnostic;
}
```

Initialize both lists per instance.

Factories:

```csharp
public static FlowPathLengthAllocationResult Success(
    int requestedTargetUsedCellCount,
    IList<int> pathLengthsByColorId);

public static FlowPathLengthAllocationResult Failure(
    int requestedTargetUsedCellCount,
    string errorCode,
    string errorMessage);
```

Success copies input values, computes the allocated sum, and creates
`generationOrderColorIds` sorted by descending path length then ascending
color ID. Failure preserves target and creates a diagnostic; seed and attempts
are zero because allocation is seed-independent failure reporting.

### Allocator API

```csharp
public sealed class FlowPathLengthAllocator
{
    public FlowPathLengthAllocationResult Allocate(
        FlowGenerationConfig config,
        int targetUsedCellCount,
        IFlowRandom random);
}
```

Rules:

1. Throw `ArgumentNullException` for null config or random.
2. Return structured failure for:
   - non-positive dimensions or color count: `InvalidDimensions`;
   - invalid path range: `InvalidPathLengthRange`;
   - `colorCount * minPathLength > board capacity`:
     `ImpossibleMinimumOccupancy`.
3. Feasible minimum is `colorCount * minPathLength`.
4. Feasible maximum is
   `min(board capacity, colorCount * maxPathLength)`.
5. Clamp the requested target into the feasible interval.
6. Initialize every color to minimum length.
7. Shuffle color IDs once using `IFlowRandom`.
8. Distribute remaining cells one at a time in shuffled cyclic order without
   exceeding maximum path length until the clamped target is reached.
9. Output `pathLengthsByColorId[colorId]`.
10. Output generation order sorted longest first, then color ID ascending.
11. Do not retry impossible arithmetic.

### Tests

Cover:

- same-seed random sequences and shuffle;
- different seeds produce a different representative sequence;
- invalid random ranges;
- impossible minimum occupancy;
- target below minimum clamps to minimum;
- target above maximum clamps to maximum;
- lengths remain in range;
- allocated sum equals clamped target;
- same seed/config returns equal allocations;
- generation order descending length and stable color-ID tie break;
- result lists are copied and not shared;
- no Unity references.

## Non-goals

- No path placement, DFS, full level generation, validation calls, difficulty
  calls, solver, persistence, batch, Editor, presets, or diagnostics advice.
- No default config values.
- No additional abstractions or files.

## Constraints

- Pure C#: `System`, `System.Collections.Generic`, Core and own namespace only.
- No Unity APIs or `UnityEngine.Random`.
- Do not depend on collection enumeration order.

## Maximum change scope

- Existing files modified: `1`
- New files including generated metas: maximum `14`
- Approximate source/test diff: `750` lines

## Acceptance criteria

- [ ] Random source is deterministic and isolated.
- [ ] Allocator handles impossible math without retry.
- [ ] Feasible allocation reaches the clamped target exactly.
- [ ] Generation order is deterministic.
- [ ] Tests pass with all previous tests.
- [ ] Only allowed files change.

## Verification steps

Create tests and test asmdef reference first; verify missing-type compilation.
Then implement and run all EditMode tests once.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|UnityEngine.Random" `
  Assets/Scripts/FlowPuzzle/Generation
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add deterministic path length allocation`

Never include `.claude/settings.local.json` or push.

## Expected output format

Use the repository task packet response contract, including red/green evidence
and local commit hash.

## What to do if blocked

Return `BLOCKED`, make no commit, and do not begin `FLOW-GENERATION-002`.

## Completion record

- Worker commit: `2981fa9`
- Final corrective commits: `07300ce`, `c211081`
- Final Codex verdict: `ACCEPT`
- Final integration verification: Unity EditMode `192/192` passed.
- Static prohibited-reference check: no matches.
