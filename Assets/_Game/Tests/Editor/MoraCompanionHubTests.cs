using NUnit.Framework;
namespace Shift.Game.Tests
{
    public sealed class MoraCompanionHubTests
    {
        [TestCase(MoraHubPage.Home, true, "MORA")]
        [TestCase(MoraHubPage.Feed, true, "Besle")]
        [TestCase(MoraHubPage.Play, true, "Oyna")]
        [TestCase(MoraHubPage.Decor, true, "Dekor")]
        [TestCase(MoraHubPage.Growth, true, "Gelişim")]
        [TestCase(MoraHubPage.Collection, true, "Koleksiyon")]
        [TestCase(MoraHubPage.Feed, false, "Feed")]
        [TestCase(MoraHubPage.Collection, false, "Collection")]
        public void AllRealMenuViewsHaveLocalizedTitles(MoraHubPage page, bool tr, string name)
        {
            Assert.That(MoraCompanionHub.TitleFor(page, tr), Is.EqualTo(name));
        }

        [TestCase(1, "Mora/MoraScene_S1")]
        [TestCase(2, "Mora/MoraScene_S2")]
        [TestCase(3, "Mora/MoraScene_S3")]
        [TestCase(4, "Mora/MoraScene_S4")]
        public void ApprovedStageSourcePathsRemainDeterministic(int stage, string expected)
        {
            Assert.That(MoraCompanionHub.StageArtPath(stage), Is.EqualTo(expected));
        }

        [Test]
        public void NoGrowthIsEarnedFromFirstFortyLevels()
        {
            var snapshot = MoraGrowthPolicy.FromCampaign(39, 40);
            Assert.That(snapshot.Stage, Is.EqualTo(1));
            Assert.That(snapshot.CompletedBeyondForty, Is.EqualTo(0));
        }
    }
}
