# Flow Puzzle Level Tool Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the final Unity Editor Flow Puzzle production tool: deterministic solution-first generation, validation, difficulty scoring, single-asset persistence, manual draft editing, exact local completion, async execution, Undo/Redo, diagnostics, and optional JSON export.

**Architecture:** Keep Core, Validation, Difficulty, Generation, and Solving as pure C# assemblies. Use Application services to orchestrate business operations, Persistence for Unity assets/JSON, and an Editor-only UI Toolkit layer for UI, AssetDatabase access, draft editing, and Undo. The board is one custom `VisualElement`; it emits editing intents and contains no generation, solving, validation, or persistence logic.

**Tech Stack:** Unity 2022.3.18f1, C#, UI Toolkit EditorWindow, UXML, USS, custom `VisualElement`, `Painter2D`, ScriptableObject, Unity Test Framework 1.1.33, NUnit, `System.Threading.Tasks`.

**Reference spec:** `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`

**Unity executable:** `D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe`

**Repository note:** `D:\Unity\UnityProj\FightMatch` is not currently a usable Git repository. Do not initialize Git without user authorization. Commit checkpoints below are therefore deferred until Git exists.

---

## Global implementation rules

- Before each task, list the files to create or modify.
- Do not modify unrelated files or the sample scene.
- Use TDD for every algorithmic behavior: failing test, minimal implementation, passing test.
- Do not implement unique-solution detection or solution counting.
- Do not require full-board coverage.
- `FlowSolutionGenerator` must never call `IFlowPuzzleSolver`.
- Do not use `UnityEngine.Random` in deterministic algorithms.
- Do not let `Dictionary` or `HashSet` enumeration order affect generation or solving.
- Do not call Unity APIs from worker threads.
- Use `CreateGUI()` and UI Toolkit for the Editor main UI; do not build the main window with `OnGUI()`.
- Do not create one `VisualElement` per board cell. Draw the whole board in one `FlowBoardView`.
- Board input emits Editor commands or intents; it must not call Generator, Solver, Validator, Repository, or AssetDatabase.
- Editor board interaction is tooling only and must not implement Runtime player drawing behavior.
- Do not save a formal `FlowLevelAsset` unless its current solution passes `FlowSolutionValidator`.
- If Unity is already open on this project, use the Editor Test Runner during incremental work. Close Unity before running batch-mode commands against the same project.

## Verification commands

Run all EditMode tests after closing the project in Unity:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' `
  -batchmode -nographics -quit `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\TestResults.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowPuzzleTests.log'
```

Expected: Unity exits with code `0`; `TestResults.xml` reports zero failed tests.

Compile-only verification:

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' `
  -batchmode -nographics -quit `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FlowPuzzleCompile.log'
```

Expected: exit code `0`, with no `CS` compiler errors in the log.

---

## File map

### Core

```text
Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef
Assets/Scripts/FlowPuzzle/Core/FlowPos.cs
Assets/Scripts/FlowPuzzle/Core/FlowPairData.cs
Assets/Scripts/FlowPuzzle/Core/FlowPathData.cs
Assets/Scripts/FlowPuzzle/Core/FlowLevelData.cs
Assets/Scripts/FlowPuzzle/Core/FlowSolutionData.cs
Assets/Scripts/FlowPuzzle/Core/FlowDifficultyTier.cs
Assets/Scripts/FlowPuzzle/Core/FlowGenerationConfig.cs
Assets/Scripts/FlowPuzzle/Core/FlowDifficultyReport.cs
Assets/Scripts/FlowPuzzle/Core/FlowGeneratedLevel.cs
Assets/Scripts/FlowPuzzle/Core/FlowFailureDiagnostic.cs
Assets/Scripts/FlowPuzzle/Core/FlowGenerationResult.cs
Assets/Scripts/FlowPuzzle/Core/FlowBoard.cs
Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs
```

### Validation

```text
Assets/Scripts/FlowPuzzle/Validation/FlowPuzzle.Validation.asmdef
Assets/Scripts/FlowPuzzle/Validation/FlowValidationResult.cs
Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs
```

### Difficulty

```text
Assets/Scripts/FlowPuzzle/Difficulty/FlowPuzzle.Difficulty.asmdef
Assets/Scripts/FlowPuzzle/Difficulty/FlowDifficultyEvaluator.cs
```

