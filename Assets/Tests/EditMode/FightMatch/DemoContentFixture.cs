using System;
using System.Collections.Generic;
using FlowPuzzle.Core;
using FlowPuzzle.Validation;

namespace FightMatch.Core.Tests
{
    internal enum SourceCoordinateConvention
    {
        Unspecified,
        OneBasedBottomLeft,
        OneBasedTopLeft
    }

    internal enum FixtureSourceKind
    {
        SourceCandidate,
        Constructed
    }

    internal sealed class FixturePairBinding
    {
        internal string SourcePair { get; }
        internal int ColorId { get; }
        internal string EnemyAlias { get; }
        internal int OriginalSlot { get; }

        internal FixturePairBinding(string sourcePair, int colorId, string enemyAlias, int originalSlot)
        {
            if (string.IsNullOrWhiteSpace(sourcePair))
                throw new ArgumentException("MissingSourcePair", nameof(sourcePair));
            if (string.IsNullOrWhiteSpace(enemyAlias))
                throw new ArgumentException("MissingEnemyAlias", nameof(enemyAlias));
            if (originalSlot < 0)
                throw new ArgumentOutOfRangeException(nameof(originalSlot));
            SourcePair = sourcePair;
            ColorId = colorId;
            EnemyAlias = enemyAlias;
            OriginalSlot = originalSlot;
        }
    }

    internal sealed class FixtureRoute
    {
        internal int ColorId { get; }
        internal IReadOnlyList<FlowPos> Cells { get; }

        internal FixtureRoute(int colorId, List<FlowPos> cells)
        {
            ColorId = colorId;
            Cells = cells == null ? null : Array.AsReadOnly(cells.ToArray());
        }
    }

    internal sealed class FixtureGeometryEvidence
    {
        internal DemoContentFixture Capture { get; }
        internal bool IsValid { get; }
        internal string ErrorCode { get; }
        internal string ErrorMessage { get; }

        internal FixtureGeometryEvidence(DemoContentFixture capture, FlowValidationResult result)
        {
            Capture = capture;
            IsValid = result.isValid;
            ErrorCode = result.errorCode;
            ErrorMessage = result.errorMessage;
        }

        internal bool IsCurrentFor(DemoContentFixture current, bool cancelled)
        {
            return !cancelled && ReferenceEquals(Capture, current);
        }
    }

    // Test-only snapshots. Source candidates do not assert a confirmed source orientation.
    internal sealed class DemoContentFixture
    {
        internal const string BoardsSourcePath = "docs/game-design/balance/results/boards.json";
        internal const string BoardsSourceSha256 =
            "671467CAF72C52EB83A03B396AD82F5557A47B2F9CBD3F9DA6D0EC560A7D2AAD";

        private readonly FlowLevelData level;
        private readonly FlowSolutionData solution;

        internal string FixtureKey { get; }
        internal long Revision { get; }
        internal FixtureSourceKind SourceKind { get; }
        internal string SourcePath { get; }
        internal string SourceSha256 { get; }
        internal string SourceLocator { get; }
        internal SourceCoordinateConvention? SourceConvention { get; }
        internal string CoordinateTransform { get; }
        internal IReadOnlyList<FixturePairBinding> Bindings { get; }
        internal IReadOnlyList<FixtureRoute> OriginalRoutes { get; }

        private DemoContentFixture(string fixtureKey, long revision, FixtureSourceKind sourceKind,
            FlowLevelData level, FlowSolutionData solution, IReadOnlyList<FixturePairBinding> bindings,
            FlowSolutionData original, SourceCoordinateConvention? convention, string sourceLocator)
        {
            FixtureKey = fixtureKey;
            Revision = revision;
            SourceKind = sourceKind;
            SourceConvention = convention;
            SourcePath = sourceKind == FixtureSourceKind.SourceCandidate ? BoardsSourcePath : null;
            SourceSha256 = sourceKind == FixtureSourceKind.SourceCandidate ? BoardsSourceSha256 : null;
            SourceLocator = sourceLocator;
            CoordinateTransform = convention == SourceCoordinateConvention.OneBasedBottomLeft
                ? "C-up-v1:(x-1,y-1)"
                : convention == SourceCoordinateConvention.OneBasedTopLeft
                    ? "C-down-v1:(x-1,height-y)" : "ZeroBasedBottomLeft:identity";
            this.level = CloneLevel(level);
            this.solution = CloneSolution(solution);
            Bindings = CopyBindings(bindings, this.level);
            if (original.paths != null)
            {
                var routes = new List<FixtureRoute>();
                foreach (var path in original.paths)
                    routes.Add(new FixtureRoute(path.colorId, path.cells));
                OriginalRoutes = routes.AsReadOnly();
            }
        }

