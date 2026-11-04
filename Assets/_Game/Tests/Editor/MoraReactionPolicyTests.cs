using NUnit.Framework;

namespace Shift.Game.Tests
{
    public sealed class MoraReactionPolicyTests
    {
        [TestCase(0, 0, MoraMoment.None)]
        [TestCase(1, 0, MoraMoment.None)]
        [TestCase(2, 0, MoraMoment.None)]
        [TestCase(3, 0, MoraMoment.Chain)]
        [TestCase(1, 2, MoraMoment.Chain)]
        [TestCase(4, 3, MoraMoment.Chain)]
        public void OnlyMeaningfulChainsTriggerAMoveReaction(int actions, int depth, MoraMoment expected)
        {
            Assert.That(MoraReactionPolicy.OnMove(actions, depth), Is.EqualTo(expected));
        }

        [TestCase(false, false, MoraMoment.None)]
        [TestCase(false, true, MoraMoment.None)]
        [TestCase(true, false, MoraMoment.Complete)]
        [TestCase(true, true, MoraMoment.Perfect)]
        public void OnlySuccessfulResultsTriggerWinMoments(bool won, bool perfect, MoraMoment expected)
        {
            Assert.That(MoraReactionPolicy.OnResult(won, perfect), Is.EqualTo(expected));
        }
    }
}