### Generation

```text
Assets/Scripts/FlowPuzzle/Generation/FlowPuzzle.Generation.asmdef
Assets/Scripts/FlowPuzzle/Generation/IFlowRandom.cs
Assets/Scripts/FlowPuzzle/Generation/SystemFlowRandom.cs
Assets/Scripts/FlowPuzzle/Generation/FlowPathLengthAllocationResult.cs
Assets/Scripts/FlowPuzzle/Generation/FlowPathLengthAllocator.cs
Assets/Scripts/FlowPuzzle/Generation/IFlowPathGenerationStrategy.cs
Assets/Scripts/FlowPuzzle/Generation/RandomizedDfsPathGenerationStrategy.cs
Assets/Scripts/FlowPuzzle/Generation/FlowSolutionGenerator.cs
Assets/Scripts/FlowPuzzle/Generation/FlowBatchRequest.cs
Assets/Scripts/FlowPuzzle/Generation/FlowBatchReport.cs
Assets/Scripts/FlowPuzzle/Generation/FlowBatchGenerator.cs
```

### Solving

```text
Assets/Scripts/FlowPuzzle/Solving/FlowPuzzle.Solving.asmdef
Assets/Scripts/FlowPuzzle/Solving/FlowSolveStatus.cs
Assets/Scripts/FlowPuzzle/Solving/FlowSolveRequest.cs
Assets/Scripts/FlowPuzzle/Solving/FlowSolveProgress.cs
Assets/Scripts/FlowPuzzle/Solving/FlowSolveResult.cs
Assets/Scripts/FlowPuzzle/Solving/IFlowPuzzleSolver.cs
Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs
Assets/Scripts/FlowPuzzle/Solving/FlowCompletionRequest.cs
Assets/Scripts/FlowPuzzle/Solving/FlowCompletionProgress.cs
Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs
Assets/Scripts/FlowPuzzle/Solving/IFlowLevelCompletionProvider.cs
Assets/Scripts/FlowPuzzle/Solving/LocalExactCompletionProvider.cs
Assets/Scripts/FlowPuzzle/Solving/Tools/FlowSolverToolContracts.cs
Assets/Scripts/FlowPuzzle/Solving/Tools/FlowSolverToolAdapter.cs
```

### Application

```text
Assets/Scripts/FlowPuzzle/Application/FlowPuzzle.Application.asmdef
Assets/Scripts/FlowPuzzle/Application/FlowLevelGenerationService.cs
Assets/Scripts/FlowPuzzle/Application/FlowLevelCompletionService.cs
```

### Persistence

```text
Assets/Scripts/FlowPuzzle/Persistence/FlowPuzzle.Persistence.asmdef
Assets/Scripts/FlowPuzzle/Persistence/FlowLevelAsset.cs
Assets/Scripts/FlowPuzzle/Persistence/FlowJsonExportResult.cs
Assets/Scripts/FlowPuzzle/Persistence/FlowLevelJsonExporter.cs
```

### Editor

```text
Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef
Assets/Scripts/FlowPuzzle/Editor/FlowDifficultyPreset.cs
Assets/Scripts/FlowPuzzle/Editor/FlowDifficultyPresetLibrary.cs
Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs
Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs
Assets/Scripts/FlowPuzzle/Editor/Undo/IFlowEditorCommand.cs
Assets/Scripts/FlowPuzzle/Editor/Undo/FlowEditorCommandHistory.cs
Assets/Scripts/FlowPuzzle/Editor/Undo/FlowSnapshotCommand.cs
Assets/Scripts/FlowPuzzle/Editor/Undo/MoveEndpointCommand.cs
Assets/Scripts/FlowPuzzle/Editor/Undo/ResizeBoardCommand.cs
Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs
Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs
Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml
Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss
Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs
Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs
Assets/Scripts/FlowPuzzle/Editor/UI/FlowParameterPanel.cs
Assets/Scripts/FlowPuzzle/Editor/UI/FlowResultPanel.cs
Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs
Assets/Scripts/FlowPuzzle/Editor/UI/FlowBatchReportPanel.cs
Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs
```

### Tests

