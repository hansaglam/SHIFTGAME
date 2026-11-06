using NUnit.Framework;

namespace Shift.Game.Tests
{
    public sealed class MoraCompanionPilotTests
    {
        [TestCase(MoraMoment.None, "Mora/Mora_Idle")]
        [TestCase(MoraMoment.Curious, "Mora/Mora_Idle")]
        [TestCase(MoraMoment.Chain, "Mora/Mora_Chain")]
        [TestCase(MoraMoment.Complete, "Mora/Mora_Happy")]
        [TestCase(MoraMoment.Perfect, "Mora/Mora_Perfect")]
        public void OptionalSpritePathsAreStable(MoraMoment moment, string path)
        {
            Assert.That(MoraCompanionPilot.OptionalSpriteResource(moment), Is.EqualTo(path));
        }

        [Test]
        public void EnlargedDockStaysLeftOfInstructionTextAndOutsideControlRow()
        {
            Assert.That(MoraCompanionPilot.DockMin.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(MoraCompanionPilot.DockMax.x, Is.LessThanOrEqualTo(.22f));
            Assert.That(MoraCompanionPilot.DockMin.y, Is.GreaterThan(.133f));
            Assert.That(MoraCompanionPilot.DockMax.y, Is.LessThanOrEqualTo(.28f));
            Assert.That(MoraCompanionPilot.DockMax.y - MoraCompanionPilot.DockMin.y,
                Is.GreaterThan(.13f));
        }
    }
}
