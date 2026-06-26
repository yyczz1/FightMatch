using System;
using FlowPuzzle.Core;

namespace FlowPuzzle.Editor
{
    public static class FlowDifficultyPresetLibrary
    {
        public static void Apply(
            FlowDifficultyPreset preset,
            FlowGenerationConfig target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            switch (preset)
            {
                case FlowDifficultyPreset.Custom:
                    break;
                case FlowDifficultyPreset.Easy:
                    ApplyEasy(target);
                    break;
                case FlowDifficultyPreset.Normal:
                    ApplyNormal(target);
                    break;
                case FlowDifficultyPreset.Hard:
                    ApplyHard(target);
                    break;
                case FlowDifficultyPreset.Expert:
                    ApplyExpert(target);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown preset.");
            }
        }

        private static void ApplyBase(FlowGenerationConfig target, int w, int h, int colors,
            float minCov, float maxCov, int minLen, int maxLen,
            float turn, float interaction,
            FlowDifficultyTier tier)
        {
            target.width = w;
            target.height = h;
            target.colorCount = colors;
            target.minCoverageRatio = minCov;
            target.maxCoverageRatio = maxCov;
            target.minPathLength = minLen;
            target.maxPathLength = maxLen;
            target.turnPreference = turn;
            target.interactionPreference = interaction;
            target.maxPathAttempt = 250;
            target.maxLevelAttempt = 100;
            target.useRandomSeed = false;
            target.useTargetDifficulty = false;
            target.targetDifficulty = tier;
            target.useTargetScoreRange = false;
            // Set score range matching tier
            switch (tier)
            {
                case FlowDifficultyTier.Easy:
                    target.minTargetDifficultyScore = 0f;
                    target.maxTargetDifficultyScore = 59.999f;
                    break;
                case FlowDifficultyTier.Normal:
                    target.minTargetDifficultyScore = 60f;
                    target.maxTargetDifficultyScore = 119.999f;
                    break;
                case FlowDifficultyTier.Hard:
                    target.minTargetDifficultyScore = 120f;
                    target.maxTargetDifficultyScore = 199.999f;
                    break;
                case FlowDifficultyTier.Expert:
                    target.minTargetDifficultyScore = 200f;
                    target.maxTargetDifficultyScore = 10000f;
                    break;
            }
        }

        private static void ApplyEasy(FlowGenerationConfig t)
            => ApplyBase(t, 5, 5, 2, 0.25f, 0.50f, 2, 5, -0.5f, -0.5f, FlowDifficultyTier.Easy);

        private static void ApplyNormal(FlowGenerationConfig t)
            => ApplyBase(t, 6, 6, 3, 0.35f, 0.65f, 2, 7, 0f, 0f, FlowDifficultyTier.Normal);

        private static void ApplyHard(FlowGenerationConfig t)
            => ApplyBase(t, 7, 7, 4, 0.50f, 0.80f, 3, 10, 0.75f, 0.75f, FlowDifficultyTier.Hard);

        private static void ApplyExpert(FlowGenerationConfig t)
            => ApplyBase(t, 8, 8, 5, 0.65f, 0.90f, 3, 13, 1.5f, 1.5f, FlowDifficultyTier.Expert);
    }
}
