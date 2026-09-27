# Perfect Complete Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add bounded deterministic candidate enumeration, parameter-driven scoring, and a `Perfect Complete` Editor action.

**Architecture:** `BacktrackingFlowPuzzleSolver` will expose a callback-based enumeration API while preserving its existing first-solution API. Application-layer scoring and provider classes will evaluate candidates without introducing a Solving→Difficulty dependency. The Editor will reuse its completion lifecycle and snapshot-based Undo/Redo handling.

**Tech Stack:** Unity 2022.3, C#, UI Toolkit, NUnit/EditMode tests.

---

### Task 1: Bounded candidate enumeration

**Files:**
- Modify: `Assets/Scripts/FlowPuzzle/Solving/BacktrackingFlowPuzzleSolver.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Solving/FlowSolveProgress.cs`
- Create: `Assets/Scripts/FlowPuzzle/Solving/FlowSolutionEnumerationResult.cs`
- Test: `Assets/Tests/EditMode/Solving/FlowSolutionEnumerationTests.cs`

- [x] Write tests proving multiple candidates, deterministic order, fixed-prefix compliance, and timeout-with-candidates reporting.
- [x] Run the focused tests and confirm RED.
- [x] Implement callback-based exhaustive search bounded by the existing timeout/node budget.
- [x] Run focused solver tests and confirm GREEN.

### Task 2: Candidate scoring and perfect provider

**Files:**
- Create: `Assets/Scripts/FlowPuzzle/Application/FlowPerfectCandidateScore.cs`
- Create: `Assets/Scripts/FlowPuzzle/Application/FlowPerfectCandidateScorer.cs`
- Create: `Assets/Scripts/FlowPuzzle/Application/FlowPerfectCompletionProvider.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionRequest.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionResult.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Solving/FlowCompletionProgress.cs`
- Test: `Assets/Tests/EditMode/Application/FlowPerfectCompletionTests.cs`

- [x] Write tests for ranking priority, deterministic tie-breaking, best-candidate selection, and best-so-far timeout behavior.
- [x] Run focused tests and confirm RED.
- [x] Implement scoring and application-layer orchestration.
- [x] Run focused tests and confirm GREEN.

### Task 3: Editor integration

**Files:**
- Modify: `Assets/Scripts/FlowPuzzle/Editor/UI/FlowDraftPanel.cs`
- Modify: `Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs`
- Modify: `Assets/Tests/EditMode/Editor/FlowCompletionUndoAndDropTests.cs`

- [x] Write tests for button placement/state, provider selection, result diagnostics, and one-command Undo/Redo.
- [x] Run focused tests and confirm RED.
- [x] Add `Perfect Complete` beside `Complete` and reuse the safe async completion lifecycle.
- [x] Run focused Editor tests and confirm GREEN.

### Task 4: Regression verification

- [x] Run all Solving, Application, and Editor tests.
- [x] Run the complete EditMode suite.
- [x] Confirm zero individual failures, zero compile errors, and no `Assets/Temp` artifacts.
- [x] Review the final diff without committing or pushing.
