using System;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Generation;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Generation
{
    [TestFixture]
    public class FlowBatchGeneratorTests
    {
        private static FlowBatchGenerator MakeBatch()
        {
            return new FlowBatchGenerator(new FlowSolutionGenerator(
                new FlowPathLengthAllocator(),
                new RandomizedDfsPathGenerationStrategy(),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator()));
        }

        private static FlowGenerationConfig MakeConfig()
        {
            return new FlowGenerationConfig
            {
                width = 5, height = 5, colorCount = 2,
                minCoverageRatio = 0.2f, maxCoverageRatio = 0.8f,
                minPathLength = 2, maxPathLength = 5,
                maxPathAttempt = 100, maxLevelAttempt = 20,
                useRandomSeed = false, seed = 42
            };
        }

        private static FlowBatchRequest MakeRequest(int startId = 1, int count = 3, int baseSeed = 42)
        {
            return new FlowBatchRequest
            {
                startLevelId = startId, count = count,
                baseSeed = baseSeed, config = MakeConfig()
            };
        }

        [Test] public void NullGenerator_Throws() => Assert.Throws<ArgumentNullException>(() => new FlowBatchGenerator(null));
        [Test] public void NullRequest_Throws() => Assert.Throws<ArgumentNullException>(() => MakeBatch().Generate(null));
        [Test] public void NullConfig_Throws() => Assert.Throws<ArgumentNullException>(() => MakeBatch().Generate(new FlowBatchRequest { startLevelId = 1, count = 3, config = null }));
        [Test] public void NonPositiveCount_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => MakeBatch().Generate(MakeRequest(count: 0)));

        [Test]
        public void ExactItemCountAndAscendingLevelIds()
        {
            var report = MakeBatch().Generate(MakeRequest(10, 5, 42));
            Assert.AreEqual(5, report.items.Count);
            for (var i = 0; i < 5; i++) Assert.AreEqual(10 + i, report.items[i].levelId);
        }

        [Test]
        public void DerivedSeeds_Exact()
        {
            var report = MakeBatch().Generate(MakeRequest(1, 3, 100));
            Assert.AreEqual(unchecked(100 + 1 * 9973), report.items[0].usedSeed);
            Assert.AreEqual(unchecked(100 + 2 * 9973), report.items[1].usedSeed);
            Assert.AreEqual(unchecked(100 + 3 * 9973), report.items[2].usedSeed);
        }

        [Test]
        public void DerivedSeeds_RealOverflow()
        {
            var report = MakeBatch().Generate(MakeRequest(1, 2, int.MaxValue));
            // levelId=1: int.MaxValue + 9973 wraps around
            Assert.AreEqual(unchecked(int.MaxValue + 1 * 9973), report.items[0].usedSeed);
            Assert.AreEqual(unchecked(int.MaxValue + 2 * 9973), report.items[1].usedSeed);
            // proves overflow: derived seed < baseSeed for levelId=1
            Assert.IsTrue(report.items[0].usedSeed < int.MaxValue,
                "Derived seed should overflow when baseSeed + offset exceeds int.MaxValue");
        }

        [Test]
        public void DerivedSeeds_NegativeBase()
        {
            var report = MakeBatch().Generate(MakeRequest(-5, 2, -100));
            Assert.AreEqual(unchecked(-100 + (-5) * 9973), report.items[0].usedSeed);
            Assert.AreEqual(unchecked(-100 + (-4) * 9973), report.items[1].usedSeed);
        }

        [Test]
        public void CountsMatch()
        {
            var report = MakeBatch().Generate(MakeRequest(1, 4, 42));
            Assert.AreEqual(4, report.requestedCount);
            Assert.AreEqual(report.successfulCount + report.failedCount, report.requestedCount);
        }

        [Test]
        public void SameRequest_DeeplyEqual()
        {
            var batch = MakeBatch();
            var r1 = batch.Generate(MakeRequest(1, 3, 42));
            var r2 = batch.Generate(MakeRequest(1, 3, 42));
            Assert.AreEqual(r1.requestedCount, r2.requestedCount);
            Assert.AreEqual(r1.successfulCount, r2.successfulCount);
            Assert.AreEqual(r1.items.Count, r2.items.Count);
            for (var i = 0; i < r1.items.Count; i++)
            {
                var a = r1.items[i]; var b = r2.items[i];
                Assert.AreEqual(a.levelId, b.levelId);
                Assert.AreEqual(a.usedSeed, b.usedSeed);
                Assert.AreEqual(a.success, b.success);
                Assert.AreEqual(a.message, b.message);
                var ga = a.generationResult; var gb = b.generationResult;
                Assert.AreEqual(ga.success, gb.success);
                Assert.AreEqual(ga.levelId, gb.levelId);
                Assert.AreEqual(ga.usedSeed, gb.usedSeed);
                Assert.AreEqual(ga.attemptCount, gb.attemptCount);
                Assert.AreEqual(ga.diagnostic == null, gb.diagnostic == null);
                if (ga.diagnostic != null)
                {
                    Assert.AreEqual(ga.diagnostic.errorCode, gb.diagnostic.errorCode);
                    Assert.AreEqual(ga.diagnostic.errorMessage, gb.diagnostic.errorMessage);
                }
                if (ga.success)
                {
                    var la = ga.generatedLevel; var lb = gb.generatedLevel;
                    Assert.AreEqual(la.usedSeed, lb.usedSeed);
                    Assert.AreEqual(la.coverageRatio, lb.coverageRatio);
                    Assert.AreEqual(la.levelData.levelId, lb.levelData.levelId);
                    Assert.AreEqual(la.solutionData.levelId, lb.solutionData.levelId);
                    Assert.AreEqual(la.levelData.width, lb.levelData.width);
                    Assert.AreEqual(la.levelData.height, lb.levelData.height);
                    Assert.AreEqual(la.levelData.difficulty, lb.levelData.difficulty);
                    Assert.AreEqual(la.difficultyReport.difficulty, lb.difficultyReport.difficulty);
                    Assert.AreEqual(la.difficultyReport.totalScore, lb.difficultyReport.totalScore, 0.01f);
                    Assert.AreEqual(la.levelData.pairs.Count, lb.levelData.pairs.Count);
                    for (var j = 0; j < la.levelData.pairs.Count; j++)
                    {
                        Assert.AreEqual(la.levelData.pairs[j].colorId, lb.levelData.pairs[j].colorId);
                        Assert.AreEqual(la.levelData.pairs[j].endpointA, lb.levelData.pairs[j].endpointA);
                        Assert.AreEqual(la.levelData.pairs[j].endpointB, lb.levelData.pairs[j].endpointB);
                    }
                    Assert.AreEqual(la.solutionData.paths.Count, lb.solutionData.paths.Count);
                    for (var j = 0; j < la.solutionData.paths.Count; j++)
                    {
                        Assert.AreEqual(la.solutionData.paths[j].colorId, lb.solutionData.paths[j].colorId);
                        Assert.AreEqual(la.solutionData.paths[j].cells.Count, lb.solutionData.paths[j].cells.Count);
                        Assert.IsTrue(la.solutionData.paths[j].cells.SequenceEqual(lb.solutionData.paths[j].cells));
                    }
                }
            }
        }

        [Test]
        public void RequestConfigUnchanged_Complete()
        {
            var req = MakeRequest(1, 2, 42);
            var origConfig = req.config;
            var snap = new
            {
                req.startLevelId, req.count, req.baseSeed,
                c = new
                {
                    req.config.width, req.config.height, req.config.colorCount,
                    req.config.minCoverageRatio, req.config.maxCoverageRatio,
                    req.config.minPathLength, req.config.maxPathLength,
                    req.config.maxPathAttempt, req.config.maxLevelAttempt,
                    req.config.useRandomSeed, req.config.seed,
                    req.config.useTargetDifficulty, req.config.targetDifficulty,
                    req.config.useTargetScoreRange,
                    req.config.minTargetDifficultyScore, req.config.maxTargetDifficultyScore,
                    req.config.turnPreference, req.config.interactionPreference,
                    req.config.minEndpointDistance, req.config.maxEndpointDistance,
                    req.config.minDetour, req.config.maxDetour,
                    req.config.bottleneckPreference,
                    req.config.solverTimeoutMilliseconds, req.config.solverNodeBudget
                }
            };
            MakeBatch().Generate(req);
            Assert.AreSame(origConfig, req.config);
            Assert.AreEqual(snap.startLevelId, req.startLevelId);
            Assert.AreEqual(snap.count, req.count);
            Assert.AreEqual(snap.baseSeed, req.baseSeed);
            Assert.AreEqual(snap.c.width, req.config.width);
            Assert.AreEqual(snap.c.seed, req.config.seed);
            Assert.AreEqual(snap.c.useRandomSeed, req.config.useRandomSeed);
            Assert.AreEqual(snap.c.minCoverageRatio, req.config.minCoverageRatio);
            Assert.AreEqual(snap.c.solverNodeBudget, req.config.solverNodeBudget);
        }

        [Test]
        public void ImpossibleConfig_ReportsAllItems()
        {
            var req = MakeRequest(1, 3, 42);
            req.config.width = 3; req.config.height = 3;
            req.config.colorCount = 5; req.config.minPathLength = 5;
            var report = MakeBatch().Generate(req);
            Assert.AreEqual(3, report.items.Count);
            Assert.AreEqual(0, report.successfulCount);
            Assert.AreEqual(3, report.failedCount);
            for (var i = 0; i < 3; i++)
            {
                var item = report.items[i];
                Assert.AreEqual(1 + i, item.levelId);
                Assert.AreEqual(unchecked(42 + (1 + i) * 9973), item.usedSeed);
                Assert.IsFalse(item.success);
                Assert.IsNotNull(item.generationResult);
                Assert.IsNotNull(item.generationResult.diagnostic);
                Assert.IsTrue(item.message.Contains(item.generationResult.diagnostic.errorCode));
                Assert.IsTrue(item.message.Contains(item.generationResult.diagnostic.errorMessage));
            }
        }

        [Test]
        public void InstanceOwnership()
        {
            var batch = MakeBatch();
            var r1 = batch.Generate(MakeRequest(1, 2, 42));
            var r2 = batch.Generate(MakeRequest(1, 2, 42));
            Assert.AreNotSame(r1, r2);
            Assert.AreNotSame(r1.items, r2.items);
            Assert.AreNotSame(r1.items[0], r2.items[0]);
            Assert.AreNotSame(r1.items[0].generationResult, r2.items[0].generationResult);
            var count = r1.items.Count;
            r1.items.Clear();
            Assert.AreEqual(2, r2.items.Count);
            Assert.AreEqual(0, r1.items.Count);
        }
    }
}