        internal static DemoContentFixture CaptureSource(int stage, long revision,
            SourceCoordinateConvention convention)
        {
            if (stage != 1 && stage != 3)
                throw new ArgumentOutOfRangeException(nameof(stage));
            CheckRevision(revision);
            if (convention == SourceCoordinateConvention.Unspecified)
                throw new ArgumentException("MissingSourceCoordinateConvention", nameof(convention));
            if (convention != SourceCoordinateConvention.OneBasedBottomLeft &&
                convention != SourceCoordinateConvention.OneBasedTopLeft)
                throw new ArgumentOutOfRangeException(nameof(convention));

            var original = new FlowSolutionData { levelId = stage };
            original.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = stage == 1
                    ? new List<FlowPos> { new FlowPos(1, 1), new FlowPos(1, 2), new FlowPos(2, 2) }
                    : new List<FlowPos> { new FlowPos(1, 4), new FlowPos(1, 3), new FlowPos(2, 3) }
            });
            original.paths.Add(new FlowPathData
            {
                colorId = 1,
                cells = stage == 1
                    ? new List<FlowPos> { new FlowPos(1, 4), new FlowPos(2, 4), new FlowPos(3, 4) }
                    : new List<FlowPos> { new FlowPos(1, 1), new FlowPos(2, 1), new FlowPos(3, 1) }
            });
            var level = new FlowLevelData { levelId = stage, width = 4, height = 4 };
            var solution = CloneSolution(original);
            foreach (var path in solution.paths)
            {
                for (var i = 0; i < path.cells.Count; i++)
                {
                    var cell = path.cells[i];
                    path.cells[i] = new FlowPos(cell.x - 1,
                        convention == SourceCoordinateConvention.OneBasedBottomLeft
                            ? cell.y - 1 : level.height - cell.y);
                }
                level.pairs.Add(new FlowPairData
                {
                    colorId = path.colorId,
                    endpointA = path.cells[0],
                    endpointB = path.cells[path.cells.Count - 1]
                });
            }
            var bindings = new[]
            {
                new FixturePairBinding("A", 0, stage == 1 ? "E01" : "E02", 0),
                new FixturePairBinding("B", 1, "E01", 1)
            };
            return new DemoContentFixture("L" + stage, revision, FixtureSourceKind.SourceCandidate,
                level, solution, bindings, original, convention, "stage=" + stage + ",phase=1");
        }

        internal static DemoContentFixture CaptureConstructed(string fixtureKey, long revision,
            FlowLevelData level, FlowSolutionData solution, IReadOnlyList<FixturePairBinding> bindings)
        {
            if (string.IsNullOrWhiteSpace(fixtureKey))
                throw new ArgumentException("MissingFixtureKey", nameof(fixtureKey));
            CheckRevision(revision);
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            if (solution == null)
                throw new ArgumentNullException(nameof(solution));
            return new DemoContentFixture(fixtureKey, revision, FixtureSourceKind.Constructed,
                level, solution, bindings, solution, null, null);
        }

        internal FlowLevelData CopyLevel()
        {
            return CloneLevel(level);
        }

        internal FlowSolutionData CopySolution()
        {
            return CloneSolution(solution);
        }

        internal FixtureGeometryEvidence ValidateGeometry()
        {
            var result = new FlowSolutionValidator().Validate(CopyLevel(), CopySolution());
            return new FixtureGeometryEvidence(this, result);
        }

        private static void CheckRevision(long revision)
        {
            if (revision <= 0)
                throw new ArgumentOutOfRangeException(nameof(revision));
        }

        private static IReadOnlyList<FixturePairBinding> CopyBindings(
            IReadOnlyList<FixturePairBinding> bindings, FlowLevelData level)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));
            if (bindings.Count == 0)
                throw new ArgumentException("MissingPairBinding", nameof(bindings));
            var copy = new List<FixturePairBinding>();
            var colors = new HashSet<int>();
            foreach (var binding in bindings)
            {
                if (binding == null)
                    throw new ArgumentException("MissingPairBinding", nameof(bindings));
                copy.Add(new FixturePairBinding(binding.SourcePair, binding.ColorId,
                    binding.EnemyAlias, binding.OriginalSlot));
                colors.Add(binding.ColorId);
            }
            if (level.pairs != null)
                foreach (var pair in level.pairs)
                    if (!colors.Contains(pair.colorId))
                        throw new ArgumentException("MissingPairBinding: " + pair.colorId, nameof(bindings));
            return copy.AsReadOnly();
        }

        private static FlowLevelData CloneLevel(FlowLevelData source)
        {
            var copy = new FlowLevelData
            {
                levelId = source.levelId, width = source.width, height = source.height,
                difficulty = source.difficulty, difficultyScore = source.difficultyScore,
                pairs = source.pairs == null ? null : new List<FlowPairData>()
            };
            if (source.pairs != null)
                foreach (var pair in source.pairs)
                {
                    if (pair == null)
                        throw new ArgumentException("MissingPairObject", nameof(source));
                    copy.pairs.Add(new FlowPairData
                    {
                        colorId = pair.colorId, endpointA = pair.endpointA, endpointB = pair.endpointB
                    });
                }
            return copy;
        }

        private static FlowSolutionData CloneSolution(FlowSolutionData source)
        {
            var copy = new FlowSolutionData
            {
                levelId = source.levelId,
                paths = source.paths == null ? null : new List<FlowPathData>()
            };
            if (source.paths != null)
                foreach (var path in source.paths)
                {
                    if (path == null)
                        throw new ArgumentException("MissingPathObject", nameof(source));
                    copy.paths.Add(new FlowPathData
                    {
                        colorId = path.colorId,
                        cells = path.cells == null ? null : new List<FlowPos>(path.cells)
                    });
                }
            return copy;
        }
    }
}
