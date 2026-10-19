using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Shift.Game.Tests
{
    public sealed class MechanicGuideTests
    {
        private sealed class PendingPrivacy : IPrivacyChoices, IRewardedAdService
        {
            public int Rewards,Forms;
            public bool IsBusy=>true;
            public bool IsPrivacyOptionsRequired=>false;
            public bool IsRewardedAdAvailable=>false;
            public void ShowPrivacyOptions(Action<bool> done){Forms++;done(false);}
            public void ShowRewardedAd(RewardReason reason,Action<bool> done){Rewards++;done(false);}
        }
        private const string Prefix="SHIFT.Tests.MechanicGuide.";
        private const string Key=Prefix+"MechanicGuide.v1";
        [SetUp] public void Before() { PlayerPrefs.DeleteKey(Key); }
        [TearDown] public void After() { PlayerPrefs.DeleteKey(Key);new SaveService(Prefix).Reset(); }
        [TestCase(MechanicTopic.Push,3)][TestCase(MechanicTopic.Direction,6)][TestCase(MechanicTopic.Exits,7)]
        [TestCase(MechanicTopic.Rotator,8)][TestCase(MechanicTopic.Gates,21)][TestCase(MechanicTopic.Channels,25)][TestCase(MechanicTopic.Objectives,36)]
        public void ActualCampaignContentDeterminesFirstEncounter(MechanicTopic topic,int first)
        {
            var levels=LevelValidation.AllLevels();
            Assert.That(Array.FindIndex(levels,l=>MechanicGuideCatalog.Contains(l,topic))+1,Is.EqualTo(first));
        }
        [Test] public void PresentedAndExplicitlyOpenedAreIndependentAndPersistent()
        {
            var state=new MechanicGuideState(Key);state.Present(new[]{MechanicTopic.Push});
            Assert.That(state.Opened(MechanicTopic.Push),Is.False);
            state=new MechanicGuideState(Key);Assert.That(state.Presented(MechanicTopic.Push),Is.True);
            Assert.That(state.Pending(LevelValidation.AllLevels()[2]),Is.Empty);
            state.Open(MechanicTopic.Gates);Assert.That(new MechanicGuideState(Key).Opened(MechanicTopic.Gates),Is.True);
        }
        [Test] public void CompletedCampaignSuppressesBacklogWithoutFalselyMarkingRead()
        {
            var levels=LevelValidation.AllLevels();var state=new MechanicGuideState(Key,levels,39);
            foreach(var level in levels)Assert.That(state.Pending(level),Is.Empty);
            foreach(var topic in MechanicGuideCatalog.Topics){Assert.That(state.Opened(topic),Is.False);Assert.That(state.Presented(topic),Is.False);}
        }
        [Test] public void PartialSaveSuppressesOnlyPreviouslyCompletedMechanics()
        {
            var levels=LevelValidation.AllLevels();var state=new MechanicGuideState(Key,levels,6);
            Assert.That(state.Pending(levels[7]),Is.EqualTo(new[]{MechanicTopic.Rotator}));
        }
        [Test] public void SeveralMechanicsAreAcknowledgedTogetherWithoutQueue()
        {
            var state=new MechanicGuideState(Key);var level=LevelValidation.AllLevels()[39];var pending=state.Pending(level);
            Assert.That(pending.Count,Is.GreaterThan(1));state.Present(pending);Assert.That(state.Pending(level),Is.Empty);
        }
        [Test] public void GuideWritesDoNotChangeProgressOrAllowances()
        {
            var saves=new SaveService(Prefix);saves.Save(new ProgressData(9,4,8));
            PlayerPrefs.SetString(Prefix+"DailyAllowance.v1","untouched");
            var state=new MechanicGuideState(Key);state.Present(MechanicGuideCatalog.Topics);state.Open(MechanicTopic.Channels);
            var data=saves.Load(40);Assert.That(data.Current,Is.EqualTo(4));Assert.That(data.HighestCompleted,Is.EqualTo(8));
            Assert.That(PlayerPrefs.GetString(Prefix+"DailyAllowance.v1"),Is.EqualTo("untouched"));PlayerPrefs.DeleteKey(Prefix+"DailyAllowance.v1");
        }
        [TestCase(GameLanguage.English)][TestCase(GameLanguage.Turkish)]
        public void EveryTopicHasLocalizedNameRulesAndGenericExample(GameLanguage language)
        {
            foreach(var topic in MechanicGuideCatalog.Topics)
            {
                foreach(var suffix in new[]{".name",".body",".example"})
                {var key=MechanicGuideCatalog.Key(topic)+suffix;Assert.That(LocalizationCatalog.Format(key,language),Is.Not.EqualTo(key));}
                Assert.That(MechanicGuideCatalog.Example(topic).Length,Is.InRange(3,4));
            }
        }
        [UnityTest] public IEnumerator EncounterModalPersistenceDailyAndScreenshots()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();var privacy=new PendingPrivacy();game.RewardedAds=privacy;game.LoadLevel(2);yield return null;yield return null;
            var notice=Object.FindFirstObjectByType<MechanicNotice>(FindObjectsInactive.Include);
            Assert.That(notice.IsVisible,Is.True);int moves=game.Board.MovesRemaining;int hints=game.Hints.Remaining;
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/s11-07c-guide-polish/after"));
            foreach(var language in new[]{GameLanguage.English,GameLanguage.Turkish})
            {
                GameLanguageService.Shared.Select(language);
                foreach(var topic in MechanicGuideCatalog.Topics)
                {
                    notice.Show(topic);yield return null;
                    var title=notice.transform.Find("New Mechanic").GetComponent<Text>();
                    Assert.That(title.text,Does.Contain(GameLanguageService.Shared.Text(MechanicGuideCatalog.Key(topic)+".name")));
                    SprintPresentationTests.Capture("s11-07c-guide-polish/after/notice-"+language+"-"+topic+".png",720,1080,()=>AssertTextFits(notice.gameObject));
                }
            }
            GameLanguageService.Shared.Select(GameLanguage.English);notice.Show(MechanicTopic.Push);yield return null;
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/01-notice-en.png");
            GameObject.Find("Learn Mechanic").GetComponent<Button>().onClick.Invoke();yield return null;
            var panel=Object.FindFirstObjectByType<MechanicGuidePanel>(FindObjectsInactive.Include);Assert.That(panel.IsOpen,Is.True);
            var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Active&&p.Data.Type==PieceType.Normal);
            piece.GetComponentInChildren<Button>().onClick.Invoke();Assert.That(game.Board.MovesRemaining,Is.EqualTo(moves));Assert.That(game.Undo(),Is.False);
            Assert.That(game.Hints.Remaining,Is.EqualTo(hints));Assert.That(new MechanicGuideState(Key).Opened(MechanicTopic.Push),Is.True);
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/02-push-en.png");
            GameObject.Find("Dismiss Guide").GetComponent<Button>().onClick.Invoke();game.Restart();yield return null;yield return null;
            notice=Object.FindFirstObjectByType<MechanicNotice>(FindObjectsInactive.Include);Assert.That(notice.IsVisible,Is.False);
            game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();
            Assert.That(game.Settings.ReducedMotion,Is.True);
            GameObject.Find("How to Play").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(Object.FindFirstObjectByType<SettingsPanel>(FindObjectsInactive.Include).IsOpen,Is.True);
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/03-index-en.png");
            GameObject.Find("Guide Gates").GetComponent<Button>().onClick.Invoke();yield return null;
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/04-gates-en.png");
            GameLanguageService.Shared.Select(GameLanguage.Turkish);yield return null;
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/05-gates-tr.png");
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/07-gates-tr-tall.png",1080,2400);
            Assert.That(GameObject.Find("Close Settings").GetComponent<Button>().IsInteractable(),Is.False,"Underlying modal cannot retain keyboard/submit input");
            Assert.That(privacy.Rewards,Is.Zero);Assert.That(privacy.Forms,Is.Zero);
            panel=Object.FindFirstObjectByType<MechanicGuidePanel>(FindObjectsInactive.Include);
            foreach(var label in panel.GetComponentsInChildren<Text>())
                Assert.That(label.cachedTextGenerator.characterCountVisible,Is.GreaterThan(0),label.name);
            foreach(var language in new[]{GameLanguage.English,GameLanguage.Turkish})
            {
                GameLanguageService.Shared.Select(language);
                foreach(var topic in MechanicGuideCatalog.Topics)
                {
                    panel.OpenTopic(topic);yield return null;
                    SprintPresentationTests.Capture("s11-07c-guide-polish/after/"+language+"-"+topic+"-short.png",720,1080,()=>
                    {
                        foreach(var text in panel.GetComponentsInChildren<Text>())
                        {
                            var settings=text.GetGenerationSettings(text.rectTransform.rect.size);
                            var generator=new TextGenerator();generator.Populate(text.text,settings);
                            Assert.That(generator.characterCountVisible,Is.GreaterThanOrEqualTo(text.text.Replace("\n","").Length),text.text);
                        }
                    });
                    SprintPresentationTests.Capture("s11-07c-guide-polish/after/"+language+"-"+topic+"-tall.png",1080,2400,()=>AssertTextFits(panel.gameObject));
                }
            }
            GameObject.Find("Dismiss Guide").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(Object.FindFirstObjectByType<SettingsPanel>(FindObjectsInactive.Include).IsOpen,Is.True);
            SprintPresentationTests.Capture("s11-07c-guide-polish/after/06-settings-tr.png");
            game.CloseSettings();game.StartDaily(game.LocalNow().Date);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<MechanicNotice>(FindObjectsInactive.Include).IsVisible,Is.False);
            yield return new ExitPlayMode();
        }
        private static void AssertTextFits(GameObject root)
        {
            foreach(var text in root.GetComponentsInChildren<Text>())
            {
                var generator=new TextGenerator();generator.Populate(text.text,text.GetGenerationSettings(text.rectTransform.rect.size));
                Assert.That(generator.characterCountVisible,Is.GreaterThanOrEqualTo(text.text.Replace("\n","").Length),text.text);
            }
        }
        [Test] public void GateTerminologyAndThreeRuleGroupsStayExplicit()
        {
            string tr=LocalizationCatalog.Format("guide.gates.body",GameLanguage.Turkish);
            Assert.That(tr,Does.Contain("İtilen renkli taşlar"));Assert.That(tr,Does.Contain("Kutular düğmeleri tetiklemez"));
            Assert.That(tr,Does.Contain("kapalı açılır, açık kapanır"));Assert.That(tr,Does.Contain("Kapalı kapıya girilemez"));
            Assert.That(tr,Does.Not.Contain("Anahtar"));
            foreach(var language in new[]{GameLanguage.English,GameLanguage.Turkish})
                Assert.That(LocalizationCatalog.Format("guide.gates.body",language).Split(new[]{"\n\n"},StringSplitOptions.None).Length,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator DismissAndNavigationDoNotReopenNotice()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;var game=Object.FindFirstObjectByType<PrototypeGame>();
            game.LoadLevel(7);yield return null;yield return null;
            GameObject.Find("Dismiss Mechanic").GetComponent<Button>().onClick.Invoke();
            game.OpenChapter();yield return null;game.CloseChapter();yield return null;
            Assert.That(Object.FindFirstObjectByType<MechanicNotice>(FindObjectsInactive.Include).IsVisible,Is.False);
            game.LoadLevel(7);yield return null;yield return null;
            Assert.That(Object.FindFirstObjectByType<MechanicNotice>(FindObjectsInactive.Include).IsVisible,Is.False);
            yield return new ExitPlayMode();
        }
    }
}
