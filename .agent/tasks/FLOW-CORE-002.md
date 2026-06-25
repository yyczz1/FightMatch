# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-CORE-002`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-01`

**Order in group:** `1`

**Depends on:** `FLOW-CORE-001`

**Goal:**

Add all remaining serializable pure-C# Core data contracts required by the
approved Flow Puzzle architecture, together with focused contract tests.

**Background:**

`FLOW-CORE-001` established `FlowPuzzle.Core`, `FlowPos`, and the EditMode test
assembly. This packet completes the non-algorithmic Core DTO layer. `FlowBoard`
and `FlowPathUtility` are deliberately deferred to `FLOW-CORE-003`.

**Current context:**

- Unity version: `2022.3.18f1`.
- Current accepted baseline includes `FlowPos` and 13 passing EditMode tests.
- `FlowPuzzle.Core` has `noEngineReferences: true`.
- Core DTOs must use `System` and `System.Collections.Generic` only.
- JSON-facing data must use fields and lists, not dictionaries.
- Approved references:
  - `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
  - `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-01.md`
- `.agent/tasks/FLOW-CORE-002.md`
- `Assets/Scripts/FlowPuzzle/Core/FlowPos.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef`
- `Assets/Tests/EditMode/Core/FlowPosTests.cs`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

**Files allowed to modify:**

- `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Core/FlowDifficultyTier.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPairData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPathData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowLevelData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowSolutionData.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowGenerationConfig.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowDifficultyReport.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowGeneratedLevel.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowFailureDiagnostic.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowGenerationResult.cs`
- `Assets/Tests/EditMode/Core/FlowCoreDataContractsTests.cs`
- Unity-generated `.meta` files corresponding exactly to the eleven files above

**Files forbidden to modify:**

- `Assets/Scripts/FlowPuzzle/Core/FlowPos.cs`
- `Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef`
- `Assets/Tests/EditMode/Core/FlowPosTests.cs`
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
files allowed above during import or test execution.

## Required behavior

### Serializable contracts

Create these types in namespace `FlowPuzzle.Core`:

```csharp
public enum FlowDifficultyTier
{
    Easy,
    Normal,
    Hard,
    Expert
}
```

Every class below must be `public sealed`, marked `[Serializable]`, and expose
the listed public fields exactly.

```csharp
public sealed class FlowPairData
{
    public int colorId;
    public FlowPos endpointA;
    public FlowPos endpointB;
}

public sealed class FlowPathData
{
    public int colorId;
    public List<FlowPos> cells;
}

public sealed class FlowLevelData
{
    public int levelId;
    public int width;
    public int height;
    public FlowDifficultyTier difficulty;
    public float difficultyScore;
    public List<FlowPairData> pairs;
}

public sealed class FlowSolutionData
{
    public int levelId;
    public List<FlowPathData> paths;
}
```

`cells`, `pairs`, and `paths` must be initialized to new non-null lists for
every owning instance.

```csharp
public sealed class FlowGenerationConfig
{
    public int width;
    public int height;
    public int colorCount;
    public float minCoverageRatio;
    public float maxCoverageRatio;
    public int minPathLength;
    public int maxPathLength;
    public int maxPathAttempt;
    public int maxLevelAttempt;
    public bool useRandomSeed;
    public int seed;
    public bool useTargetDifficulty;
    public FlowDifficultyTier targetDifficulty;
    public bool useTargetScoreRange;
    public float minTargetDifficultyScore;
    public float maxTargetDifficultyScore;
    public float turnPreference;
    public float interactionPreference;
    public int minEndpointDistance;
    public int maxEndpointDistance;
    public int minDetour;
    public int maxDetour;
    public float bottleneckPreference;
    public int solverTimeoutMilliseconds;
    public int solverNodeBudget;
}
```

Do not add range validation or preset defaults in this packet.

```csharp
public sealed class FlowDifficultyReport
{
    public FlowDifficultyTier difficulty;
    public float totalScore;
    public float boardSizeScore;
    public float colorCountScore;
    public float coverageScore;
    public float turnScore;
    public float detourScore;
    public float interactionScore;
    public float endpointDistanceScore;
    public float bottleneckScore;
    public int totalTurnCount;
    public int totalDetour;
    public int differentColorAdjacentCount;
    public int totalEndpointManhattanDistance;
    public int bottleneckCount;
}

public sealed class FlowGeneratedLevel
{
    public FlowLevelData levelData;
    public FlowSolutionData solutionData;
    public FlowDifficultyReport difficultyReport;
    public int usedSeed;
    public float coverageRatio;
}
```

Each `FlowGeneratedLevel` instance must initialize `levelData`, `solutionData`,
and `difficultyReport` to distinct non-null instances.

```csharp
public sealed class FlowFailureDiagnostic
{
    public string errorCode;
    public string errorMessage;
    public int usedSeed;
    public int attemptCount;
}

