using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    // Geometric design acceptance for SHIFT's intentionally restrained
    // curved BAĞ link. Rendering still needs visual approval in Game view.
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
            bond.Bind(a, b, 7, 3, null);
            bond.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            if (board != null) Object.DestroyImmediate(board);
        }

        private RectTransform Segment(int index) =>
            bond.transform.Find("Bond Arc Filament " + index)
                .GetComponent<RectTransform>();

        [Test]
        public void CurveIsThinVisibleAndDoesNotInterceptInput()
        {
            Assert.That(bond.BondVisible, Is.True);
            Assert.That(bond.IsNonInteractive, Is.True);
            var images = bond.GetComponentsInChildren<Image>(true);
            Assert.That(images.Length, Is.EqualTo(24));
            foreach (var image in images)
            {
                Assert.That(image.enabled, Is.True);
                Assert.That(image.raycastTarget, Is.False);
            }
            var centerSegment = Segment(5);
            Assert.That(centerSegment.sizeDelta.x, Is.GreaterThan(12f));
            Assert.That(centerSegment.sizeDelta.y,
                Is.InRange(2.5f, 5.5f),
                "BAĞ must never become a thick bar over puzzle icons.");
            Assert.That(bond.GetComponent<CanvasGroup>().blocksRaycasts,
                Is.False);
        }

        [Test]
        public void BondArcRisesAboveTheStoneCentersToAvoidPuzzleSymbols()
        {
            // This exact arrangement reproduces Level 101's 7x3 board:
            // all meaningful switch/exit symbols occupy the middle row.
            var middle = Segment(5);
            Vector2 blue = bond.transform.InverseTransformPoint(
                a.TransformPoint(a.rect.center));
            Vector2 red = bond.transform.InverseTransformPoint(
                b.TransformPoint(b.rect.center));
            Assert.That(middle.anchoredPosition.y,
                Is.GreaterThan(Mathf.Max(blue.y, red.y) + 55f),
                "The connection must go through the top EMPTY lane, " +
                "not horizontally through the switch, exit or arrows.");
            Assert.That(middle.anchoredPosition.y, Is.LessThan(145f));
        }

        [Test]
        public void CurvedBondMovesWithAnimatedStones()
        {
            var middle = Segment(5);
            float originalX = middle.anchoredPosition.x;
            b.anchorMin = new Vector2(4f / 7f, 1f / 3f);
            b.anchorMax = new Vector2(5f / 7f, 2f / 3f);
            bond.Refresh();
            Assert.That(middle.anchoredPosition.x,
                Is.GreaterThan(originalX + 35f),
                "Curved BAĞ must follow a moving endpoint each frame.");
            Assert.That(bond.BondVisible, Is.True);
        }

        [Test]
        public void DeliveredEndpointHidesArcAndReappearsOnUndo()
        {
            b.gameObject.SetActive(false);
            bond.Refresh();
            Assert.That(bond.BondVisible, Is.False);
            b.gameObject.SetActive(true);
            bond.Refresh();
            Assert.That(bond.BondVisible, Is.True);
            Assert.That(bond.IsNonInteractive, Is.True);
        }

        [Test]
        public void UpperEdgeOfBoardRoutesCurveDownward()
        {
            // Future levels may place both linked stones at the top row.
            // They must not draw a link outside the board.
            a.anchorMin = new Vector2(0f, 2f / 3f);
            a.anchorMax = new Vector2(1f / 7f, 1f);
            b.anchorMin = new Vector2(3f / 7f, 2f / 3f);
            b.anchorMax = new Vector2(4f / 7f, 1f);
            bond.Refresh();

            Vector2 blue = bond.transform.InverseTransformPoint(
                a.TransformPoint(a.rect.center));
            var middle = Segment(5);
            Assert.That(middle.anchoredPosition.y,
                Is.LessThan(blue.y),
                "At top-edge positions the arc should route BELOW the stones.");
        }
    }
}
