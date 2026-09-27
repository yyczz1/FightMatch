using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor.Persistence;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;
using NUnit.Framework;
using UnityEngine;

namespace FlowPuzzle.Tests.Persistence
{
    [TestFixture]
    public sealed class FlowLevelPersistencePureTests
    {
        private static readonly string JsonTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        private struct LevelSnapshot
        {
            public int usedSeed;
            public float coverageRatio;
            public string levelJson;
            public string solutionJson;
            public string reportJson;
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(JsonTestDir))
                Directory.Delete(JsonTestDir, true);
        }

        [Test]
        public void Asset_InitializesOwnedInstances()
        {
            var first = ScriptableObject.CreateInstance<FlowLevelAsset>();
            var second = ScriptableObject.CreateInstance<FlowLevelAsset>();

            Assert.IsNotNull(first.levelData);
            Assert.IsNotNull(first.solutionData);
            Assert.IsNotNull(first.difficultyReport);
            Assert.AreNotSame(first.levelData, second.levelData);
            Assert.AreNotSame(first.solutionData, second.solutionData);
            Assert.AreNotSame(first.difficultyReport, second.difficultyReport);

            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
        }

        [Test]
        public void Asset_AssignData_PreservesAllFields()
        {
            var level = MakeTestLevel();
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            asset.levelData = level.levelData;
            asset.solutionData = level.solutionData;
            asset.difficultyReport = level.difficultyReport;
            asset.generationSeed = level.usedSeed;
            asset.coverageRatio = level.coverageRatio;

            Assert.AreEqual(1001, asset.levelData.levelId);
            Assert.AreEqual(4, asset.levelData.width);
            Assert.AreEqual(3, asset.levelData.height);
            Assert.AreEqual(42, asset.generationSeed);
            Assert.AreEqual(0.33f, asset.coverageRatio, 0.01f);
            Assert.AreEqual(FlowDifficultyTier.Normal, asset.difficultyReport.difficulty);
            Assert.AreEqual(1, asset.solutionData.paths.Count);

            UnityEngine.Object.DestroyImmediate(asset);
        }