public sealed class FlowGenerationResult
{
    public bool success;
    public int levelId;
    public int usedSeed;
    public int attemptCount;
    public FlowGeneratedLevel generatedLevel;
    public FlowFailureDiagnostic diagnostic;
}
```

`FlowGenerationResult` must provide:

```csharp
public static FlowGenerationResult Success(
    int levelId,
    int usedSeed,
    int attemptCount,
    FlowGeneratedLevel generatedLevel);

public static FlowGenerationResult Failure(
    int levelId,
    int usedSeed,
    int attemptCount,
    string errorCode,
    string errorMessage);
```

`Success` sets `success=true`, preserves all arguments, sets
`generatedLevel`, and leaves `diagnostic` null.

`Failure` sets `success=false`, preserves `levelId`, `usedSeed`, and
`attemptCount`, leaves `generatedLevel` null, and creates a diagnostic that
preserves the error code, message, seed, and attempt count.

### Tests

Create `FlowCoreDataContractsTests` covering:

1. every class listed above has `[Serializable]`;
2. `FlowDifficultyTier` contains exactly `Easy`, `Normal`, `Hard`, `Expert` in
   that order;
3. `FlowPathData.cells`, `FlowLevelData.pairs`, and `FlowSolutionData.paths`
   are non-null and not shared between instances;
4. `FlowGeneratedLevel` initializes its three nested objects and does not share
   them between instances;
5. `FlowGenerationResult.Success(...)` preserves all values and result shape;
6. `FlowGenerationResult.Failure(...)` preserves all diagnostic values and
   result shape.

Do not add tests that merely enumerate every public field by reflection.
Compilation and focused object-state tests are sufficient for field presence.

## Non-goals

- Do not implement `FlowBoard` or `FlowPathUtility`.
- Do not implement validation, scoring, generation, solving, persistence,
  Application services, Editor UI, presets, or JSON.
- Do not add constructors, interfaces, base classes, factories, helpers,
  extension methods, dictionaries, or Unity-type conversion methods beyond the
  two required result factories.
- Do not add validation or normalize configuration values.
- Do not modify existing source, asmdef, tests, packages, settings, or docs.
- No unrelated cleanup, formatting, comments, or renaming.

## Constraints

- Production files may use only `System` and, where lists are required,
  `System.Collections.Generic`.
- No `UnityEngine` or `UnityEditor`.
- Keep one public type per file.
- Use direct field initialization for owned lists and nested DTOs.
- Do not introduce speculative abstractions.
- If exact completion requires another file or type, return `BLOCKED`.

## Project-specific constraints

- Unity version is fixed at `2022.3.18f1`.
- `FlowPuzzle.Core` remains a pure C# assembly.
- Serialized contracts use fields and `List<T>`; no dictionaries.
- Do not implement unique-solution or full-board concepts.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `YES` | Create only the types and members listed in this packet |
| New dependency/package | `NO` | None |
| Build/configuration changes | `NO` | None |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `YES` | Introduce only the listed Core DTO fields |
| Unity asset or `.meta` changes | `YES` | Listed `.cs` files and their Unity-generated `.meta` files only |
| Generated-file changes | `NO` | No `.csproj`, `.sln`, Library, Temp, or Logs files committed |

## Maximum change scope

**Maximum changed production files:** `0`

**Maximum changed test files:** `0`

**Maximum new files:** `22` including Unity-generated `.meta` files

**Approximate maximum diff:** `650 changed lines` excluding `.meta` files

If the task cannot fit this budget, return `BLOCKED`.

## Acceptance criteria

- [ ] All ten required Core contract source files are created.
- [ ] All required production classes are `[Serializable]`.
- [ ] Owned lists and nested DTOs are non-null and instance-independent.
- [ ] Success and Failure factories preserve the required result data.
- [ ] Focused contract tests pass.
- [ ] Core production files contain no Unity references.
- [ ] No existing file is modified.
- [ ] No file outside the whitelist changes.
- [ ] No unapproved protected change is made.

## Verification steps

### Red verification

Create the test file first and run EditMode tests before creating the production
types.

Expected: compilation fails because the new Core types do not exist. Record a
short excerpt naming at least one missing type.

### Green verification

After implementation, verify no Unity dependencies:

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
The original 13 FlowPos tests still pass.
```

Also run:

```powershell
git diff --check
git status --short
```

Before the packet commit, expected changes are only this packet's allowed new
files and Unity-generated `.meta` files.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow puzzle core data contracts`

Commit only this packet after green verification passes. Never push, merge,
rebase, amend, squash, reset history, or include another packet's work.

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

Return `BLOCKED` with the exact missing information. Make no local commit and do
not begin `FLOW-CORE-003`.
