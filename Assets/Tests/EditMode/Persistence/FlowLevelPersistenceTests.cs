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

        private struct LevelSnapshot
        {
            public int usedSeed;
            public float coverageRatio;
            public string levelJson;
            public string solutionJson;
            public string reportJson;
        }

        private static LevelSnapshot TakeSnapshot(FlowGeneratedLevel l)
        {
            return new LevelSnapshot
            {
                usedSeed = l.usedSeed,
                coverageRatio = l.coverageRatio,
                levelJson = JsonUtility.ToJson(l.levelData),
                solutionJson = JsonUtility.ToJson(l.solutionData),
                reportJson = JsonUtility.ToJson(l.difficultyReport)
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
                difficultyReport = new FlowDifficultyReport { difficulty = FlowDifficultyTier.Normal, totalScore = 75f },
                usedSeed = 42, coverageRatio = 0.33f
            };
        }

        private static FlowLevelAssetRepository MakeRepo()
        {
            return new FlowLevelAssetRepository(new FlowSolutionValidator(), new FlowDifficultyEvaluator());
        }

        [SetUp] public void SetUp() { tempExistedBefore = AssetDatabase.IsValidFolder("Assets/Temp"); CleanAssetFolder(); }
        [TearDown] public void TearDown()
        {
            CleanAssetFolder();
            if (!tempExistedBefore && AssetDatabase.IsValidFolder("Assets/Temp") && AssetDatabase.GetSubFolders("Assets/Temp").Length == 0)
                AssetDatabase.DeleteAsset("Assets/Temp");
            if (Directory.Exists(JsonTestDir)) Directory.Delete(JsonTestDir, true);
        }
        private static void CleanAssetFolder() { if (AssetDatabase.IsValidFolder(AssetTestFolder)) AssetDatabase.DeleteAsset(AssetTestFolder); }

        // ── FlowLevelAsset in-memory ──

        [Test]
        public void Asset_InitializesOwnedInstances()
        {
            var a = ScriptableObject.CreateInstance<FlowLevelAsset>();
            Assert.IsNotNull(a.levelData); Assert.IsNotNull(a.solutionData); Assert.IsNotNull(a.difficultyReport);
            UnityEngine.Object.DestroyImmediate(a);
        }

        [Test]
        public void Asset_AssignData_PreservesAllFields()
        {
            var level = MakeTestLevel();
            var a = ScriptableObject.CreateInstance<FlowLevelAsset>();
            a.levelData = level.levelData; a.solutionData = level.solutionData;
            a.difficultyReport = level.difficultyReport; a.generationSeed = level.usedSeed;
            a.coverageRatio = level.coverageRatio;
            Assert.AreEqual(1001, a.levelData.levelId);
            Assert.AreEqual(4, a.levelData.width); Assert.AreEqual(3, a.levelData.height);
            Assert.AreEqual(42, a.generationSeed); Assert.AreEqual(0.33f, a.coverageRatio, 0.01f);
            Assert.AreEqual(FlowDifficultyTier.Normal, a.difficultyReport.difficulty);
            Assert.AreEqual(1, a.solutionData.paths.Count);
            UnityEngine.Object.DestroyImmediate(a);
        }

        // ── JSON export ──

        [Test]
        public void Export_RoundTrip_ExactDtoComparison()
        {
            var level = MakeTestLevel();
            var r = new FlowLevelJsonExporter().Export(level, JsonTestDir);
            Assert.IsTrue(r.success);

            var lvl = JsonUtility.FromJson<FlowLevelData>(File.ReadAllText(r.levelFilePath));
            var sol = JsonUtility.FromJson<FlowSolutionData>(File.ReadAllText(r.solutionFilePath));
            Assert.AreEqual(1001, lvl.levelId); Assert.AreEqual(4, lvl.width); Assert.AreEqual(3, lvl.height);
            Assert.AreEqual(1, lvl.pairs.Count); Assert.AreEqual(0, lvl.pairs[0].colorId);
            Assert.AreEqual(0, lvl.pairs[0].endpointA.x); Assert.AreEqual(0, lvl.pairs[0].endpointA.y);
            Assert.AreEqual(3, lvl.pairs[0].endpointB.x); Assert.AreEqual(0, lvl.pairs[0].endpointB.y);
            Assert.AreEqual(1001, sol.levelId); Assert.AreEqual(1, sol.paths.Count);
            Assert.AreEqual(0, sol.paths[0].colorId); Assert.AreEqual(4, sol.paths[0].cells.Count);
            for (var i = 0; i < 4; i++)
            {
                Assert.AreEqual(level.solutionData.paths[0].cells[i].x, sol.paths[0].cells[i].x);
                Assert.AreEqual(level.solutionData.paths[0].cells[i].y, sol.paths[0].cells[i].y);
            }
        }

        [Test]
        public void Export_LevelNoPaths_SolutionHasPaths()
        {
            var r = new FlowLevelJsonExporter().Export(MakeTestLevel(), JsonTestDir);
            Assert.IsFalse(File.ReadAllText(r.levelFilePath).Contains("\"paths\""));
            Assert.IsTrue(File.ReadAllText(r.solutionFilePath).Contains("\"paths\""));
        }

        [Test]
        public void Export_NullReport_IncompleteLevel()
        {
            var l = MakeTestLevel(); l.difficultyReport = null;
            var r = new FlowLevelJsonExporter().Export(l, JsonTestDir);
            Assert.IsFalse(r.success); Assert.AreEqual("IncompleteLevel", r.diagnostic.errorCode);
        }

        [Test]
        public void Export_Whitespace_InvalidOutputPath()
        {
            var r = new FlowLevelJsonExporter().Export(MakeTestLevel(), "   ");
            Assert.IsFalse(r.success); Assert.AreEqual("InvalidOutputPath", r.diagnostic.errorCode);
        }

        [Test]
        public void Export_InvalidPathChars_ReturnsFailure()
        {
            var r = new FlowLevelJsonExporter().Export(MakeTestLevel(), "X:\0invalid");
            Assert.IsFalse(r.success); Assert.IsNotNull(r.diagnostic);
        }

        [Test]
        public void Export_InputUnchanged()
        {
            var level = MakeTestLevel();
            var snap = TakeSnapshot(level);
            new FlowLevelJsonExporter().Export(level, JsonTestDir);
            AssertUnchanged(snap, level);
        }

        // ── Repository: SaveNew ──

        [Test]
        public void SaveNew_NullLevel_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => MakeRepo().SaveNew(null, AssetTestFolder));
        }

        [Test]
        public void SaveNew_CreatesAndReloads()
        {
            var level = MakeTestLevel();
            var snap = TakeSnapshot(level);
            var asset = MakeRepo().SaveNew(level, AssetTestFolder);
            AssertUnchanged(snap, level);

            var path = AssetDatabase.GetAssetPath(asset);
            Assert.IsTrue(path.StartsWith("Assets/Temp/FlowPuzzleTests/"));
            Assert.IsTrue(path.EndsWith("Level_1001.asset"));

            var reloaded = AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(path);
            Assert.AreEqual(1001, reloaded.levelData.levelId); Assert.AreEqual(1001, reloaded.solutionData.levelId);
            Assert.AreEqual(4, reloaded.levelData.width); Assert.AreEqual(3, reloaded.levelData.height);
            Assert.AreEqual(42, reloaded.generationSeed);
            Assert.AreEqual(4f / 12f, reloaded.coverageRatio, 0.001f);
            Assert.AreEqual(reloaded.difficultyReport.difficulty, reloaded.levelData.difficulty);
            Assert.AreEqual(reloaded.difficultyReport.totalScore, reloaded.levelData.difficultyScore, 0.001f);
            Assert.AreEqual(1, reloaded.levelData.pairs.Count); Assert.AreEqual(0, reloaded.levelData.pairs[0].colorId);
            Assert.AreEqual(1, reloaded.solutionData.paths.Count); Assert.AreEqual(4, reloaded.solutionData.paths[0].cells.Count);
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
        public void SaveNew_RecalculatesCoverage() => Assert.AreEqual(4f / 12f, MakeRepo().SaveNew(MakeTestLevel(), AssetTestFolder).coverageRatio, 0.001f);

        [Test]
        public void SaveNew_DifficultyRecalculated()
        {
            var level = MakeTestLevel();
            var fresh = new FlowDifficultyEvaluator().Evaluate(level.levelData, level.solutionData);
            var asset = MakeRepo().SaveNew(level, AssetTestFolder);
            Assert.AreEqual(fresh.difficulty, asset.difficultyReport.difficulty);
            Assert.AreEqual(fresh.totalScore, asset.difficultyReport.totalScore, 0.001f);
        }

        [Test]
        public void SaveNew_Existing_Throws()
        {
            var repo = MakeRepo(); repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<InvalidOperationException>(() => repo.SaveNew(MakeTestLevel(), AssetTestFolder));
        }

        [Test]
        public void SaveNew_InvalidRecommendation_Throws()
        {
            var level = MakeTestLevel(); level.solutionData.paths[0].cells.Clear();
            Assert.Throws<InvalidOperationException>(() => MakeRepo().SaveNew(level, AssetTestFolder));
        }

        // ── Repository: Overwrite ──

        [Test]
        public void Overwrite_NullAsset_Throws() => Assert.Throws<ArgumentNullException>(() => MakeRepo().Overwrite(null, MakeTestLevel()));
        [Test]
        public void Overwrite_NullLevel_Throws()
        {
            var a = MakeRepo().SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentNullException>(() => MakeRepo().Overwrite(a, null));
        }

        [Test]
        public void Overwrite_UpdatesData_LeavesInput()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var level = MakeTestLevel(); level.levelData.width = 99;
            var snap = TakeSnapshot(level);
            repo.Overwrite(a1, level);
            AssertUnchanged(snap, level);
            Assert.AreEqual(99, AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(AssetDatabase.GetAssetPath(a1)).levelData.width);
        }

        [Test]
        public void Overwrite_Failed_ThrowsAndAssetUnchanged()
        {
            var repo = MakeRepo();
            var a1 = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var origWidth = a1.levelData.width;
            var invalid = MakeTestLevel(); invalid.solutionData.paths[0].cells.Clear();
            Assert.Throws<InvalidOperationException>(() => repo.Overwrite(a1, invalid));
            Assert.AreEqual(origWidth, a1.levelData.width);
        }

        // ── Repository: SaveAs ──

        [Test]
        public void SaveAs_NullSource_Throws() => Assert.Throws<ArgumentNullException>(() => MakeRepo().SaveAs(null, MakeTestLevel(), AssetTestFolder, "X"));
        [Test]
        public void SaveAs_NullLevel_Throws()
        {
            var s = MakeRepo().SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentNullException>(() => MakeRepo().SaveAs(s, null, AssetTestFolder, "X"));
        }

        [Test]
        public void SaveAs_CreatesNew_LeavesSourceAndInput()
        {
            var repo = MakeRepo();
            var level = MakeTestLevel();
            var snap = TakeSnapshot(level);
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var sourcePath = AssetDatabase.GetAssetPath(source);

            var saved = repo.SaveAs(source, level, AssetTestFolder, "Renamed");
            AssertUnchanged(snap, level);
            Assert.AreNotEqual(source, saved);
            Assert.AreEqual(sourcePath, AssetDatabase.GetAssetPath(source));
            Assert.IsTrue(AssetDatabase.GetAssetPath(saved).EndsWith("Renamed.asset"));
        }

        [Test]
        public void SaveAs_SourceUnchanged()
        {
            var repo = MakeRepo();
            var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var origWidth = source.levelData.width; var origSeed = source.generationSeed; var origCov = source.coverageRatio;
            repo.SaveAs(source, MakeTestLevel(), AssetTestFolder, "Cloned");
            Assert.AreEqual(origWidth, source.levelData.width); Assert.AreEqual(origSeed, source.generationSeed);
            Assert.AreEqual(origCov, source.coverageRatio);
        }

        [Test]
        public void SaveAs_Existing_Throws()
        {
            var repo = MakeRepo();
            var s = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<InvalidOperationException>(() => repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, "Level_1001"));
        }

        [Test]
        public void SaveAs_BackslashFolder_Normalized()
        {
            var repo = MakeRepo(); var source = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            var saved = repo.SaveAs(source, MakeTestLevel(), AssetTestFolder + "\\Sub\\Deep", "MyAsset");
            Assert.IsTrue(AssetDatabase.GetAssetPath(saved).Contains("Assets/Temp/FlowPuzzleTests/Sub/Deep/"));
            Assert.IsTrue(AssetDatabase.IsValidFolder(AssetTestFolder + "/Sub/Deep"));
        }

        [Test]
        public void SaveAs_ExtensionNormalization()
        {
            var repo = MakeRepo(); var s = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.IsTrue(AssetDatabase.GetAssetPath(repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, "A.asset")).EndsWith("A.asset"));
            Assert.IsTrue(AssetDatabase.GetAssetPath(repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, "B.ASSET")).EndsWith("B.asset"));
            Assert.IsTrue(AssetDatabase.GetAssetPath(repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, "C.asset.asset")).EndsWith("C.asset"));
        }

        [Test]
        public void SaveAs_Rejects_NakedExtension()
        {
            var repo = MakeRepo(); var s = repo.SaveNew(MakeTestLevel(), AssetTestFolder);
            Assert.Throws<ArgumentException>(() => repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, ".asset"));
            Assert.Throws<ArgumentException>(() => repo.SaveAs(s, MakeTestLevel(), AssetTestFolder, ".asset.asset"));
        }

        // ── Folder/name safety ──

        [Test] public void Folder_Rejects_AssetsOutside() => Assert.Throws<ArgumentException>(() => MakeRepo().SaveNew(MakeTestLevel(), "AssetsOutside"));
        [Test] public void Folder_Rejects_Traversal() => Assert.Throws<ArgumentException>(() => MakeRepo().SaveNew(MakeTestLevel(), "Assets/../Outside"));
        [Test] public void Folder_Rejects_Absolute() => Assert.Throws<ArgumentException>(() => MakeRepo().SaveNew(MakeTestLevel(), "C:/Something"));
        [Test] public void Folder_CreatesMissingNested() { CleanAssetFolder(); var a = MakeRepo().SaveNew(MakeTestLevel(), AssetTestFolder + "/Sub/Deep"); Assert.IsNotNull(a); Assert.IsTrue(AssetDatabase.IsValidFolder(AssetTestFolder + "/Sub/Deep")); }
        [Test] public void Name_Rejects_Separator() { var s = MakeRepo().SaveNew(MakeTestLevel(), AssetTestFolder); Assert.Throws<ArgumentException>(() => MakeRepo().SaveAs(s, MakeTestLevel(), AssetTestFolder, "sub/Bad")); }
        [Test] public void NullConstructor_Throws() { Assert.Throws<ArgumentNullException>(() => new FlowLevelAssetRepository(null, new FlowDifficultyEvaluator())); Assert.Throws<ArgumentNullException>(() => new FlowLevelAssetRepository(new FlowSolutionValidator(), null)); }
    }
}
