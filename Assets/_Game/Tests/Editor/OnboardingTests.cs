using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class OnboardingTests
    {
        private const string Prefix="SHIFT.Tests.Onboarding.";
        private const string Key=Prefix+"Onboarding.v1";
        [SetUp] public void Reset() { FirstLaunchOnboardingService.ResetForTesting(Key); }
        [TearDown] public void Cleanup() { PlayerPrefs.DeleteKey(Key); }
        [Test] public void FreshInstallBeginsOnceAndDoesNotPersistUntilFinished()
        {
            var s=new FirstLaunchOnboardingService(Key); Assert.That(s.Completed,Is.False);
            Assert.That(s.Begin(),Is.True); Assert.That(s.Begin(),Is.False); Assert.That(PlayerPrefs.HasKey(Key),Is.False);
        }
        [TestCase("")][TestCase("garbage")][TestCase("completed")][TestCase("1")][TestCase("Completed ")]
        public void MalformedStateReopensSafely(string value)
        { PlayerPrefs.SetString(Key,value); Assert.That(new FirstLaunchOnboardingService(Key).Completed,Is.False); }
        [TestCase(true)][TestCase(false)]
        public void CompletionPersistsBeforeCallbackAndCannotTransitionTwice(bool skip)
        {
            var s=new FirstLaunchOnboardingService(Key); s.Begin(); if(!skip)for(int i=0;i<3;i++)s.Next();
            int transitions=0;
            Action done=()=>{Assert.That(new FirstLaunchOnboardingService(Key).Completed,Is.True);transitions++;};
            Assert.That(s.Finish(skip,done),Is.True); Assert.That(s.Finish(skip,done),Is.False);
            Assert.That(transitions,Is.EqualTo(1)); Assert.That(new FirstLaunchOnboardingService(Key).Begin(),Is.False);
        }
        [Test] public void NavigationBoundsAndRevisitsProduceOnlyOneViewPerPage()
        {
            var events=new List<string>();var s=new FirstLaunchOnboardingService(Key,(n,p)=>events.Add(n+":"+p));
            Assert.That(s.Next(),Is.False);s.Begin();Assert.That(s.Back(),Is.False);
            Assert.That(s.Finish(false,null),Is.False);
            for(int i=1;i<4;i++){Assert.That(s.Next(),Is.True);Assert.That(s.PageIndex,Is.EqualTo(i));}
            Assert.That(s.Next(),Is.False); Assert.That(s.Finish(true,null),Is.False);
            for(int i=2;i>=0;i--){Assert.That(s.Back(),Is.True);Assert.That(s.PageIndex,Is.EqualTo(i));}
            for(int i=0;i<3;i++)s.Next();s.Finish(false,null);s.Finish(false,null);
            Assert.That(events.Count(x=>x.StartsWith("onboarding_started")),Is.EqualTo(1));
            Assert.That(events.Count(x=>x.StartsWith("onboarding_page_viewed")),Is.EqualTo(4));
            Assert.That(events.Count(x=>x.StartsWith("onboarding_completed")),Is.EqualTo(1));
        }
        [Test] public void SkipAnalyticsOnceAndNoCompletionEvent()
        {
            var events=new List<string>();var s=new FirstLaunchOnboardingService(Key,(n,p)=>events.Add(n));
            s.Begin();s.Finish(true,null);s.Finish(true,null);Assert.That(events,Is.EqualTo(new[]{"onboarding_started","onboarding_page_viewed","onboarding_skipped"}));
        }
        [TestCase(GameLanguage.English)][TestCase(GameLanguage.Turkish)]
        public void AllCopyAndTurkishGlyphsCovered(GameLanguage language)
        {
            var keys=LocalizationCatalog.Keys.Where(k=>k.StartsWith("onboarding.")).ToArray();Assert.That(keys.Length,Is.EqualTo(25));
            foreach(var k in keys)Assert.That(LocalizationCatalog.Format(k,language),Is.Not.Empty,k);
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");font.RequestCharactersInTexture("ÇçĞğİıÖöŞşÜü",32);
            foreach(char ch in "ÇçĞğİıÖöŞşÜü")Assert.That(font.HasCharacter(ch),Is.True,ch.ToString());
        }
        [Test] public void TelemetryUsesExistingProviderWithoutAttemptOrIdentifiers()
        {
            var local=new LocalAnalyticsService();var telemetry=new TelemetryTracker(local,()=>0);telemetry.StartSession();
            telemetry.OnboardingEvent("onboarding_started",0);var e=local.Events.Last();
            Assert.That(e.name,Is.EqualTo("onboarding_started"));Assert.That(e.pageIndex,Is.Zero);Assert.That(e.source,Is.EqualTo("first_launch"));
            Assert.That(e.language,Is.EqualTo(GameLanguageService.Shared.CurrentLanguage.ToString()));Assert.That(e.attempt,Is.Null);
        }
        [Test] public void GameplayAssetsRulesCertificatesDailyAndAdCodeAreUnchanged()
        {
            using var sha=SHA256.Create();
            foreach(string line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/OnboardingProtectedFiles.txt")))
            {
                int split=line.IndexOf(' ');string hash=line.Substring(0,split),path=line.Substring(split+1);
                string actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,"..",path)))).Replace("-","").ToLowerInvariant();
                Assert.That(actual,Is.EqualTo(hash),path);
            }
        }
        private sealed class NativeBusyAds : IRewardedAdService,IPrivacyChoices
        {
            public bool Busy=true; public int Requests;
            public bool IsRewardedAdAvailable=>false;
            public bool IsPrivacyOptionsRequired=>false;
            public bool IsBusy=>Busy;
            public void ShowPrivacyOptions(Action<bool> done){Requests++;done(false);}
            public void ShowRewardedAd(RewardReason reason,Action<bool> done){Requests++;done(false);}
        }
        [UnityTest] public IEnumerator NativeConsentAtStartupDefersFirstPageAndAnalytics()
        {
            var root=new GameObject("Onboarding native wait test",typeof(RectTransform));
            var panel=root.AddComponent<FirstLaunchOnboardingPanel>();
            var events=new List<string>();var state=new FirstLaunchOnboardingService(Key,(n,p)=>events.Add(n));
            var circle=PlaceholderVisuals.CreateCircle();var rounded=PlaceholderVisuals.CreateRounded();bool busy=true;
            panel.Build(state,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),circle,rounded,LevelValidation.AllLevels(),new GameFeelSettings(),()=>busy,()=>{},()=>{});
            Assert.That(panel.GetComponent<CanvasGroup>().alpha,Is.Zero);Assert.That(events,Is.Empty);
            Assert.That(root.GetComponentsInChildren<OnboardingBoardPreview>().Length,Is.Zero);
            panel.Primary();Assert.That(state.PageIndex,Is.Zero);
            busy=false;
            typeof(FirstLaunchOnboardingPanel).GetMethod("Update",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(panel,null);
            Assert.That(events,Is.EqualTo(new[]{"onboarding_started","onboarding_page_viewed"}));
            Assert.That(root.GetComponentsInChildren<OnboardingBoardPreview>().Length,Is.EqualTo(1));
            Object.DestroyImmediate(root);Object.DestroyImmediate(circle.texture);Object.DestroyImmediate(circle);Object.DestroyImmediate(rounded.texture);Object.DestroyImmediate(rounded);
            yield return null;
        }
        private static void FreshScene()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            FirstLaunchOnboardingService.ResetForTesting(Key);
        }
        [UnityTest] public IEnumerator FourPagesBothLanguagesNativeWaitIsolationAndRealCaptures()
        {
            FreshScene();yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();var panel=Object.FindFirstObjectByType<FirstLaunchOnboardingPanel>();
            Assert.That(game.IsOnboardingActive,Is.True);Assert.That(panel,Is.Not.Null);Assert.That(game.Board.Pieces,Is.Null);
            var ad=new NativeBusyAds();game.RewardedAds=ad;yield return new WaitForSecondsRealtime(.1f);
            Assert.That(panel.GetComponent<CanvasGroup>().alpha,Is.Zero);panel.Primary();Assert.That(panel.PageIndex,Is.Zero);
            game.Restart();Assert.That(game.SelectLevel(1),Is.False);Assert.That(game.StartDaily(DateTime.Now),Is.False);
            Assert.That(game.Board.Pieces,Is.Null);Assert.That(Object.FindObjectsByType<FirstLaunchOnboardingPanel>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            int hints=game.Allowances.HintsRemaining,undos=game.Allowances.UndosRemaining;
            int current=game.Progress.Current,complete=game.Progress.HighestCompleted;
            string daily=PlayerPrefs.GetString(Prefix+"Daily.v1.History","missing");
            bool sound=game.Settings.Sound,haptic=game.Settings.Haptics,motion=game.Settings.ReducedMotion;
            ad.Busy=false;yield return null;
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/onboarding-polish"));
            for(int page=0;page<4;page++)
            {
                Assert.That(panel.PageIndex,Is.EqualTo(page));
                foreach(var language in new[]{GameLanguage.English,GameLanguage.Turkish})
                {
                    GameLanguageService.Shared.Select(language);yield return new WaitForSecondsRealtime(.2f);
                    Assert.That(panel.PageIndex,Is.EqualTo(page));
                    Assert.That(GameObject.Find("Onboarding Title").GetComponent<Text>().text,Is.EqualTo(LocalizationCatalog.Format("onboarding."+new[]{"welcome","how","chain","ready"}[page]+".title",language)));
                    Capture((page*2+1+(language==GameLanguage.Turkish?1:0)).ToString("00")+"-page"+(page+1)+"-"+language+".png");
                }
                Assert.That(panel.GetComponentsInChildren<Piece>().Length,Is.Zero);Assert.That(panel.GetComponentsInChildren<BoardView>().Length,Is.Zero);
                Assert.That(panel.GetComponentsInChildren<Button>().Length,Is.EqualTo(2));
                Assert.That(GameObject.Find("Onboarding Secondary").GetComponent<Button>().IsInteractable(),Is.True);
                if(page==2) Assert.That(GameObject.Find("Chain Sequence").transform.childCount,Is.EqualTo(3));
                if(page==3)
                {
                    var hero=panel.GetComponentInChildren<OnboardingBoardPreview>();
                    Assert.That(hero.transform.Find("Preview Normal"),Is.Not.Null);
                    Assert.That(hero.transform.Find("Preview Direction"),Is.Not.Null);
                    Assert.That(hero.transform.Find("Preview Exit"),Is.Not.Null);
                    Assert.That(hero.transform.Find("Preview Gate"),Is.Null);
                    Assert.That(hero.transform.Find("Preview Switch"),Is.Null);
                    Assert.That(hero.Source,Is.Null,"Illustration must not mutate a campaign asset");
                }
                foreach(var preview in panel.GetComponentsInChildren<OnboardingBoardPreview>())
                    Assert.That(preview.GetComponentsInChildren<Graphic>().All(x=>!x.raycastTarget),Is.True);
                for(int i=0;i<4;i++)Assert.That(GameObject.Find("Page Indicator "+i).GetComponent<Image>().color,Is.EqualTo(i==page?IdentityStyle.Teal:(Color)new Color32(178,194,197,255)));
                if(page<3)panel.Primary();
            }
            Capture("09-final-clean-cta.png");
            panel.Secondary();GameLanguageService.Shared.Select(GameLanguage.English);yield return null;panel.Primary();Assert.That(panel.PageIndex,Is.EqualTo(3));
            yield return new WaitForSecondsRealtime(.2f);
            Capture("12-live-language-switch.png");
            Capture("11-short-portrait.png",1080,1600);
            Capture("15-tall-safe-area.png",1080,2400,true);
            Assert.That(game.Allowances.HintsRemaining,Is.EqualTo(hints));Assert.That(game.Allowances.UndosRemaining,Is.EqualTo(undos));
            Assert.That(game.Hints.Stage(game.CurrentLevel),Is.Zero);Assert.That(game.Board.CanUndo,Is.False);
            Assert.That(game.Progress.Current,Is.EqualTo(current));Assert.That(game.Progress.HighestCompleted,Is.EqualTo(complete));
            Assert.That(PlayerPrefs.GetString(Prefix+"Daily.v1.History","missing"),Is.EqualTo(daily));
            Assert.That(game.Settings.Sound,Is.EqualTo(sound));Assert.That(game.Settings.Haptics,Is.EqualTo(haptic));Assert.That(game.Settings.ReducedMotion,Is.EqualTo(motion));Assert.That(ad.Requests,Is.Zero);
            panel.Primary();panel.Primary();yield return null;
            Assert.That(game.IsOnboardingActive,Is.False);Assert.That(new FirstLaunchOnboardingService(Key).Completed,Is.True);
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level01"));Assert.That(game.IsDaily,Is.False);
            Capture("13-first-level-destination.png");game.Restart();yield return null;Assert.That(Object.FindFirstObjectByType<FirstLaunchOnboardingPanel>(),Is.Null);
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator ReducedMotionSkipAndExistingProgressContinueSafely()
        {
            FreshScene();var progress=new LevelProgression(40,new SaveService(Prefix),LevelValidation.AllLevels());
            progress.Complete(0);progress.Select(1);
            new SettingsService(Prefix+"Settings.").Save(true,false,true);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();var panel=Object.FindFirstObjectByType<FirstLaunchOnboardingPanel>();
            panel.Primary();yield return null;Assert.That(panel.PageIndex,Is.EqualTo(1));
            Assert.That(GameObject.Find("Onboarding Page 1").GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            Capture("10-reduced-motion.png");
            panel.Primary();yield return null;
            var cue=GameObject.Find("Chain Sequence");Assert.That(cue,Is.Not.Null);
            Assert.That(cue.GetComponentsInChildren<BoardIcon>().Length,Is.EqualTo(3));
            var positions=cue.GetComponentsInChildren<BoardIcon>().Select(x=>x.rectTransform.anchoredPosition).ToArray();
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(cue.GetComponentsInChildren<BoardIcon>().Select(x=>x.rectTransform.anchoredPosition),Is.EqualTo(positions));
            Capture("16-page3-reduced-motion.png");
            GameObject.Find("Onboarding Secondary").GetComponent<Button>().onClick.Invoke();
            panel.Secondary();panel.Secondary();panel.Secondary();yield return null;
            Assert.That(game.IsOnboardingActive,Is.False);Assert.That(game.Progress.HighestCompleted,Is.Zero);Assert.That(game.Progress.Current,Is.EqualTo(1));
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level02"));
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator CompletedSaveSurvivesSceneReloadReturningLaunch()
        {
            FreshScene();PlayerPrefs.SetString(Key,"Completed");
            yield return new EnterPlayMode();yield return null;
            Assert.That(Object.FindFirstObjectByType<FirstLaunchOnboardingPanel>(),Is.Null);
            var game=Object.FindFirstObjectByType<PrototypeGame>();Assert.That(game.IsOnboardingActive,Is.False);
            Capture("14-returning-player.png");
            yield return new ExitPlayMode();
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            Assert.That(Object.FindFirstObjectByType<FirstLaunchOnboardingPanel>(),Is.Null);
            yield return new ExitPlayMode();
        }
        private static void Capture(string name,int width=1080,int height=1920,bool safeInsets=false)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Validation/onboarding-polish"));
            Directory.CreateDirectory(directory);
            // Batch mode has no presented Game view; render the actual UI through a portrait camera target.
            var canvas = GameObject.Find("SHIFT UI").GetComponent<Canvas>();
            var scaler = canvas.GetComponent<CanvasScaler>();
            var camera = GameObject.Find("Background Camera").GetComponent<Camera>();
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera;
            float previousScale = canvas.scaleFactor, previousPlane = canvas.planeDistance;
            int previousMask = camera.cullingMask;
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            var safe=canvas.transform.Find("Safe Area") as RectTransform;
            var oldMin=safe.anchorMin; var oldMax=safe.anchorMax;
            try
            {
                camera.targetTexture = target; camera.cullingMask = -1;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                scaler.enabled = false; canvas.scaleFactor = 1; Canvas.ForceUpdateCanvases();
                if(safeInsets) {safe.anchorMin=new Vector2(.025f,.04f);safe.anchorMax=new Vector2(.975f,.955f);Canvas.ForceUpdateCanvases();}
                var panel=canvas.GetComponentInChildren<FirstLaunchOnboardingPanel>();
                if(panel!=null)
                {
                    foreach(var text in panel.GetComponentsInChildren<Text>())
                    {
                        var settings=text.GetGenerationSettings(text.rectTransform.rect.size);
                        var generator=new TextGenerator();generator.Populate(text.text,settings);
                        Assert.That(generator.characterCountVisible,Is.GreaterThanOrEqualTo(text.text.Count(c=>!char.IsWhiteSpace(c))),text.name+": "+text.text);
                    }
                    var a=GameObject.Find("Onboarding Primary").GetComponent<RectTransform>();
                    var b=GameObject.Find("Onboarding Secondary").GetComponent<RectTransform>();
                    Assert.That(a.rect.height,Is.EqualTo(b.rect.height).Within(.1f));Assert.That(a.rect.height,Is.GreaterThan(85));
                    Assert.That(a.rect.width,Is.GreaterThan(b.rect.width));
                }
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                    new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = target });
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(directory, name), image.EncodeToPNG());
            }
            finally
            {
                safe.anchorMin=oldMin;safe.anchorMax=oldMax;
                RenderTexture.active = previousActive; camera.targetTexture = previousTarget; camera.cullingMask = previousMask;
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = previousPlane;
                canvas.scaleFactor = previousScale; scaler.enabled = true; Canvas.ForceUpdateCanvases();
                RenderTexture.ReleaseTemporary(target); Object.DestroyImmediate(image);
            }
        }
    }
}
