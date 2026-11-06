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
