using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    // Render-geometry regression for the actual in-board BAĞ overlay.
    // Uses standard UI Images, so its geometry remains testable without
    // an external asset, a phone screenshot, or a running campaign.
    public sealed class TetherLinkVisualTests
    {
        private GameObject board;
        private RectTransform a, b;
        private TetherLinkGraphic bond;

        [SetUp]
        public void SetUp()
        {
            board = new GameObject("Bond Visual Test Board",
                typeof(RectTransform), typeof(Canvas));
            var rect = (RectTransform)board.transform;
            rect.sizeDelta = new Vector2(700f, 300f);

            a = PlaceholderVisuals.Rect("Blue Normal", rect,
                new Vector2(0f, 1f / 3f),
                new Vector2(1f / 7f, 2f / 3f));
            b = PlaceholderVisuals.Rect("Red Normal", rect,
                new Vector2(3f / 7f, 1f / 3f),
                new Vector2(4f / 7f, 2f / 3f));

            var overlay = PlaceholderVisuals.Rect("Tether Bond", rect,
                Vector2.zero, Vector2.one);
            bond = overlay.gameObject.AddComponent<TetherLinkGraphic>();
            // A null custom sprite is fine: Image uses Unity's standard
            // built-in white texture. The game passes its rounded sprite.
            bond.Bind(a, b, 7, 3, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (board != null)
                Object.DestroyImmediate(board);
        }

        [Test]
        public void LinkedStonesHaveVisibleGoldUIImagesNotAnEmptyMesh()
        {
            Assert.That(bond.BondVisible, Is.True);
            Assert.That(bond.IsNonInteractive, Is.True);
            var images = bond.GetComponentsInChildren<Image>(true);
            Assert.That(images.Length, Is.EqualTo(4));
            foreach (var image in images)
            {
                Assert.That(image.enabled, Is.True);
                Assert.That(image.raycastTarget, Is.False);
                Assert.That(image.color.a, Is.GreaterThan(.4f));
            }
            var core = bond.transform.Find("Bond Bright Center")
                .GetComponent<Image>();
            Assert.That(core.rectTransform.sizeDelta.x, Is.GreaterThan(150f),
                "The link must be a real strip connecting the two stones.");
            Assert.That(core.rectTransform.sizeDelta.y, Is.GreaterThan(3f));
        }

        [Test]
        public void BondLineTracksLivePieceCoordinates()
        {
            var core = bond.transform.Find("Bond Bright Center")
                .GetComponent<Image>().rectTransform;
            var oldCenter = core.anchoredPosition;
            b.anchorMin = new Vector2(4f / 7f, 1f / 3f);
            b.anchorMax = new Vector2(5f / 7f, 2f / 3f);
            bond.Refresh();
            Assert.That(core.anchoredPosition.x,
                Is.GreaterThan(oldCenter.x + 40f),
                "The bond must follow the actual animated piece.");
            Assert.That(bond.BondVisible, Is.True);
        }

        [Test]
        public void DeliveredEndpointHidesLinkWithoutDisablingGameInput()
        {
            b.gameObject.SetActive(false);
            bond.Refresh();
            Assert.That(bond.BondVisible, Is.False);
            b.gameObject.SetActive(true);
            bond.Refresh();
            Assert.That(bond.BondVisible, Is.True);
            Assert.That(bond.IsNonInteractive, Is.True);
            var group = bond.GetComponent<CanvasGroup>();
            Assert.That(group, Is.Not.Null);
            Assert.That(group.blocksRaycasts, Is.False);
        }
    }
}