```text
Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef
Assets/Tests/EditMode/Core/FlowBoardTests.cs
Assets/Tests/EditMode/Core/FlowPathUtilityTests.cs
Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs
Assets/Tests/EditMode/Difficulty/FlowDifficultyEvaluatorTests.cs
Assets/Tests/EditMode/Generation/FlowPathLengthAllocatorTests.cs
Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs
Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs
Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs
Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs
Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs
Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs
Assets/Tests/EditMode/Solving/LocalExactCompletionProviderTests.cs
```

---

### Task 1: Establish assembly boundaries and Core data contracts

**Files:**
- Create all Core files listed in the file map except `FlowBoard.cs` and `FlowPathUtility.cs`
- Create: `Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef`
- Create: `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

- [ ] **Step 1: Create the Core asmdef**

Use:

```json
{
  "name": "FlowPuzzle.Core",
  "rootNamespace": "FlowPuzzle.Core",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": true
}
```

- [ ] **Step 2: Create the test asmdef**

Reference `FlowPuzzle.Core` and add `"optionalUnityReferences": ["TestAssemblies"]`. Restrict it to the Editor platform.

- [ ] **Step 3: Write serialization contract tests**

Create a small NUnit fixture proving:

- `FlowPos` value equality works.
- list fields are initialized and non-null.
- `FlowGenerationResult.Failure(...)` preserves error code, message, seed, and attempts.

- [ ] **Step 4: Run the tests and verify they fail because types do not exist**

Expected: compile failure naming the missing Core types.

- [ ] **Step 5: Implement minimal Core contracts**

Required signatures:

```csharp
[Serializable]
public struct FlowPos : IEquatable<FlowPos>
{
    public int x;
    public int y;
}

[Serializable]
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

`FlowGenerationConfig` must include:

- dimensions and color count;
- path length and coverage ranges;
- path and level attempt budgets;
- fixed/random seed;
- optional target difficulty tier or score range;
- soft turn, interaction, endpoint-distance, detour, and bottleneck preferences;
- solver timeout and node budget.

Keep fields serializable and avoid dictionaries.

- [ ] **Step 6: Run Core tests**

Expected: all contract tests pass and `FlowPuzzle.Core` compiles without UnityEngine references.

---

### Task 2: Implement FlowBoard and path utilities

**Files:**
- Create: `Assets/Scripts/FlowPuzzle/Core/FlowBoard.cs`
- Create: `Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs`
- Create: `Assets/Tests/EditMode/Core/FlowBoardTests.cs`
- Create: `Assets/Tests/EditMode/Core/FlowPathUtilityTests.cs`

- [ ] **Step 1: Write failing FlowBoard tests**

Cover:

- constructor rejects non-positive dimensions;
- empty cells contain `-1`;
- inside/outside checks;
- set, get, and clear;
- deterministic neighbor order: right, left, up, down, filtered to bounds;
- occupied-cell count.

- [ ] **Step 2: Run tests and confirm failure**

Expected: missing `FlowBoard`.

- [ ] **Step 3: Implement FlowBoard**

Use `int[,]`; expose no mutable backing array. Accept and return `FlowPos`, not `Vector2Int`.

- [ ] **Step 4: Write failing path utility tests**

Cover:

```text
straight path → 0 turns
L path → 1 turn
diagonal cells → not adjacent
duplicate cell → true
move count = cells.Count - 1
detour = move count - endpoint Manhattan distance
null/short list → safe zero values where meaningful
```

- [ ] **Step 5: Implement FlowPathUtility**

Use deterministic, allocation-light loops. Duplicate detection may use a `HashSet<FlowPos>` because enumeration order is irrelevant.

- [ ] **Step 6: Run Core test suite**

Expected: all Core tests pass.

---

### Task 3: Implement strict recommendation validation

**Files:**
- Create: `Assets/Scripts/FlowPuzzle/Validation/FlowPuzzle.Validation.asmdef`
- Create: `Assets/Scripts/FlowPuzzle/Validation/FlowValidationResult.cs`
- Create: `Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs`
- Create: `Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs`
- Modify: `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

- [ ] **Step 1: Configure assembly references**

`FlowPuzzle.Validation` references only `FlowPuzzle.Core` and sets `noEngineReferences: true`.

- [ ] **Step 2: Write failing validator tests**

Create explicit cases for:

- valid solution with unused cells;
- null level and null solution;
- invalid dimensions;
- duplicate pair color;
- missing, duplicate, or extra path color;
- path shorter than two cells;
- mismatched endpoints;
- out-of-bounds cell;
- diagonal/disconnected step;
- self-intersection;
- overlap between colors;
- path passing through another color endpoint.

- [ ] **Step 3: Implement result factories**

```csharp
public sealed class FlowValidationResult
{
    public bool isValid;
    public string errorCode;
    public string errorMessage;

