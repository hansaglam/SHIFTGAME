namespace Shift.Game
{
    public enum ReactionTier { Normal, Chain, BigShift, MegaShift }
    public static class ReactionPresentation
    {
        // Depth is the committed BoardAction count, including turns and gate events.
        public static ReactionTier Classify(int depth) => depth < 2 ? ReactionTier.Normal :
            depth < 4 ? ReactionTier.Chain : depth < 6 ? ReactionTier.BigShift : ReactionTier.MegaShift;
        public static string Label(ReactionTier tier) => tier == ReactionTier.MegaShift ? "MEGA SHIFT" :
            tier == ReactionTier.BigShift ? "BIG SHIFT" : tier == ReactionTier.Chain ? "CHAIN" : string.Empty;
    }
}
