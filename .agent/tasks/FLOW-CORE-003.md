# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-CORE-003`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-01`

**Order in group:** `2`

**Depends on:** `FLOW-CORE-002`

**Goal:**

Implement the final pure-C# `FlowBoard` and `FlowPathUtility` Core algorithms
with deterministic behavior and focused EditMode tests.

**Background:**

After `FLOW-CORE-002`, the Core assembly contains all data contracts. This
packet completes the Phase 1 Core foundation with the mutable algorithm board
and stateless path calculations required by validation, difficulty, generation,
and solving.

**Current context:**

- `FlowPos` is the only coordinate type allowed in Core.
- Empty board cells use color ID `-1`.
- Neighbor order must be deterministic: right, left, up, down.
- Paths are orthogonal; unique-solution and full-board behavior are out of
  scope.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-01.md`
- `.agent/tasks/FLOW-CORE-003.md`
- `Assets/Scripts/FlowPuzzle/Core/FlowPos.cs`
- all source files created by `FLOW-CORE-002`
- `Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef`
- `Assets/Tests/EditMode/Core/FlowPosTests.cs`
- `Assets/Tests/EditMode/Core/FlowCoreDataContractsTests.cs`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

**Files allowed to modify:**

- `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Core/FlowBoard.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs`
- `Assets/Tests/EditMode/Core/FlowBoardTests.cs`
- `Assets/Tests/EditMode/Core/FlowPathUtilityTests.cs`
- Unity-generated `.meta` files corresponding exactly to the four files above

**Files forbidden to modify:**

- every source or test file from `FLOW-CORE-001` and `FLOW-CORE-002`
- `Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `AGENTS.md`
- `.agent/**`
- `CLAUDE.md`
- `.claude/**`
- `Packages/**`
- `ProjectSettings/**`
- `docs/**`
- every file not explicitly listed under “Files allowed to create”

Do not create `.meta` files manually. Unity may generate only the exact `.meta`
files allowed above.

## Required behavior

### FlowBoard API

Create this public API in namespace `FlowPuzzle.Core`:

```csharp
public sealed class FlowBoard
{
    public const int EmptyColorId = -1;

    public int Width { get; }
    public int Height { get; }
    public int OccupiedCellCount { get; }

    public FlowBoard(int width, int height);

    public bool IsInside(FlowPos position);
    public bool IsEmpty(FlowPos position);
    public int Get(FlowPos position);
    public void Set(FlowPos position, int colorId);
    public void Clear(FlowPos position);
    public List<FlowPos> GetNeighbors(FlowPos position);
}
```

Required semantics:

1. Constructor throws `ArgumentOutOfRangeException` when width or height is
   non-positive.
2. Store cells in a private `int[,]`; expose no mutable backing array.
3. Every cell starts as `EmptyColorId`.
4. `IsInside` returns false rather than throwing.
5. `Get`, `Set`, `Clear`, `IsEmpty`, and `GetNeighbors` throw
   `ArgumentOutOfRangeException` for an out-of-bounds position.
6. `Set` rejects negative color IDs with `ArgumentOutOfRangeException`.
7. `OccupiedCellCount` increases only when an empty cell becomes occupied,
   decreases only when an occupied cell is cleared, and does not change when
   replacing one occupied color with another.
8. `Clear` is idempotent.
9. `GetNeighbors` returns only in-bounds orthogonal positions in deterministic
   base order: right, left, up, down.

Do not add cloning, resizing, serialization, iteration interfaces, events, or
Unity conversions.

### FlowPathUtility API

Create a `public static class FlowPathUtility` with:

```csharp
public static bool AreAdjacent(FlowPos a, FlowPos b);
public static int GetManhattanDistance(FlowPos a, FlowPos b);
public static int GetMoveCount(IList<FlowPos> cells);
public static int CountTurns(IList<FlowPos> cells);
public static bool HasDuplicateCells(IList<FlowPos> cells);
public static int GetDetour(IList<FlowPos> cells);
```

Required semantics:

1. `AreAdjacent` is true only when Manhattan distance is exactly `1`.
2. `GetMoveCount` returns `max(0, Count - 1)`.
3. `CountTurns` counts direction changes between consecutive orthogonal moves.
4. `HasDuplicateCells` detects repeated coordinates.
5. `GetDetour` equals move count minus Manhattan distance between the first and
   last cell, clamped to zero.
