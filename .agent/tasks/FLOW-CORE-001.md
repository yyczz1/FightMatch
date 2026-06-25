# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-CORE-001`

**Status:** `ACCEPTED`

**Goal:**

Create the smallest compilable Flow Puzzle Core foundation: a pure C# Core assembly, the `FlowPos` value type, and focused EditMode tests for its value semantics.

**Background:**

The approved Flow Puzzle implementation plan starts with pure C# Core contracts. This packet intentionally implements only `FlowPos` and the minimum assembly/test scaffolding. Remaining DTOs belong to later packets.

**Current context:**

- Unity project version: `2022.3.18f1`.
- Unity Test Framework `1.1.33` is available through existing package dependencies.
- No project C# files or asmdefs currently exist.
- Core algorithms must not depend on `UnityEngine`.
- Approved references:
  - `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
  - `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/CODING_RULES.md`
- `.agent/VALIDATION.md`
- `ProjectSettings/ProjectVersion.txt`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md`
- `docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md`

**Files allowed to modify:**

- `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef`
- `Assets/Scripts/FlowPuzzle/Core/FlowPos.cs`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- `Assets/Tests/EditMode/Core/FlowPosTests.cs`

**Files forbidden to modify:**

- `AGENTS.md`
- `.agent/**`
- `CLAUDE.md`
- `.claude/**`
- `Packages/**`
- `ProjectSettings/**`
- `docs/**`
- `.gitignore`
- `.github/**`
- all existing scenes and assets
- every file not explicitly listed under “Files allowed to create”

Do not create Unity `.meta` files manually. Unity/Codex will generate and review them after the patch is applied.

## Required behavior

1. Create `FlowPuzzle.Core` as a pure C# asmdef with `noEngineReferences: true`.
2. Create a serializable `FlowPos` struct in namespace `FlowPuzzle.Core`.
3. `FlowPos` must expose public integer fields `x` and `y`.
4. `FlowPos` must provide a constructor accepting `x` and `y`.
5. `FlowPos` must implement `IEquatable<FlowPos>`.
6. Equality must depend only on `x` and `y`.
7. `Equals(object)`, `GetHashCode()`, `==`, and `!=` must agree with typed equality.
8. Create an Editor-only EditMode test asmdef referencing `FlowPuzzle.Core` and Unity test assemblies.
9. Add focused NUnit tests for constructor values, equality, inequality, object equality, and equal hash codes.

## Non-goals

- Do not create any other Flow Puzzle DTO.
- Do not create `FlowBoard`, path utilities, validation, generation, difficulty, solving, persistence, application, or Editor UI code.
- Do not add `Vector2Int` conversion helpers.
- Do not add JSON helpers.
- Do not add documentation comments unless existing style requires them.
- Do not refactor, format, rename, or edit unrelated files.

## Constraints

- Use only `System` in `FlowPos.cs`.
- Do not reference `UnityEngine` or `UnityEditor` from `FlowPuzzle.Core`.
- Keep implementation direct and self-contained.
- Do not add an interface, base class, utility class, factory, or extension method.
- Do not add dependencies or modify package files.
- Do not modify files outside the whitelist.
- If required work exceeds this packet, return `BLOCKED`.

## Project-specific constraints

- Unity version is fixed at `2022.3.18f1`.
- `FlowPuzzle.Core.asmdef` must use root namespace `FlowPuzzle.Core`.
- Test assembly must be restricted to the Editor platform.
- Test assembly must reference `FlowPuzzle.Core`.
- Test assembly must declare `TestAssemblies` through the asmdef's supported optional Unity references.
- Unity-generated `.meta`, `.csproj`, and `.sln` files are outside this task.

## Protected-change permissions

| Change type | Allowed? | Exact allowed scope |
|---|---:|---|
| Public API changes | `YES` | Create public `FlowPuzzle.Core.FlowPos` only |
| New dependency/package | `NO` | None |
| Build/configuration changes | `YES` | Create only the two listed `.asmdef` files |
| Lockfile changes | `NO` | None |
| CI changes | `NO` | None |
| Database/schema migration | `NO` | None |
| Serialized format changes | `YES` | Introduce only `FlowPos.x` and `FlowPos.y` |
| Unity asset or `.meta` changes | `YES` | Create only the listed `.cs` and `.asmdef` source assets; no `.meta` files |
| Generated-file changes | `NO` | None |

## Maximum change scope

**Maximum changed production files:** `0`

**Maximum changed test files:** `0`

**Maximum new files:** `4`

**Approximate maximum diff:** `120 changed lines`

If the task cannot fit this budget, return `BLOCKED` before expanding scope.

## Acceptance criteria

- [ ] Exactly the four allowed files are created.
- [ ] `FlowPuzzle.Core` has no Unity engine references.
- [ ] `FlowPos` is serializable and has public `int x` and `int y`.
- [ ] All equality APIs produce consistent results.
- [ ] Tests cover equal and unequal coordinates plus hash-code consistency.
- [ ] Test assembly is Editor-only and references `FlowPuzzle.Core`.
- [ ] No existing file changes.
- [ ] No files outside the whitelist change.
- [ ] No unapproved protected change is made.

## Verification steps

First perform static checks:

```powershell
rg -n "UnityEngine|UnityEditor" Assets/Scripts/FlowPuzzle/Core
```

Expected result:

```text
No matches.
```

If Unity 2022.3.18f1 is available and this project is not open in another Unity process, run:

```powershell
& $env:UNITY_EXE `
  -batchmode -nographics -quit `
  -projectPath 'D:\Unity\UnityProj\FightMatch' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\Unity\UnityProj\FightMatch\TestResults.xml' `
  -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FLOW-CORE-001.log'
```

Expected result:

```text
Unity exits with code 0 and FlowPos tests report zero failures.
```

If Unity is unavailable or the project is open, report the test command as `NOT RUN` with the exact reason. Do not invent a passing result.

## Expected output format

Return only:

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <exit code / test count / concise evidence>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO
- Protected changes made: YES/NO

PATCH:
<unified diff preferred; if edits were applied directly, provide a concise diff summary>

BLOCKER:
<required only when STATUS is BLOCKED>
```

Do not include an architecture essay or unrelated recommendations.

## What to do if blocked

Return:

```text
BLOCKED
Reason:
Missing information:
Required decision:
Files inspected:
No changes made: Yes/No
```

Do not guess. Do not create a partial out-of-scope solution.

## Completion record

- Worker status: `COMPLETED`
- Initial Codex verdict: `NEEDS_FIX`
- Corrective task: `FLOW-CORE-001-FIX-01`
- Final Codex verdict: `ACCEPT`
- Verification: Unity EditMode tests passed `13/13`; compiler errors `0`
- Accepted implementation commit: `039ee4f`
- Validation-environment cleanup commit: `9829e99`