    public static FlowValidationResult Valid();
    public static FlowValidationResult Invalid(string code, string message);
}
```

- [ ] **Step 4: Implement validator in one deterministic pass**

Use lookup dictionaries only for direct lookup; never serialize them. Validate pair/path cardinality before path geometry. Keep the first error stable for testability.

- [ ] **Step 5: Run validator tests**

Expected: all tests pass; no fill-board or uniqueness behavior exists.

---

### Task 4: Implement deterministic difficulty scoring

**Files:**
- Create: `Assets/Scripts/FlowPuzzle/Difficulty/FlowPuzzle.Difficulty.asmdef`
- Create: `Assets/Scripts/FlowPuzzle/Difficulty/FlowDifficultyEvaluator.cs`
- Create: `Assets/Tests/EditMode/Difficulty/FlowDifficultyEvaluatorTests.cs`
- Modify: `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

- [ ] **Step 1: Write failing formula tests**

Use hand-built paths and assert each field:

```text
boardSizeScore
colorCountScore
coverageScore
turnScore
detourScore
interactionScore
endpointDistanceScore
bottleneckScore
totalScore
difficulty
```

- [ ] **Step 2: Define exact interaction and bottleneck semantics in tests**

Use:

- interaction: orthogonally adjacent occupied cells of different colors, counted once per undirected pair;
- bottleneck: empty or occupied board cell with at most two traversable orthogonal neighbors, excluding endpoint cells; count each cell once.

These definitions must remain stable unless playtest data later justifies a spec revision.

- [ ] **Step 3: Implement evaluator**

Use the initial weights from the design. Boundaries:

```text
score < 60       Easy
score < 120      Normal
score < 200      Hard
otherwise        Expert
```

- [ ] **Step 4: Run difficulty tests**

Expected: exact formula tests and threshold tests pass.

---

### Task 5: Implement deterministic random source and path-length allocation

