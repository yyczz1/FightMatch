# External DeepSeek Task Packet

## Metadata

**Task ID:** `FLOW-EDITOR-RETRY-001`

**Group:** `FLOW-GROUP-08`

**Order:** `2`

**Depends on:** `FLOW-DIAGNOSTICS-001`

**Goal:** Present structured suggestions and add safe Apply Suggestion, Retry
Same Seed, and Retry New Seed actions.

## Allowed files

**Modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowParameterPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

Every other file is forbidden.

## Required behavior

1. Show code, message, parameter, current value, direction, seed, attempts, and
   suggestions without parsing free-form strings.
2. Show Apply Suggestion only when one safe scalar value is explicitly present.
3. Applying changes only that named control and immediately revalidates button
   states. It does not automatically generate.
4. Retry Same Seed preserves every visible parameter and uses the exact failed
   `usedSeed`.
5. Retry New Seed changes only seed, using deterministic unchecked
   `usedSeed + 1`, then retries.
6. For random-seed mode, retry may use a transient fixed-seed request but must
   not silently rewrite unrelated visible controls.
7. Retry controls are disabled during completion/batch work and when no
   eligible diagnostic exists.
8. Retrying does not overwrite or save assets.
9. Repeated `CreateGUI` does not duplicate callbacks.

## Tests

Use complete before/after snapshots of all config fields. Prove same-seed
equality, new-seed-only change, overflow, safe single-field application,
ambiguous suggestion disabling, no asset writes, and duplicate-callback
prevention.

## Verification

Run Editor tests, then all EditMode tests.

## Commit

`feat: add diagnostic retry actions`
