using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlowPuzzle.Core;
using FlowPuzzle.Difficulty;
using FlowPuzzle.Persistence;
using FlowPuzzle.Validation;
using UnityEditor;
using UnityEngine;

namespace FlowPuzzle.Editor.Persistence
{
    public sealed class FlowLevelAssetRepository
    {
        private readonly FlowSolutionValidator validator;
        private readonly FlowDifficultyEvaluator difficultyEvaluator;

        public FlowLevelAssetRepository(
            FlowSolutionValidator validator,
            FlowDifficultyEvaluator difficultyEvaluator)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.difficultyEvaluator = difficultyEvaluator
                ?? throw new ArgumentNullException(nameof(difficultyEvaluator));
        }

        public FlowLevelAsset SaveNew(
            FlowGeneratedLevel level,
            string folder)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            var normalized = NormalizeFolder(folder);

            var canonical = BuildCanonical(level);

            var assetName = $"Level_{canonical.levelData.levelId}.asset";
            var assetPath = normalized + "/" + assetName;

            if (AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(assetPath) != null)
                throw new InvalidOperationException($"Asset already exists at {assetPath}.");

            EnsureFolderExists(normalized);

            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            PopulateAsset(asset, canonical);

            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }

        public void Overwrite(
            FlowLevelAsset asset,
            FlowGeneratedLevel level)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            var canonical = BuildCanonical(level);

            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("Asset has no valid path.", nameof(asset));

            Undo.RecordObject(asset, "Overwrite Flow Level");
            PopulateAsset(asset, canonical);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public FlowLevelAsset SaveAs(
            FlowLevelAsset source,
            FlowGeneratedLevel level,
            string folder,
            string name)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (level == null)
                throw new ArgumentNullException(nameof(level));
            var normalizedFolder = NormalizeFolder(folder);
            var normalizedName = NormalizeAssetName(name);

            var canonical = BuildCanonical(level);

            var assetPath = normalizedFolder + "/" + normalizedName;

            if (AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(assetPath) != null)
                throw new InvalidOperationException($"Asset already exists at {assetPath}.");

            EnsureFolderExists(normalizedFolder);

            var newAsset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            PopulateAsset(newAsset, canonical);

            AssetDatabase.CreateAsset(newAsset, assetPath);
            EditorUtility.SetDirty(newAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return newAsset;
        }

        private FlowGeneratedLevel BuildCanonical(FlowGeneratedLevel level)
        {
            if (level.levelData == null || level.solutionData == null || level.difficultyReport == null)
                throw new InvalidOperationException("Generated level has missing data.");

            var validation = validator.Validate(level.levelData, level.solutionData);
            if (!validation.isValid)
                throw new InvalidOperationException(
                    $"Recommendation is invalid: {validation.errorCode} — {validation.errorMessage}.");

            var report = difficultyEvaluator.Evaluate(level.levelData, level.solutionData);

            var board = new FlowBoard(level.levelData.width, level.levelData.height);
            foreach (var path in level.solutionData.paths)
                foreach (var cell in path.cells)
                    board.Set(cell, path.colorId);
            var coverage = (float)board.OccupiedCellCount
                / (level.levelData.width * level.levelData.height);

            var levelData = DeepCopyLevelData(level.levelData);
            levelData.difficulty = report.difficulty;
            levelData.difficultyScore = report.totalScore;

            return new FlowGeneratedLevel
            {
                levelData = levelData,
                solutionData = DeepCopySolutionData(level.solutionData),
                difficultyReport = DeepCopyReport(report),
                usedSeed = level.usedSeed,
                coverageRatio = coverage
            };
        }

        private static void PopulateAsset(FlowLevelAsset asset, FlowGeneratedLevel canonical)
        {
            asset.levelData = canonical.levelData;
            asset.solutionData = canonical.solutionData;
            asset.difficultyReport = canonical.difficultyReport;
            asset.generationSeed = canonical.usedSeed;
            asset.coverageRatio = canonical.coverageRatio;
        }

        private static string NormalizeFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentException("Folder is null or empty.", nameof(folder));

            var normalized = folder.Replace('\\', '/');

            if (normalized != "Assets" && !normalized.StartsWith("Assets/"))
                throw new ArgumentException("Folder must be exactly Assets or begin with Assets/.", nameof(folder));

            var segments = normalized.Split('/');
            foreach (var seg in segments)
            {
                if (string.IsNullOrEmpty(seg))
                    throw new ArgumentException("Folder contains empty path segment.", nameof(folder));
                if (seg == "." || seg == "..")
                    throw new ArgumentException("Folder must not contain . or .. segments.", nameof(folder));
            }

            return normalized;
        }

        private static string NormalizeAssetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty or whitespace.", nameof(name));

            var invalidChars = Path.GetInvalidFileNameChars();
            if (name.Any(c => invalidChars.Contains(c)))
                throw new ArgumentException("Name contains invalid characters.", nameof(name));

            var normalized = name.Replace('\\', '/');
            if (normalized.Contains("/") || normalized.Contains(".."))
                throw new ArgumentException("Name must not contain path separators or traversal.", nameof(name));

            // Strip all trailing .asset suffixes (case-insensitive)
            while (normalized.Length >= 6 &&
                   string.Equals(normalized.Substring(normalized.Length - 6), ".asset",
                       StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(0, normalized.Length - 6);

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("Asset base name must not be empty.", nameof(name));

            return normalized + ".asset";
        }

        private static FlowLevelData DeepCopyLevelData(FlowLevelData src)
        {
            var copy = new FlowLevelData
            {
                levelId = src.levelId, width = src.width, height = src.height,
                difficulty = src.difficulty, difficultyScore = src.difficultyScore
            };
            foreach (var p in src.pairs)
                copy.pairs.Add(new FlowPairData
                {
                    colorId = p.colorId,
                    endpointA = new FlowPos(p.endpointA.x, p.endpointA.y),
                    endpointB = new FlowPos(p.endpointB.x, p.endpointB.y)
                });
            return copy;
        }

        private static FlowSolutionData DeepCopySolutionData(FlowSolutionData src)
        {
            var copy = new FlowSolutionData { levelId = src.levelId };
            foreach (var p in src.paths)
            {
                var cellsCopy = new List<FlowPos>(p.cells.Count);
                foreach (var c in p.cells)
                    cellsCopy.Add(new FlowPos(c.x, c.y));
                copy.paths.Add(new FlowPathData { colorId = p.colorId, cells = cellsCopy });
            }
            return copy;
        }

        private static FlowDifficultyReport DeepCopyReport(FlowDifficultyReport src)
        {
            return new FlowDifficultyReport
            {
                difficulty = src.difficulty, totalScore = src.totalScore,
                boardSizeScore = src.boardSizeScore, colorCountScore = src.colorCountScore,
                coverageScore = src.coverageScore, turnScore = src.turnScore,
                detourScore = src.detourScore, interactionScore = src.interactionScore,
                endpointDistanceScore = src.endpointDistanceScore,
                bottleneckScore = src.bottleneckScore,
                totalTurnCount = src.totalTurnCount, totalDetour = src.totalDetour,
                differentColorAdjacentCount = src.differentColorAdjacentCount,
                totalEndpointManhattanDistance = src.totalEndpointManhattanDistance,
                bottleneckCount = src.bottleneckCount
            };
        }

        private static void EnsureFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
