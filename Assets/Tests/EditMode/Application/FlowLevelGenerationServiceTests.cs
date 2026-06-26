using FlowPuzzle.Core;
using FlowPuzzle.Application;
using FlowPuzzle.Generation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Application
{
    [TestFixture]
    public class FlowLevelGenerationServiceTests
    {
        private static FlowGenerationConfig MakeConfig(int w = 5, int h = 5, int colors = 2, int seed = 42)
        {
            return new FlowGenerationConfig
            {
                width = w, height = h, colorCount = colors,
                minCoverageRatio = 0.2f, maxCoverageRatio = 0.8f,
                minPathLength = 2, maxPathLength = 5,
                maxPathAttempt = 100, maxLevelAttempt = 20,
                useRandomSeed = false, seed = seed
            };
        }

        [Test]
        public void GenerateOne_FixedSeed_Succeeds()
        {
            var svc = new FlowLevelGenerationService();
            var result = svc.GenerateOne(1, MakeConfig());
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.generatedLevel);
        }

        [Test]
        public void GenerateOne_SameSeed_Reproduces()
        {
            var svc = new FlowLevelGenerationService();
            var r1 = svc.GenerateOne(1, MakeConfig(seed: 42));
            var r2 = svc.GenerateOne(1, MakeConfig(seed: 42));
            Assert.IsTrue(r1.success && r2.success);
            Assert.AreEqual(r1.generatedLevel.coverageRatio, r2.generatedLevel.coverageRatio);
            Assert.AreEqual(r1.generatedLevel.usedSeed, r2.generatedLevel.usedSeed);
        }

        [Test]
        public void GenerateOne_ResultPassesValidation()
        {
            var svc = new FlowLevelGenerationService();
            var result = svc.GenerateOne(1, MakeConfig());
            var v = svc.Validate(result.generatedLevel);
            Assert.IsTrue(v.isValid, v.errorMessage);
        }

        [Test]
        public void GenerateBatch_ExactCountAndSeeds()
        {
            var svc = new FlowLevelGenerationService();
            var req = new FlowBatchRequest
            {
                startLevelId = 1, count = 3, baseSeed = 42, config = MakeConfig()
            };
            var report = svc.GenerateBatch(req);
            Assert.AreEqual(3, report.items.Count);
            for (var i = 0; i < 3; i++)
                Assert.AreEqual(1 + i, report.items[i].levelId);
        }

        [Test]
        public void GenerateBatch_Repeated_Equal()
        {
            var svc = new FlowLevelGenerationService();
            var req = new FlowBatchRequest { startLevelId = 1, count = 2, baseSeed = 42, config = MakeConfig() };
            var r1 = svc.GenerateBatch(req);
            var r2 = svc.GenerateBatch(req);
            Assert.AreEqual(r1.successfulCount, r2.successfulCount);
            Assert.AreEqual(r1.items.Count, r2.items.Count);
        }

        [Test]
        public void Validate_NullLevel_ReturnsInvalid()
        {
            var svc = new FlowLevelGenerationService();
            var v = svc.Validate(null);
            Assert.IsFalse(v.isValid);
        }
    }
}