**Files:**
- Create: `Assets/Scripts/FlowPuzzle/Generation/FlowPuzzle.Generation.asmdef`
- Create: `Assets/Scripts/FlowPuzzle/Generation/IFlowRandom.cs`
- Create: `Assets/Scripts/FlowPuzzle/Generation/SystemFlowRandom.cs`
- Create: `Assets/Scripts/FlowPuzzle/Generation/FlowPathLengthAllocationResult.cs`
- Create: `Assets/Scripts/FlowPuzzle/Generation/FlowPathLengthAllocator.cs`
- Create: `Assets/Tests/EditMode/Generation/FlowPathLengthAllocatorTests.cs`
- Modify: `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

- [ ] **Step 1: Write deterministic random tests**

Two instances with the same seed must produce identical `NextInt`, `NextFloat`, and shuffle results.

- [ ] **Step 2: Implement IFlowRandom**

Required API:

```csharp
public interface IFlowRandom
{
    int NextInt(int minInclusive, int maxExclusive);
    float NextFloat(float minInclusive, float maxInclusive);
    void Shuffle<T>(IList<T> items);
}
```

Back it with a private `System.Random`.

- [ ] **Step 3: Write failing allocation tests**

Cover:

- impossible minimum occupancy;
- target below minimum clamps to feasible minimum;
- target above maximum clamps to feasible maximum;
- lengths remain in range;
- sum is as close as possible to target;
- output order is descending length with stable colorId tie-break.

- [ ] **Step 4: Implement allocator**

Return a structured failure diagnostic for impossible configurations. Do not retry impossible math.

- [ ] **Step 5: Run allocation tests**

Expected: deterministic, feasible allocations pass.

---

### Task 6: Implement the final solution-first generator

**Files:**
- Create: `Assets/Scripts/FlowPuzzle/Generation/IFlowPathGenerationStrategy.cs`
- Create: `Assets/Scripts/FlowPuzzle/Generation/RandomizedDfsPathGenerationStrategy.cs`
- Create: `Assets/Scripts/FlowPuzzle/Generation/FlowSolutionGenerator.cs`
- Create: `Assets/Tests/EditMode/Generation/FlowSolutionGeneratorTests.cs`

- [ ] **Step 1: Write failing single-path DFS tests**

Use small boards to prove:

- exact target length is reached;
- existing occupied cells are avoided;
- returned path is simple and orthogonal;
- fixed seed returns the same path;
- failure returns without mutating the board.

- [ ] **Step 2: Implement randomized DFS with rollback**

Required behavior:

- candidate neighbors are collected in deterministic base order;
- scoring applies soft straight/turn and adjacency preferences;
- candidates are stably sorted, then random tie-breaking is applied;
- board cells are committed only after a complete path succeeds.

- [ ] **Step 3: Write failing level-generation tests**

Cover:

- same seed and config produce deep-equal data;
- different derived seeds usually produce different layouts;
- 5×5, 6×6, and 7×7 representative configs succeed;
- all paths validate;
- actual coverage falls inside the hard range;
- path lengths fall inside the hard range;
- target difficulty rejection ends at `maxLevelAttempt`;
- impossible config returns a diagnostic and never returns null.

- [ ] **Step 4: Implement FlowSolutionGenerator**

Constructor-inject:

```csharp
FlowPathLengthAllocator
IFlowPathGenerationStrategy
FlowSolutionValidator
FlowDifficultyEvaluator
```

Create a fresh `IFlowRandom` per Generate call from the resolved seed. Generate longest paths first while preserving color IDs. Do not invoke Solver.

- [ ] **Step 5: Run generation tests repeatedly**

Run the same fixture at least three times. Expected: no flaky failures.

---

### Task 7: Implement single-asset persistence, JSON, and batch generation

**Files:**
- Create Persistence files from the file map
- Create Batch files from the Generation file map
- Create: `Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs`
- Create: `Assets/Tests/EditMode/Persistence/FlowLevelPersistenceTests.cs`

- [ ] **Step 1: Create Persistence asmdef**

Reference Core and Difficulty. Keep asset class runtime-safe; do not reference UnityEditor.

- [ ] **Step 2: Write failing FlowLevelAsset tests**

Create a temporary asset under `Assets/Temp/FlowPuzzleTests/`, assign level, solution, report, seed, and coverage, save/reload, and assert all values persist.

- [ ] **Step 3: Implement FlowLevelAsset**

Use `[CreateAssetMenu]` only if it helps manual inspection; generated assets still go through the repository.

- [ ] **Step 4: Write failing JSON tests**

Assert:

- level JSON contains pairs but no `paths`;
- solution JSON contains paths;
- filenames are `level_1001.json` and `solution_1001.json`;
- failure is structured when output path is invalid.

- [ ] **Step 5: Implement JSON exporter**

Use `JsonUtility.ToJson(..., true)` and `System.IO`. Do not call AssetDatabase here.

- [ ] **Step 6: Implement Editor asset repository**

Required operations:

```csharp
SaveNew(FlowGeneratedLevel level, string folder)
Overwrite(FlowLevelAsset asset, FlowGeneratedLevel level)
SaveAs(FlowLevelAsset source, FlowGeneratedLevel level, string folder, string name)
```

Validate before save. Use `Undo.RecordObject` only for overwrite. Call `SetDirty`, `SaveAssets`, and `Refresh` on the main thread.

- [ ] **Step 7: Write and implement batch generation**

Use:

```text
levelSeed = baseSeed + levelId * 9973
```

Continue after individual failures. Return requested, successful, failed counts and per-level messages.

- [ ] **Step 8: Run persistence and batch tests**

Clean only the test-created temporary folder after tests.

---

### Task 8: Implement Application services and UI Toolkit automatic-generation workflow

**Files:**
- Create Application files from the file map
- Create: `Assets/Scripts/FlowPuzzle/Application/FlowPuzzle.Application.asmdef`
- Create Editor preset, UXML, USS, UI panel, board-view, and window files from the file map
- Create: `Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef`
- Create: `Assets/Tests/EditMode/Editor/FlowBoardViewGeometryTests.cs`

- [ ] **Step 1: Implement generation service**

Expose:

```csharp
FlowGenerationResult GenerateOne(int levelId, FlowGenerationConfig config);
FlowValidationResult Validate(FlowGeneratedLevel level);
```

The service composes concrete dependencies once; the EditorWindow does not.

- [ ] **Step 2: Implement preset library**

Provide Custom, Easy, Normal, Hard, Expert. Applying a preset copies values into editable config; it never locks fields.

- [ ] **Step 3: Write failing board geometry tests**

Test cell rectangles, pointer-to-cell conversion, aspect-fit board bounds, outside-board rejection, and Y-axis orientation without requiring a rendered panel.

- [ ] **Step 4: Implement FlowBoardViewGeometry**

Keep coordinate math separate from `VisualElement` event code so it is deterministic and testable.

- [ ] **Step 5: Build the UI Toolkit window shell**

Use:

- `CreateGUI()` as the entry point;
- UXML for the stable window hierarchy;
- USS for split layout, spacing, minimum widths, and status classes;
- a `TwoPaneSplitView` or equivalent responsive structure;
- `ScrollView` for long parameter and result panels.

Do not implement a main `OnGUI()` method. If an exceptional compatibility control needs IMGUI, isolate it in an `IMGUIContainer`.

- [ ] **Step 6: Build parameter and status panels**

Use UI Toolkit fields and controls for:

- base and advanced `Foldout` settings;
- preset and difficulty `EnumField`;
- numeric parameters;
- action `Button`s;
- `HelpBox` diagnostics;
- `ProgressBar`;
- score labels;
- virtualized `ListView` for batch results.

Validate impossible ranges before enabling Generate.

- [ ] **Step 7: Build automatic-generation actions**

Implement:

- Apply Preset
- Generate One
- Generate Batch
- Save Current Asset
- Export Current JSON
- Validate Current
- Clear Preview

Generate One updates only memory. Batch saves successful assets.

- [ ] **Step 8: Implement FlowBoardView**

Derive from `VisualElement`. Use `generateVisualContent` with `Painter2D` to render empty cells, colored paths, grid lines, and larger endpoint markers. Expose data-setting methods that call `MarkDirtyRepaint()`.

Register pointer callbacks, but in this phase emit only hover/selection intents. The view must not call application services or mutate assets.

- [ ] **Step 9: Restore UI state after domain reload**

Store only serializable window state such as selected tab, foldout state, selected preset, and loaded asset reference. Re-query visual elements and re-register callbacks in `CreateGUI()`.

- [ ] **Step 10: Run geometry tests, compile, and perform manual smoke test**

Open `Tools/Flow Puzzle/Level Generator`, generate a seeded 6×6 level, validate it, save it, and reload the asset.

---

### Task 9: Implement Draft editing and command-based Undo/Redo

**Files:**
- Create Draft files and Undo files from the file map
- Create: `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- Create: `Assets/Tests/EditMode/Editor/FlowEditorCommandHistoryTests.cs`

