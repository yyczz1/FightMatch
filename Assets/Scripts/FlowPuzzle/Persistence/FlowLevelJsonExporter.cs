using System.IO;
using FlowPuzzle.Core;
using UnityEngine;

namespace FlowPuzzle.Persistence
{
    public sealed class FlowLevelJsonExporter
    {
        public FlowJsonExportResult Export(
            FlowGeneratedLevel level,
            string outputFolder)
        {
            if (level == null || level.levelData == null || level.solutionData == null)
                return FlowJsonExportResult.Failure(
                    "IncompleteLevel", "Generated level is null or has missing data.");

            if (string.IsNullOrEmpty(outputFolder))
                return FlowJsonExportResult.Failure(
                    "InvalidOutputPath", "Output folder is null or empty.");

            try
            {
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                var levelId = level.levelData.levelId;
                var levelFileName = $"level_{levelId}.json";
                var solutionFileName = $"solution_{levelId}.json";

                var levelPath = Path.Combine(outputFolder, levelFileName);
                var solutionPath = Path.Combine(outputFolder, solutionFileName);

                var levelJson = JsonUtility.ToJson(level.levelData, true);
                var solutionJson = JsonUtility.ToJson(level.solutionData, true);

                File.WriteAllText(levelPath, levelJson);
                File.WriteAllText(solutionPath, solutionJson);

                return FlowJsonExportResult.Success(levelPath, solutionPath);
            }
            catch (System.Exception ex)
            {
                return FlowJsonExportResult.Failure(
                    "JsonExportFailed", $"Export failed: {ex.Message}");
            }
        }
    }
}
