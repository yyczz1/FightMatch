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
        private bool tempExistedBefore;

        private static FlowGeneratedLevel MakeTestLevel()
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
                    difficulty = FlowDifficultyTier.Normal, totalScore = 75f
                },
                usedSeed = 42, coverageRatio = 0.33f
            };
        }

        private static FlowLevelAssetRepository MakeRepo()
        {
            return new FlowLevelAssetRepository(
                new FlowSolutionValidator(), new FlowDifficultyEvaluator());
        }

        private static string SnapshotLevel(FlowGeneratedLevel l)
        {
            return JsonUtility.ToJson(new
            {
                l.usedSeed, l.coverageRatio,
                level = JsonUtility.ToJson(l.levelData),
                solution = JsonUtility.ToJson(l.solutionData),
                report = JsonUtility.ToJson(l.difficultyReport)
            });
        }

        [SetUp]
        public void SetUp()
        {
            tempExistedBefore = AssetDatabase.IsValidFolder("Assets/Temp");
            CleanAssetFolder();
        }

        [TearDown]
        public void TearDown()
        {
            CleanAssetFolder();
            if (!tempExistedBefore && AssetDatabase.IsValidFolder("Assets/Temp"))
            {
                var children = AssetDatabase.GetSubFolders("Assets/Temp");
                if (children.Length == 0)
                    AssetDatabase.DeleteAsset("Assets/Temp");
            }
            CleanJsonDir();
        }

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
        public void Asset_InitializesOwnedInstances()
        {
            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            Assert.IsNotNull(asset.levelData);
            Assert.IsNotNull(asset.solutionData);
            Assert.IsNotNull(asset.difficultyReport);
            UnityEngine.Object.DestroyImmediate(asset);
        }

        // ── JSON round-trip (exact DTO) ──

        [Test]
        public void Export_RoundTrip_ExactDtoComparison()
        {
            var exporter = new FlowLevelJsonExporter();
            var level = MakeTestLevel();
            var result = exporter.Export(level, JsonTestDir);
            Assert.IsTrue(result.success);

            var levelJson = File.ReadAllText(result.levelFilePath);
            var solutionJson = File.ReadAllText(result.solutionFilePath);

            var desLvl = JsonUtility.FromJson<FlowLevelData>(levelJson);
            var desSol = JsonUtility.FromJson<FlowSolutionData>(solutionJson);

            Assert.AreEqual(1001, desLvl.levelId);
            Assert.AreEqual(4, desLvl.width);
            Assert.AreEqual(3, desLvl.height);
            Assert.AreEqual(1, desLvl.pairs.Count);
            Assert.AreEqual(0, desLvl.pairs[0].colorId);
            Assert.AreEqual(0, desLvl.pairs[0].endpointA.x);
            Assert.AreEqual(0, desLvl.pairs[0].endpointA.y);
            Assert.AreEqual(3, desLvl.pairs[0].endpointB.x);
            Assert.AreEqual(0, desLvl.pairs[0].endpointB.y);

            Assert.AreEqual(1001, desSol.levelId);
            Assert.AreEqual(1, desSol.paths.Count);
            Assert.AreEqual(0, desSol.paths[0].colorId);
            Assert.AreEqual(4, desSol.paths[0].cells.Count);
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(level.solutionData.paths[0].cells[i].x,
                    desSol.paths[0].cells[i].x);
                Assert.AreEqual(level.solutionData.paths[0].cells[i].y,
                    desSol.paths[0].cells[i].y);
            }
        }

        [Test]
        public void Export_LevelJson_NoPaths_SolutionJson_HasPaths()
        {
            var exporter = new FlowLevelJsonExporter();
            var result = exporter.Export(MakeTestLevel(), JsonTestDir);
            Assert.IsFalse(File.ReadAllText(result.levelFilePath).Contains("\"paths\""));
            Assert.IsTrue(File.ReadAllText(result.solutionFilePath).Contains("\"paths\""));
        }

        [Test]
        public void Export_NullReport_IncompleteLevel()
        {
            var level = MakeTestLevel(); level.difficultyReport = null;
            var r = new FlowLevelJsonExporter().Export(level, JsonTestDir);
            Assert.IsFalse(r.success);
            Assert.AreEqual("IncompleteLevel", r.diagnostic.errorCode);
        }

        [Test]
        public void Export_Whitespace_InvalidOutputPath()
        {
            var r = new FlowLevelJsonExporter().Export(MakeTestLevel(), "   ");
            Assert.IsFalse(r.success);
            Assert.AreEqual("InvalidOutputPath", r.diagnostic.errorCode);
        }

        [Test]
        public void Export_InputUnchanged()
        {
            var level = MakeTestLevel();
            var snap = SnapshotLevel(level);
            new FlowLevelJsonExporter().Export(level, JsonTestDir);
            Assert.AreEqual(snap, SnapshotLevel(level));
        }

        // ── Repository: SaveNew ──

        [Test]
        public void SaveNew_CreatesAndReloads()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            var snap = SnapshotLevel(level);

            var asset = repo.SaveNew(level, AssetTestFolder);
            Assert.AreEqual(snap, SnapshotLevel(level)); // input unchanged

            var path = AssetDatabase.GetAssetPath(asset);
            Assert.IsTrue(path.StartsWith("Assets/Temp/FlowPuzzleTests/"));
            Assert.IsTrue(path.EndsWith("Level_1001.asset"));

            var reloaded = AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(path);
            Assert.AreEqual(1001, reloaded.levelData.levelId);
            Assert.AreEqual(1001, reloaded.solutionData.levelId);
            Assert.AreEqual(4, reloaded.levelData.width);
            Assert.AreEqual(3, reloaded.levelData.height);
            Assert.AreEqual(42, reloaded.generationSeed);
            Assert.AreEqual(4f / 12f, reloaded.coverageRatio, 0.001f);
            Assert.AreEqual(reloaded.difficultyReport.difficulty, reloaded.levelData.difficulty);
            Assert.AreEqual(reloaded.difficultyReport.totalScore, reloaded.levelData.difficultyScore, 0.001f);
            Assert.AreEqual(1, reloaded.levelData.pairs.Count);
            Assert.AreEqual(0, reloaded.levelData.pairs[0].colorId);
            Assert.AreEqual(1, reloaded.solutionData.paths.Count);
            Assert.AreEqual(4, reloaded.solutionData.paths[0].cells.Count);
        }

        [Test]
        public void SaveNew_ListsNotShared()
        {
            var level = MakeTestLevel();
            var asset = MakeRepo().SaveNew(level, AssetTestFolder);
            level.levelData.pairs.Clear();
            Assert.AreEqual(1, asset.levelData.pairs.Count);
        }

        [Test]
        public void SaveNew_RecalculatesCoverage()
        {
            var level = MakeTestLevel();
            var asset = MakeRepo().SaveNew(level, AssetTestFolder);
            Assert.AreEqual(4f / 12f, asset.coverageRatio, 0.001f);
        }

        [Test]
        public void SaveNew_DifficultyRecalculated()
        {
            var level = MakeTestLevel();
            var freshEvaluator = new FlowDifficultyEvaluator();
            var fresh = freshEvaluator.Evaluate(level.levelData, level.solutionData);

            var asset = MakeRepo().SaveNew(level, AssetTestFolder);
            Assert.AreEqual(fresh.difficulty, asset.difficultyReport.difficulty);
            Assert.AreEqual(fresh.totalScore, asset.difficultyReport.totalScore, 0.001f);
        }

        [Test]
        public void SaveNew_Existing_Throws()
        {
            var repo = MakeRepo();
            repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<InvalidOperationException>(() => repo.SaveNew(MakeTestLevel(), AssetTestFolder));
        }

        [Test]
        public void SaveNew_InvalidRecommendation_Throws()
        {
            var level = MakeTestLevel();
            level.solutionData.paths[0].cells.Clear();
            Assert.Throws<InvalidOperationException>(() => MakeRepo().SaveNew(level, AssetTestFolder));
        }

        // ── Repository: Overwrite ──

        [Test]
        public void Overwrite_UpdatesData_LeavesInput()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var level = MakeTestLevel(); level.levelData.width = 99;
            var snap = SnapshotLevel(level);

            repo.Overwrite(a1, level);
            Assert.AreEqual(snap, SnapshotLevel(level));

            var reloaded = AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(AssetDatabase.GetAssetPath(a1));
            Assert.AreEqual(99, reloaded.levelData.width);
        }

        [Test]
        public void Overwrite_Failed_AssetUnchanged()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var origWidth = a1.levelData.width;
            var invalid = MakeTestLevel(); invalid.solutionData.paths[0].cells.Clear();
            try { repo.Overwrite(a1, invalid); } catch { }
            Assert.AreEqual(origWidth, a1.levelData.width);
        }

        // ── Repository: SaveAs ──

        [Test]
        public void SaveAs_CreatesNew_LeavesSourceAndInput()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            var snap = SnapshotLevel(level);
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var sourcePath = AssetDatabase.GetAssetPath(source);

            var saved = repo.SaveAs(source, level, AssetTestFolder, "Renamed");
            Assert.AreEqual(snap, SnapshotLevel(level));
            Assert.AreNotEqual(source, saved);
            Assert.AreEqual(sourcePath, AssetDatabase.GetAssetPath(source));
            Assert.IsTrue(AssetDatabase.GetAssetPath(saved).EndsWith("Renamed.asset"));
        }

        [Test]
        public void SaveAs_BackslashFolder_Normalized()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var nested = AssetTestFolder + "\\Sub\\Deep";
            var saved = repo.SaveAs(source, MakeTestLevel(), nested, "MyAsset");
            Assert.IsTrue(AssetDatabase.GetAssetPath(saved).Contains("Assets/Temp/FlowPuzzleTests/Sub/Deep/"));
            Assert.IsTrue(AssetDatabase.IsValidFolder(AssetTestFolder + "/Sub/Deep"));
        }

        [Test]
        public void SaveAs_ExtensionNormalization()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);

            var r1 = repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "A.asset");
            Assert.IsTrue(AssetDatabase.GetAssetPath(r1).EndsWith("A.asset"));
            var r2 = repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "B.ASSET");
            Assert.IsTrue(AssetDatabase.GetAssetPath(r2).EndsWith("B.asset"));
            var r3 = repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "C.asset.asset");
            Assert.IsTrue(AssetDatabase.GetAssetPath(r3).EndsWith("C.asset"));
        }

        [Test]
        public void SaveAs_Rejects_NakedExtension()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentException>(() => repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, ".asset"));
            Assert.Throws<ArgumentException>(() => repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, ".asset.asset"));
        }

        // ── Folder safety ──

        [Test]
        public void Folder_Rejects_AssetsOutside() => Assert.Throws<ArgumentException>(() => MakeRepo().SaveNew(MakeTestLevel(), "AssetsOutside"));
        [Test]
        public void Folder_Rejects_Traversal() => Assert.Throws<ArgumentException>(() => MakeRepo().SaveNew(MakeTestLevel(), "Assets/../Outside"));
        [Test]
        public void Folder_Rejects_Absolute() => Assert.Throws<ArgumentException>(() => MakeRepo().SaveNew(MakeTestLevel(), "C:/Something"));

        [Test]
        public void Folder_CreatesMissingNested()
        {
            var repo = MakeRepo();
            CleanAssetFolder();
            var asset = repo.SaveNew(MakeTestLevel(), AssetTestFolder + "/Sub/Deep");
            Assert.IsNotNull(asset);
            Assert.IsTrue(AssetDatabase.IsValidFolder(AssetTestFolder + "/Sub/Deep"));
        }

        [Test]
        public void Name_Rejects_Separator()
        {
            var repo = MakeRepo();
            var s = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentException>(() => repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, "sub/Bad"));
        }

        [Test]
        public void NullConstructor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FlowLevelAssetRepository(null, new FlowDifficultyEvaluator()));
        }
    }
}