- [ ] **Step 1: Write failing Draft invariant tests**

Cover:

- exactly two endpoints per configured color;
- edits mark solution dirty and validation false;
- resize removes or reports out-of-bounds data deterministically;
- mapping from asset deep-copies data;
- mapping never mutates the source asset.

- [ ] **Step 2: Implement FlowLevelDraft and mapper**

Draft is the only mutable Editor model. Do not store UnityEngine objects inside it.

- [ ] **Step 3: Write failing command-history tests**

Cover execute, undo, redo, redo clearing after a new command, snapshot restoration, and one stroke undoing multiple cells.

- [ ] **Step 4: Implement command history**

Use exact commands for endpoint movement. Use before/after deep snapshots for resize and multi-field operations.

- [ ] **Step 5: Add Draft editing UI**

Support:

- New Draft
- Load Asset
- Add/Remove Color
- Place/Move/Remove Endpoint
- Save/Save As only when solution is valid
- Undo/Redo

Route `FlowBoardView` pointer intents through the Draft command layer. The view does not mutate Draft directly.

At the end of this task, `Complete` must be visible but disabled with the message “Available after local solver is installed.” Do not add a temporary solver.

- [ ] **Step 6: Run Draft and command tests**

Expected: all tests pass.

---

### Task 10: Implement endpoint-anchored fixed-path strokes

