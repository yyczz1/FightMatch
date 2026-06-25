# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-VALIDATION-001`

**Status:** `ACCEPTED`

**Group ID:** `FLOW-GROUP-02`

**Order in group:** `1`

**Depends on:** `FLOW-GROUP-01`

**Goal:**

Implement deterministic validation for a supplied Flow Puzzle recommendation
without checking full-board coverage, uniqueness, or alternative solutions.

**Background:**

Core contracts, `FlowBoard`, and path utilities are accepted. This packet adds
the pure-C# Validation assembly and verifies that one given recommendation
connects every configured pair legally.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `.agent/groups/FLOW-GROUP-02.md`
- `.agent/tasks/FLOW-VALIDATION-001.md`
- `Assets/Scripts/FlowPuzzle/Core/**`
- `Assets/Tests/EditMode/Core/**`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

**Files allowed to modify:**

- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Validation/FlowPuzzle.Validation.asmdef`
- `Assets/Scripts/FlowPuzzle/Validation/FlowValidationResult.cs`
- `Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs`
- `Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs`
- Unity-generated `.meta` files for the new Validation and test folders
- Unity-generated `.meta` files corresponding to the four new files

**Files forbidden to modify:**

- `Assets/Scripts/FlowPuzzle/Core/**`
- existing Core tests
- existing `.meta` files
- `AGENTS.md`
- `.agent/**`
- `CLAUDE.md`
- `Packages/**`
- `ProjectSettings/**`
- `docs/**`
- every file not explicitly allowed above

Do not write `.meta` files manually.

## Required behavior

### Assembly

Create `FlowPuzzle.Validation`:

- root namespace `FlowPuzzle.Validation`;
- references only `FlowPuzzle.Core`;
- `noEngineReferences: true`.

Add `FlowPuzzle.Validation` to the existing test asmdef references without
changing other settings or reference order unnecessarily.

### Result contract

In namespace `FlowPuzzle.Validation`:

```csharp
[Serializable]
public sealed class FlowValidationResult
{
    public bool isValid;
    public string errorCode;
    public string errorMessage;

    public static FlowValidationResult Valid();
    public static FlowValidationResult Invalid(string code, string message);
}
```

`Valid()` returns `isValid=true` with null error fields.
`Invalid(...)` returns `isValid=false` and preserves the supplied values.

### Validator API

```csharp
public sealed class FlowSolutionValidator
{
    public FlowValidationResult Validate(
        FlowLevelData level,
        FlowSolutionData solution);
}
```

Return the first deterministic error in this order:

1. `NullLevel`
2. `NullSolution`
3. `MissingPairs` when `level.pairs` is null
4. `MissingPaths` when `solution.paths` is null
5. `InvalidDimensions` when width or height is non-positive
6. `DuplicatePairColor`
7. `EndpointOutOfBounds`
8. `DuplicatePathColor`
9. `MissingPath`
10. `ExtraPathColor`
11. Per-path geometry in `level.pairs` list order:
    - `PathTooShort`
    - `EndpointMismatch`
    - `CellOutOfBounds`
    - `InvalidAdjacency`
    - `SelfIntersection`
    - `ForeignEndpointTraversal`
    - `PathOverlap`

Required rules:

- pair `colorId` values are unique;
- both endpoints are inside the board;
- each pair has exactly one path with the same color ID;
- no path exists for an unknown color;
- every path contains at least two cells;
- path endpoints may match `endpointA → endpointB` or the reverse;
- all cells are inside the board;
- consecutive cells are orthogonally adjacent;
- one path cannot repeat a cell;
- a path cannot contain another color's endpoint at any position;
- different colors cannot share any cell;
- unused board cells are legal;
- path count and occupied-cell count need not fill the board;
- do not search for, count, or reject multiple solutions.

Use dictionaries or hash sets only for lookup and duplicate detection. Do not
serialize them and do not let their enumeration order choose the first error.
Iterate pairs and paths through their list order when error ordering matters.

Error messages must identify the offending color ID and coordinate where
applicable. Tests should assert stable error codes, not whole message prose.

### Tests

Create explicit tests for:

- valid solution with unused cells;
- valid reversed endpoint orientation;
- null level and null solution;
- null pairs and null paths;
- invalid dimensions;
- duplicate pair color;
- pair endpoint out of bounds;
- duplicate path color;
- missing path;
- extra path color;
- path shorter than two cells;
- mismatched endpoints;
- path cell out of bounds;
- diagonal or disconnected step;
- path self-intersection;
- overlap between two colors;
- path passing through another color endpoint;
- a board with multiple possible solutions is not rejected merely for being
  non-unique.

Use small hand-built boards. Do not call a solver or generator in tests.

## Non-goals

- No unique-solution detection or solution counting.
- No full-board requirement.
- No difficulty evaluation, generation, solving, persistence, JSON, Application
  service, or Editor behavior.
- Do not repair or normalize malformed data.
- Do not add interfaces, factories beyond the two result factories, or
  speculative abstractions.
- No unrelated formatting, cleanup, comments, or renaming.

## Constraints

- Production code may use only `System`, `System.Collections.Generic`,
  `FlowPuzzle.Core`, and its own namespace.
- No `UnityEngine` or `UnityEditor`.
- Keep the first error deterministic.
- Preserve existing Core behavior and tests.
- If another type or existing source modification is required, return
  `BLOCKED`.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `YES` | Create only `FlowValidationResult` and `FlowSolutionValidator` APIs listed above |
| New dependency/package | `NO` | None |
| Build/configuration changes | `YES` | Create Validation asmdef and add its reference to test asmdef |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Serialized format changes | `YES` | Introduce only `FlowValidationResult` fields |
| Unity asset or `.meta` changes | `YES` | Listed source, asmdef, test, folders, and generated metas |
| Generated-file changes | `NO` | No Library, Temp, Logs, csproj, or sln files committed |

## Maximum change scope

**Maximum changed production files:** `0`

**Maximum changed test/config files:** `1`

**Maximum new files:** `12` including generated folder/file metas

**Approximate maximum diff:** `750 changed lines` excluding `.meta`

## Acceptance criteria

- [ ] Validation assembly references only Core and has no engine references.
- [ ] Result factories match the required contract.
- [ ] Validator implements every required rule and stable error code.
- [ ] Valid recommendations may leave cells unused.
- [ ] Reversed endpoint orientation is accepted.
- [ ] Multiple possible solutions are not searched or rejected.
- [ ] Focused validator tests pass.
- [ ] All 78 existing tests remain passing.
- [ ] Only the allowed test asmdef is modified.
- [ ] No file outside the whitelist changes.

## Verification steps

### Red verification

Create the test file and required test asmdef reference first, before production
implementation.

Run EditMode tests once. Expected: compile failure naming missing Validation
types. Record a short excerpt.

### Green verification

```powershell
rg -n "UnityEngine|UnityEditor" Assets/Scripts/FlowPuzzle/Validation
```

Expected: no matches.

Run all EditMode tests with the single-process command in
`.agent/groups/FLOW-GROUP-02.md`.

Expected:

```text
Unity exit 0.
XML result Passed.
Failed=0.
At least 78 previous tests plus the new validation tests execute.
```

Before commit:

```powershell
git status --short |
  Where-Object { $_ -notmatch '^ M \.claude/settings\.local\.json$' }
```

Expected: only packet-approved files.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: validate flow puzzle solutions`

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

Return `BLOCKED`, make no local commit, and do not begin
`FLOW-DIFFICULTY-001`.

## Completion record

- Primary commit: `8c7ed5b6fdf1e5317ec2993c9342f1dc25f92685`
- Corrective commit:
  `009b0abc006bfcc4acaae8b3561d041def90b1ae`
- Final Codex verdict: `ACCEPT`
- Group verification: Unity EditMode `136/136` passed
