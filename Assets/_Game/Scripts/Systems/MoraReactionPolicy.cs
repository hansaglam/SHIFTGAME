namespace Shift.Game
{
    public enum MoraMoment { None, Curious, Chain, Complete, Perfect }

    // Pure event-to-reaction policy. MORA does not affect the simulation.
    public static class MoraReactionPolicy
    {
        public static MoraMoment OnMove(int actionCount, int reactionDepth)
        {
            // Frequent small taps must not cause distracting repeated pop-ups.
            // Reserve MORA's puzzle appearance for meaningful chain reactions.
            if (actionCount < 3 && reactionDepth < 2) return MoraMoment.None;
            return MoraMoment.Chain;
        }

        public static MoraMoment OnResult(bool won, bool verifiedPerfect)
        {
            if (!won) return MoraMoment.None;
            return verifiedPerfect ? MoraMoment.Perfect : MoraMoment.Complete;
        }
    }
}
