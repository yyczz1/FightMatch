# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-GENERATION-002`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-03`

**Order in group:** `2`

**Depends on:** `FLOW-GENERATION-001`

**Goal:** Implement deterministic randomized DFS generation of one exact-length
simple path with rollback.

## Scope whitelist

**Files allowed to read:**

- project AI rules and Group 3 documents
- Core, Validation, Difficulty, and Generation source created so far
- existing EditMode tests and test asmdef
- approved Flow Puzzle spec and plan

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Generation/IFlowPathGenerationStrategy.cs`
- `Assets/Scripts/FlowPuzzle/Generation/RandomizedDfsPathGenerationStrategy.cs`
- `Assets/Tests/EditMode/Generation/RandomizedDfsPathGenerationStrategyTests.cs`
- generated `.meta` files for these files

**Files forbidden to modify:**

- every existing source, test, asmdef, and `.meta`
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- packages, settings, docs, generated project files

## Required behavior

### Interface

```csharp
public interface IFlowPathGenerationStrategy
{
    bool TryGeneratePath(
        FlowBoard board,
        int colorId,
        int targetLength,
        int maxAttempt,
        float turnPreference,
        float interactionPreference,
        IFlowRandom random,
        out FlowPathData path);
}
```

### Strategy rules

`RandomizedDfsPathGenerationStrategy` must:

1. Throw for null board/random and negative color ID.
2. Return false with null path for target length below `2`, non-positive
   attempt budget, or insufficient empty cells.
3. Treat each attempt as a new random empty start.
4. Search only in-bounds empty cells.
5. Produce exactly `targetLength` distinct orthogonally adjacent cells.
6. Keep tentative cells outside the board state during DFS.
7. On failure, leave the board byte-for-byte logically unchanged.
8. On success, create `FlowPathData`, then commit every path cell to the board.
9. Collect candidate neighbors in board base order.
10. Shuffle candidates before stable score sorting so equal-score ties are
    random but seed-reproducible.
11. Soft score:
    - `turnPreference > 0` favors turns;
    - `turnPreference < 0` favors straight continuation;
    - `interactionPreference > 0` favors candidates adjacent to already
      occupied cells;
    - negative interaction preference avoids them.
12. Do not step through occupied cells or rely on dictionary/hash-set
    enumeration order.

The score is a candidate-order heuristic, not a hard constraint.

### Tests

Cover:

- exact target length;
- simple orthogonal path;
- existing occupied cells avoided;
- success commits cells and updates occupied count;
- fixed seed and identical board produce equal path;
- impossible length fails without board mutation;
- failed search leaves pre-existing cells unchanged;
- maxAttempt zero fails immediately;
- positive turn preference can be exercised on a small deterministic fixture;
- no Unity references.

Do not assert that two different seeds must always differ.

## Non-goals

- No full level generation, pair construction, coverage filtering, validation,
  difficulty, solver, persistence, batch, or Editor behavior.
- No interface changes from packet 1.

## Maximum change scope

- New files including metas: maximum `6`
- Approximate diff: `700` lines

## Verification steps

Tests first, then implementation. Run all EditMode tests once after green.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|UnityEngine.Random" `
  Assets/Scripts/FlowPuzzle/Generation
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add randomized dfs path generation`

## Expected output format

Use the repository task packet response contract.

## What to do if blocked

Return `BLOCKED`, make no commit, and do not begin `FLOW-GENERATION-003`.

## Completion record

- Worker commit: `b82d8f2`
- Final corrective commits: `07300ce`, `c211081`
- Final Codex verdict: `ACCEPT`
- Final integration verification: Unity EditMode `192/192` passed.
- DFS exact-length generation, rollback, deterministic tie handling, and
  board mutation boundaries passed review.
