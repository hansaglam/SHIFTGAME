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

        [TestCase(MoraMoment.None, "Mora/MoraSeed_Idle")]
        [TestCase(MoraMoment.Curious, "Mora/MoraSeed_Idle")]
        [TestCase(MoraMoment.Chain, "Mora/MoraSeed_Chain")]
        [TestCase(MoraMoment.Complete, "Mora/MoraSeed_Happy")]
        [TestCase(MoraMoment.Perfect, "Mora/MoraSeed_Perfect")]
        public void DefaultPlayerCompanionUsesThePrimarySeedNotTheTealPrototype(MoraMoment moment, string expected)
        {
            Assert.That(MoraCompanionPilot.PrimarySpriteResource(moment), Is.EqualTo(expected));
        }

        [Test]
        public void UserTestedDockPlacementRemainsUnchanged()
        {
            Assert.That(MoraCompanionPilot.DockMin.x, Is.EqualTo(0f));
            Assert.That(MoraCompanionPilot.DockMax.x, Is.EqualTo(.22f).Within(.0001f));
            Assert.That(MoraCompanionPilot.DockMin.y, Is.EqualTo(.135f).Within(.0001f));
            Assert.That(MoraCompanionPilot.DockMax.y, Is.EqualTo(.277f).Within(.0001f));
        }

        [TestCase(MoraMoment.Chain, 1.3f)]
        [TestCase(MoraMoment.Complete, 1.9f)]
        [TestCase(MoraMoment.Perfect, 2.4f)]
        [TestCase(MoraMoment.None, 0f)]
        public void MotionDurationPreservesExistingTiming(MoraMoment moment, float expected)
        {
            Assert.That(MoraMotionProfile.Duration(moment), Is.EqualTo(expected).Within(.001f));
        }

        [Test]
        public void ReducedMotionNeverMovesOrTiltsMora()
        {
            var types = new[] { MoraMoment.Chain, MoraMoment.Complete, MoraMoment.Perfect };
            foreach (var type in types)
            {
                for (int step = 0; step <= 20; step++)
                {
                    var frame = MoraMotionProfile.Sample(type, step / 20f, true);
                    Assert.That(frame.Opacity, Is.EqualTo(1f));
                    Assert.That(frame.VerticalOffset, Is.Zero);
                    Assert.That(frame.TiltDegrees, Is.Zero);
                    Assert.That(frame.ScaleX, Is.EqualTo(1f));
                    Assert.That(frame.ScaleY, Is.EqualTo(1f));
                }
            }
        }

        [Test]
        public void MotionStaysWithinSafeVisualLimits()
        {
            var types = new[] { MoraMoment.Chain, MoraMoment.Complete, MoraMoment.Perfect };
            foreach (var type in types)
            {
                for (int step = 0; step <= 100; step++)
                {
                    var frame = MoraMotionProfile.Sample(type, step / 100f, false);
                    Assert.That(frame.Opacity, Is.InRange(0f, 1f));
                    Assert.That(frame.VerticalOffset, Is.InRange(0f, 40f));
                    Assert.That(frame.TiltDegrees, Is.InRange(-8f, 8f));
                    Assert.That(frame.ScaleX, Is.InRange(.88f, 1.12f));
                    Assert.That(frame.ScaleY, Is.InRange(.88f, 1.12f));
                }
            }
        }

        [Test]
        public void QuietMomentsNeverProduceAnAnimation()
        {
            Assert.That(MoraMotionProfile.Sample(MoraMoment.None, .5f, false).Opacity, Is.Zero);
            Assert.That(MoraMotionProfile.Sample(MoraMoment.Curious, .5f, false).Opacity, Is.Zero);
        }
    }
}
