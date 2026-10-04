using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class PrivacyOptionsSettingsTests
    {
        private sealed class Privacy : IPrivacyChoices, IRewardedAdService
        {
            public bool Required, Busy, ThrowRead, ThrowShow;
            public int Shows, Ads;
            public Action<bool> Closed;
            public bool IsPrivacyOptionsRequired => ThrowRead ? throw new InvalidOperationException() : Required;
            public bool IsBusy => Busy;
            public bool IsRewardedAdAvailable => false;
            public void ShowPrivacyOptions(Action<bool> done)
            { Shows++; if (ThrowShow) throw new InvalidOperationException(); Busy = true; Closed = done; }
            public void ShowRewardedAd(RewardReason reason, Action<bool> done) { Ads++; done(false); }
            public void Finish(bool success) { Busy = false; Closed(success); }
        }
        private const string Prefix = "SHIFT.Tests.PrivacySettings.";
        private GameObject root;
        private SettingsPanel Build(Privacy provider)
        {
            root = new GameObject("Settings", typeof(RectTransform));
            var panel = root.AddComponent<SettingsPanel>();
            panel.Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), null,
                new SettingsService(Prefix), new GameFeelSettings(), null, () => {}, () => provider);
            panel.Open(); return panel;
        }
        [TearDown] public void Cleanup()
        { if (root != null) Object.DestroyImmediate(root); new SettingsService(Prefix).Reset(); }
        [TestCase("Required", true)] [TestCase("NotRequired", false)]
        [TestCase("Unknown", false)] [TestCase("Error", false)] [TestCase("Unavailable", false)]
        public void RequirementControlsVisibilityAndOriginalHiddenLayout(string state, bool visible)
        {
            var provider = state == "Unavailable" ? null : new Privacy { Required = state == "Required", ThrowRead = state == "Error" };
            var panel = Build(provider);
            Assert.That(panel.transform.Find("Privacy Choices").gameObject.activeSelf, Is.EqualTo(visible));
            Assert.That(panel.GetComponentsInChildren<Button>().Length, Is.EqualTo(visible ? 6 : 5));
            if (!visible)
            {
                var language = (RectTransform)panel.transform.Find("Language Setting");
                Assert.That(language.anchorMin.y, Is.EqualTo(.24f).Within(.00001f));
                Assert.That(language.anchorMax.y, Is.EqualTo(.34f).Within(.00001f));
            }
            var back = (RectTransform)panel.transform.Find("Close Settings");
            Assert.That(back.anchorMin.y, Is.EqualTo(.12f)); Assert.That(back.anchorMax.y, Is.EqualTo(.21f));
        }
        [Test] public void ThrowingFormKeepsSettingsUsableAndDoesNotReward()
        {
            var provider = new Privacy { Required = true, ThrowShow = true }; var panel = Build(provider);
            var button = panel.transform.Find("Privacy Choices").GetComponent<Button>();
            Assert.DoesNotThrow(() => button.onClick.Invoke());
            Assert.That(panel.IsOpen, Is.True); Assert.That(button.interactable, Is.True);
            Assert.That(provider.Shows, Is.EqualTo(1)); Assert.That(provider.Ads, Is.Zero);
        }
        [UnityTest] public IEnumerator SettingsLabelsCloseFailureAndPortraitCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return new WaitForSecondsRealtime(.35f);
            var game = Object.FindFirstObjectByType<PrototypeGame>(); var provider = new Privacy { Required = true };
            game.RewardedAds = provider; game.DailyLanguage = HintLanguage.English; game.OpenSettings();
            yield return new WaitForSecondsRealtime(.15f);
            var panel = Object.FindFirstObjectByType<SettingsPanel>();
            var button = panel.transform.Find("Privacy Choices").GetComponent<Button>();
            Assert.That(button.GetComponentInChildren<Text>().text, Is.EqualTo("Privacy Choices"));
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Validation/privacy-options"));
            SprintPresentationTests.Capture("privacy-options/01-settings-required-en.png");
            int hints = game.Allowances.HintsRemaining, undos = game.Allowances.UndosRemaining;
            button.onClick.Invoke(); button.onClick.Invoke();
            Assert.That(provider.Shows, Is.EqualTo(1)); Assert.That(button.interactable, Is.False);
            provider.Finish(false); yield return null;
            Assert.That(panel.IsOpen, Is.True); Assert.That(button.interactable, Is.True);
            game.DailyLanguage = HintLanguage.Turkish; yield return new WaitForSecondsRealtime(.15f);
            Assert.That(button.GetComponentInChildren<Text>().text, Is.EqualTo("Gizlilik Tercihleri"));
            SprintPresentationTests.Capture("privacy-options/02-settings-required-tr.png");
            button.onClick.Invoke(); provider.Required = false; provider.Finish(true); yield return null;
            Assert.That(button.gameObject.activeSelf, Is.False);
            Assert.That(panel.GetComponentsInChildren<Button>().Length, Is.EqualTo(5));
            Assert.That(provider.Ads, Is.Zero); Assert.That(game.Allowances.HintsRemaining, Is.EqualTo(hints));
            Assert.That(game.Allowances.UndosRemaining, Is.EqualTo(undos));
            SprintPresentationTests.Capture("privacy-options/03-settings-not-required.png");
            yield return new ExitPlayMode();
        }
    }
}
