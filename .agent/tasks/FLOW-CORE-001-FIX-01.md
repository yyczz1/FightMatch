# External DeepSeek Corrective Task Packet

## Task metadata

**Task ID:** `FLOW-CORE-001-FIX-01`

**Status:** `ACCEPTED`

**Goal:**

Correct the two bounded review findings from `FLOW-CORE-001`: restore the unauthorized Claude permission-file change and remove a hash-code test that exceeds the .NET equality contract.

## Background

The original implementation files are otherwise usable. Review found:

1. `.claude/settings.local.json` was modified outside the original whitelist by adding `PowerShell(Get-Process *)`.
2. `FlowPosTests.cs` asserts that two unequal values must have different hash codes. Hash collisions are legal; the task only requires equal values to have equal hash codes.

Unity-generated `.meta` files now exist because Codex imported the assets during independent validation. They are not part of this corrective task and must not be edited or deleted.

## Scope whitelist

**Files allowed to read:**

- `AGENTS.md`
- `.agent/tasks/FLOW-CORE-001.md`
- `.agent/tasks/FLOW-CORE-001-FIX-01.md`
- `.claude/settings.local.json`
- `Assets/Tests/EditMode/Core/FlowPosTests.cs`

**Files allowed to modify:**

- `.claude/settings.local.json`
- `Assets/Tests/EditMode/Core/FlowPosTests.cs`

**Files allowed to create:**

- `NONE`

**Files forbidden to modify:**

- `Assets/Scripts/**`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`
- all `.meta` files
- `Packages/**`
- `ProjectSettings/**`
- `Library/**`
- `Temp/**`
- `Logs/**`
- every file not explicitly listed under “Files allowed to modify”

## Required behavior

1. Remove only the added `"PowerShell(Get-Process *)"` entry from `.claude/settings.local.json`.
2. Restore `.claude/settings.local.json` so it has no Git diff relative to `HEAD`.
3. Remove only the `GetHashCode_DifferentValues_UsuallyDiffers` test method from `FlowPosTests.cs`.
4. Preserve `GetHashCode_SameValues_ReturnsEqual`.
5. Do not change `FlowPos`, either asmdef, formatting outside the removed test, or any generated `.meta` file.

## Non-goals

- Do not change the hash-code implementation.
- Do not add replacement tests requiring unequal values to have unequal hashes.
- Do not repair or delete Unity package caches.
- Do not change packages, dependencies, settings, architecture, or public APIs.
- Do not run `Get-Process`; it is unnecessary for this task.
- Do not run Unity batch mode for this corrective task.

## Maximum change scope

- Maximum files touched: `2`
- Maximum persistent Git diff files after correction: `1`
- Maximum changed lines in `FlowPosTests.cs`: deletion of the single invalid test method only
- New files: `0`

## Acceptance criteria

- [ ] `.claude/settings.local.json` has no diff relative to `HEAD`.
- [ ] `GetHashCode_DifferentValues_UsuallyDiffers` no longer exists.
- [ ] `GetHashCode_SameValues_ReturnsEqual` remains unchanged.
- [ ] No other test or production code changes.
- [ ] No file outside the two-file whitelist is modified by the worker.

## Verification steps

```powershell
git diff -- .claude/settings.local.json
```

Expected: no output.

```powershell
rg -n "GetHashCode_DifferentValues_UsuallyDiffers|PowerShell\(Get-Process \*\)" `
  Assets/Tests/EditMode/Core/FlowPosTests.cs `
  .claude/settings.local.json
```

Expected: no matches.

```powershell
rg -n "GetHashCode_SameValues_ReturnsEqual" `
  Assets/Tests/EditMode/Core/FlowPosTests.cs
```

Expected: exactly one match.

```powershell
git diff --check
```

Expected: no errors.

Do not claim Unity tests passed. Codex's independent Unity run was blocked by missing DLLs in the existing `Library/PackageCache/com.unity.collab-proxy@2.12.4` cache.

## Expected output format

```text
STATUS: COMPLETED | BLOCKED

CHANGED FILES:
- <path>

ACCEPTANCE CRITERIA:
- PASS | FAIL | NOT VERIFIED — <criterion and evidence>

VERIFICATION:
- RUN | NOT RUN — <command>
- Result: <exit code / concise evidence>

SELF-CHECK:
- Scope whitelist respected: YES/NO
- Forbidden files untouched: YES/NO
- Unrelated formatting/refactoring avoided: YES/NO
- New dependencies added: YES/NO

PATCH:
<unified diff or concise diff summary>

BLOCKER:
<required only when STATUS is BLOCKED>
```

## What to do if blocked

Return `BLOCKED` with the exact missing information. Do not change additional files and do not attempt to repair Unity, package, or cache state.

## Completion record

- Worker status: `COMPLETED`
- Codex verdict: `ACCEPT`
- Verification:
  - `.claude/settings.local.json` restored to `HEAD`
  - invalid unequal-value hash-code assertion removed
  - Unity EditMode tests passed `13/13`
- Accepted implementation commit: `039ee4f`
