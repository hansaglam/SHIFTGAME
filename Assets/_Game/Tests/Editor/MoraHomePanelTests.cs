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
                Assert.Ignore("Local test art exists; this fallback is verified in clean content builds.");
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
