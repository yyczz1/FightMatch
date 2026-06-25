using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Core
{
    [TestFixture]
    public class FlowCoreDataContractsTests
    {
        [Test]
        public void FlowPairData_IsSerializable()
        {
            var type = typeof(FlowPairData);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowPairData should be [Serializable]");
        }

        [Test]
        public void FlowPathData_IsSerializable()
        {
            var type = typeof(FlowPathData);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowPathData should be [Serializable]");
        }

        [Test]
        public void FlowLevelData_IsSerializable()
        {
            var type = typeof(FlowLevelData);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowLevelData should be [Serializable]");
        }

        [Test]
        public void FlowSolutionData_IsSerializable()
        {
            var type = typeof(FlowSolutionData);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowSolutionData should be [Serializable]");
        }

        [Test]
        public void FlowGenerationConfig_IsSerializable()
        {
            var type = typeof(FlowGenerationConfig);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowGenerationConfig should be [Serializable]");
        }

        [Test]
        public void FlowDifficultyReport_IsSerializable()
        {
            var type = typeof(FlowDifficultyReport);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowDifficultyReport should be [Serializable]");
        }

        [Test]
        public void FlowGeneratedLevel_IsSerializable()
        {
            var type = typeof(FlowGeneratedLevel);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowGeneratedLevel should be [Serializable]");
        }

        [Test]
        public void FlowFailureDiagnostic_IsSerializable()
        {
            var type = typeof(FlowFailureDiagnostic);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowFailureDiagnostic should be [Serializable]");
        }

        [Test]
        public void FlowGenerationResult_IsSerializable()
        {
            var type = typeof(FlowGenerationResult);
            Assert.IsTrue(Attribute.IsDefined(type, typeof(SerializableAttribute)),
                "FlowGenerationResult should be [Serializable]");
        }

        [Test]
        public void FlowDifficultyTier_HasCorrectValues()
        {
            var values = Enum.GetValues(typeof(FlowDifficultyTier))
                .Cast<FlowDifficultyTier>()
                .ToArray();

            Assert.AreEqual(4, values.Length);
            Assert.AreEqual(FlowDifficultyTier.Easy, values[0]);
            Assert.AreEqual(FlowDifficultyTier.Normal, values[1]);
            Assert.AreEqual(FlowDifficultyTier.Hard, values[2]);
            Assert.AreEqual(FlowDifficultyTier.Expert, values[3]);
        }

        [Test]
        public void FlowPathData_Cells_InitializedNonShared()
        {
            var a = new FlowPathData();
            var b = new FlowPathData();

            Assert.IsNotNull(a.cells);
            Assert.IsNotNull(b.cells);
            Assert.AreNotSame(a.cells, b.cells);
        }

        [Test]
        public void FlowLevelData_Pairs_InitializedNonShared()
        {
            var a = new FlowLevelData();
            var b = new FlowLevelData();

            Assert.IsNotNull(a.pairs);
            Assert.IsNotNull(b.pairs);
            Assert.AreNotSame(a.pairs, b.pairs);
        }

        [Test]
        public void FlowSolutionData_Paths_InitializedNonShared()
        {
            var a = new FlowSolutionData();
            var b = new FlowSolutionData();

            Assert.IsNotNull(a.paths);
            Assert.IsNotNull(b.paths);
            Assert.AreNotSame(a.paths, b.paths);
        }

        [Test]
        public void FlowGeneratedLevel_InitializesNestedObjects()
        {
            var level = new FlowGeneratedLevel();

            Assert.IsNotNull(level.levelData);
            Assert.IsNotNull(level.solutionData);
            Assert.IsNotNull(level.difficultyReport);
        }

        [Test]
        public void FlowGeneratedLevel_NestedObjectsNotShared()
        {
            var a = new FlowGeneratedLevel();
            var b = new FlowGeneratedLevel();

            Assert.AreNotSame(a.levelData, b.levelData);
            Assert.AreNotSame(a.solutionData, b.solutionData);
            Assert.AreNotSame(a.difficultyReport, b.difficultyReport);
        }

        [Test]
        public void FlowGenerationResult_Success_PreservesValues()
        {
            var generated = new FlowGeneratedLevel
            {
                usedSeed = 42,
                coverageRatio = 0.75f
            };

            var result = FlowGenerationResult.Success(1001, 42, 3, generated);

            Assert.IsTrue(result.success);
            Assert.AreEqual(1001, result.levelId);
            Assert.AreEqual(42, result.usedSeed);
            Assert.AreEqual(3, result.attemptCount);
            Assert.AreSame(generated, result.generatedLevel);
            Assert.IsNull(result.diagnostic);
        }

        [Test]
        public void FlowGenerationResult_Failure_PreservesDiagnostic()
        {
            var result = FlowGenerationResult.Failure(1001, 42, 5,
                "ImpossibleCoverage", "Coverage too high for board size");

            Assert.IsFalse(result.success);
            Assert.AreEqual(1001, result.levelId);
            Assert.AreEqual(42, result.usedSeed);
            Assert.AreEqual(5, result.attemptCount);
            Assert.IsNull(result.generatedLevel);
            Assert.IsNotNull(result.diagnostic);
            Assert.AreEqual("ImpossibleCoverage", result.diagnostic.errorCode);
            Assert.AreEqual("Coverage too high for board size", result.diagnostic.errorMessage);
            Assert.AreEqual(42, result.diagnostic.usedSeed);
            Assert.AreEqual(5, result.diagnostic.attemptCount);
        }
    }
}
