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
        private static bool DeepEquals(FlowGeneratedLevel a, FlowGeneratedLevel b)
        {
            if (a == null || b == null) return a == b;
            if (a.usedSeed != b.usedSeed) return false;
            if (a.coverageRatio != b.coverageRatio) return false;
            if (a.levelData.levelId != b.levelData.levelId) return false;
            if (a.solutionData.levelId != b.solutionData.levelId) return false;
            if (a.levelData.width != b.levelData.width) return false;
            if (a.levelData.height != b.levelData.height) return false;
            if (a.levelData.difficulty != b.levelData.difficulty) return false;
            if (a.levelData.difficultyScore != b.levelData.difficultyScore) return false;
            if (a.difficultyReport.difficulty != b.difficultyReport.difficulty) return false;
            if (a.difficultyReport.totalScore != b.difficultyReport.totalScore) return false;

            if (a.levelData.pairs.Count != b.levelData.pairs.Count) return false;
            for (var i = 0; i < a.levelData.pairs.Count; i++)
                if (!DeepEquals(a.levelData.pairs[i], b.levelData.pairs[i])) return false;

            if (a.solutionData.paths.Count != b.solutionData.paths.Count) return false;
            for (var i = 0; i < a.solutionData.paths.Count; i++)
                if (!DeepEquals(a.solutionData.paths[i], b.solutionData.paths[i])) return false;

            return true;
        }

        private static bool LayoutsEqual(FlowGeneratedLevel a, FlowGeneratedLevel b)
        {
            if (a == null || b == null) return a == b;
            if (a.levelData.width != b.levelData.width) return false;
            if (a.levelData.height != b.levelData.height) return false;

            if (a.levelData.pairs.Count != b.levelData.pairs.Count) return false;
            for (var i = 0; i < a.levelData.pairs.Count; i++)
                if (!DeepEquals(a.levelData.pairs[i], b.levelData.pairs[i])) return false;

            if (a.solutionData.paths.Count != b.solutionData.paths.Count) return false;
            for (var i = 0; i < a.solutionData.paths.Count; i++)
                if (!DeepEquals(a.solutionData.paths[i], b.solutionData.paths[i])) return false;

            return true;
        }

        // ── seed-preservation immediate failures ──

        [Test]
        public void Generate_InvalidDimensions_PreservesSeedAndZeroAttempts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(0, 5, 2, seed: 77);
            var r = g.Generate(1, cfg);
            Assert.IsFalse(r.success);
            Assert.AreEqual(77, r.usedSeed);
            Assert.AreEqual(0, r.attemptCount);
            Assert.AreEqual("InvalidDimensions", r.diagnostic.errorCode);
        }

        [Test]
        public void Generate_InvalidCoverageRange_PreservesSeedAndZeroAttempts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, 1.5f, 2f, seed: 88);
            var r = g.Generate(1, cfg);
            Assert.IsFalse(r.success);
            Assert.AreEqual(88, r.usedSeed);
            Assert.AreEqual(0, r.attemptCount);
            Assert.AreEqual("ImpossibleCoverageRange", r.diagnostic.errorCode);
        }

        [Test]
        public void Generate_InvalidPathLengthRange_PreservesSeedAndZeroAttempts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, minLen: 5, maxLen: 2, seed: 99);
            var r = g.Generate(1, cfg);
            Assert.IsFalse(r.success);
            Assert.AreEqual(99, r.usedSeed);
            Assert.AreEqual(0, r.attemptCount);
            Assert.AreEqual("InvalidPathLengthRange", r.diagnostic.errorCode);
        }

        [Test]
        public void Generate_InvalidAttemptBudget_PreservesSeedAndZeroAttempts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 55);
            cfg.maxLevelAttempt = 0;
            var r = g.Generate(1, cfg);
            Assert.IsFalse(r.success);
            Assert.AreEqual(55, r.usedSeed);
            Assert.AreEqual(0, r.attemptCount);
            Assert.AreEqual("InvalidAttemptBudget", r.diagnostic.errorCode);
        }

        // ── deterministic reproduction ──

        [Test]
        public void Generate_SameSeed_3x_DeeplyEqual()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 42);

            var r1 = g.Generate(1001, cfg);
            Assert.IsTrue(r1.success);

            for (var i = 0; i < 2; i++)
            {
                var r = g.Generate(1001, cfg);
                Assert.IsTrue(r.success);
                Assert.IsTrue(DeepEquals(r1.generatedLevel, r.generatedLevel),
                    $"Run {i + 2} not deeply equal to run 1");
            }
        }

        [Test]
        public void Generate_DifferentSeeds_DifferentLayouts()
        {
            var g = MakeGenerator();
            var r1 = g.Generate(1, MakeConfig(6, 6, 3, seed: 42, maxLevel: 30));
            var r2 = g.Generate(1, MakeConfig(6, 6, 3, seed: 123, maxLevel: 30));

            Assert.IsTrue(r1.success);
            Assert.IsTrue(r2.success);
            Assert.IsFalse(LayoutsEqual(r1.generatedLevel, r2.generatedLevel));
        }

        // ── board size coverage ──

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
        public void Generate_6x6_Succeeds()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(6, 6, 3, seed: 42, minLen: 2, maxLen: 6, maxLevel: 40);
            var r = g.Generate(1, cfg);
            Assert.IsTrue(r.success);
            Assert.AreEqual(6, r.generatedLevel.levelData.width);
            Assert.AreEqual(6, r.generatedLevel.levelData.height);
        }

        [Test]
        public void Generate_7x7_Succeeds()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(7, 7, 3, seed: 42, minLen: 2, maxLen: 8, maxLevel: 50);
            var r = g.Generate(1, cfg);
            Assert.IsTrue(r.success);
            Assert.AreEqual(7, r.generatedLevel.levelData.width);
            Assert.AreEqual(7, r.generatedLevel.levelData.height);
        }

        // ── result contracts ──

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
        public void Generate_DifficultySyncedToLevelData()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 42, maxLevel: 30);
            var r = g.Generate(1, cfg);
            Assert.IsTrue(r.success);
            Assert.AreEqual(r.generatedLevel.difficultyReport.difficulty,
                r.generatedLevel.levelData.difficulty);
            Assert.AreEqual(r.generatedLevel.difficultyReport.totalScore,
                r.generatedLevel.levelData.difficultyScore);
        }

        // ── range enforcement ──

        [Test]
        public void Generate_CoverageInRange()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, 0.3f, 0.7f, seed: 42, maxLevel: 30);
            var r = g.Generate(1, cfg);
            Assert.IsTrue(r.success);
            Assert.IsTrue(r.generatedLevel.coverageRatio >= 0.3f);
            Assert.IsTrue(r.generatedLevel.coverageRatio <= 0.7f);
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
                Assert.IsTrue(path.cells.Count >= 2);
                Assert.IsTrue(path.cells.Count <= 5);
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
        public void Generate_TargetScoreRange_Inclusive_Success()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(5, 5, 2, seed: 42, maxLevel: 30);

            // Get baseline score
            var baseline = g.Generate(1, cfg);
            Assert.IsTrue(baseline.success);
            var score = baseline.generatedLevel.difficultyReport.totalScore;

            // Set inclusive range around the score
            cfg.useTargetScoreRange = true;
            cfg.minTargetDifficultyScore = score;
            cfg.maxTargetDifficultyScore = score;

            var r = g.Generate(1, cfg);
            Assert.IsTrue(r.success, "Should succeed when target score matches");
        }

        [Test]
        public void Generate_TargetScoreRange_NonContaining_Exhausts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(3, 3, 1, 0.2f, 0.5f, 2, 3, seed: 42, maxLevel: 5);

            // Set impossible score range
            cfg.useTargetScoreRange = true;
            cfg.minTargetDifficultyScore = 9999f;
            cfg.maxTargetDifficultyScore = 99999f;

            var r = g.Generate(1, cfg);
            Assert.IsFalse(r.success);
            Assert.AreEqual(5, r.attemptCount);
        }

        [Test]
        public void Generate_TargetDifficultyMismatch_ExhaustsAttempts()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(3, 3, 1, 0.2f, 0.5f, 2, 3, seed: 42, maxLevel: 5);
            cfg.useTargetDifficulty = true;
            cfg.targetDifficulty = FlowDifficultyTier.Expert;
            var r = g.Generate(1, cfg);
            Assert.IsFalse(r.success);
            Assert.AreEqual(5, r.attemptCount);
        }

        [Test]
        public void Generate_ImpossibleMinimumOccupancy_FailsFast()
        {
            var g = MakeGenerator();
            var cfg = MakeConfig(3, 3, 5, minLen: 5, maxLevel: 5);
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
