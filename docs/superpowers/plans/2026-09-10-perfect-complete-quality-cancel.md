# Perfect Complete Quality and Cancellation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prefer clean tied candidates, expose the existing timeout clearly, and add safe user cancellation.

**Architecture:** Extend the application-layer candidate score with intrinsic cleanliness metrics while preserving explicit parameter priority. Reuse the existing cancellation token source in the Editor window and add one UI Toolkit button; no new thread or timer system is introduced.

**Tech Stack:** Unity 2022.3, C#, UI Toolkit, NUnit/EditMode tests.

---

### Task 1: Clean candidate tie-breaking

**Files:**
- Modify: `Assets/Scripts/FlowPuzzle/Application/FlowPerfectCandidateScore.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Application/FlowPerfectCandidateScorer.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Application/FlowPerfectCompletionProvider.cs`
- Test: `Assets/Tests/EditMode/Application/FlowPerfectCompletionTests.cs`

- [x] Add failing tests for neutral clean-path preference and retaining an equal current recommendation.
- [x] Add detour, turn, and occupied-cell tie-break metrics.
- [x] Seed candidate selection with the clean current solution when supplied.
- [x] Run the focused application tests.

### Task 2: Timeout explanation and stop action

**Files:**
- Modify: `Assets/Scripts/FlowPuzzle/Editor/UI/FlowParameterPanel.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- Test: `Assets/Tests/EditMode/Editor/FlowCompletionUndoAndDropTests.cs`

- [x] Add failing tests for the timeout label and Stop Solving lifecycle.
- [x] Add and wire `Stop Solving` to the existing cancellation token source.
- [x] Ensure cancellation preserves Draft data and history.
- [x] Run focused Editor tests.

### Task 3: Regression verification

- [x] Run the full EditMode test suite.
- [x] Confirm zero compile errors and no `Assets/Temp` artifacts.
- [x] Review the final diff without committing or pushing.
