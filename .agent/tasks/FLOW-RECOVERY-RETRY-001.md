# External DeepSeek Recovery Packet

**Task ID:** `FLOW-RECOVERY-RETRY-001`  
**Group:** `FLOW-RECOVERY-01`  
**Order:** `7`  
**Depends on:** `FLOW-RECOVERY-DIAGNOSTICS-001`  
**Goal:** Implement Apply Suggestion, Retry Same Seed, and Retry New Seed
without changing unrelated configuration or writing assets.

## Scope

**Allowed to modify:**

- `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDiagnosticsPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowParameterPanel.cs`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uxml`
- `Assets/Scripts/FlowPuzzle/Editor/UI/FlowLevelGeneratorWindow.uss`
- `Assets/Tests/EditMode/Editor/FlowLevelGeneratorWindowTests.cs`

**Allowed to create:**

- `Assets/Tests/EditMode/Editor/FlowDiagnosticRetryTests.cs`
- generated `.meta` for that test only

Every other file is forbidden.

## Required behavior

1. Apply Suggestion is visible/enabled only for exactly one supported scalar
   field with an explicit safe value.
2. Apply changes only the named UI field, performs no generation, and updates
   validation/button state.
3. Retry Same Seed uses the exact failed `usedSeed` and preserves every other
   config/request/UI field.
4. Retry New Seed uses deterministic `unchecked(usedSeed + 1)` and changes
   only the effective seed.
5. Random-seed retries may use a transient fixed request but may not silently
   rewrite unrelated visible controls.
6. Retry actions only generate in memory. They never save, overwrite, export,
   or batch.
7. Actions are disabled without eligible diagnostic and during active
   completion/batch operations.
8. Repeated `CreateGUI` registers each callback once.

## Required tests

Snapshot all config fields and relevant UI controls. Prove same-seed deep
layout equality, new-seed-only change including overflow, single-field apply,
ambiguous/no-safe suggestion disabling, no asset writes, and one callback per
click.

## Verification and commit

Focused retry tests, then full EditMode suite.

Commit exactly:

```text
fix: implement diagnostic retry actions
```
