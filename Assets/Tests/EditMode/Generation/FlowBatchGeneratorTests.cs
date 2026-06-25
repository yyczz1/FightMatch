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
                new FlowPathLengthAllocator(), new RandomizedDfsPathGenerationStrategy(),
                new FlowSolutionValidator(), new FlowDifficultyEvaluator()));
        }

        private static FlowGenerationConfig MakeConfig() => new()
        {
            width = 5, height = 5, colorCount = 2,
            minCoverageRatio = 0.2f, maxCoverageRatio = 0.8f,
            minPathLength = 2, maxPathLength = 5,
            maxPathAttempt = 100, maxLevelAttempt = 20,
            useRandomSeed = false, seed = 42
        };

        private static FlowBatchRequest MakeRequest(int startId = 1, int count = 3, int baseSeed = 42) => new()
        { startLevelId = startId, count = count, baseSeed = baseSeed, config = MakeConfig() };

        [Test] public void NullGenerator_Throws() => Assert.Throws<ArgumentNullException>(() => new FlowBatchGenerator(null));
        [Test] public void NullRequest_Throws() => Assert.Throws<ArgumentNullException>(() => MakeBatch().Generate(null));
        [Test] public void NullConfig_Throws() => Assert.Throws<ArgumentNullException>(() => MakeBatch().Generate(new FlowBatchRequest { startLevelId = 1, count = 3, config = null }));
        [Test] public void NonPositiveCount_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => MakeBatch().Generate(MakeRequest(count: 0)));

        [Test] public void ExactItemCountAndAscendingLevelIds()
        {
            var r = MakeBatch().Generate(MakeRequest(10, 5, 42));
            Assert.AreEqual(5, r.items.Count);
            for (var i = 0; i < 5; i++) Assert.AreEqual(10 + i, r.items[i].levelId);
        }

        [Test] public void DerivedSeeds_Exact()
        {
            var r = MakeBatch().Generate(MakeRequest(1, 3, 100));
            Assert.AreEqual(unchecked(100 + 1 * 9973), r.items[0].usedSeed);
            Assert.AreEqual(unchecked(100 + 2 * 9973), r.items[1].usedSeed);
            Assert.AreEqual(unchecked(100 + 3 * 9973), r.items[2].usedSeed);
        }

        [Test] public void DerivedSeeds_RealOverflow()
        {
            var r = MakeBatch().Generate(MakeRequest(1, 2, int.MaxValue));
            Assert.AreEqual(unchecked(int.MaxValue + 1 * 9973), r.items[0].usedSeed);
            Assert.AreEqual(unchecked(int.MaxValue + 2 * 9973), r.items[1].usedSeed);
            Assert.IsTrue(r.items[0].usedSeed < int.MaxValue);
        }

        [Test] public void DerivedSeeds_NegativeBase()
        {
            var r = MakeBatch().Generate(MakeRequest(-5, 2, -100));
            Assert.AreEqual(unchecked(-100 + (-5) * 9973), r.items[0].usedSeed);
            Assert.AreEqual(unchecked(-100 + (-4) * 9973), r.items[1].usedSeed);
        }

        [Test] public void CountsMatch()
        {
            var r = MakeBatch().Generate(MakeRequest(1, 4, 42));
            Assert.AreEqual(4, r.requestedCount);
            Assert.AreEqual(r.successfulCount + r.failedCount, r.requestedCount);
        }

        [Test] public void SameRequest_DeeplyEqual()
        {
            var batch = MakeBatch();
            var r1 = batch.Generate(MakeRequest(1, 3, 42));
            var r2 = batch.Generate(MakeRequest(1, 3, 42));
            Assert.AreEqual(r1.requestedCount, r2.requestedCount);
            Assert.AreEqual(r1.successfulCount, r2.successfulCount);
            Assert.AreEqual(r1.failedCount, r2.failedCount);
            Assert.AreEqual(r1.items.Count, r2.items.Count);
            for (var i = 0; i < r1.items.Count; i++)
            {
                var a = r1.items[i]; var b = r2.items[i];
                Assert.AreEqual(a.levelId, b.levelId); Assert.AreEqual(a.usedSeed, b.usedSeed);
                Assert.AreEqual(a.success, b.success); Assert.AreEqual(a.message, b.message);
                var ga = a.generationResult; var gb = b.generationResult;
                Assert.AreEqual(ga.success, gb.success); Assert.AreEqual(ga.levelId, gb.levelId);
                Assert.AreEqual(ga.usedSeed, gb.usedSeed); Assert.AreEqual(ga.attemptCount, gb.attemptCount);
                Assert.AreEqual(ga.diagnostic == null, gb.diagnostic == null);
                if (ga.diagnostic != null) { Assert.AreEqual(ga.diagnostic.errorCode, gb.diagnostic.errorCode); Assert.AreEqual(ga.diagnostic.errorMessage, gb.diagnostic.errorMessage); }
                if (ga.success)
                {
                    var la = ga.generatedLevel; var lb = gb.generatedLevel;
                    Assert.AreEqual(la.usedSeed, lb.usedSeed); Assert.AreEqual(la.coverageRatio, lb.coverageRatio);
                    Assert.AreEqual(la.levelData.levelId, lb.levelData.levelId); Assert.AreEqual(la.solutionData.levelId, lb.solutionData.levelId);
                    Assert.AreEqual(la.levelData.width, lb.levelData.width); Assert.AreEqual(la.levelData.height, lb.levelData.height);
                    Assert.AreEqual(la.levelData.difficulty, lb.levelData.difficulty);
                    Assert.AreEqual(la.levelData.difficultyScore, lb.levelData.difficultyScore, 0.01f);
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

        [Test] public void RequestConfigUnchanged_Complete()
        {
            var req = MakeRequest(1, 2, 42);
            var orig = req.config;
            var s = (start: req.startLevelId, count: req.count, baseSeed: req.baseSeed,
                w: req.config.width, h: req.config.height, cc: req.config.colorCount,
                minC: req.config.minCoverageRatio, maxC: req.config.maxCoverageRatio,
                minL: req.config.minPathLength, maxL: req.config.maxPathLength,
                mpa: req.config.maxPathAttempt, mla: req.config.maxLevelAttempt,
                urs: req.config.useRandomSeed, sd: req.config.seed,
                utd: req.config.useTargetDifficulty, td: req.config.targetDifficulty,
                utsr: req.config.useTargetScoreRange, minS: req.config.minTargetDifficultyScore,
                maxS: req.config.maxTargetDifficultyScore, tp: req.config.turnPreference,
                ip: req.config.interactionPreference, minEd: req.config.minEndpointDistance,
                maxEd: req.config.maxEndpointDistance, minDt: req.config.minDetour,
                maxDt: req.config.maxDetour, bp: req.config.bottleneckPreference,
                stms: req.config.solverTimeoutMilliseconds, snb: req.config.solverNodeBudget);
            MakeBatch().Generate(req);
            Assert.AreSame(orig, req.config);
            Assert.AreEqual(s.start, req.startLevelId); Assert.AreEqual(s.count, req.count); Assert.AreEqual(s.baseSeed, req.baseSeed);
            Assert.AreEqual(s.w, req.config.width); Assert.AreEqual(s.h, req.config.height); Assert.AreEqual(s.cc, req.config.colorCount);
            Assert.AreEqual(s.minC, req.config.minCoverageRatio); Assert.AreEqual(s.maxC, req.config.maxCoverageRatio);
            Assert.AreEqual(s.minL, req.config.minPathLength); Assert.AreEqual(s.maxL, req.config.maxPathLength);
            Assert.AreEqual(s.mpa, req.config.maxPathAttempt); Assert.AreEqual(s.mla, req.config.maxLevelAttempt);
            Assert.AreEqual(s.urs, req.config.useRandomSeed); Assert.AreEqual(s.sd, req.config.seed);
            Assert.AreEqual(s.utd, req.config.useTargetDifficulty); Assert.AreEqual(s.td, req.config.targetDifficulty);
            Assert.AreEqual(s.utsr, req.config.useTargetScoreRange);
            Assert.AreEqual(s.minS, req.config.minTargetDifficultyScore); Assert.AreEqual(s.maxS, req.config.maxTargetDifficultyScore);
            Assert.AreEqual(s.tp, req.config.turnPreference); Assert.AreEqual(s.ip, req.config.interactionPreference);
            Assert.AreEqual(s.minEd, req.config.minEndpointDistance); Assert.AreEqual(s.maxEd, req.config.maxEndpointDistance);
            Assert.AreEqual(s.minDt, req.config.minDetour); Assert.AreEqual(s.maxDt, req.config.maxDetour);
            Assert.AreEqual(s.bp, req.config.bottleneckPreference);
            Assert.AreEqual(s.stms, req.config.solverTimeoutMilliseconds); Assert.AreEqual(s.snb, req.config.solverNodeBudget);
        }

        [Test] public void ImpossibleConfig_ReportsAllItems()
        {
            var req = MakeRequest(1, 3, 42); req.config.width = 3; req.config.height = 3; req.config.colorCount = 5; req.config.minPathLength = 5;
            var r = MakeBatch().Generate(req);
            Assert.AreEqual(3, r.items.Count); Assert.AreEqual(0, r.successfulCount); Assert.AreEqual(3, r.failedCount);
            for (var i = 0; i < 3; i++)
            {
                var it = r.items[i]; Assert.AreEqual(1 + i, it.levelId);
                Assert.AreEqual(unchecked(42 + (1 + i) * 9973), it.usedSeed);
                Assert.IsFalse(it.success); Assert.IsNotNull(it.generationResult); Assert.IsNotNull(it.generationResult.diagnostic);
                Assert.IsTrue(it.message.Contains(it.generationResult.diagnostic.errorCode));
                Assert.IsTrue(it.message.Contains(it.generationResult.diagnostic.errorMessage));
            }
        }

        [Test] public void InstanceOwnership()
        {
            var batch = MakeBatch(); var r1 = batch.Generate(MakeRequest(1, 2, 42)); var r2 = batch.Generate(MakeRequest(1, 2, 42));
            Assert.AreNotSame(r1, r2); Assert.AreNotSame(r1.items, r2.items);
            Assert.AreNotSame(r1.items[0], r2.items[0]); Assert.AreNotSame(r1.items[0].generationResult, r2.items[0].generationResult);
            var c = r1.items.Count; r1.items.Clear();
            Assert.AreEqual(2, r2.items.Count); Assert.AreEqual(0, r1.items.Count);
        }
    }
}
