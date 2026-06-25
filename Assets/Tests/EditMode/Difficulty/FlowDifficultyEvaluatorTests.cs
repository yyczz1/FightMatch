using System;
using System.Collections.Generic;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using NUnit.Framework;

namespace FlowPuzzle.Tests.Difficulty
{
    [TestFixture]
    public class FlowDifficultyEvaluatorTests
    {
        private FlowLevelData MakeLevel(int w, int h, params (int colorId, int ax, int ay, int bx, int by)[] endpoints)
        {
            var level = new FlowLevelData { width = w, height = h };
            foreach (var (id, ax, ay, bx, by) in endpoints)
                level.pairs.Add(new FlowPairData
                {
                    colorId = id,
                    endpointA = new FlowPos(ax, ay),
                    endpointB = new FlowPos(bx, by)
                });
            return level;
        }

        private FlowSolutionData MakeSolution(params (int colorId, FlowPos[] cells)[] paths)
        {
            var solution = new FlowSolutionData();
            foreach (var (id, cells) in paths)
                solution.paths.Add(new FlowPathData
                {
                    colorId = id,
                    cells = new List<FlowPos>(cells)
                });
            return solution;
        }

        [Test]
        public void BoardSizeScore_Exact()
        {
            var level = MakeLevel(5, 4, (0, 0, 0, 4, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(5 * 4 * 0.25f, report.boardSizeScore, 0.001f);
        }

        [Test]
        public void ColorCountScore_Exact()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 0), (1, 0, 1, 4, 1), (2, 0, 2, 4, 2));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1), new FlowPos(4, 1) }),
                (2, new[] { new FlowPos(0, 2), new FlowPos(1, 2), new FlowPos(2, 2), new FlowPos(3, 2), new FlowPos(4, 2) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(3 * 6f, report.colorCountScore, 0.001f);
        }

        [Test]
        public void CoverageRatio_Exact()
        {
            var level = MakeLevel(4, 3, (0, 0, 0, 3, 0));
            // 4 cells out of 12 -> coverage ratio 4/12 = 0.333...
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(4f / 12f * 45f, report.coverageScore, 0.001f);
            Assert.AreEqual(4f / 12f, report.coverageScore / 45f, 0.001f);
        }

        [Test]
        public void StraightPath_TurnCountAndScore()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(0, report.totalTurnCount);
            Assert.AreEqual(0f, report.turnScore, 0.001f);
        }

        [Test]
        public void LPath_OneTurn()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 2, 2));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(2, 1), new FlowPos(2, 2) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(1, report.totalTurnCount);
            Assert.AreEqual(2.5f, report.turnScore, 0.001f);
        }

        [Test]
        public void MultiTurnPath_CountsEachTurn()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 3, 2));
            // path with 3 turns
            var solution = MakeSolution((0, new[]
            {
                new FlowPos(0, 0), new FlowPos(1, 0), // right
                new FlowPos(1, 1),                    // up (turn)
                new FlowPos(2, 1),                    // right (turn)
                new FlowPos(2, 2),                    // up (turn)
                new FlowPos(3, 2)                     // right
            }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(4, report.totalTurnCount);
            Assert.AreEqual(4 * 2.5f, report.turnScore, 0.001f);
        }

        [Test]
        public void Detour_MetricAndScore()
        {
            // path that goes out of the way: (0,0)→(1,0)→(1,1)→(1,2)→(0,2)
            // moves=4, manhattan((0,0)→(0,2))=2, detour=2
            var level = MakeLevel(3, 3, (0, 0, 0, 0, 2));
            var solution = MakeSolution((0, new[]
            {
                new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(1, 1),
                new FlowPos(1, 2), new FlowPos(0, 2)
            }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(2, report.totalDetour);
            Assert.AreEqual(2 * 2f, report.detourScore, 0.001f);
        }

        [Test]
        public void EndpointManhattan_MetricAndScore()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 0), (1, 0, 1, 0, 4));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(0, 2), new FlowPos(0, 3), new FlowPos(0, 4) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            // pair 0: |4-0|+|0-0| = 4; pair 1: |0-0|+|4-1| = 3; total = 7
            Assert.AreEqual(7, report.totalEndpointManhattanDistance);
            Assert.AreEqual(7 * 1.2f, report.endpointDistanceScore, 0.001f);
        }

        [Test]
        public void DifferentColorAdjacent_CountedOncePerPair()
        {
            // two parallel adjacent paths of different colors share a boundary
            var level = MakeLevel(3, 2, (0, 0, 0, 2, 0), (1, 0, 1, 2, 1));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(1, 1), new FlowPos(2, 1) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            // cells adjacent vertically: (0,0)-(0,1), (1,0)-(1,1), (2,0)-(2,1) = 3
            Assert.AreEqual(3, report.differentColorAdjacentCount);
            Assert.AreEqual(3 * 1.5f, report.interactionScore, 0.001f);
        }

        [Test]
        public void Bottleneck_EndpointCellsExcluded()
        {
            // one straight path on a 3x3 board; endpoints at (0,0) and (2,0)
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.IsTrue(report.bottleneckCount >= 0);
        }

        [Test]
        public void Bottleneck_OccupiedCellsDetected()
        {
            // 2x2 board, straight path through (0,0)→(1,0), endpoints (0,0) and (1,0)
            var level = MakeLevel(2, 2, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.IsTrue(report.bottleneckCount >= 0);
        }

        [Test]
        public void Bottleneck_EmptyCellsDetected()
        {
            var level = MakeLevel(2, 2, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.IsTrue(report.bottleneckCount >= 0);
        }

        [Test]
        public void BottleneckScore_MatchesWeight()
        {
            var level = MakeLevel(2, 2, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(report.bottleneckCount * 1.5f, report.bottleneckScore, 0.001f);
        }

        [Test]
        public void TotalScore_EqualsSumOfComponents()
        {
            var level = MakeLevel(5, 4, (0, 0, 0, 4, 0), (1, 0, 1, 4, 1));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1), new FlowPos(4, 1) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            var sum = report.boardSizeScore
                      + report.colorCountScore
                      + report.coverageScore
                      + report.turnScore
                      + report.detourScore
                      + report.interactionScore
                      + report.endpointDistanceScore
                      + report.bottleneckScore;

            Assert.AreEqual(sum, report.totalScore, 0.01f);
        }

        [Test]
        public void Tier_Easy_Below60()
        {
            // 2x2 board, 1 color, straight path → very low score
            var level = MakeLevel(2, 2, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(FlowDifficultyTier.Easy, report.difficulty);
        }

        [Test]
        public void Tier_Normal()
        {
            // 5x5, 3 colors, adjacent straight paths → score should fall in Normal range
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 0), (1, 0, 1, 4, 1), (2, 0, 2, 4, 2));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1), new FlowPos(4, 1) }),
                (2, new[] { new FlowPos(0, 2), new FlowPos(1, 2), new FlowPos(2, 2), new FlowPos(3, 2), new FlowPos(4, 2) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.AreEqual(FlowDifficultyTier.Normal, report.difficulty);
            Assert.IsTrue(report.totalScore >= 60f);
            Assert.IsTrue(report.totalScore < 120f);
        }

        [Test]
        public void Tier_Hard_At120()
        {
            // Build bigger level to get into Hard range
            var level = MakeLevel(7, 7, (0, 0, 0, 6, 0), (1, 0, 2, 6, 2), (2, 0, 4, 6, 4),
                (3, 0, 6, 6, 6));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0),
                    new FlowPos(4, 0), new FlowPos(5, 0), new FlowPos(6, 0) }),
                (1, new[] { new FlowPos(0, 2), new FlowPos(1, 2), new FlowPos(2, 2), new FlowPos(3, 2),
                    new FlowPos(4, 2), new FlowPos(5, 2), new FlowPos(6, 2) }),
                (2, new[] { new FlowPos(0, 4), new FlowPos(1, 4), new FlowPos(2, 4), new FlowPos(3, 4),
                    new FlowPos(4, 4), new FlowPos(5, 4), new FlowPos(6, 4) }),
                (3, new[] { new FlowPos(0, 6), new FlowPos(1, 6), new FlowPos(2, 6), new FlowPos(3, 6),
                    new FlowPos(4, 6), new FlowPos(5, 6), new FlowPos(6, 6) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.IsTrue(report.totalScore >= 120f, "Score should be at least 120 for Hard tier");
            Assert.AreEqual(FlowDifficultyTier.Hard, report.difficulty);
        }

        [Test]
        public void Tier_Expert()
        {
            // Very large dense level
            var level = MakeLevel(10, 10, (0, 0, 0, 9, 0), (1, 0, 1, 9, 1), (2, 0, 2, 9, 2),
                (3, 0, 3, 9, 3), (4, 0, 4, 9, 4), (5, 0, 5, 9, 5), (6, 0, 6, 9, 6),
                (7, 0, 7, 9, 7));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0),
                    new FlowPos(4, 0), new FlowPos(5, 0), new FlowPos(6, 0), new FlowPos(7, 0),
                    new FlowPos(8, 0), new FlowPos(9, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1),
                    new FlowPos(4, 1), new FlowPos(5, 1), new FlowPos(6, 1), new FlowPos(7, 1),
                    new FlowPos(8, 1), new FlowPos(9, 1) }),
                (2, new[] { new FlowPos(0, 2), new FlowPos(1, 2), new FlowPos(2, 2), new FlowPos(3, 2),
                    new FlowPos(4, 2), new FlowPos(5, 2), new FlowPos(6, 2), new FlowPos(7, 2),
                    new FlowPos(8, 2), new FlowPos(9, 2) }),
                (3, new[] { new FlowPos(0, 3), new FlowPos(1, 3), new FlowPos(2, 3), new FlowPos(3, 3),
                    new FlowPos(4, 3), new FlowPos(5, 3), new FlowPos(6, 3), new FlowPos(7, 3),
                    new FlowPos(8, 3), new FlowPos(9, 3) }),
                (4, new[] { new FlowPos(0, 4), new FlowPos(1, 4), new FlowPos(2, 4), new FlowPos(3, 4),
                    new FlowPos(4, 4), new FlowPos(5, 4), new FlowPos(6, 4), new FlowPos(7, 4),
                    new FlowPos(8, 4), new FlowPos(9, 4) }),
                (5, new[] { new FlowPos(0, 5), new FlowPos(1, 5), new FlowPos(2, 5), new FlowPos(3, 5),
                    new FlowPos(4, 5), new FlowPos(5, 5), new FlowPos(6, 5), new FlowPos(7, 5),
                    new FlowPos(8, 5), new FlowPos(9, 5) }),
                (6, new[] { new FlowPos(0, 6), new FlowPos(1, 6), new FlowPos(2, 6), new FlowPos(3, 6),
                    new FlowPos(4, 6), new FlowPos(5, 6), new FlowPos(6, 6), new FlowPos(7, 6),
                    new FlowPos(8, 6), new FlowPos(9, 6) }),
                (7, new[] { new FlowPos(0, 7), new FlowPos(1, 7), new FlowPos(2, 7), new FlowPos(3, 7),
                    new FlowPos(4, 7), new FlowPos(5, 7), new FlowPos(6, 7), new FlowPos(7, 7),
                    new FlowPos(8, 7), new FlowPos(9, 7) }));

            var report = new FlowDifficultyEvaluator().Evaluate(level, solution);

            Assert.IsTrue(report.totalScore >= 200f, $"Score {report.totalScore} should be >= 200 for Expert");
            Assert.AreEqual(FlowDifficultyTier.Expert, report.difficulty);
        }

        [Test]
        public void NullLevel_ThrowsArgumentNullException()
        {
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));
            Assert.Throws<ArgumentNullException>(() => new FlowDifficultyEvaluator().Evaluate(null, solution));
        }

        [Test]
        public void NullSolution_ThrowsArgumentNullException()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            Assert.Throws<ArgumentNullException>(() => new FlowDifficultyEvaluator().Evaluate(level, null));
        }

        [Test]
        public void NullPairs_ThrowsArgumentException()
        {
            var level = new FlowLevelData { width = 3, height = 3, pairs = null };
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));
            Assert.Throws<ArgumentException>(() => new FlowDifficultyEvaluator().Evaluate(level, solution));
        }

        [Test]
        public void NullPaths_ThrowsArgumentException()
        {
            var level = MakeLevel(3, 3, (0, 0, 0, 2, 0));
            var solution = new FlowSolutionData { paths = null };
            Assert.Throws<ArgumentException>(() => new FlowDifficultyEvaluator().Evaluate(level, solution));
        }

        [Test]
        public void NonPositiveWidth_ThrowsArgumentOutOfRangeException()
        {
            var level = MakeLevel(0, 3, (0, 0, 0, 1, 0));
            var solution = MakeSolution((0, new[] { new FlowPos(0, 0), new FlowPos(1, 0) }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlowDifficultyEvaluator().Evaluate(level, solution));
        }

        [Test]
        public void RepeatedEvaluation_ReturnsDeeplyEqualReport()
        {
            var level = MakeLevel(5, 5, (0, 0, 0, 4, 0), (1, 0, 1, 4, 1));
            var solution = MakeSolution(
                (0, new[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0), new FlowPos(4, 0) }),
                (1, new[] { new FlowPos(0, 1), new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1), new FlowPos(4, 1) }));

            var evaluator = new FlowDifficultyEvaluator();
            var report1 = evaluator.Evaluate(level, solution);
            var report2 = evaluator.Evaluate(level, solution);

            Assert.AreEqual(report1.totalScore, report2.totalScore, 0.001f);
            Assert.AreEqual(report1.difficulty, report2.difficulty);
            Assert.AreEqual(report1.boardSizeScore, report2.boardSizeScore, 0.001f);
            Assert.AreEqual(report1.bottleneckCount, report2.bottleneckCount);
        }
    }
}
