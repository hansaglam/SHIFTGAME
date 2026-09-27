using System;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class MultiColorObjectiveTests
    {
        private GameLanguageService previousLanguage;
        [SetUp] public void EnglishPresentationLocale()
        {
            previousLanguage = GameLanguageService.Shared;
            const string key = "SHIFT.Tests.MultiColor.Language.v1";
            PlayerPrefs.SetString(key, "English");
            GameLanguageService.UseForValidation(new GameLanguageService(SystemLanguage.English, key));
        }
        private LevelData level;
        private GameObject root;
        private GameFeelSettings feel;
        private static PiecePlacement P(PieceType t, PieceColor c, int x, int y, Direction d = Direction.None)
            => new PiecePlacement(t,c,d,new GridPosition(x,y));
        private BoardManager Create(params PieceColor[] targets)
        {
            level=ScriptableObject.CreateInstance<LevelData>();
            level.Configure(4,4,4,PieceColor.Red,new[]{
                P(PieceType.Normal,PieceColor.Red,0,0,Direction.Right),P(PieceType.Exit,PieceColor.Red,1,0),
                P(PieceType.Normal,PieceColor.Blue,0,1,Direction.Right),P(PieceType.Exit,PieceColor.Blue,1,1),
                P(PieceType.Normal,PieceColor.Yellow,0,2,Direction.Right),P(PieceType.Exit,PieceColor.Yellow,1,2)});
            level.ConfigureTargetColors(targets);var b=new BoardManager();b.Load(level);return b;
        }
        private static void Tap(BoardManager b,int y) { Assert.That(b.RequestMove(new GridPosition(0,y)),Is.True);b.CompleteResolution(); }
        [TearDown] public void Cleanup(){if(root!=null)Object.DestroyImmediate(root);if(level!=null)Object.DestroyImmediate(level);GameLanguageService.UseForValidation(previousLanguage);}
        [Test] public void LegacyFallbackAndCompletionIgnoreNonTargets()
        {
            var b=Create();Assert.That(level.ResolvedTargetColors,Is.EqualTo(new[]{PieceColor.Red}));
            Tap(b,0);Assert.That(b.State,Is.EqualTo(GameState.Won));Assert.That(b.Pieces.Count(p=>p.Active&&p.Type==PieceType.Normal),Is.EqualTo(2));
        }
        [TestCase(0,1)] [TestCase(1,0)] public void BothColorsRequiredInEitherDeliveryOrder(int first,int last)
        {
            var b=Create(PieceColor.Red,PieceColor.Blue);Tap(b,first);Assert.That(b.State,Is.EqualTo(GameState.Playing));
            Tap(b,last);Assert.That(b.State,Is.EqualTo(GameState.Won));Assert.That(b.Pieces.Single(p=>p.Type==PieceType.Normal&&p.Color==PieceColor.Yellow).Active,Is.True);
        }
        [Test] public void AllThreeColorsRequired()
        {
            var b=Create(PieceColor.Red,PieceColor.Blue,PieceColor.Yellow);
            Tap(b,1);Tap(b,0);Assert.That(b.State,Is.EqualTo(GameState.Playing));Tap(b,2);Assert.That(b.State,Is.EqualTo(GameState.Won));
        }
        [Test] public void LoadSnapshotsObjectiveAndExplicitSingleOverrideWorks()
        {
            var b=Create(PieceColor.Blue);level.ConfigureTargetColors(new[]{PieceColor.Red});Tap(b,1);Assert.That(b.State,Is.EqualTo(GameState.Won));
        }
        [Test] public void InvalidObjectivesAndMissingColorExitAreRejected()
        {
            Create();foreach(var colors in new[]{new[]{PieceColor.None},new[]{PieceColor.Red,PieceColor.Red},new[]{PieceColor.Green},new[]{(PieceColor)99},new[]{PieceColor.Red,PieceColor.Blue,PieceColor.Yellow,PieceColor.Green}})
            {level.ConfigureTargetColors(colors);Assert.That(level.Validate(out _),Is.False);}
            var pieces=level.Placements.Where(p=>!(p.type==PieceType.Exit&&p.color==PieceColor.Blue)).ToArray();
            level.Configure(4,4,4,PieceColor.Red,pieces);level.ConfigureTargetColors(new[]{PieceColor.Red,PieceColor.Blue});
            Assert.That(level.Validate(out _),Is.True);Assert.That(level.ValidatePlayable(out _),Is.False);
        }
        [Test] public void FingerprintsBindAllTargetsAndPreserveLegacyCertificate()
        {
            Create();var legacy=VerifiedOptimality.Fingerprint(level);level.ConfigureTargetColors(new[]{PieceColor.Red});
            Assert.That(VerifiedOptimality.Fingerprint(level),Is.EqualTo(legacy));
            level.ConfigureTargetColors(new[]{PieceColor.Red,PieceColor.Blue});var multi=VerifiedOptimality.Fingerprint(level);
            Assert.That(multi,Is.Not.EqualTo(legacy));level.ConfigureTargetColors(new[]{PieceColor.Blue,PieceColor.Red});
            Assert.That(VerifiedOptimality.Fingerprint(level),Is.EqualTo(multi));
        }
        [Test] public void Legacy35LevelsReplayIdenticallyWithExplicitSingleObjective()
        {
            foreach(var original in LevelValidation.AllLevels().Take(35))
            {
                var clone=Object.Instantiate(original);
                try
                {
                    clone.ConfigureTargetColors(new[]{original.TargetColor});var a=new BoardManager();var b=new BoardManager();a.Load(original);b.Load(clone);
                    Assert.That(VerifiedOptimality.Fingerprint(clone),Is.EqualTo(VerifiedOptimality.Fingerprint(original)));
                    foreach(var tap in original.KnownSolution)
                    {
                        Assert.That(a.RequestMove(tap),Is.EqualTo(b.RequestMove(tap)));a.CompleteResolution();b.CompleteResolution();
                        Assert.That(a.State,Is.EqualTo(b.State));Assert.That(a.MovesRemaining,Is.EqualTo(b.MovesRemaining));
                        Assert.That(a.Actions.Select(x=>$"{x.Type}:{x.PieceId}:{x.From}:{x.To}:{x.Direction}"),Is.EqualTo(b.Actions.Select(x=>$"{x.Type}:{x.PieceId}:{x.From}:{x.To}:{x.Direction}")));
                        Assert.That(a.Pieces.Select(x=>$"{x.Id}:{x.Position}:{x.Direction}:{x.Active}:{x.GateOpen}"),Is.EqualTo(b.Pieces.Select(x=>$"{x.Id}:{x.Position}:{x.Direction}:{x.Active}:{x.GateOpen}")));
                    }
                    Assert.That(a.State,Is.EqualTo(GameState.Won));
                } finally {Object.DestroyImmediate(clone);}
            }
        }
        [TestCase(1,"Clear Red")] [TestCase(2,"Clear Both")] [TestCase(3,"Clear All")]
        public void HudRetainsLegacyAndFitsMultiColor(int count,string text)
        {
            Create(Enumerable.Range(1,count).Select(n=>(PieceColor)n).ToArray());
            root=new GameObject("Objective HUD",typeof(RectTransform),typeof(Canvas));root.GetComponent<RectTransform>().sizeDelta=new Vector2(1080,1920);
            feel=new GameFeelSettings();root.AddComponent<GameHud>().Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),level,null,feel,()=>{});
            Canvas.ForceUpdateCanvases();var goal=root.GetComponentsInChildren<Text>().Single(t=>t.name=="Goal");Assert.That(goal.text,Is.EqualTo(text));
            var discs=root.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Target Color",StringComparison.Ordinal)).ToArray();Assert.That(discs.Length,Is.EqualTo(count));
            Assert.That(goal.preferredWidth,Is.LessThanOrEqualTo(goal.rectTransform.rect.width));
            if(count==1){Assert.That(goal.fontSize,Is.EqualTo(36));Assert.That(discs[0].rectTransform.sizeDelta,Is.EqualTo(new Vector2(78,78)));Assert.That(discs[0].rectTransform.anchorMin,Is.EqualTo(new Vector2(.093f,.5f)));}
            var tr=ObjectiveText.Format(level.ResolvedTargetColors,"tr-TR");Assert.That(tr,Does.Contain("temizle"));
            goal.text=tr;if(count>1)Assert.That(goal.preferredWidth,Is.LessThanOrEqualTo(goal.rectTransform.rect.width));
        }
        [Test] public void TelemetryRecordsObjectiveWithoutChangingLegacyField()
        {
            Create(PieceColor.Red,PieceColor.Blue);var t=new TelemetryTracker(null,()=>0);t.StartLevel(level,0,null);t.Finish(true);
            Assert.That(t.Result.targetColor,Is.EqualTo("Red"));Assert.That(t.Result.targetColors,Is.EqualTo("Red+Blue"));
        }
    }
}

