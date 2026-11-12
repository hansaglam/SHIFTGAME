using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class ChapterDraftLocalizationTests
    {
        [Test]
        public void EveryDraftLevel41To100HasBilingualTitleAndHint()
        {
            for (int number = 41; number <= 100; number++)
            {
                var level = number <= 60 ? Chapter3DraftAudit.Get(number) :
                    number <= 65 ? Chapter4ResonanceDrafts.Get(number) :
                    number <= 80 ? Chapter4ResonanceExtendedDrafts.Get(number) :
                    Chapter5Drafts.Get(number);
                Assert.That(level, Is.Not.Null, "Missing Level " + number);
                Assert.That(ChapterDraftLocalization.TryNumber(
                    level, out int parsed), Is.True);
                Assert.That(parsed, Is.EqualTo(number));
                Assert.That(ChapterDraftLocalization.TryTitle(
                    level, GameLanguage.English, out string enTitle), Is.True);
                Assert.That(ChapterDraftLocalization.TryTitle(
                    level, GameLanguage.Turkish, out string trTitle), Is.True);
                Assert.That(enTitle, Does.StartWith(number + " - "),
                    "Missing English chapter number in " + number);
                Assert.That(trTitle, Does.StartWith(number + " - "),
                    "Missing Turkish chapter number in " + number);
                Assert.That(enTitle, Is.Not.EqualTo(trTitle),
                    "Untranslated title in Level " + number);
                Assert.That(ChapterDraftLocalization.TryInstruction(
                    level, GameLanguage.English, out string en), Is.True);
                Assert.That(ChapterDraftLocalization.TryInstruction(
                    level, GameLanguage.Turkish, out string tr), Is.True);
                Assert.That(en.Trim().Length, Is.GreaterThan(8),
                    "English hint too short in " + number);
                Assert.That(tr.Trim().Length, Is.GreaterThan(8),
                    "Turkish hint too short in " + number);
                Assert.That(en, Is.Not.EqualTo(tr),
                    "Translated hints should not reuse the same text in " + number);
            }
        }

        [Test]
        public void ShippedLevelKeysRemainTheAuthoritativeSourceForFirstForty()
        {
            var shipped = ChapterLevelWiring.LoadExpected();
            Assert.That(shipped.Length, Is.EqualTo(40));
            for (int i = 0; i < shipped.Length; i++)
            {
                Assert.That(ChapterDraftLocalization.TryNumber(
                    shipped[i], out int _), Is.False);
                Assert.That(LocalizationCatalog.Contains(
                    "level." + shipped[i].name + ".title"), Is.True);
                Assert.That(LocalizationCatalog.Contains(
                    "level." + shipped[i].name + ".instruction"), Is.True);
            }
        }

        [Test]
        public void FutureChapterFinaleMessagesMustHaveBilingualChapterNumbers()
        {
            foreach (var language in new[] { GameLanguage.English, GameLanguage.Turkish })
            {
                var next = LocalizationCatalog.Format(
                    "results.chapter_next", language, 3, 4);
                Assert.That(next, Does.Contain("3"));
                Assert.That(next, Does.Contain("4"));
                Assert.That(next, Does.Contain("\n"));
                var final = LocalizationCatalog.Format(
                    "results.chapter_final", language, 5, 6);
                Assert.That(final, Does.Contain("5"));
                Assert.That(final, Does.Not.Contain("6"));
                Assert.That(final, Does.Contain("\n"));
            }
        }

        [TestCase(60)]
        [TestCase(80)]
        [TestCase(100)]
        public void ExpandedOnboardingCopyUsesActualCampaignSize(int count)
        {
            foreach (var language in new[] {
                GameLanguage.English, GameLanguage.Turkish })
            {
                var levels = LocalizationCatalog.Format(
                    "onboarding.extended_levels", language, count);
                var campaign = LocalizationCatalog.Format(
                    "onboarding.extended_campaign", language, count);
                Assert.That(levels, Does.Contain(count.ToString()));
                Assert.That(campaign, Does.Contain(count.ToString()));
                Assert.That(campaign, Does.Not.Contain("40"));
            }
            Assert.That(LocalizationCatalog.Format(
                "onboarding.welcome.levels", GameLanguage.English),
                Does.Contain("40"));
        }

        [Test]
        public void LevelHundredHasIndependentEnglishAndTurkishHints()
        {
            var level = Chapter5Drafts.Get(100);
            Assert.That(level, Is.Not.Null);
            ChapterDraftLocalization.TryTitle(level,
                GameLanguage.Turkish, out string trTitle);
            ChapterDraftLocalization.TryTitle(level,
                GameLanguage.English, out string enTitle);
            Assert.That(trTitle, Is.EqualTo("100 - Açık Ufuk"));
            Assert.That(enTitle, Is.EqualTo("100 - The Open Horizon"));
            ChapterDraftLocalization.TryInstruction(level,
                GameLanguage.English, out string enHint);
            ChapterDraftLocalization.TryInstruction(level,
                GameLanguage.Turkish, out string trHint);
            Assert.That(enHint, Does.Contain("distant signals"));
            Assert.That(trHint, Does.Contain("uzak sinyal"));
        }
    }
}