        [Test]
        public void Export_RoundTrip_ExactDtoComparison()
        {
            var level = MakeTestLevel();
            var result = new FlowLevelJsonExporter().Export(level, JsonTestDir);

            Assert.IsTrue(result.success);

            var levelDto = JsonUtility.FromJson<FlowLevelData>(File.ReadAllText(result.levelFilePath));
            var solutionDto = JsonUtility.FromJson<FlowSolutionData>(File.ReadAllText(result.solutionFilePath));
            Assert.AreEqual(1001, levelDto.levelId);
            Assert.AreEqual(4, levelDto.width);
            Assert.AreEqual(3, levelDto.height);
            Assert.AreEqual(1, levelDto.pairs.Count);
            Assert.AreEqual(0, levelDto.pairs[0].colorId);
            Assert.AreEqual(0, levelDto.pairs[0].endpointA.x);
            Assert.AreEqual(0, levelDto.pairs[0].endpointA.y);
            Assert.AreEqual(3, levelDto.pairs[0].endpointB.x);
            Assert.AreEqual(0, levelDto.pairs[0].endpointB.y);
            Assert.AreEqual(1001, solutionDto.levelId);
            Assert.AreEqual(1, solutionDto.paths.Count);
            Assert.AreEqual(0, solutionDto.paths[0].colorId);
            Assert.AreEqual(4, solutionDto.paths[0].cells.Count);
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(level.solutionData.paths[0].cells[i].x, solutionDto.paths[0].cells[i].x);
                Assert.AreEqual(level.solutionData.paths[0].cells[i].y, solutionDto.paths[0].cells[i].y);
            }
        }

        [Test]
        public void Export_LevelNoPaths_SolutionHasPaths()
        {
            var result = new FlowLevelJsonExporter().Export(MakeTestLevel(), JsonTestDir);

            Assert.IsFalse(File.ReadAllText(result.levelFilePath).Contains("\"paths\""));
            Assert.IsTrue(File.ReadAllText(result.solutionFilePath).Contains("\"paths\""));
        }

        [Test]
        public void Export_Failures_ReturnDiagnostics()
        {
            var noReport = MakeTestLevel();
            noReport.difficultyReport = null;
            var incomplete = new FlowLevelJsonExporter().Export(noReport, JsonTestDir);
            var whitespace = new FlowLevelJsonExporter().Export(MakeTestLevel(), "   ");
            var invalidChars = new FlowLevelJsonExporter().Export(MakeTestLevel(), "X:\0invalid");

            Assert.IsFalse(incomplete.success);
            Assert.AreEqual("IncompleteLevel", incomplete.diagnostic.errorCode);
            Assert.IsFalse(whitespace.success);
            Assert.AreEqual("InvalidOutputPath", whitespace.diagnostic.errorCode);
            Assert.IsFalse(invalidChars.success);
            Assert.IsNotNull(invalidChars.diagnostic);
        }

        [Test]
        public void Export_InputUnchanged()
        {
            var level = MakeTestLevel();
            var snapshot = TakeSnapshot(level);

            new FlowLevelJsonExporter().Export(level, JsonTestDir);

            AssertUnchanged(snapshot, level);
        }

        [Test]
        public void BuildCanonical_RecalculatesCoverageDifficultyAndOwnsData()
        {
            var level = MakeTestLevel();

            var canonical = BuildCanonical(level);

            Assert.AreEqual(4f / 12f, canonical.coverageRatio, 0.001f);
            Assert.AreEqual(canonical.difficultyReport.difficulty, canonical.levelData.difficulty);
            Assert.AreEqual(canonical.difficultyReport.totalScore, canonical.levelData.difficultyScore, 0.001f);
            Assert.AreNotSame(level.levelData, canonical.levelData);
            Assert.AreNotSame(level.solutionData, canonical.solutionData);
            Assert.AreNotSame(level.difficultyReport, canonical.difficultyReport);

            level.levelData.pairs.Clear();
            level.solutionData.paths[0].cells[0] = new FlowPos(9, 9);

            Assert.AreEqual(1, canonical.levelData.pairs.Count);
            Assert.AreEqual(new FlowPos(0, 0), canonical.solutionData.paths[0].cells[0]);
        }

        [Test]
        public void BuildCanonical_InputUnchanged()
        {
            var level = MakeTestLevel();
            var snapshot = TakeSnapshot(level);

            BuildCanonical(level);

            AssertUnchanged(snapshot, level);
        }

        [Test]
        public void BuildCanonical_InvalidInputsThrow()
        {
            var invalid = MakeTestLevel();
            invalid.solutionData.paths[0].cells.Clear();

            AssertInnerThrows<InvalidOperationException>(() => BuildCanonical(invalid));
            AssertInnerThrows<InvalidOperationException>(() =>
                BuildCanonical(new FlowGeneratedLevel { levelData = null }));
        }

        [Test]
        public void NormalizeFolder_AcceptsAssetsAndNestedBackslash()
        {
            Assert.AreEqual("Assets", InvokeStringNormalizer("NormalizeFolder", "Assets"));
            Assert.AreEqual(
                "Assets/Temp/FlowPuzzleTests/Sub",
                InvokeStringNormalizer("NormalizeFolder", "Assets\\Temp\\FlowPuzzleTests\\Sub"));
        }

        [Test]
        public void NormalizeFolder_RejectsUnsafePaths()
        {
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeFolder", "AssetsOutside"));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeFolder", "Assets/../Outside"));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeFolder", "C:/Something"));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeFolder", "Assets//Bad"));
        }

        [Test]
        public void NormalizeAssetName_NormalizesExtensions()
        {
            Assert.AreEqual("A.asset", InvokeStringNormalizer("NormalizeAssetName", "A.asset"));
            Assert.AreEqual("B.asset", InvokeStringNormalizer("NormalizeAssetName", "B.ASSET"));
            Assert.AreEqual("C.asset", InvokeStringNormalizer("NormalizeAssetName", "C.asset.asset"));
            Assert.AreEqual("Plain.asset", InvokeStringNormalizer("NormalizeAssetName", "Plain"));
        }

        [Test]
        public void NormalizeAssetName_RejectsUnsafeNames()
        {
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeAssetName", ""));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeAssetName", "   "));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeAssetName", ".asset"));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeAssetName", ".asset.asset"));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeAssetName", "sub/Bad"));
            AssertInnerThrows<ArgumentException>(() => InvokeStringNormalizer("NormalizeAssetName", "sub\\Bad"));
        }

        [Test]
        public void Repository_PublicNullContractsThrowBeforeAssetDatabaseAccess()
        {
            var repo = MakeRepo();
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();

            Assert.Throws<ArgumentNullException>(() => repo.SaveNew(null, "Assets"));
            Assert.Throws<ArgumentNullException>(() => repo.Overwrite(null, MakeTestLevel()));
            Assert.Throws<ArgumentNullException>(() => repo.Overwrite(asset, null));
            Assert.Throws<ArgumentNullException>(() => repo.SaveAs(null, MakeTestLevel(), "Assets", "X"));
            Assert.Throws<ArgumentNullException>(() => repo.SaveAs(asset, null, "Assets", "X"));
            Assert.Throws<ArgumentNullException>(() => repo.SaveAs((FlowGeneratedLevel)null, "Assets", "X"));

            UnityEngine.Object.DestroyImmediate(asset);
        }

        [Test]
        public void Repository_UnsafePathsThrowBeforeAssetDatabaseMutation()
        {
            var repo = MakeRepo();
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();

            Assert.Throws<ArgumentException>(() => repo.SaveNew(MakeTestLevel(), "AssetsOutside"));
            Assert.Throws<ArgumentException>(() => repo.SaveNew(MakeTestLevel(), "Assets/../Outside"));
            Assert.Throws<ArgumentException>(() => repo.SaveNew(MakeTestLevel(), "C:/Something"));
            Assert.Throws<ArgumentException>(() =>
                repo.SaveAs(asset, MakeTestLevel(), "Assets", "sub/Bad"));

            UnityEngine.Object.DestroyImmediate(asset);
        }

        [Test]
        public void NullConstructor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FlowLevelAssetRepository(null, new FlowDifficultyEvaluator()));
            Assert.Throws<ArgumentNullException>(() =>
                new FlowLevelAssetRepository(new FlowSolutionValidator(), null));
        }

        private static FlowGeneratedLevel MakeTestLevel()
        {
            var levelData = new FlowLevelData
            {
                levelId = 1001,
                width = 4,
                height = 3,
                difficulty = FlowDifficultyTier.Normal,
                difficultyScore = 75f
            };
            levelData.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0),
                endpointB = new FlowPos(3, 0)
            });

            var solutionData = new FlowSolutionData { levelId = 1001 };
            solutionData.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos>
                {
                    new FlowPos(0, 0),
                    new FlowPos(1, 0),
                    new FlowPos(2, 0),
                    new FlowPos(3, 0)
                }
            });

            return new FlowGeneratedLevel
            {
                levelData = levelData,
                solutionData = solutionData,
                difficultyReport = new FlowDifficultyReport
                {
                    difficulty = FlowDifficultyTier.Normal,
                    totalScore = 75f
                },
                usedSeed = 42,
                coverageRatio = 0.33f
            };
        }

        private static FlowGeneratedLevel BuildCanonical(FlowGeneratedLevel level)
        {
            var method = typeof(FlowLevelAssetRepository).GetMethod(
                "BuildCanonical",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (FlowGeneratedLevel)method.Invoke(MakeRepo(), new object[] { level });
        }

        private static FlowLevelAssetRepository MakeRepo()
        {
            return new FlowLevelAssetRepository(
                new FlowSolutionValidator(),
                new FlowDifficultyEvaluator());
        }

        private static string InvokeStringNormalizer(string methodName, string value)
        {
            var method = typeof(FlowLevelAssetRepository).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic);
            return (string)method.Invoke(null, new object[] { value });
        }

        private static void AssertInnerThrows<T>(TestDelegate action) where T : Exception
        {
            var ex = Assert.Throws<TargetInvocationException>(action);
            Assert.IsInstanceOf<T>(ex.InnerException);
        }

        private static LevelSnapshot TakeSnapshot(FlowGeneratedLevel level)
        {
            return new LevelSnapshot
            {
                usedSeed = level.usedSeed,
                coverageRatio = level.coverageRatio,
                levelJson = JsonUtility.ToJson(level.levelData),
                solutionJson = JsonUtility.ToJson(level.solutionData),
                reportJson = JsonUtility.ToJson(level.difficultyReport)
            };
        }

        private static void AssertUnchanged(LevelSnapshot before, FlowGeneratedLevel after)
        {
            Assert.AreEqual(before.usedSeed, after.usedSeed);
            Assert.AreEqual(before.coverageRatio, after.coverageRatio);
            Assert.AreEqual(before.levelJson, JsonUtility.ToJson(after.levelData));
            Assert.AreEqual(before.solutionJson, JsonUtility.ToJson(after.solutionData));
            Assert.AreEqual(before.reportJson, JsonUtility.ToJson(after.difficultyReport));
        }
    }
}
