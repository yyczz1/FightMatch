# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-EDITOR-PRESET-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-05`

**Order in group:** `2`

**Depends on:** `FLOW-APPLICATION-001`

**Goal:** Add deterministic, editable Editor presets that copy practical
starting values into an existing generation config.

## Scope whitelist

**Files allowed to read:**

- project rules and Group 5 documents
- approved spec and plan
- Core config and current Editor assembly
- existing tests

**Files allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef`
- `Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowDifficultyPreset.cs`
- `Assets/Scripts/FlowPuzzle/Editor/FlowDifficultyPresetLibrary.cs`
- `Assets/Tests/EditMode/Editor/FlowDifficultyPresetLibraryTests.cs`
- generated `.meta` files for new folders/files

**Files forbidden to modify:** every other file.

## Required API and values

```csharp
public enum FlowDifficultyPreset
{
    Custom,
    Easy,
    Normal,
    Hard,
    Expert
}

public static class FlowDifficultyPresetLibrary
{
    public static void Apply(
        FlowDifficultyPreset preset,
        FlowGenerationConfig target);
}
```

Throw for null target and undefined enum. `Custom` changes nothing.

Apply these practical starting values:

| Preset | Board | Colors | Coverage | Path length | Turn | Interaction |
|---|---|---:|---|---|---:|---:|
| Easy | 5×5 | 2 | 0.25–0.50 | 2–5 | -0.5 | -0.5 |
| Normal | 6×6 | 3 | 0.35–0.65 | 2–7 | 0 | 0 |
| Hard | 7×7 | 4 | 0.50–0.80 | 3–10 | 0.75 | 0.75 |
| Expert | 8×8 | 5 | 0.65–0.90 | 3–13 | 1.5 | 1.5 |

All non-Custom presets also set:

- `maxPathAttempt = 250`;
- `maxLevelAttempt = 100`;
- `useRandomSeed = false`;
- `useTargetDifficulty = false`;
- `targetDifficulty` to the matching tier;
- `useTargetScoreRange = false`;
- target score bounds to the matching tier range
  (`Easy 0–59.999`, `Normal 60–119.999`, `Hard 120–199.999`,
  `Expert 200–10000`);
- endpoint/detour/bottleneck/solver fields remain unchanged.

Applying a preset copies values; fields remain editable and no live binding is
stored.

Update Editor asmdef to reference Application and Generation. Keep Editor-only.

## Tests

Cover every exact table value, Custom no-op, undefined/null rejection,
unmentioned fields unchanged, and independent editable configs.

## Non-goals

- No ScriptableObject preset assets, localization, UI, runtime presets, or
  dynamic registration.

## Verification

Run all EditMode tests. Confirm Editor asmdef remains Editor-only.

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add editable difficulty presets`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
