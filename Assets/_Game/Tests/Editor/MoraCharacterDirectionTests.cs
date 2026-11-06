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

        [TestCase(1, "Mora/MoraSeed_S1_Idle", "Mora/MoraHabitat_S1")]
        [TestCase(2, "Mora/MoraSeed_S2_Idle", "Mora/MoraHabitat_S2")]
        [TestCase(3, "Mora/MoraSeed_S3_Idle", "Mora/MoraHabitat_S3")]
        [TestCase(4, "Mora/MoraSeed_S4_Idle", "Mora/MoraHabitat_S4")]
        public void FourStagePreviewUsesDifferentRealArtFiles(int stage, string avatar, string habitat)
        {
            Assert.That(MoraHomePanel.PreviewStageSpriteResource(stage), Is.EqualTo(avatar));
            Assert.That(MoraHomePanel.PreviewStageHabitatResource(stage), Is.EqualTo(habitat));
        }

        [Test]
        public void PrototypeDoesNotCountLegacyLevelCompletionsTowardSeedGrowth()
        {
            Assert.That(MoraGrowthPolicy.FromCampaign(39, 40).CompletedBeyondForty, Is.Zero);
        }
    }
}
