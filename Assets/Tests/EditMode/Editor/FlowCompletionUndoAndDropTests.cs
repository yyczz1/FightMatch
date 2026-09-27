using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FlowPuzzle.Application;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor;
using FlowPuzzle.Editor.Commands;
using FlowPuzzle.Editor.Draft;
using FlowPuzzle.Editor.UI;
using FlowPuzzle.Persistence;
using FlowPuzzle.Solving;
using FlowPuzzle.Validation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace FlowPuzzle.Tests.Editor
{
    [TestFixture]
    public sealed class FlowCompletionUndoAndDropTests
    {
        private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        [Test]
        public void ApplyCompletionResult_Solved_IsOneUndoableCommand()
        {
            var window = MakeWindow();
            var draft = MakeCompleteDraft(4101);
            draft.coverage = 0.36f;
            draft.ApplyConstraint(0, new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0) });
            SetField(window, "currentDraft", draft);

            Invoke(window, "ApplyCompletionResult", MakeSolvedResult(4101));

            var history = GetField<FlowEditorCommandHistory>(window, "commandHistory");
            Assert.IsTrue(history.CanUndo, "A successful Complete must create one undo entry.");
            Assert.AreEqual(0.84f, draft.coverage, 0.001f);
            Assert.AreEqual(6, draft.currentSolution.paths[0].cells.Count);

            Invoke(window, "DoUndo");

            Assert.AreEqual(0.36f, draft.coverage, 0.001f, "Undo restores the exact pre-Complete coverage.");
            Assert.AreEqual(4, draft.currentSolution.paths[0].cells.Count, "Undo restores the previous recommendation.");
            Assert.IsTrue(draft.isSolutionDirty);
            Assert.IsFalse(draft.isValidated);
            Assert.AreEqual(2, draft.fixedConstraints[0].cells.Count);
            Assert.IsFalse(history.CanUndo, "Complete must add exactly one history entry.");
            Assert.IsTrue(history.CanRedo);
            Assert.IsFalse(GetField<FlowDiagnosticsPanel>(window, "diagnosticsPanel").helpBox.visible,
                "Undo must not leave the stale Completed message visible.");

            Invoke(window, "DoRedo");

            Assert.AreEqual(0.84f, draft.coverage, 0.001f);
            Assert.AreEqual(6, draft.currentSolution.paths[0].cells.Count);
            Assert.IsFalse(draft.isSolutionDirty);
            Assert.IsTrue(draft.isValidated);
            Assert.IsTrue(history.CanUndo);
            Object.DestroyImmediate(window);
        }

        [Test]
        public void DoCompleteDraftAsync_AlreadyValidDraft_DoesNotReplaceRecommendation()
        {
            var window = MakeWindow();
            var draft = MakeCompleteDraft(4201);
            draft.coverage = 0.36f;
            SetField(window, "currentDraft", draft);
            SetField(window, "completionService", new FlowLevelCompletionService(
                new ThrowingCompletionProvider(),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator()));

            var task = (Task<FlowCompletionResult>)Invoke(window, "DoCompleteDraftAsync");
            var result = task.GetAwaiter().GetResult();

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.AreEqual(4, draft.currentSolution.paths[0].cells.Count);
            Assert.AreEqual(0.36f, draft.coverage, 0.001f);
            Assert.IsFalse(GetField<FlowEditorCommandHistory>(window, "commandHistory").CanUndo,
                "A no-op Complete must not create history.");
            Assert.That(GetField<FlowDiagnosticsPanel>(window, "diagnosticsPanel").helpBox.text,
                Does.Contain("already complete"));
            Object.DestroyImmediate(window);
        }

        [Test]
        public void DraftPanel_PerfectCompleteAppearsBeforeSave()
        {
            var window = MakeWindow();
            var panel = GetField<FlowDraftPanel>(window, "draftPanel");
            var parent = panel.saveBtn.parent;

            Assert.IsNotNull(panel.perfectCompleteBtn);
            Assert.IsNotNull(panel.stopSolvingBtn);
            Assert.Less(parent.IndexOf(panel.completeBtn), parent.IndexOf(panel.perfectCompleteBtn));
            Assert.Less(parent.IndexOf(panel.perfectCompleteBtn), parent.IndexOf(panel.stopSolvingBtn));
            Assert.Less(parent.IndexOf(panel.stopSolvingBtn), parent.IndexOf(panel.saveBtn));
            Assert.That(panel.completeBtn.tooltip, Does.Contain("first legal"));
            Assert.That(panel.perfectCompleteBtn.tooltip, Does.Contain("current parameters"));
            Assert.IsFalse(panel.stopSolvingBtn.enabledSelf);
            Object.DestroyImmediate(window);
        }

        [Test]
        public void SolverTimeoutField_ExplainsMillisecondsAndBestSoFarBehavior()
        {
            var window = MakeWindow();
            var field = GetField<FlowParameterPanel>(window, "paramPanel").solverTimeoutField;

            Assert.AreEqual("Solver Timeout (ms)", field.label);
            Assert.That(field.tooltip, Does.Contain("10000 = 10 seconds"));
            Assert.That(field.tooltip, Does.Contain("best candidate found so far"));
            Object.DestroyImmediate(window);
        }

        [UnityTest]
        public IEnumerator PerfectComplete_AlreadyValidDraftRunsAndIsOneUndoableCommand()
        {
            var window = MakeWindow();
            var draft = MakeCompleteDraft(4251);
            draft.coverage = 0.36f;
            SetField(window, "currentDraft", draft);
            GetField<FlowParameterPanel>(window, "paramPanel").minPathLenField.value = 5;
            var provider = new RecordingCompletionProvider(MakeSolvedResult(4251));
            SetField(window, "perfectCompletionService", new FlowLevelCompletionService(
                provider,
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator()));

            var task = (Task<FlowCompletionResult>)Invoke(window, "DoPerfectCompleteDraftAsync");
            while (!task.IsCompleted)
                yield return null;
            var result = task.Result;

            Assert.AreEqual(FlowSolveStatus.Solved, result.status);
            Assert.AreEqual(1, provider.CallCount, "Perfect Complete must optimize even when the Draft already has a valid recommendation.");
            Assert.IsNotNull(provider.LastRequest.qualityConfig);
            Assert.AreEqual(5, provider.LastRequest.qualityConfig.minPathLength);
            Assert.AreEqual(6, draft.currentSolution.paths[0].cells.Count);
            var history = GetField<FlowEditorCommandHistory>(window, "commandHistory");
            Assert.IsTrue(history.CanUndo);

            Invoke(window, "DoUndo");

            Assert.AreEqual(4, draft.currentSolution.paths[0].cells.Count);
            Assert.IsFalse(history.CanUndo, "Perfect Complete must add exactly one history entry.");
            Object.DestroyImmediate(window);
        }

        [Test]
        public void ApplyPerfectCompletionResult_SearchLimitReportsBestSoFarDetails()
        {
            var window = MakeWindow();
            SetField(window, "currentDraft", MakeCompleteDraft(4271));
            var result = MakeSolvedResult(4271);
            result.candidateCount = 7;
            result.candidateScore = 912.5f;
            result.candidateTotalPathCells = 9;
            result.candidateTotalDetour = 3;
            result.candidateTotalTurns = 4;
            result.searchLimitReached = true;

            Invoke(window, "ApplyPerfectCompletionResult", result);

            var message = GetField<FlowDiagnosticsPanel>(window, "diagnosticsPanel").helpBox.text;
            Assert.That(message, Does.Contain("best of 7 candidate"));
            Assert.That(message, Does.Contain("912.5"));
            Assert.That(message, Does.Contain("path cells 9"));
            Assert.That(message, Does.Contain("detour 3"));
            Assert.That(message, Does.Contain("turns 4"));
            Assert.That(message, Does.Contain("before the solver limit"));
            Object.DestroyImmediate(window);
        }

        [UnityTest]
        public IEnumerator StopSolvingButton_CancelsWithoutChangingDraftOrHistory()
        {
            var window = MakeWindow();
            var draft = MakeCompleteDraft(4281);
            var originalCells = new List<FlowPos>(draft.currentSolution.paths[0].cells);
            var originalCoverage = draft.coverage;
            SetField(window, "currentDraft", draft);
            var provider = new CancellableCompletionProvider();
            SetField(window, "perfectCompletionService", new FlowLevelCompletionService(
                provider,
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator()));

            var task = (Task<FlowCompletionResult>)Invoke(window, "DoPerfectCompleteDraftAsync");
            Assert.IsTrue(provider.Started);
            var panel = GetField<FlowDraftPanel>(window, "draftPanel");
            Assert.IsTrue(panel.stopSolvingBtn.enabledSelf);
            Assert.IsFalse(panel.completeBtn.enabledSelf);
            Assert.IsFalse(panel.perfectCompleteBtn.enabledSelf);

            var clickInvoke = typeof(Clickable).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(clickInvoke);
            clickInvoke.Invoke(panel.stopSolvingBtn.clickable, new object[] { null });
            Assert.AreEqual("Stopping...", panel.stopSolvingBtn.text);
            Assert.IsFalse(panel.stopSolvingBtn.enabledSelf);

            while (!task.IsCompleted)
                yield return null;

            Assert.AreEqual(FlowSolveStatus.Cancelled, task.Result.status);
            CollectionAssert.AreEqual(originalCells, draft.currentSolution.paths[0].cells);
            Assert.AreEqual(originalCoverage, draft.coverage, 0.001f);
            Assert.IsFalse(GetField<FlowEditorCommandHistory>(window, "commandHistory").CanUndo);
            Assert.AreEqual("Stop Solving", panel.stopSolvingBtn.text);
            Assert.IsFalse(panel.stopSolvingBtn.enabledSelf);
            Assert.That(GetField<FlowDiagnosticsPanel>(window, "diagnosticsPanel").helpBox.text,
                Does.Contain("stopped"));
            Object.DestroyImmediate(window);
        }

        [Test]
        public void LoadDroppedAsset_UsesNormalLoadWorkflow()
        {
            var asset = MakeAsset(4301);
            var window = MakeWindow();
            var oldDraft = MakeCompleteDraft(4001);
            SetField(window, "currentDraft", oldDraft);
            var history = GetField<FlowEditorCommandHistory>(window, "commandHistory");
            history.Execute(new FlowSnapshotCommand(oldDraft, oldDraft.Clone(), oldDraft.Clone(), "Old edit"));

            var loaded = (bool)Invoke(window, "DoLoadDroppedAssets", (object)new Object[] { asset });

            var draft = GetField<FlowLevelDraft>(window, "currentDraft");
            Assert.IsTrue(loaded);
            Assert.AreSame(asset, GetField<FlowLevelAsset>(window, "loadedAsset"));
            Assert.AreSame(asset, GetField<FlowDraftPanel>(window, "draftPanel").assetField.value);
            Assert.AreEqual(4301, draft.levelId);
            Assert.AreEqual(77, draft.seed);
            Assert.IsFalse(history.CanUndo, "Dropping an Asset starts a new editing session like Load Asset.");
            draft.pairs[0].endpointA = new FlowPos(2, 2);
            Assert.AreEqual(new FlowPos(0, 0), asset.levelData.pairs[0].endpointA,
                "Dropped Asset data must be deep-copied.");
            Object.DestroyImmediate(window);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void LoadDroppedAsset_RejectsInvalidOrMultipleObjectsWithoutMutation()
        {
            var window = MakeWindow();
            var draft = MakeCompleteDraft(4401);
            SetField(window, "currentDraft", draft);
            var first = MakeAsset(4402);
            var second = MakeAsset(4403);

            Assert.IsFalse((bool)Invoke(window, "DoLoadDroppedAssets", (object)new Object[0]));
            Assert.IsFalse((bool)Invoke(window, "DoLoadDroppedAssets", (object)new Object[] { first, second }));
            Assert.AreSame(draft, GetField<FlowLevelDraft>(window, "currentDraft"));
            Assert.IsNull(GetField<FlowLevelAsset>(window, "loadedAsset"));
            Assert.That(GetField<FlowDiagnosticsPanel>(window, "diagnosticsPanel").helpBox.text,
                Does.Contain("exactly one Flow Level Asset"));
            Object.DestroyImmediate(window);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void BoardDragPerformHandler_LoadsFlowLevelAsset()
        {
            var asset = MakeAsset(4501);
            var window = MakeWindow();
            var boardContainer = GetField<VisualElement>(window, "boardContainer");
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new Object[] { asset };

            using (var evt = DragPerformEvent.GetPooled())
            {
                evt.target = boardContainer;
                Invoke(window, "OnBoardDragPerform", evt);
            }

            Assert.AreSame(asset, GetField<FlowLevelAsset>(window, "loadedAsset"));
            Assert.AreEqual(4501, GetField<FlowLevelDraft>(window, "currentDraft").levelId);
            DragAndDrop.PrepareStartDrag();
            Object.DestroyImmediate(window);
            Object.DestroyImmediate(asset);
        }

        [UnityTest]
        public IEnumerator CompleteDraft_DraftChangedWhileSolving_DiscardsStaleResult()
        {
            var provider = new ControlledCompletionProvider();
            var window = MakeWindow();
            var draft = MakeCompleteDraft(4601);
            draft.coverage = 0.36f;
            draft.ApplyConstraint(0, new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0) });
            SetField(window, "currentDraft", draft);
            SetField(window, "completionService", new FlowLevelCompletionService(
                provider,
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator()));

            var task = (Task<FlowCompletionResult>)Invoke(window, "DoCompleteDraftAsync");
            Assert.IsFalse(task.IsCompleted);
            draft.PlaceEndpoint(0, false, new FlowPos(4, 0));
            provider.Complete(MakeSolvedResult(4601));
            while (!task.IsCompleted)
                yield return null;

            Assert.AreEqual(FlowSolveStatus.Cancelled, task.Result.status);
            Assert.AreEqual(new FlowPos(4, 0), draft.pairs[0].endpointB.Value);
            Assert.AreEqual(0.36f, draft.coverage, 0.001f);
            Assert.AreEqual(4, draft.currentSolution.paths[0].cells.Count,
                "The stale completion result must not replace the recommendation.");
            Assert.IsFalse(GetField<FlowEditorCommandHistory>(window, "commandHistory").CanUndo);
            Assert.That(GetField<FlowDiagnosticsPanel>(window, "diagnosticsPanel").helpBox.text,
                Does.Contain("changed while completion was running"));
            Object.DestroyImmediate(window);
        }

        private static FlowLevelGeneratorWindow MakeWindow()
        {
            var window = ScriptableObject.CreateInstance<FlowLevelGeneratorWindow>();
            Invoke(window, "CreateGUI");
            return window;
        }

        private static FlowLevelDraft MakeCompleteDraft(int levelId)
        {
            var draft = new FlowLevelDraft
            {
                levelId = levelId,
                width = 5,
                height = 5,
                colorCount = 1,
                seed = 42,
                coverage = 0.16f,
                isSolutionDirty = false,
                isValidated = true,
                currentSolution = new FlowSolutionData { levelId = levelId },
                currentDifficulty = new FlowDifficultyReport
                {
                    difficulty = FlowDifficultyTier.Easy,
                    totalScore = 48.9f
                }
            };
            draft.pairs.Add(new FlowDraftPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(3, 0)
            });
            draft.currentSolution.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(1, 0),
                    new FlowPos(2, 0),
                    new FlowPos(3, 0)
                }
            });
            return draft;
        }

        private static FlowCompletionResult MakeSolvedResult(int levelId)
        {
            var level = new FlowGeneratedLevel
            {
                levelData = new FlowLevelData { levelId = levelId, width = 5, height = 5 },
                solutionData = new FlowSolutionData { levelId = levelId },
                difficultyReport = new FlowDifficultyReport
                {
                    difficulty = FlowDifficultyTier.Hard,
                    totalScore = 121.5f
                },
                coverageRatio = 0.84f,
                usedSeed = 42
            };
            level.levelData.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(3, 0)
            });
            level.solutionData.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(0, 1),
                    new FlowPos(1, 1),
                    new FlowPos(2, 1),
                    new FlowPos(3, 1),
                    new FlowPos(3, 0)
                }
            });
            return new FlowCompletionResult
            {
                status = FlowSolveStatus.Solved,
                generatedLevel = level,
                visitedNodes = 58
            };
        }

        private static FlowLevelAsset MakeAsset(int levelId)
        {
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            asset.levelData = new FlowLevelData { levelId = levelId, width = 4, height = 4 };
            asset.levelData.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(3, 0)
            });
            asset.solutionData = new FlowSolutionData { levelId = levelId };
            asset.solutionData.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(1, 0),
                    new FlowPos(2, 0),
                    new FlowPos(3, 0)
                }
            });
            asset.difficultyReport = new FlowDifficultyReport
            {
                difficulty = FlowDifficultyTier.Easy,
                totalScore = 50f
            };
            asset.generationSeed = 77;
            asset.coverageRatio = 0.25f;
            return asset;
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, InstanceMembers);
            Assert.IsNotNull(method, $"Missing method {methodName}.");
            return method.Invoke(target, args);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, InstanceMembers);
            Assert.IsNotNull(field, $"Missing field {fieldName}.");
            return (T)field.GetValue(target);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, InstanceMembers);
            Assert.IsNotNull(field, $"Missing field {fieldName}.");
            field.SetValue(target, value);
        }

        private sealed class ThrowingCompletionProvider : IFlowLevelCompletionProvider
        {
            public string DisplayName => "Must not run";

            public Task<FlowCompletionResult> CompleteAsync(
                FlowCompletionRequest request,
                System.IProgress<FlowCompletionProgress> progress,
                CancellationToken cancellationToken)
            {
                throw new System.InvalidOperationException("The provider must not run for an already complete Draft.");
            }
        }

        private sealed class ControlledCompletionProvider : IFlowLevelCompletionProvider
        {
            private readonly TaskCompletionSource<FlowCompletionResult> source =
                new TaskCompletionSource<FlowCompletionResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            public string DisplayName => "Controlled";

            public Task<FlowCompletionResult> CompleteAsync(
                FlowCompletionRequest request,
                System.IProgress<FlowCompletionProgress> progress,
                CancellationToken cancellationToken)
            {
                return source.Task;
            }

            public void Complete(FlowCompletionResult result)
            {
                source.SetResult(result);
            }
        }

        private sealed class RecordingCompletionProvider : IFlowLevelCompletionProvider
        {
            private readonly FlowCompletionResult result;

            public RecordingCompletionProvider(FlowCompletionResult result)
            {
                this.result = result;
            }

            public string DisplayName => "Recording";
            public int CallCount { get; private set; }
            public FlowCompletionRequest LastRequest { get; private set; }

            public Task<FlowCompletionResult> CompleteAsync(
                FlowCompletionRequest request,
                System.IProgress<FlowCompletionProgress> progress,
                CancellationToken cancellationToken)
            {
                CallCount++;
                LastRequest = request;
                return Task.FromResult(result);
            }
        }

        private sealed class CancellableCompletionProvider : IFlowLevelCompletionProvider
        {
            private readonly TaskCompletionSource<FlowCompletionResult> source =
                new TaskCompletionSource<FlowCompletionResult>();

            public string DisplayName => "Cancellable";
            public bool Started { get; private set; }

            public Task<FlowCompletionResult> CompleteAsync(
                FlowCompletionRequest request,
                System.IProgress<FlowCompletionProgress> progress,
                CancellationToken cancellationToken)
            {
                Started = true;
                cancellationToken.Register(() => source.TrySetResult(new FlowCompletionResult
                {
                    status = FlowSolveStatus.Cancelled,
                    errorCode = "Cancelled",
                    errorMessage = "Stopped by the designer."
                }));
                return source.Task;
            }
        }
    }
}
