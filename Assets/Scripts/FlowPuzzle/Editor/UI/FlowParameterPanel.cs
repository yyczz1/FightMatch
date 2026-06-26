using FlowPuzzle.Core;
using UnityEngine.UIElements;

namespace FlowPuzzle.Editor.UI
{
    public sealed class FlowParameterPanel
    {
        public IntegerField levelIdField, widthField, heightField, colorCountField;
        public FloatField minCoverageField, maxCoverageField;
        public IntegerField minPathLenField, maxPathLenField;
        public IntegerField seedField;
        public Toggle useRandomSeedToggle;
        public IntegerField pathAttemptField, levelAttemptField;
        public IntegerField batchCountField;
        public TextField outputFolderField;
        public EnumField presetField;
        public Toggle targetTierToggle;
        public EnumField targetTierField;
        public Toggle scoreRangeToggle;
        public FloatField minScoreField, maxScoreField;
        public Foldout advancedFoldout;
        public FloatField turnPrefField, interactionPrefField;
        public IntegerField minEndpointDistField, maxEndpointDistField;
        public IntegerField minDetourField, maxDetourField;
        public FloatField bottleneckPrefField;
        public IntegerField solverTimeoutField, solverNodeBudgetField;

        public void Build(VisualElement root)
        {
            presetField = new EnumField("Preset", FlowDifficultyPreset.Custom); root.Add(presetField);
            levelIdField = new IntegerField("Level ID") { value = 1 }; root.Add(levelIdField);
            widthField = new IntegerField("Width") { value = 5 }; root.Add(widthField);
            heightField = new IntegerField("Height") { value = 5 }; root.Add(heightField);
            colorCountField = new IntegerField("Colors") { value = 2 }; root.Add(colorCountField);
            minCoverageField = new FloatField("Min Coverage") { value = 0.25f }; root.Add(minCoverageField);
            maxCoverageField = new FloatField("Max Coverage") { value = 0.5f }; root.Add(maxCoverageField);
            minPathLenField = new IntegerField("Min Path Len") { value = 2 }; root.Add(minPathLenField);
            maxPathLenField = new IntegerField("Max Path Len") { value = 5 }; root.Add(maxPathLenField);
            seedField = new IntegerField("Seed") { value = 42 }; root.Add(seedField);
            useRandomSeedToggle = new Toggle("Use Random Seed"); root.Add(useRandomSeedToggle);
            pathAttemptField = new IntegerField("Max Path Attempt") { value = 250 }; root.Add(pathAttemptField);
            levelAttemptField = new IntegerField("Max Level Attempt") { value = 100 }; root.Add(levelAttemptField);
            batchCountField = new IntegerField("Batch Count") { value = 1 }; root.Add(batchCountField);
            outputFolderField = new TextField("Output Folder") { value = "Assets/FlowPuzzleGenerated/Levels" }; root.Add(outputFolderField);
            targetTierToggle = new Toggle("Target Tier"); root.Add(targetTierToggle);
            targetTierField = new EnumField("Target Tier", FlowDifficultyTier.Easy); root.Add(targetTierField);
            scoreRangeToggle = new Toggle("Target Score Range"); root.Add(scoreRangeToggle);
            minScoreField = new FloatField("Min Score"); root.Add(minScoreField);
            maxScoreField = new FloatField("Max Score"); root.Add(maxScoreField);
            advancedFoldout = new Foldout { text = "Advanced" };
            turnPrefField = new FloatField("Turn Preference") { value = 0f }; advancedFoldout.Add(turnPrefField);
            interactionPrefField = new FloatField("Interaction") { value = 0f }; advancedFoldout.Add(interactionPrefField);
            minEndpointDistField = new IntegerField("Min Endpoint Dist"); advancedFoldout.Add(minEndpointDistField);
            maxEndpointDistField = new IntegerField("Max Endpoint Dist"); advancedFoldout.Add(maxEndpointDistField);
            minDetourField = new IntegerField("Min Detour"); advancedFoldout.Add(minDetourField);
            maxDetourField = new IntegerField("Max Detour"); advancedFoldout.Add(maxDetourField);
            bottleneckPrefField = new FloatField("Bottleneck Pref"); advancedFoldout.Add(bottleneckPrefField);
            solverTimeoutField = new IntegerField("Solver Timeout"); advancedFoldout.Add(solverTimeoutField);
            solverNodeBudgetField = new IntegerField("Solver Budget"); advancedFoldout.Add(solverNodeBudgetField);
            root.Add(advancedFoldout);
        }

        public FlowGenerationConfig ReadConfig()
        {
            return new FlowGenerationConfig
            {
                width = widthField.value, height = heightField.value, colorCount = colorCountField.value,
                minCoverageRatio = minCoverageField.value, maxCoverageRatio = maxCoverageField.value,
                minPathLength = minPathLenField.value, maxPathLength = maxPathLenField.value,
                useRandomSeed = useRandomSeedToggle.value, seed = seedField.value,
                maxPathAttempt = pathAttemptField.value, maxLevelAttempt = levelAttemptField.value,
                useTargetDifficulty = targetTierToggle.value, targetDifficulty = (FlowDifficultyTier)targetTierField.value,
                useTargetScoreRange = scoreRangeToggle.value,
                minTargetDifficultyScore = minScoreField.value, maxTargetDifficultyScore = maxScoreField.value,
                turnPreference = turnPrefField.value, interactionPreference = interactionPrefField.value,
                minEndpointDistance = minEndpointDistField.value, maxEndpointDistance = maxEndpointDistField.value,
                minDetour = minDetourField.value, maxDetour = maxDetourField.value,
                bottleneckPreference = bottleneckPrefField.value,
                solverTimeoutMilliseconds = solverTimeoutField.value, solverNodeBudget = solverNodeBudgetField.value
            };
        }

        public void ApplyPresetValues(FlowGenerationConfig config)
        {
            widthField.value = config.width;
            heightField.value = config.height;
            colorCountField.value = config.colorCount;
            minCoverageField.value = config.minCoverageRatio;
            maxCoverageField.value = config.maxCoverageRatio;
            minPathLenField.value = config.minPathLength;
            maxPathLenField.value = config.maxPathLength;
            turnPrefField.value = config.turnPreference;
            interactionPrefField.value = config.interactionPreference;
            pathAttemptField.value = config.maxPathAttempt;
            levelAttemptField.value = config.maxLevelAttempt;
            useRandomSeedToggle.value = config.useRandomSeed;
            seedField.value = config.seed;
            targetTierToggle.value = config.useTargetDifficulty;
            targetTierField.value = config.targetDifficulty;
            scoreRangeToggle.value = config.useTargetScoreRange;
            minScoreField.value = config.minTargetDifficultyScore;
            maxScoreField.value = config.maxTargetDifficultyScore;
        }
    }
}
