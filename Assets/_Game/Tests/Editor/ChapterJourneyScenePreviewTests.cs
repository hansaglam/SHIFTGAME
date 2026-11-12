using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Tests
{
    // Pure Editor wiring checks. Never creates or saves the preview scene.
    public sealed class ChapterJourneyScenePreviewTests
    {
        [Test]
        public void EightyLevelPreviewKeepsAllPublishedAssetsInOriginalOrder()
        {
            var original = ChapterLevelWiring.LoadExpected();
            var preview = ChapterJourneyScenePreview.Load80();
            Assert.That(original.Length, Is.EqualTo(40));
            Assert.That(preview.Length, Is.EqualTo(80));
            for (int i = 0; i < 40; i++)
                Assert.That(preview[i], Is.SameAs(original[i]),
                    "Never rewrite released level " + (i + 1));
            for (int number = 41; number <= 80; number++)
            {
                var expected = number <= 60 ? Chapter3DraftAudit.Get(number)
                    : number <= 65 ? Chapter4ResonanceDrafts.Get(number)
                    : Chapter4ResonanceExtendedDrafts.Get(number);
                Assert.That(preview[number - 1], Is.SameAs(expected),
                    "Draft level reference drift: " + number);
            }
        }

        [Test]
        public void HundredLevelPreviewMustPreserveTheFirstEightyAndAppendTwenty()
        {
            var original = ChapterJourneyScenePreview.Load80();
            var hundred = ChapterJourneyScenePreview.Load100();
            Assert.That(hundred.Length, Is.EqualTo(100));
            for (int i = 0; i < original.Length; i++)
                Assert.That(hundred[i], Is.SameAs(original[i]),
                    "Old LevelData identity changed at " + (i + 1));
            for (int number = 81; number <= 100; number++)
                Assert.That(hundred[number - 1], Is.SameAs(Chapter5Drafts.Get(number)),
                    "Chapter 5 incorrect reference: " + number);
        }

        [Test]
        public void PreviewSceneMustNeverBeAnEnabledBuildScene()
        {
            Assert.That(ChapterJourneyScenePreview.PreviewScenePath,
                Is.Not.EqualTo(PrototypeSetup.ScenePath));
            Assert.That(ChapterJourneyScenePreview.Preview100ScenePath,
                Is.Not.EqualTo(PrototypeSetup.ScenePath));
            Assert.That(ChapterJourneyScenePreview.Preview100ScenePath,
                Is.Not.EqualTo(ChapterJourneyScenePreview.PreviewScenePath));
            foreach (var scene in EditorBuildSettings.scenes)
            {
                Assert.That(scene.path,
                    Is.Not.EqualTo(ChapterJourneyScenePreview.PreviewScenePath),
                    "Opt-in preview scenes must never appear in production build settings.");
                Assert.That(scene.path,
                    Is.Not.EqualTo(ChapterJourneyScenePreview.Preview100ScenePath),
                    "100-level preview must not be included in production builds.");
            }
        }

        [TestCase(40, false, false)]
        [TestCase(80, true, false)]
        [TestCase(100, true, true)]
        public void ChapterSelectorOnlyCreatesNewWorldsForExtendedCampaign(
            int count, bool hasJourney, bool hasFifthChapter)
        {
            var root = new GameObject("Journey Chapter UI Test", typeof(RectTransform));
            var rounded = PlaceholderVisuals.CreateRounded();
            try
            {
                var select = root.AddComponent<ChapterSelect>();
                select.Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                    rounded, count, _ => { }, () => { }, () => { });
                var content = root.transform.Find("Chapter Content");
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Find("Chapter 1 Tab"), Is.Not.Null);
                Assert.That(content.Find("Chapter 2 Tab"), Is.Not.Null);
                Assert.That(content.Find("Chapter 3 Tab") != null,
                    Is.EqualTo(hasJourney));
                Assert.That(content.Find("Chapter 4 Tab") != null,
                    Is.EqualTo(hasJourney));
                Assert.That(content.Find("Chapter 5 Tab") != null,
                    Is.EqualTo(hasFifthChapter));
                Assert.That(content.Find("Chapter Journey") != null,
                    Is.EqualTo(hasJourney));
                var world = content.Find("Chapter World Landscape");
                Assert.That(world != null, Is.EqualTo(hasJourney));
                if (hasJourney)
                {
                    var graphic = world.GetComponent<ChapterWorldBackdrop>();
                    Assert.That(graphic, Is.Not.Null);
                    Assert.That(graphic.raycastTarget, Is.False,
                        "World artwork must not steal taps from puzzle selection.");
                    var title = content.Find("Chapter Title") as RectTransform;
                    var summary = content.Find("Chapter Hint") as RectTransform;
                    var track = content.Find("Chapter Journey") as RectTransform;
                    Assert.That(title, Is.Not.Null);
                    Assert.That(summary, Is.Not.Null);
                    Assert.That(track, Is.Not.Null);
                    Assert.That(summary.anchorMax.y,
                        Is.LessThan(title.anchorMin.y),
                        "Chapter title and purpose must not overlap.");
                    Assert.That(track.anchorMax.y,
                        Is.LessThan(summary.anchorMin.y),
                        "World route needs its own readable vertical space.");
                }
                Assert.That(content.Find("Level " + count), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (rounded != null)
                {
                    var texture = rounded.texture;
                    Object.DestroyImmediate(rounded);
                    if (texture != null) Object.DestroyImmediate(texture);
                }
            }
        }
    }
}
