using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class CompletionEntranceTests
    {
        private GameObject root;
        private GameHud hud;
        private BoardManager board;
        private LevelData level;
        private GameFeelSettings feel;
        private RectTransform panel;
        private CanvasGroup group;
        private static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        [SetUp] public void Setup()
        {
            level = LevelValidation.AllLevels().ElementAt(9);
            board = new BoardManager(); board.Load(level);
            root = new GameObject("Completion test", typeof(RectTransform));
            hud = root.AddComponent<GameHud>(); feel = new GameFeelSettings();
            hud.Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), level, null, feel, () => {});
            panel = (RectTransform)root.transform.Find("Result Panel"); group = panel.GetComponent<CanvasGroup>();
            hud.Refresh(board);
        }
        [TearDown] public void Cleanup() => Object.DestroyImmediate(root);
        private void Win()
        {
            foreach (var tap in level.KnownSolution) { Assert.That(board.RequestMove(tap), Is.True); board.CompleteResolution(); }
            Assert.That(board.State, Is.EqualTo(GameState.Won));
            hud.Refresh(board); hud.SetCompletion(true, false, true);
        }
        private void Step(float delta) => typeof(GameHud).GetMethod("AdvanceCompletion", Private).Invoke(hud, new object[]{delta});
        private void Rest()
        {
            Assert.That(group.alpha, Is.EqualTo(1));
            Assert.That(panel.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(panel.localScale, Is.EqualTo(Vector3.one));
            Assert.That(root.GetComponentInChildren<PerfectStatusFade>().Opacity, Is.EqualTo(1));
        }
        [Test] public void SuccessEntersOnceAndRefreshKeepsSpecializedCopyAndFinalPose()
        {
            var anchors = panel.anchorMin; var max = panel.anchorMax; Win();
            Assert.That(group.alpha, Is.Zero); Assert.That(panel.anchoredPosition.y, Is.EqualTo(-16));
            Step(.13f); float alpha = group.alpha;
            Assert.That(alpha, Is.InRange(.8f,.9f)); Assert.That(panel.anchoredPosition.y, Is.InRange(-3f,-1f));
            hud.ShowDailyCompletion(true,false,HintLanguage.English);
            string text = root.transform.Find("Result Panel/Status").GetComponent<Text>().text;
            hud.Refresh(board); Assert.That(group.alpha, Is.EqualTo(alpha));
            Assert.That(root.transform.Find("Result Panel/Status").GetComponent<Text>().text, Is.EqualTo(text));
            Step(.13f); Rest(); hud.Refresh(board); Step(2); Rest();
            Assert.That(panel.anchorMin, Is.EqualTo(anchors)); Assert.That(panel.anchorMax, Is.EqualTo(max));
            Assert.That(panel.Find("Reward Gleam").GetComponent<CanvasGroup>().alpha, Is.Zero, "Normal completion has no Perfect accent");
        }
        [Test] public void PerfectStaggersOnceAndDoesNotStartTheOldPulse()
        {
            Win(); var result = new AttemptMetrics { completed=true,hasOptimal=true,perfectShift=true,successfulMoves=4 };
            hud.ShowMastery(result,true);
            var fade = root.GetComponentInChildren<PerfectStatusFade>(); Assert.That(fade.Opacity, Is.Zero);
            Step(.05f); Assert.That(fade.Opacity, Is.Zero);
            Step(.1f); Assert.That(fade.Opacity, Is.InRange(.45f,.55f));
            var gleam = panel.GetComponent<RewardGleam>(); gleam.Cancel();
            hud.ShowMastery(result,true); hud.Refresh(board);
            Assert.That(panel.Find("Reward Gleam").GetComponent<CanvasGroup>().alpha, Is.Zero, "Repeated presentation does not retrigger gleam");
            Assert.That((float)typeof(GameHud).GetField("resultPulse",Private).GetValue(hud), Is.Zero);
            Step(.2f); Rest(); Assert.That(hud.MasteryVisible, Is.True);
        }
        [TestCase(false)] [TestCase(true)] public void ReducedMotionCompletesImmediatelyIncludingMidAnimationToggle(bool toggle)
        {
            feel.reducedMotion = !toggle; Win();
            if (toggle) { Step(.08f); feel.reducedMotion = true; Step(0); }
            hud.ShowMastery(new AttemptMetrics{completed=true,hasOptimal=true,perfectShift=true},true);
            Rest(); Assert.That(panel.Find("Reward Gleam").GetComponent<CanvasGroup>().alpha, Is.Zero);
        }
        [Test] public void InterruptedEntranceResetsAndAllowsAnotherTerminalTransition()
        {
            Win(); Step(.1f); hud.enabled=false; typeof(GameHud).GetMethod("OnDisable",Private).Invoke(hud,null); Rest(); hud.enabled=true; // Explicit lifecycle in non-ExecuteAlways EditMode fixture.
            board.Load(level); hud.Refresh(board); hud.SetCompletion(false,false,false); Rest();
            hud.ShowHint("test",1); Assert.That((bool)typeof(GameHud).GetField("successPresented",Private).GetValue(hud), Is.False);
            Assert.That(board.RequestMove(level.KnownSolution[0]), Is.True); hud.Refresh(board);
            Assert.That(group.alpha, Is.EqualTo(1)); Assert.That(panel.anchoredPosition, Is.EqualTo(Vector2.zero));
            board.Load(level); Win(); Assert.That(group.alpha, Is.Zero); Step(.26f); Rest();
        }
        [Test] public void LossAndHintKeepExistingPulseWithoutSuccessEntrance()
        {
            board.Load(LevelValidation.AllLevels().ElementAt(3));
            foreach(var tap in new[]{new GridPosition(2,2),new GridPosition(2,3)})
            { Assert.That(board.RequestMove(tap),Is.True);board.CompleteResolution(); }
            Assert.That(board.State,Is.EqualTo(GameState.Lost));hud.Refresh(board);hud.SetCompletion(false,false,false);
            var pulse=typeof(GameHud).GetField("resultPulse",Private);
            Assert.That((float)pulse.GetValue(hud),Is.EqualTo(feel.successDuration));
            Assert.That((bool)typeof(GameHud).GetField("successPresented",Private).GetValue(hud),Is.False);
            board.Load(level);hud.Refresh(board);hud.ShowHint("test",1);hud.SetCompletion(false,false,false);
            Assert.That((float)pulse.GetValue(hud),Is.EqualTo(feel.hudPulseDuration));
        }
        [Test] public void UnverifiedResultNeverGetsPerfectAccent()
        {
            Win(); hud.ShowMastery(new AttemptMetrics{completed=true,perfectShift=true,hasOptimal=false});
            Assert.That(hud.MasteryVisible, Is.False);
            Assert.That(panel.Find("Reward Gleam").GetComponent<CanvasGroup>().alpha, Is.Zero);
        }
        [Test] public void PerfectMeshFadeKeepsHeadingVisibleWithoutChangingGeometry()
        {
            var fade = root.GetComponentInChildren<PerfectStatusFade>(); fade.Opacity=.5f;
            using (var mesh = new VertexHelper())
            {
                mesh.AddVert(Vector3.zero,new Color32(144,100,20,255),Vector2.zero);
                mesh.AddVert(Vector3.one,new Color32(31,54,71,255),Vector2.one);
                fade.ModifyMesh(mesh); var v=new UIVertex();mesh.PopulateUIVertex(ref v,0);
                Assert.That(v.color.a, Is.EqualTo(128)); Assert.That(v.position,Is.EqualTo(Vector3.zero));
                mesh.PopulateUIVertex(ref v,1); Assert.That(v.color.a,Is.EqualTo(255));Assert.That(v.position,Is.EqualTo(Vector3.one));
            }
        }
        [TestCase(false)] [TestCase(true)] public void RapidOldReplayOrNextCallbackCannotNavigateTwice(bool next)
        {
            Object.DestroyImmediate(root); root=new GameObject("Completion callbacks",typeof(RectTransform));hud=root.AddComponent<GameHud>();
            int calls=0; System.Action action=()=>{calls++;root.SetActive(false);};
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hud.Build(font,level,null,feel,action);hud.BuildNavigation(font,null,action,()=>{},true);
            panel=(RectTransform)root.transform.Find("Result Panel");group=panel.GetComponent<CanvasGroup>();
            Win(); var button=root.transform.Find(next?"Next Level":"Restart").GetComponent<Button>();
            Assert.That(button.interactable,Is.True);
            button.onClick.Invoke();button.onClick.Invoke();Assert.That(calls,Is.EqualTo(1));
        }
    }
}
