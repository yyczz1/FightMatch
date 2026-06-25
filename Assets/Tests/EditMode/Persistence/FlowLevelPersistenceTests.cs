using System;
using System.Collections.Generic;
using System.IO;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Editor.Persistence;
using FlowPuzzle.Generation;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FlowPuzzle.Tests.Persistence
{
    [TestFixture]
    public class FlowLevelPersistenceTests
    {
        private const string AssetTestFolder = "Assets/Temp/FlowPuzzleTests";
        private static readonly string JsonTestDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        private static FlowGeneratedLevel MakeTestLevel(float? scoreOverride = null)
        {
            var levelData = new FlowLevelData
            {
                levelId = 1001, width = 4, height = 3,
                difficulty = FlowDifficultyTier.Normal, difficultyScore = 75f
            };
            levelData.pairs.Add(new FlowPairData
            {
                colorId = 0,
                endpointA = new FlowPos(0, 0), endpointB = new FlowPos(3, 0)
            });

            var solutionData = new FlowSolutionData { levelId = 1001 };
            solutionData.paths.Add(new FlowPathData
            {
                colorId = 0,
                cells = new List<FlowPos> { new(0,0), new(1,0), new(2,0), new(3,0) }
            });

            return new FlowGeneratedLevel
            {
                levelData = levelData, solutionData = solutionData,
                difficultyReport = new FlowDifficultyReport
                {
                    difficulty = FlowDifficultyTier.Normal,
                    totalScore = scoreOverride ?? 75f
                },
                usedSeed = 42, coverageRatio = 0.33f
            };
        }

        private static FlowLevelAssetRepository MakeRepo()
        {
            return new FlowLevelAssetRepository(
                new FlowSolutionValidator(), new FlowDifficultyEvaluator());
        }

        [SetUp] public void SetUp() { CleanAssetFolder(); }
        [TearDown] public void TearDown() { CleanAssetFolder(); CleanJsonDir(); }

        private static void CleanAssetFolder()
        {
            if (AssetDatabase.IsValidFolder(AssetTestFolder))
                AssetDatabase.DeleteAsset(AssetTestFolder);
        }
        private static void CleanJsonDir()
        {
            if (Directory.Exists(JsonTestDir))
                Directory.Delete(JsonTestDir, true);
        }

        // ── FlowLevelAsset ──

        [Test]
        public void FlowLevelAsset_InitializesOwnedInstances()
        {
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            Assert.IsNotNull(asset.levelData);
            Assert.IsNotNull(asset.solutionData);
            Assert.IsNotNull(asset.difficultyReport);
            UnityEngine.Object.DestroyImmediate(asset);
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
            Assert.AreEqual(42, asset.generationSeed);
            UnityEngine.Object.DestroyImmediate(asset);
        }

        // ── JSON export ──

        [Test]
        public void Export_RoundTrip_DeserializesLevelAndSolution()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var result = exporter.Export(level, JsonTestDir);
            Assert.IsTrue(result.success);

            var levelJson = File.ReadAllText(result.levelFilePath);
            var solutionJson = File.ReadAllText(result.solutionFilePath);

            var deserializedLevel = JsonUtility.FromJson<FlowLevelData>(levelJson);
            var deserializedSolution = JsonUtility.FromJson<FlowSolutionData>(solutionJson);

            Assert.AreEqual(1001, deserializedLevel.levelId);
            Assert.AreEqual(4, deserializedLevel.width);
            Assert.AreEqual(3, deserializedLevel.height);
            Assert.AreEqual(1, deserializedLevel.pairs.Count);
            Assert.AreEqual(0, deserializedLevel.pairs[0].colorId);
            Assert.AreEqual(0, deserializedLevel.pairs[0].endpointA.x);
            Assert.AreEqual(3, deserializedLevel.pairs[0].endpointB.x);

            Assert.AreEqual(1001, deserializedSolution.levelId);
            Assert.AreEqual(1, deserializedSolution.paths.Count);
            Assert.AreEqual(0, deserializedSolution.paths[0].colorId);
            Assert.AreEqual(4, deserializedSolution.paths[0].cells.Count);
        }

        [Test]
        public void Export_LevelJson_NoPaths_SolutionJson_HasPaths()
        {
            var exporter = new FlowLevelJsonExporter();
            var result = exporter.Export(MakeTestLevel(), JsonTestDir);
            var levelJson = File.ReadAllText(result.levelFilePath);
            var solutionJson = File.ReadAllText(result.solutionFilePath);
            Assert.IsFalse(levelJson.Contains("\"paths\""));
            Assert.IsTrue(solutionJson.Contains("\"paths\""));
        }

        [Test]
        public void Export_NullDifficultyReport_ReturnsIncompleteLevel()
        {
            var level = MakeTestLevel();
            level.difficultyReport = null;
            var result = new FlowLevelJsonExporter().Export(level, JsonTestDir);
            Assert.IsFalse(result.success);
            Assert.AreEqual("IncompleteLevel", result.diagnostic.errorCode);
        }

        [Test]
        public void Export_WhitespaceFolder_ReturnsInvalidOutputPath()
        {
            var result = new FlowLevelJsonExporter().Export(MakeTestLevel(), "   ");
            Assert.IsFalse(result.success);
            Assert.AreEqual("InvalidOutputPath", result.diagnostic.errorCode);
        }

        [Test]
        public void Export_InvalidPathChars_ReturnsFailure()
        {
            var result = new FlowLevelJsonExporter().Export(MakeTestLevel(), "X:\0invalid");
            Assert.IsFalse(result.success);
            Assert.IsNotNull(result.diagnostic);
        }

        [Test]
        public void Export_InputUnchanged()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var snapBefore = JsonUtility.ToJson(level.levelData);
            exporter.Export(level, JsonTestDir);
            var snapAfter = JsonUtility.ToJson(level.levelData);
            Assert.AreEqual(snapBefore, snapAfter);
        }

        // ── Repository: SaveNew ──

        [Test]
        public void Repository_ConstructorNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FlowLevelAssetRepository(null, new FlowDifficultyEvaluator()));
            Assert.Throws<ArgumentNullException>(() => new FlowLevelAssetRepository(new FlowSolutionValidator(), null));
        }

        [Test]
        public void SaveNew_CreatesAsset_ReloadPreservesData()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            var snapshotBefore = JsonUtility.ToJson(level.levelData);

            var asset = repo.SaveNew(level, AssetTestFolder);
            Assert.IsNotNull(asset);
            Assert.AreEqual("Level_1001.asset", Path.GetFileName(AssetDatabase.GetAssetPath(asset)));

            var reloaded = AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(AssetDatabase.GetAssetPath(asset));
            Assert.AreEqual(1001, reloaded.levelData.levelId);
            Assert.AreEqual(4, reloaded.levelData.width);
            Assert.AreEqual(3, reloaded.levelData.height);
            Assert.AreEqual(42, reloaded.generationSeed);

            // input unchanged
            Assert.AreEqual(snapshotBefore, JsonUtility.ToJson(level.levelData));
        }

        [Test]
        public void SaveNew_ListsNotShared()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            var asset = repo.SaveNew(level, AssetTestFolder);

            level.levelData.pairs.Clear();
            Assert.AreEqual(1, asset.levelData.pairs.Count, "Asset pairs should be copied, not shared");
        }

        [Test]
        public void SaveNew_RecommitsDifficultyAndCoverage()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel(scoreOverride: 999f); // stale score

            var asset = repo.SaveNew(level, AssetTestFolder);

            Assert.IsTrue(asset.difficultyReport.totalScore < 999f);
        }

        [Test]
        public void SaveNew_ExistingAsset_Throws()
        {
            var repo = MakeRepo();
            repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<InvalidOperationException>(() => repo.SaveNew(MakeTestLevel(), AssetTestFolder));
        }

        [Test]
        public void SaveNew_InvalidRecommendation_Throws()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            level.solutionData.paths[0].cells.Clear();
            Assert.Throws<InvalidOperationException>(() => repo.SaveNew(level, AssetTestFolder));
        }

        // ── Repository: Overwrite ──

        [Test]
        public void Overwrite_PreservesPath_UpdatesData()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var a2 = MakeTestLevel();
            a2.levelData.width = 99;

            repo.Overwrite(a1, a2);

            var reloaded = AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(AssetDatabase.GetAssetPath(a1));
            Assert.AreEqual(99, reloaded.levelData.width);
        }

        [Test]
        public void Overwrite_LeavesInputUnchanged()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var level = MakeTestLevel();
            var snapBefore = JsonUtility.ToJson(level.levelData);

            repo.Overwrite(a1, level);

            Assert.AreEqual(snapBefore, JsonUtility.ToJson(level.levelData));
        }

        [Test]
        public void Overwrite_Failed_LeavesAssetUnchanged()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var origWidth = a1.levelData.width;
            var invalid = MakeTestLevel();
            invalid.solutionData.paths[0].cells.Clear();

            try { repo.Overwrite(a1, invalid); } catch { }

            Assert.AreEqual(origWidth, a1.levelData.width);
        }

        // ── Repository: SaveAs ──

        [Test]
        public void SaveAs_CreatesNewAsset_LeavesSourceAndInput()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            var snapBefore = JsonUtility.ToJson(level.levelData);
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var sourcePath = AssetDatabase.GetAssetPath(source);

            var saved = repo.SaveAs(source, level, AssetTestFolder, "Renamed");

            Assert.AreNotEqual(source, saved);
            Assert.AreEqual(sourcePath, AssetDatabase.GetAssetPath(source));
            Assert.AreEqual(snapBefore, JsonUtility.ToJson(level.levelData));
            var reloaded = AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(AssetDatabase.GetAssetPath(saved));
            Assert.IsNotNull(reloaded);
        }

        [Test]
        public void SaveAs_Existing_Throws()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<InvalidOperationException>(() => repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "Level_1001"));
        }

        // ── Folder/name safety ──

        [Test]
        public void ValidateFolder_Rejects_AssetsOutside()
        {
            var repo = MakeRepo();
            Assert.Throws<ArgumentException>(() => repo.SaveNew(MakeTestLevel(), "AssetsOutside"));
        }

        [Test]
        public void ValidateFolder_Rejects_Traversal()
        {
            var repo = MakeRepo();
            Assert.Throws<ArgumentException>(() => repo.SaveNew(MakeTestLevel(), "Assets/../Outside"));
        }

        [Test]
        public void ValidateFolder_Rejects_AbsolutePath()
        {
            var repo = MakeRepo();
            Assert.Throws<ArgumentException>(() => repo.SaveNew(MakeTestLevel(), "C:/Something"));
        }

        [Test]
        public void ValidateFolder_CreatesMissingNestedFolders()
        {
            var repo = MakeRepo();
            var nested = AssetTestFolder + "/Sub/Deep";
            CleanAssetFolder(); // ensure fresh
            var asset = repo.SaveNew(MakeTestLevel(), nested);
            Assert.IsNotNull(asset);
            Assert.IsTrue(AssetDatabase.IsValidFolder(nested));
        }

        [Test]
        public void ValidateName_Rejects_Separator()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentException>(() => repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "sub/Bad"));
        }

        [Test]
        public void ValidateName_Rejects_EmptyBaseName()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentException>(() => repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, ".asset"));
        }

        [Test]
        public void ValidateName_NormalizesExtension()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var saved = repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "MyAsset");
            Assert.IsTrue(AssetDatabase.GetAssetPath(saved).EndsWith("MyAsset.asset"));
        }
    }
}
