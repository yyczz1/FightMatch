using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;
using NUnit.Framework;
using UnityEngine;

namespace FlowPuzzle.Tests.Persistence
{
    [TestFixture]
    public class FlowLevelPersistenceTests
    {
        private const string TestDir = "Assets/../FlowPuzzleTestOutput";

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
                    new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0)
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

        [SetUp]
        public void SetUp()
        {
            CleanTestDir();
        }

        [TearDown]
        public void TearDown()
        {
            CleanTestDir();
        }

        private static void CleanTestDir()
        {
            if (Directory.Exists(TestDir))
                Directory.Delete(TestDir, true);
        }

        [Test]
        public void FlowLevelAsset_InitializesOwnedInstances()
        {
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();

            Assert.IsNotNull(asset.levelData);
            Assert.IsNotNull(asset.solutionData);
            Assert.IsNotNull(asset.difficultyReport);
            Assert.AreEqual(0, asset.levelData.pairs.Count);
            Assert.AreEqual(0, asset.solutionData.paths.Count);

            ScriptableObject.DestroyImmediate(asset);
        }

        [Test]
        public void FlowLevelAsset_AssignData_PreservesAllFields()
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
            Assert.AreEqual(FlowDifficultyTier.Normal, asset.difficultyReport.difficulty);
            Assert.AreEqual(75f, asset.difficultyReport.totalScore, 0.01f);
            Assert.AreEqual(42, asset.generationSeed);
            Assert.AreEqual(0.33f, asset.coverageRatio, 0.01f);
            Assert.AreEqual(1, asset.solutionData.paths.Count);

            ScriptableObject.DestroyImmediate(asset);
        }

        [Test]
        public void Export_Success_CreatesBothFiles()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();

            var result = exporter.Export(level, TestDir);

            Assert.IsTrue(result.success);
            Assert.IsTrue(File.Exists(result.levelFilePath));
            Assert.IsTrue(File.Exists(result.solutionFilePath));
            Assert.IsTrue(result.levelFilePath.Contains("level_1001.json"));
            Assert.IsTrue(result.solutionFilePath.Contains("solution_1001.json"));
        }

        [Test]
        public void Export_LevelJson_ContainsPairsButNotPaths()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var result = exporter.Export(level, TestDir);

            var levelJson = File.ReadAllText(result.levelFilePath);
            Assert.IsTrue(levelJson.Contains("\"pairs\""));
            Assert.IsFalse(levelJson.Contains("\"paths\""));
        }

        [Test]
        public void Export_SolutionJson_ContainsPaths()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var result = exporter.Export(level, TestDir);

            var solutionJson = File.ReadAllText(result.solutionFilePath);
            Assert.IsTrue(solutionJson.Contains("\"paths\""));
        }

        [Test]
        public void Export_Json_RoundTripsData()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var result = exporter.Export(level, TestDir);

            var levelJson = File.ReadAllText(result.levelFilePath);
            Assert.IsTrue(levelJson.Contains("1001"), "Level JSON should contain levelId");
            Assert.IsTrue(levelJson.Contains("\"levelId\""));
            Assert.IsTrue(levelJson.Contains("\"width\""));
            Assert.IsTrue(levelJson.Contains("\"height\""));
            Assert.IsTrue(levelJson.Contains("\"colorId\""));
        }

        [Test]
        public void Export_CreatesMissingDirectory()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var nested = Path.Combine(TestDir, "sub", "deep");

            var result = exporter.Export(level, nested);

            Assert.IsTrue(result.success);
            Assert.IsTrue(Directory.Exists(nested));
        }

        [Test]
        public void Export_NullLevel_ReturnsFailure()
        {
            var exporter = new FlowLevelJsonExporter();
            var result = exporter.Export(null, TestDir);

            Assert.IsFalse(result.success);
            Assert.IsNotNull(result.diagnostic);
            Assert.AreEqual("IncompleteLevel", result.diagnostic.errorCode);
        }

        [Test]
        public void Export_EmptyPath_ReturnsFailure()
        {
            var exporter = new FlowLevelJsonExporter();
            var result = exporter.Export(MakeTestLevel(), "");

            Assert.IsFalse(result.success);
            Assert.AreEqual("InvalidOutputPath", result.diagnostic.errorCode);
        }

        [Test]
        public void Export_DoesNotMutateInput()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var originalLevelId = level.levelData.levelId;
            var originalSeed = level.usedSeed;

            exporter.Export(level, TestDir);

            Assert.AreEqual(originalLevelId, level.levelData.levelId);
            Assert.AreEqual(originalSeed, level.usedSeed);
        }

        [Test]
        public void FlowJsonExportResult_Success_SetsFields()
        {
            var r = FlowJsonExportResult.Success("level.json", "solution.json");

            Assert.IsTrue(r.success);
            Assert.AreEqual("level.json", r.levelFilePath);
            Assert.AreEqual("solution.json", r.solutionFilePath);
            Assert.IsNull(r.diagnostic);
        }

        [Test]
        public void FlowJsonExportResult_Failure_SetsFields()
        {
            var r = FlowJsonExportResult.Failure("ERR", "msg");

            Assert.IsFalse(r.success);
            Assert.AreEqual("ERR", r.diagnostic.errorCode);
            Assert.AreEqual("msg", r.diagnostic.errorMessage);
        }
    }
}
