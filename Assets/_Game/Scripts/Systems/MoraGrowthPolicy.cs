using System;

namespace Shift.Game
{
    /// <summary>
    /// Derives MORA growth strictly from the existing authoritative campaign save.
    /// No extra save key, no replay farming, no purchases, no edits to the first 40 levels.
    /// </summary>
    public readonly struct MoraGrowthSnapshot
    {
        public readonly int CompletedBeyondForty;
        public readonly int Stage;
        public readonly int StageStart;
        public readonly int NextThreshold;
        public readonly int NextMilestoneLevel;
        public readonly bool HasPostFortyLevels;

        public MoraGrowthSnapshot(int completed, int stage, int stageStart,
            int nextThreshold, int nextMilestoneLevel, bool hasPostFortyLevels)
        {
            CompletedBeyondForty = completed;
            Stage = stage;
            StageStart = stageStart;
            NextThreshold = nextThreshold;
            NextMilestoneLevel = nextMilestoneLevel;
            HasPostFortyLevels = hasPostFortyLevels;
        }

        public bool IsMaxStage => Stage >= 4;
        public float Progress01 => IsMaxStage ? 1f
            : Math.Max(0f, Math.Min(1f,
                (float)(CompletedBeyondForty - StageStart) / (NextThreshold - StageStart)));
    }

    public static class MoraGrowthPolicy
    {
        // Stage 1: introduction with Level 41+, Stage 2: complete Level 50,
        // Stage 3: complete Level 70, Stage 4: complete Level 100.
        public static MoraGrowthSnapshot FromCampaign(int highestCompletedZeroBased, int levelCount)
        {
            int last = levelCount <= 0 ? -1 : Math.Min(highestCompletedZeroBased, levelCount - 1);
            int completed = Math.Max(0, last - 39);
            int stage = completed >= 60 ? 4 : completed >= 30 ? 3 : completed >= 10 ? 2 : 1;
            int start = stage == 4 ? 60 : stage == 3 ? 30 : stage == 2 ? 10 : 0;
            int threshold = stage == 4 ? 60 : stage == 3 ? 60 : stage == 2 ? 30 : 10;
            int milestone = stage == 4 ? 0 : stage == 3 ? 100 : stage == 2 ? 70 : 50;
            return new MoraGrowthSnapshot(completed, stage, start, threshold, milestone, levelCount > 40);
        }

        /// <summary>Editor-only visual preview. Not persisted or awarded to players.</summary>
        public static MoraGrowthSnapshot PreviewStage(int stage)
        {
            int completed = stage >= 4 ? 60 : stage == 3 ? 30 : stage == 2 ? 10 : 0;
            return FromCampaign(39 + completed, 100);
        }

        public static string StageName(int stage, bool turkish)
        {
            switch (stage)
            {
                case 2: return turkish ? "Uyanış" : "Awakening";
                case 3: return turkish ? "Çiçeklenme" : "Bloom";
                case 4: return turkish ? "Işıltı" : "Radiance";
                default: return turkish ? "Filiz" : "Seedling";
            }
        }
    }
}
