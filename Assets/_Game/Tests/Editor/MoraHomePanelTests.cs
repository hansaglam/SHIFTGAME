using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class MoraHomePanelTests
    {
        [Test]
        public void LivingWorldPreviewDoesNotCreateAnObjectWithoutTheRequiredPortrait()
        {
            if (Resources.Load<Sprite>("Mora/Mora_Idle") != null)
                Assert.Ignore("Local MORA art exists; missing-art fallback requires a clean fixture.");
            var safe = new GameObject("Safe Area", typeof(RectTransform)).GetComponent<RectTransform>();
            try
            {
                Assert.That(MoraHomePanel.TryCreate(safe, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                    null, null, new GameFeelSettings(), () => { }), Is.Null);
            }
            finally { Object.DestroyImmediate(safe.gameObject); }
        }

        [TestCase(true, 0, "MORA'ya dokun, seni selamlasın.")]
        [TestCase(true, 1, "MORA seni gördüğüne sevindi!")]
        [TestCase(true, 2, "MORA seni merakla izliyor.")]
        [TestCase(false, 0, "Tap MORA to say hello.")]
        [TestCase(false, 1, "MORA is happy to see you!")]
        [TestCase(false, 2, "MORA looks at you curiously.")]
        public void GreetingStateStaysLocalizedAndDeterministic(bool turkish, int touches, string expected)
        {
            Assert.That(MoraHomePanel.GreetingText(turkish, touches), Is.EqualTo(expected));
        }

        [TestCase(0, MoraHomeMood.Idle)]
        [TestCase(1, MoraHomeMood.Happy)]
        [TestCase(2, MoraHomeMood.Curious)]
        [TestCase(3, MoraHomeMood.Happy)]
        [TestCase(4, MoraHomeMood.Curious)]
        public void FaceStateDependsOnlyOnAcceptedTouches(int touches, MoraHomeMood expected)
        {
            Assert.That(MoraHabitatInteractionPolicy.MoodForTouch(touches), Is.EqualTo(expected));
        }

        [Test]
        public void RapidTapsUseShortCooldownWithoutHidingCharacter()
        {
            float first = 14f, next = first + MoraHabitatInteractionPolicy.TapCooldownSeconds;
            Assert.That(MoraHabitatInteractionPolicy.CanAcceptTap(first, float.NegativeInfinity), Is.True);
            Assert.That(MoraHabitatInteractionPolicy.CanAcceptTap(first + .1f, next), Is.False);
            Assert.That(MoraHabitatInteractionPolicy.CanAcceptTap(next, next), Is.True);
            Assert.That(MoraHabitatInteractionPolicy.FaceBlendSeconds, Is.LessThan(MoraHabitatInteractionPolicy.TapCooldownSeconds));
            Assert.That(MoraHabitatInteractionPolicy.ExpressionHoldSeconds, Is.GreaterThan(.8f));
        }

        [Test]
        public void ExpressionBlendingIsClampedAndMonotonic()
        {
            Assert.That(MoraHabitatInteractionPolicy.FaceBlendWeight(-1f), Is.Zero);
            Assert.That(MoraHabitatInteractionPolicy.FaceBlendWeight(0f), Is.Zero);
            Assert.That(MoraHabitatInteractionPolicy.FaceBlendWeight(
                MoraHabitatInteractionPolicy.FaceBlendSeconds), Is.EqualTo(1f));
            Assert.That(MoraHabitatInteractionPolicy.FaceBlendWeight(4f), Is.EqualTo(1f));
            float previous = 0f;
            for (int i = 1; i <= 30; i++)
            {
                float weight = MoraHabitatInteractionPolicy.FaceBlendWeight(
                    i * MoraHabitatInteractionPolicy.FaceBlendSeconds / 30f);
                Assert.That(weight, Is.InRange(previous, 1f));
                previous = weight;
            }
        }

        [Test]
        public void ExistingMoraReactionPolicyRemainsUnchanged()
        {
            Assert.That(MoraReactionPolicy.OnMove(1, 0), Is.EqualTo(MoraMoment.None));
            Assert.That(MoraReactionPolicy.OnMove(3, 0), Is.EqualTo(MoraMoment.Chain));
            Assert.That(MoraReactionPolicy.OnResult(true, false), Is.EqualTo(MoraMoment.Complete));
            Assert.That(MoraReactionPolicy.OnResult(true, true), Is.EqualTo(MoraMoment.Perfect));
        }
    }
}
