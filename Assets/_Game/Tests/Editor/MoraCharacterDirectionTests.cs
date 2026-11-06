using NUnit.Framework;

namespace Shift.Game.Tests
{
    public sealed class MoraCharacterDirectionTests
    {
        [TestCase(true, MoraHomeMood.Idle, "Mora/MoraSeed_Idle")]
        [TestCase(true, MoraHomeMood.Happy, "Mora/MoraSeed_Happy")]
        [TestCase(true, MoraHomeMood.Curious, "Mora/MoraSeed_Curious")]
        [TestCase(false, MoraHomeMood.Idle, "Mora/Mora_Idle")]
        [TestCase(false, MoraHomeMood.Happy, "Mora/Mora_Happy")]
        [TestCase(false, MoraHomeMood.Curious, "Mora/Mora_Curious")]
        public void MainSeedArtAndSpecialPrototypeUseSeparateResources(
            bool primary, MoraHomeMood mood, string expected)
        {
            Assert.That(MoraHomePanel.CharacterSpritePath(primary, mood), Is.EqualTo(expected));
        }

        [Test]
        public void PrototypeDoesNotCountLegacyLevelCompletionsTowardSeedGrowth()
        {
            Assert.That(MoraGrowthPolicy.FromCampaign(39, 40).CompletedBeyondForty, Is.Zero);
        }
    }
}