**Files:**
- Modify: `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/Undo/DrawConstraintStrokeCommand.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardView.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs`
- Modify: Draft and command tests

- [ ] **Step 1: Write failing constraint tests**

Accept only:

- a chain beginning at exactly one endpoint of its own color;
- at least one non-endpoint path cell;
- orthogonal continuity;
- no self-intersection, branching, other endpoint traversal, or overlap.

Reject floating, multi-segment, and complete endpoint-to-endpoint constraints.

- [ ] **Step 2: Implement constraint mutation APIs**

Draft must expose atomic operations that either apply a legal stroke or leave state unchanged with a diagnostic.

- [ ] **Step 3: Implement stroke command grouping**

`PointerDownEvent` captures the pointer and starts a draft stroke; `PointerMoveEvent` accumulates cells; `PointerUpEvent` releases capture and commits one `DrawConstraintStrokeCommand`. `PointerCancelEvent` abandons the uncommitted stroke. Erase follows the same grouping.

- [ ] **Step 4: Render fixed constraints distinctly**

Use `Painter2D` with a visible pattern, border, or width difference so constraints are distinguishable from solver-generated continuation.

- [ ] **Step 5: Run tests and manually verify one-stroke Undo**

---

### Task 11: Implement the synchronous exact local solver

**Files:**
- Create Solving core files through `BacktrackingFlowPuzzleSolver.cs`
- Create: `Assets/Scripts/FlowPuzzle/Solving/FlowPuzzle.Solving.asmdef`
- Create: `Assets/Tests/EditMode/Solving/BacktrackingFlowPuzzleSolverTests.cs`

- [ ] **Step 1: Write known-solution tests**

Start with deterministic 3×3 and 4×4 endpoint layouts. Assert `Solved`, validate the returned solution, and stop after the first solution.

- [ ] **Step 2: Write known-no-solution tests**

Use tiny layouts where connectivity is provably blocked. Assert `NoSolution` only when search completes.

- [ ] **Step 3: Write fixed-prefix tests**

Prove an endpoint-anchored fixed path is preserved and extended from its open end.

- [ ] **Step 4: Write timeout/cancellation semantics tests**

Inject a tiny node budget and a cancelled token. Assert `Timeout` and `Cancelled`, never `NoSolution`.

- [ ] **Step 5: Implement solver input validation and BFS prechecks**

Reject malformed endpoint pairs and constraints as `InvalidInput`.

- [ ] **Step 6: Implement recursive color/path search**

Algorithm:

1. sort colors by fixed-prefix presence, reachable area, then Manhattan distance;
2. enumerate one color’s simple paths using DFS;
3. after committing a path, BFS-check every remaining pair;
4. backtrack on disconnection;
5. return the first fully valid solution.

Use a node counter and cancellation checks at bounded intervals. Candidate cells must use deterministic ordering.

- [ ] **Step 7: Run solver tests**

Expected: small cases pass quickly and deterministically.

---

### Task 12: Implement async completion provider and Completion service

**Files:**
- Create remaining Completion files under Solving
- Create: `Assets/Scripts/FlowPuzzle/Application/FlowLevelCompletionService.cs`
- Create: `Assets/Tests/EditMode/Solving/LocalExactCompletionProviderTests.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`

- [ ] **Step 1: Write failing provider tests**

Assert:

- work executes asynchronously;
- cancellation propagates;
- progress can be observed;
- returned solution is validated;
- timeout remains timeout.

- [ ] **Step 2: Implement LocalExactCompletionProvider**

Wrap only pure-data solver work in `Task.Run`. Do not capture or touch EditorWindow, ScriptableObject, AssetDatabase, GUIContent, Color, or other Unity objects.

- [ ] **Step 3: Implement Completion service**

On `Solved`:

1. validate returned solution;
2. compute difficulty;
3. build a `FlowGeneratedLevel`;
4. return immutable completion data for the main thread to apply.

- [ ] **Step 4: Enable Complete and Cancel in Editor**

Copy Draft data into a request before starting. Poll task state from `EditorApplication.update`; apply results only on the main thread. Close/dispose cancellation sources when the window disables.

