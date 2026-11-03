namespace Shift.Game
{
    public enum MoraMoment { None, Curious, Chain, Complete, Perfect }

    // Pure event-to-reaction policy. No reliance on random values or animation timing.
    public static class MoraReactionPolicy
    {
        public static MoraMoment OnMove(int actionCount, int reactionDepth)
        {
            if (actionCount <= 0) return MoraMoment.None;
            return actionCount >= 3 || reactionDepth >= 2 ? MoraMoment.Chain : MoraMoment.Curious;
        }

        public static MoraMoment OnResult(bool won, bool verifiedPerfect)
        {
            if (!won) return MoraMoment.None;
            return verifiedPerfect ? MoraMoment.Perfect : MoraMoment.Complete;
        }
    }
}
