using System;
using System.Collections.Generic;
using System.IO;
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
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentException("Folder is null or empty.", nameof(folder));
            if (!folder.StartsWith("Assets"))
                throw new ArgumentException("Folder must be under Assets.", nameof(folder));

            ValidateAndPrepare(level);

            var assetName = $"Level_{level.levelData.levelId}.asset";
            var assetPath = Path.Combine(folder, assetName).Replace('\\', '/');

            if (AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(assetPath) != null)
                throw new InvalidOperationException($"Asset already exists at {assetPath}.");

            EnsureFolderExists(folder);

            var asset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            PopulateAsset(asset, level);

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

            ValidateAndPrepare(level);

            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("Asset has no valid path.", nameof(asset));

            Undo.RecordObject(asset, "Overwrite Flow Level");
            PopulateAsset(asset, level);

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
            if (string.IsNullOrEmpty(folder) || !folder.StartsWith("Assets"))
                throw new ArgumentException("Folder must be under Assets.", nameof(folder));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name must not be empty.", nameof(name));

            ValidateAndPrepare(level);

            var fileName = name.EndsWith(".asset") ? name : $"{name}.asset";
            var assetPath = Path.Combine(folder, fileName).Replace('\\', '/');

            if (AssetDatabase.LoadAssetAtPath<FlowLevelAsset>(assetPath) != null)
                throw new InvalidOperationException($"Asset already exists at {assetPath}.");

            EnsureFolderExists(folder);

            var newAsset = ScriptableObject.CreateInstance<FlowLevelAsset>();
            PopulateAsset(newAsset, level);

            AssetDatabase.CreateAsset(newAsset, assetPath);
            EditorUtility.SetDirty(newAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return newAsset;
        }

        private void ValidateAndPrepare(FlowGeneratedLevel level)
        {
            if (level.levelData == null || level.solutionData == null || level.difficultyReport == null)
                throw new InvalidOperationException("Generated level has missing data.");

            var validation = validator.Validate(level.levelData, level.solutionData);
            if (!validation.isValid)
                throw new InvalidOperationException(
                    $"Recommendation is invalid: {validation.errorCode} — {validation.errorMessage}.");

            // Recalculate difficulty
            var report = difficultyEvaluator.Evaluate(level.levelData, level.solutionData);

            // Sync into level data
            level.levelData.difficulty = report.difficulty;
            level.levelData.difficultyScore = report.totalScore;

            // Recalculate coverage
            var board = new FlowBoard(level.levelData.width, level.levelData.height);
            foreach (var path in level.solutionData.paths)
                foreach (var cell in path.cells)
                    board.Set(cell, path.colorId);

            level.coverageRatio = (float)board.OccupiedCellCount
                / (level.levelData.width * level.levelData.height);
            level.difficultyReport = report;
        }

        private static void PopulateAsset(FlowLevelAsset asset, FlowGeneratedLevel level)
        {
            asset.levelData = DeepCopy(level.levelData);
            asset.solutionData = DeepCopy(level.solutionData);
            asset.difficultyReport = DeepCopy(level.difficultyReport);
            asset.generationSeed = level.usedSeed;
            asset.coverageRatio = level.coverageRatio;
        }

        private static FlowLevelData DeepCopy(FlowLevelData src)
        {
            var copy = new FlowLevelData
            {
                levelId = src.levelId,
                width = src.width,
                height = src.height,
                difficulty = src.difficulty,
                difficultyScore = src.difficultyScore
            };
            foreach (var p in src.pairs)
                copy.pairs.Add(new FlowPairData
                {
                    colorId = p.colorId,
                    endpointA = p.endpointA,
                    endpointB = p.endpointB
                });
            return copy;
        }

        private static FlowSolutionData DeepCopy(FlowSolutionData src)
        {
            var copy = new FlowSolutionData { levelId = src.levelId };
            foreach (var p in src.paths)
            {
                var cellsCopy = new List<FlowPos>(p.cells.Count);
                foreach (var c in p.cells)
                    cellsCopy.Add(new FlowPos(c.x, c.y));
                copy.paths.Add(new FlowPathData
                {
                    colorId = p.colorId,
                    cells = cellsCopy
                });
            }
            return copy;
        }

        private static FlowDifficultyReport DeepCopy(FlowDifficultyReport src)
        {
            return new FlowDifficultyReport
            {
                difficulty = src.difficulty,
                totalScore = src.totalScore,
                boardSizeScore = src.boardSizeScore,
                colorCountScore = src.colorCountScore,
                coverageScore = src.coverageScore,
                turnScore = src.turnScore,
                detourScore = src.detourScore,
                interactionScore = src.interactionScore,
                endpointDistanceScore = src.endpointDistanceScore,
                bottleneckScore = src.bottleneckScore,
                totalTurnCount = src.totalTurnCount,
                totalDetour = src.totalDetour,
                differentColorAdjacentCount = src.differentColorAdjacentCount,
                totalEndpointManhattanDistance = src.totalEndpointManhattanDistance,
                bottleneckCount = src.bottleneckCount
            };
        }

        private static void EnsureFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            var parts = folder.Split('/');
            var current = parts[0]; // "Assets"
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
