# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-APPLICATION-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-05`

**Order in group:** `1`

**Depends on:** `FLOW-GROUP-04`

**Goal:** Add the pure-C# application facade used by the Editor without
exposing dependency composition inside the window.

## Scope whitelist

**Files allowed to read:**

- project rules and all Group 5 documents
- approved Flow Puzzle spec and implementation plan
- Core, Validation, Difficulty, Generation, and Persistence source
- all EditMode tests and test asmdef

**Files allowed to modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Application/FlowPuzzle.Application.asmdef`
- `Assets/Scripts/FlowPuzzle/Application/FlowLevelGenerationService.cs`
- `Assets/Tests/EditMode/Application/FlowLevelGenerationServiceTests.cs`
- generated `.meta` files for the new folders and files

**Files forbidden to modify:**

- all existing production source
- all existing tests
- existing `.meta` files
- `.agent/**`, `AGENTS.md`, `.claude/settings.local.json`
- `Packages/**`, `ProjectSettings/**`, `docs/**`

## Required API

```csharp
public sealed class FlowLevelGenerationService
{
    public FlowLevelGenerationService();

    public FlowGenerationResult GenerateOne(
        int levelId,
        FlowGenerationConfig config);

    public FlowBatchReport GenerateBatch(
        FlowBatchRequest request);

    public FlowValidationResult Validate(
        FlowGeneratedLevel level);
}
```

The constructor composes one `FlowSolutionGenerator`, one `FlowBatchGenerator`,
and one `FlowSolutionValidator` from the accepted concrete components. Do not
add a service interface or container.

Rules:

- methods delegate without mutating caller data;
- null behavior follows the accepted underlying APIs;
- Validation returns the validator's structured result;
- no Persistence, AssetDatabase, Editor, Unity, solver, async, or UI code.

Create `FlowPuzzle.Application` referencing Core, Validation, Difficulty, and
Generation with `noEngineReferences: true`. Add it to the test asmdef.

## Tests

Cover:

- fixed seeded GenerateOne succeeds and matches direct generator output;
- generated recommendation validates;
- GenerateBatch preserves exact derived seeds and count;
- repeated fixed calls are deeply deterministic;
- null config/request/level behavior matches underlying contracts;
- Application has no Unity, Persistence, Editor, or solver references.

## Non-goals

- No saving, exporting, presets, UI, Draft, Solver, diagnostics suggestions,
  async, progress, or dependency injection framework.

## Maximum change scope

- Existing files modified: `1`
- New files including metas: maximum `8`
- Approximate diff: `650` lines

## Verification

Run all EditMode tests. Static:

```powershell
rg -n "UnityEngine|UnityEditor|FlowPuzzle.Persistence|IFlowPuzzleSolver" `
  Assets/Scripts/FlowPuzzle/Application
```

Expected: no matches.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add flow level generation service`

## What to do if blocked

Return `BLOCKED`, make no commit, and do not begin the next packet.