6. For `null`, empty, or one-cell lists:
   - `GetMoveCount`, `CountTurns`, and `GetDetour` return `0`;
   - `HasDuplicateCells` returns `false`.
7. Use direct loops. A local `HashSet<FlowPos>` is allowed only for duplicate
   detection because enumeration order does not affect output.

### Tests

`FlowBoardTests` must cover:

- invalid dimensions;
- dimensions and initial empty state;
- inside/outside checks;
- set/get/clear;
- negative color rejection;
- out-of-bounds access;
- occupied count for set, replacement, clear, and repeated clear;
- neighbor order at center, edge, and corner.

`FlowPathUtilityTests` must cover:

- orthogonal adjacency and diagonal/same-cell rejection;
- Manhattan distance;
- straight path has zero turns;
- L path has one turn;
- a multi-turn path counts every change;
- duplicate and non-duplicate paths;
- move count;
- detour;
- null, empty, and one-cell safe results.

## Non-goals

- Do not implement validator behavior inside either class.
- Do not verify path bounds, overlap, endpoints, colors, or unique solutions.
- Do not add random behavior, generation, solving, difficulty scoring, JSON,
  persistence, Editor code, or Unity types.
- Do not change any existing DTO or test.
- No unrelated cleanup, formatting, comments, or renaming.

## Constraints

- Production code may use only `System` and `System.Collections.Generic`.
- No `UnityEngine` or `UnityEditor`.
- Keep implementations direct and deterministic.
- Do not create interfaces, factories, extension methods, or abstractions.
- If exact completion requires changing an existing file, return `BLOCKED`.

## Project-specific constraints

- Empty board cells are `-1`.
- Neighbor order is right, left, up, down.
- Coordinates use `FlowPos`, never `Vector2Int`.
- Empty cells are legal; no full-board assumption.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `YES` | Create only the two APIs listed in this packet |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `NO` | None |
| Unity asset or `.meta` changes | `YES` | Listed `.cs` files and their Unity-generated `.meta` files only |
| Generated-file changes | `NO` | No `.csproj`, `.sln`, Library, Temp, or Logs files committed |

## Maximum change scope

**Maximum changed production files:** `0`

**Maximum changed test files:** `0`

**Maximum new files:** `8` including Unity-generated `.meta` files

**Approximate maximum diff:** `500 changed lines` excluding `.meta` files

If the task cannot fit this budget, return `BLOCKED`.

## Acceptance criteria

- [ ] `FlowBoard` matches the required public API and behavior.
- [ ] `FlowPathUtility` matches the required public API and behavior.
- [ ] Neighbor order and occupied count are deterministic.
- [ ] Null/short path utility inputs return the required safe values.
- [ ] Focused board and path tests pass.
- [ ] All earlier Core tests still pass.
- [ ] Core production files contain no Unity references.
- [ ] No existing file is modified.
- [ ] No file outside the whitelist changes.
- [ ] No unapproved protected change is made.

## Verification steps

### Red verification

Create both test files first and run EditMode tests before creating production
files.

Expected: compilation fails because `FlowBoard` and `FlowPathUtility` do not
exist. Record a short missing-type excerpt.

### Green verification

```powershell
rg -n "UnityEngine|UnityEditor" Assets/Scripts/FlowPuzzle/Core
```

Expected: no matches.

Run all EditMode tests using the command pattern in
`.agent/groups/FLOW-GROUP-01.md`.

Expected:

```text
Unity exit code 0.
Test XML result Passed.
Failed=0.
All FLOW-CORE-001 and FLOW-CORE-002 tests remain passing.
```

Before the packet commit:

```powershell
git diff --check
git status --short
```

Expected changes: only this packet's four new `.cs` files and Unity-generated
`.meta` files.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow board and path utilities`

Commit only this packet after green verification passes. Never push, merge,
rebase, amend, squash, reset history, or include prior packet changes.

## Expected output format

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RED RUN — <command and missing-type evidence>
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

Return `BLOCKED` with the exact missing information. Make no local commit.

## Completion record

- Worker status: `COMPLETED`
- Codex verdict: `ACCEPT`
- Commit: `994c1f143db2276b98fad5335b50d5fdf4a7ec10`
- Group integration verification: Unity EditMode `78/78` passed; compiler
  errors `0`
