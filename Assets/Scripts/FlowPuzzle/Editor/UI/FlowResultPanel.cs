using FlowPuzzle.Core;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowResultPanel
    {
        public Label difficultyLabel, coverageLabel, seedLabel, scoreLabel;

        public void Build(VisualElement root)
        {
            difficultyLabel = new Label("Difficulty: —"); root.Add(difficultyLabel);
            coverageLabel = new Label("Coverage: —"); root.Add(coverageLabel);
            seedLabel = new Label("Seed: —"); root.Add(seedLabel);
            scoreLabel = new Label("Score: —"); root.Add(scoreLabel);
        }

        public void Show(FlowGeneratedLevel level)
        {
            difficultyLabel.text = $"Difficulty: {level.difficultyReport.difficulty}";
            coverageLabel.text = $"Coverage: {level.coverageRatio:P1}";
            seedLabel.text = $"Seed: {level.usedSeed}";
            scoreLabel.text = $"Score: {level.difficultyReport.totalScore:F1}";
        }

        public void Clear()
        {
            difficultyLabel.text = "Difficulty: —";
            coverageLabel.text = "Coverage: —";
            seedLabel.text = "Seed: —";
            scoreLabel.text = "Score: —";
        }
    }
}
