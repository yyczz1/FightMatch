using System;
using System.Collections.Generic;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Generation;
using FlowPuzzle.Validation;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Generation
{
    [TestFixture]
    public class FlowSolutionGeneratorTests
    {
        private static FlowSolutionGenerator MakeGenerator()
        {
            return new FlowSolutionGenerator(
                new FlowPathLengthAllocator(),
                new RandomizedDfsPathGenerationStrategy(),
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());
        }

        private static FlowGenerationConfig MakeConfig(int w, int h, int colors,
            float minCov = 0.2f, float maxCov = 0.9f,
            int minLen = 2, int maxLen = 6, int seed = 42,
            int maxPath = 100, int maxLevel = 20)
        {
            return new FlowGenerationConfig
            {
                width = w, height = h, colorCount = colors,
                minCoverageRatio = minCov, maxCoverageRatio = maxCov,
                minPathLength = minLen, maxPathLength = maxLen,
                useRandomSeed = false, seed = seed,
                maxPathAttempt = maxPath, maxLevelAttempt = maxLevel,
                useTargetDifficulty = false, useTargetScoreRange = false
            };
        }

        private static bool DeepEquals(FlowPos a, FlowPos b) => a.x == b.x && a.y == b.y;
        private static bool DeepEquals(FlowPairData a, FlowPairData b) =>
            a.colorId == b.colorId && DeepEquals(a.endpointA, b.endpointA) && DeepEquals(a.endpointB, b.endpointB);
        private static bool DeepEquals(FlowPathData a, FlowPathData b) =>
            a.colorId == b.colorId && a.cells.SequenceEqual(b.cells);
        private static bool DeepEquals(FlowGeneratedLevel a, FlowGeneratedLevel b) =>
            a.usedSeed == b.usedSeed && a.coverageRatio == b.coverageRatio &&
            a.levelData.width == b.levelData.width && a.levelData.height == b.levelData.height &&
            a.levelData.pairs.All(p => b.levelData.pairs.Any(q => DeepEquals(p, q))) &&
            a.solutionData.paths.All(p => b.solutionData.paths.Any(q => DeepEquals(p, q)));

        [Test]
        public void Generate_SameSeed_DeeplyEqual()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 42);

            var r1 = g.Generate(1001, cfg);
            var r2 = g.Generate(1001, cfg);

            Assert.IsTrue(r1.success);
            Assert.IsTrue(r2.success);
            Assert.IsTrue(DeepEquals(r1.generatedLevel, r2.generatedLevel));
        }

        [Test]
        public void Generate_RecordsLevelIdAndSeed()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 77);

            var r = g.Generate(2001, cfg);

            Assert.IsTrue(r.success);
            Assert.AreEqual(2001, r.levelId);
            Assert.AreEqual(77, r.usedSeed);
            Assert.AreEqual(2001, r.generatedLevel.levelData.levelId);
        }

        [Test]
        public void Generate_5x5_Succeeds()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 42, maxLevel: 50);

            var r = g.Generate(1, cfg);

            Assert.IsTrue(r.success);
            Assert.IsNotNull(r.generatedLevel);
        }

        [Test]
        public void Generate_RecommendationPassesValidator()
        {
            var g = MakeGenerator();
            var validator = new FlowSolutionValidator();
            var cfg = MakeConfig(5, 5, 2, seed: 42, maxLevel: 30);

            var r = g.Generate(1, cfg);

            Assert.IsTrue(r.success);
            var v = validator.Validate(r.generatedLevel.levelData, r.generatedLevel.solutionData);
            Assert.IsTrue(v.isValid, $"Validation failed: {v.errorCode} — {v.errorMessage}");
        }

        [Test]
        public void Generate_CoverageInRange()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, 0.3f, 0.7f, seed: 42, maxLevel: 30);

            var r = g.Generate(1, cfg);

            Assert.IsTrue(r.success);
            Assert.IsTrue(r.generatedLevel.coverageRatio >= 0.3f,
                $"Coverage {r.generatedLevel.coverageRatio} < 0.3");
            Assert.IsTrue(r.generatedLevel.coverageRatio <= 0.7f,
                $"Coverage {r.generatedLevel.coverageRatio} > 0.7");
        }

        [Test]
        public void Generate_PathLengthsInRange()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, minLen: 2, maxLen: 5, seed: 42, maxLevel: 30);

            var r = g.Generate(1, cfg);

            Assert.IsTrue(r.success);
            foreach (var path in r.generatedLevel.solutionData.paths)
            {
                Assert.IsTrue(path.cells.Count >= 2, $"Path too short: {path.cells.Count}");
                Assert.IsTrue(path.cells.Count <= 5, $"Path too long: {path.cells.Count}");
            }
        }

        [Test]
        public void Generate_PairsAndPaths_ColorIdSorted()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 3, seed: 42, maxLevel: 30);

            var r = g.Generate(1, cfg);

            Assert.IsTrue(r.success);
            var pairIds = r.generatedLevel.levelData.pairs.Select(p => p.colorId).ToList();
            var pathIds = r.generatedLevel.solutionData.paths.Select(p => p.colorId).ToList();
            Assert.IsTrue(pairIds.SequenceEqual(pathIds));
            for (var i = 1; i < pairIds.Count; i++)
                Assert.IsTrue(pairIds[i] > pairIds[i - 1], "Pairs not sorted");
        }

        [Test]
        public void Generate_TargetDifficultyMismatch_ExhaustsAttempts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(3, 3, 1, 0.2f, 0.5f, 2, 3, seed: 42, maxLevel: 5);
            cfg.useTargetDifficulty = true;
            cfg.targetDifficulty = FlowDifficultyTier.Expert; // impossible on 3x3

            var r = g.Generate(1, cfg);

            Assert.IsFalse(r.success);
            Assert.AreEqual(5, r.attemptCount);
        }

        [Test]
        public void Generate_ImpossibleMinimumOccupancy_FailsFast()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(3, 3, 5, minLen: 5, maxLevel: 5);
            // 5 colors * 5 minLen = 25 = 3*3 capacity, need 25 cells for 5 paths of 5 each
            // But feasibleMax = min(9, 5*5) = 9, feasibleMin = 5*5=25 > 9 → impossible

            var r = g.Generate(1, cfg);

            Assert.IsFalse(r.success);
            Assert.IsNotNull(r.diagnostic);
        }

        [Test]
        public void Generate_InvalidCoverageRange_FailsFast()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, 1.5f, 2f, seed: 42);

            var r = g.Generate(1, cfg);

            Assert.IsFalse(r.success);
            Assert.IsNotNull(r.diagnostic);
        }

        [Test]
        public void Generate_EmptyCellsAllowed()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, 0.2f, 0.5f, seed: 42, maxLevel: 30);

            var r = g.Generate(1, cfg);

            Assert.IsTrue(r.success);
            var board = new FlowBoard(5, 5);
            foreach (var path in r.generatedLevel.solutionData.paths)
            foreach (var cell in path.cells)
                board.Set(cell, path.colorId);
            // not all cells occupied → coverage < 1.0
            Assert.IsTrue(r.generatedLevel.coverageRatio < 1.0f);
        }

        [Test]
        public void Generate_NullConfig_Throws()
        {
            var g = MakeGenerator();
            Assert.Throws<ArgumentNullException>(() => g.Generate(1, null));
        }

        [Test]
        public void Generate_Repeatability_RunThreeTimes()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(6, 6, 3, seed: 123, maxLevel: 30);

            for (var i = 0; i < 3; i++)
            {
                var r = g.Generate(1, cfg);
                Assert.IsTrue(r.success, $"Run {i} failed");
                Assert.IsNotNull(r.generatedLevel);
            }
        }
    }
}
