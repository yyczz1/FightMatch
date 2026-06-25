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
                startLevelId = startId,
                count = count,
                baseSeed = baseSeed,
                config = MakeConfig()
            };
        }

        [Test]
        public void Generate_NullGenerator_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FlowBatchGenerator(null));
        }

        [Test]
        public void Generate_NullRequest_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => MakeBatch().Generate(null));
        }

        [Test]
        public void Generate_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => MakeBatch().Generate(
                new FlowBatchRequest { startLevelId = 1, count = 3, config = null }));
        }

        [Test]
        public void Generate_NonPositiveCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => MakeBatch().Generate(MakeRequest(count: 0)));
        }

        [Test]
        public void Generate_ExactItemCountAndAscendingLevelIds()
        {
            var report = MakeBatch().Generate(MakeRequest(10, 5, 42));

            Assert.AreEqual(5, report.requestedCount);
            Assert.AreEqual(5, report.items.Count);
            for (var i = 0; i < 5; i++)
                Assert.AreEqual(10 + i, report.items[i].levelId);
        }

        [Test]
        public void Generate_DerivedSeeds_Exact()
        {
            var report = MakeBatch().Generate(MakeRequest(1, 3, 100));

            Assert.AreEqual(unchecked(100 + 1 * 9973), report.items[0].usedSeed);
            Assert.AreEqual(unchecked(100 + 2 * 9973), report.items[1].usedSeed);
            Assert.AreEqual(unchecked(100 + 3 * 9973), report.items[2].usedSeed);
        }

        [Test]
        public void Generate_CountsMatch()
        {
            var report = MakeBatch().Generate(MakeRequest(1, 4, 42));

            Assert.AreEqual(4, report.requestedCount);
            Assert.AreEqual(report.successfulCount + report.failedCount, report.requestedCount);
            Assert.AreEqual(4, report.items.Count);
        }

        [Test]
        public void Generate_SuccessItem_SeedEqualsDerived()
        {
            var report = MakeBatch().Generate(MakeRequest(1, 3, 42));

            foreach (var item in report.items)
            {
                if (item.success)
                    Assert.AreEqual(item.usedSeed, item.generationResult.usedSeed);
            }
        }

        [Test]
        public void Generate_SameRequest_DeepllyEqual()
        {
            var batch = MakeBatch();
            var r1 = batch.Generate(MakeRequest(1, 3, 42));
            var r2 = batch.Generate(MakeRequest(1, 3, 42));

            Assert.AreEqual(r1.successfulCount, r2.successfulCount);
            Assert.AreEqual(r1.failedCount, r2.failedCount);
            for (var i = 0; i < r1.items.Count; i++)
            {
                Assert.AreEqual(r1.items[i].levelId, r2.items[i].levelId);
                Assert.AreEqual(r1.items[i].usedSeed, r2.items[i].usedSeed);
                Assert.AreEqual(r1.items[i].success, r2.items[i].success);
            }
        }

        [Test]
        public void Generate_RequestConfigUnchanged()
        {
            var req = MakeRequest(1, 2, 42);
            var origSeed = req.config.seed;
            var origRandom = req.config.useRandomSeed;

            MakeBatch().Generate(req);

            Assert.AreEqual(origSeed, req.config.seed);
            Assert.AreEqual(origRandom, req.config.useRandomSeed);
        }

        [Test]
        public void Generate_ImpossibleConfig_ReportsAllItems()
        {
            var req = MakeRequest(1, 3, 42);
            req.config.width = 3;
            req.config.height = 3;
            req.config.colorCount = 5;
            req.config.minPathLength = 5;

            var report = MakeBatch().Generate(req);

            Assert.AreEqual(3, report.items.Count);
            Assert.AreEqual(3, report.failedCount);
            foreach (var item in report.items)
            {
                Assert.IsFalse(item.success);
                Assert.IsNotNull(item.generationResult.diagnostic);
            }
        }
    }
}
