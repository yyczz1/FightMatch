# External DeepSeek Task Packet

## Task metadata

**Task ID:** `FLOW-DRAFT-001`

**Status:** `APPROVED_FOR_WORKER`

**Group ID:** `FLOW-GROUP-06`

**Order in group:** `1`

**Depends on:** `FLOW-GROUP-05`

**Goal:** Add the serializable, deeply copyable mutable Draft model and
Asset/generated-result mapping required by manual level editing.

## Scope whitelist

**Files allowed to read:**

- project rules and all Group 6 documents
- approved Flow Puzzle specification sections 3.8, 6.1, 6.2, and Phase 6
- accepted Core and Persistence source
- current Editor repository source and EditMode test assembly

**Files allowed to modify:** `NONE`

**Files allowed to create:**

- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftPairData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftConstraintData.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMutationResult.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftEditTool.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowLevelDraft.cs`
- `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs`
- `Assets/Tests/EditMode/Editor/FlowLevelDraftTests.cs`
- generated `.meta` files for the listed files and new Draft folder

**Files forbidden to modify:** every existing file.

## Required behavior

1. Draft contains serializable plain data only and stores no `UnityEngine.Object`.
2. Each unique non-negative color ID may temporarily contain zero, one, or two
   endpoints while editing, but never more than two.
3. `HasCompleteEndpoints` is true only when every configured color has exactly
   two in-bounds endpoints.
4. Endpoints cannot overlap another endpoint.
5. Support adding the lowest unused color ID, removing a selected color,
   placing/moving/removing endpoints, resizing, deep copying, and restoring
   from a deep snapshot.
6. Every structural edit sets `isSolutionDirty=true` and `isValidated=false`.
   It must not silently make an old solution valid again.
7. Shrinking the board deterministically removes out-of-bounds endpoints and
   clears any constraint containing an out-of-bounds cell.
8. Draft stores at least level ID, dimensions, pairs, fixed constraints,
   current solution, current difficulty, dirty/validated state, and seed.
9. `FlowDraftMapper` supports `FromAsset`, `FromGeneratedLevel`, and
   `ToGeneratedLevel`.
10. Every mapping and snapshot is a complete deep copy with no shared mutable
    lists and no source mutation.
11. `ToGeneratedLevel` refuses incomplete endpoints, missing solution, dirty
    solution, or unvalidated Draft.
12. A valid Asset or generated level maps to an initially clean and validated
    Draft.
13. Mutation failures return a stable code/message, are deterministic, and
    leave Draft completely unchanged.

`FlowDraftConstraintData` exists in this packet as a serializable data
container only. Full constraint legality is implemented by
`FLOW-CONSTRAINT-001`.

## Tests

Cover:

- incomplete endpoint editing and exact completion detection;
- duplicate/third/overlapping endpoint rejection;
- add/remove color behavior;
- edit invalidation of old solution;
- resize cleanup;
- full deep copy and `RestoreFrom`;
- Asset and generated-result source immutability;
- valid and invalid `ToGeneratedLevel`;
- failed mutations are atomic.

## Non-goals

- No command history, UI, pointer handling, complete constraint validation,
  solver, async work, runtime code, or persistence format change.

## Verification

Run the new Draft tests, then all EditMode tests and compile.

Static:

```powershell
rg -n "UnityEngine|UnityEditor|AssetDatabase|IFlowPuzzleSolver|Task\.Run" `
  Assets/Scripts/FlowPuzzle/Editor/Draft
```

Expected: no matches.

## Maximum change scope

- production files: 6
- test files: 1
- approximate maximum diff: 1000 changed lines

## Git checkpoint permission

**Local commit allowed:** `YES`

**Required commit message:** `feat: add mutable flow level draft`

## What to do if blocked

Return `BLOCKED`, make no commit, and stop the group.
