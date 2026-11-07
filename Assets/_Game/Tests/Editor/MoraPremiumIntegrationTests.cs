using NUnit.Framework;

namespace Shift.Game.Tests
{
    public sealed class MoraPremiumIntegrationTests
    {
        [TestCase(1, "MoraPremium/MoraStage1")]
        [TestCase(2, "MoraPremium/MoraStage2")]
        [TestCase(3, "MoraPremium/MoraStage3")]
        [TestCase(4, "MoraPremium/MoraStage4")]
        public void RealGrowthArtUsesSeparateResources(int stage, string expected)
        {
            Assert.That(MoraCompanionHub.PremiumStagePath(stage), Is.EqualTo(expected));
        }

        [Test]
        public void ExistingFortyLevelsNeverGrantGrowth()
        {
            var snapshot = MoraGrowthPolicy.FromCampaign(39, 40);
            Assert.That(snapshot.Stage, Is.EqualTo(1));
            Assert.That(snapshot.CompletedBeyondForty, Is.Zero);
        }

        [Test]
        public void CompanionMenuTranslationsRemainStable()
        {
            Assert.That(MoraCompanionHub.TitleFor(MoraHubPage.Feed, true), Is.EqualTo("Besle"));
            Assert.That(MoraCompanionHub.TitleFor(MoraHubPage.Play, false), Is.EqualTo("Play"));
            Assert.That(MoraCompanionHub.TitleFor(MoraHubPage.Collection, true), Is.EqualTo("Koleksiyon"));
        }
    }
}