Update `ProgressBar`, `HelpBox`, buttons, and `FlowBoardView` only from the main thread. Call `MarkDirtyRepaint()` after applying solved data.

- [ ] **Step 5: Run provider tests and manually test responsive cancellation**

---

### Task 13: Complete diagnostics, suggestions, and model-tool contracts

**Files:**
- Create Tool contract files from the file map
- Modify generation, solving, application, and Editor diagnostic handling
- Add focused diagnostic tests to existing fixtures

- [ ] **Step 1: Define stable diagnostic codes**

At minimum:

```text
InvalidDimensions
ImpossibleMinimumOccupancy
ImpossibleCoverageRange
PathGenerationFailed
CoverageOutOfRange
ValidationFailed
DifficultyOutOfRange
MaxLevelAttemptsReached
InvalidFixedConstraint
SolverTimeout
SolverCancelled
NoSolution
AssetAlreadyExists
InvalidOutputFolder
```

- [ ] **Step 2: Map diagnostics to parameter suggestions**

Suggestions identify parameter names and directions. Only expose Apply Suggestion when one safe scalar edit is unambiguous.

- [ ] **Step 3: Implement Retry Same Seed and Retry New Seed**

Same-seed retry must preserve all config values. New-seed retry must change only the seed.

- [ ] **Step 4: Implement FlowSolverToolAdapter contract**

Provide pure DTO methods corresponding to:

- solve board;
- solve with endpoint-anchored constraints;
- validate solution;
- evaluate difficulty;
- analyze failure.

Do not implement HTTP, API keys, model selection, or an LLM provider.

- [ ] **Step 5: Test diagnostic stability**

Assert common impossible configurations produce the expected code and mention the actionable parameter.

---

### Task 14: Final integration, performance checks, and acceptance verification

**Files:**
- Modify only files that fail integration checks
- Update tests where an actual cross-module contract requires it

- [ ] **Step 1: Run all EditMode tests**

Use the batch command at the top of this plan after closing Unity.

- [ ] **Step 2: Inspect test and compile logs**

Search:

```powershell
Select-String `
  -Path 'D:\Unity\UnityProj\FightMatch\Logs\FlowPuzzleTests.log' `
  -Pattern 'error CS|FAIL|Exception'
```

Expected: no compiler errors or failing tests.

- [ ] **Step 3: Run deterministic generation matrix**

Generate at least:

```text
5×5 Easy, seed 101
6×6 Normal, seed 202
7×7 Hard, seed 303
```

Generate each twice and compare serialized level/solution data for equality.

- [ ] **Step 4: Run solver performance matrix**

Use known 3×3 through 7×7 fixtures. Record elapsed time and visited nodes. Confirm timeout/cancel remain responsive.

- [ ] **Step 5: Perform complete Editor acceptance flow**

1. Open the tool from the menu.
2. Confirm the window is built through UI Toolkit and remains usable when resized.
3. Apply a preset and customize parameters.
4. Generate, preview, validate, save SO, and export JSON.
5. Create a Draft, place endpoints, and complete it.
6. Draw an endpoint-anchored fixed path and complete it.
7. Undo/redo an endpoint move and a whole stroke.
8. Save As and reload the asset.
9. Run a batch where one level fails without stopping the rest.
10. Trigger a script reload and confirm required UI state and Draft selection recover.

- [ ] **Step 6: Confirm prohibited behavior is absent**

Search code for:

```text
UnityEngine.Random
unique solution
fill entire board
IFlowPuzzleSolver usage inside FlowSolutionGenerator
AssetDatabase usage outside Editor
main-window OnGUI implementation
one VisualElement per board cell
```

Expected: no prohibited dependency or rule.

- [ ] **Step 7: Final compile-only verification**

Run the compile-only command. Expected: exit code `0`.

---

## Deferred commit checkpoints

When the project becomes a Git repository, commit after each task with focused messages such as:

```text
feat: add flow puzzle core contracts
feat: validate flow puzzle solutions
feat: score flow puzzle difficulty
feat: generate deterministic flow puzzle levels
feat: persist flow puzzle level assets
feat: add flow puzzle editor workflow
feat: add draft editing and undo
feat: solve manual flow puzzle layouts
test: verify flow puzzle editor tool
```

Do not combine unrelated phases into one commit.
