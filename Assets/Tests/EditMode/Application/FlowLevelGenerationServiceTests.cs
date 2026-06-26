using FlowPuzzle.Core;
using FlowPuzzle.Application;
using FlowPuzzle.Generation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Application
{
    [TestFixture]
    public class FlowLevelGenerationServiceTests
    {
        private static FlowGenerationConfig MakeConfig(int seed = 42)
        {
            return new FlowGenerationConfig { width = 5, height = 5, colorCount = 2,
                minCoverageRatio = 0.2f, maxCoverageRatio = 0.8f,
                minPathLength = 2, maxPathLength = 5,
                maxPathAttempt = 100, maxLevelAttempt = 20,
                useRandomSeed = false, seed = seed };
        }

        [Test] public void GenerateOne_FixedSeed_Succeeds()
        {
            var r = new FlowLevelGenerationService().GenerateOne(1, MakeConfig());
            Assert.IsTrue(r.success); Assert.IsNotNull(r.generatedLevel);
        }

        [Test] public void GenerateOne_SameSeed_Reproduces()
        {
            var svc = new FlowLevelGenerationService();
            var r1 = svc.GenerateOne(1, MakeConfig(42)); var r2 = svc.GenerateOne(1, MakeConfig(42));
            Assert.IsTrue(r1.success && r2.success);
            Assert.AreEqual(r1.generatedLevel.coverageRatio, r2.generatedLevel.coverageRatio);
        }

        [Test] public void GenerateOne_ResultPassesValidation()
        {
            var svc = new FlowLevelGenerationService(); var r = svc.GenerateOne(1, MakeConfig());
            Assert.IsTrue(svc.Validate(r.generatedLevel).isValid);
        }

        [Test] public void GenerateBatch_ExactCount()
        {
            var r = new FlowLevelGenerationService().GenerateBatch(new FlowBatchRequest
                { startLevelId = 1, count = 3, baseSeed = 42, config = MakeConfig() });
            Assert.AreEqual(3, r.items.Count);
        }

        [Test] public void Validate_NullLevel_ReturnsNullLevel()
        {
            var v = new FlowLevelGenerationService().Validate(null);
            Assert.IsFalse(v.isValid); Assert.AreEqual("NullLevel", v.errorCode);
        }
    }
}
