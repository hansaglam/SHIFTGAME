using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Shift.Game.Tests
{
    public sealed class BoardVisualSystemTests
    {
        private const string Prefix="SHIFT.Tests.BoardVisual3.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix+"Settings.").Reset(); }
        [Test] public void AllLevelsRulesProgressionAndStepTwoLayoutAreUnchanged()
        {
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/BoardVisualFrozenFiles.txt")))
            {
                var pair=line.Split('|'); using var sha=SHA256.Create();
                var hash=System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,pair[0])))).Replace("-","");
                Assert.That(hash,Is.EqualTo(pair[1]),pair[0]);
            }
        }
        [Test] public void GeneratedSurfacesAreReusedAndReleasedWithTheirOwner()
        {
            var owner=new GameObject("Board art lifetime"); var art=owner.AddComponent<BoardVisualResources>();
            var sprite=art.Surface(BoardSurface.Disc,Color.red); var texture=sprite.texture;
            Assert.That(art.Surface(BoardSurface.Disc,Color.red),Is.SameAs(sprite)); Assert.That(art.SurfaceCount,Is.EqualTo(1));
            Object.DestroyImmediate(owner); Assert.That(sprite==null,Is.True); Assert.That(texture==null,Is.True);
        }
        [TestCase(128,128)][TestCase(32,32)][TestCase(64,32)][TestCase(32,64)]
        public void RotatorMeshIsClockwiseCenteredAndNondegenerate(int width,int height)
        {
            var root=new GameObject("Clockwise geometry",typeof(RectTransform));
            var mesh=new Mesh();
            try
            {
                var icon=root.AddComponent<BoardIcon>();icon.rectTransform.sizeDelta=new Vector2(width,height);icon.Configure(BoardIconShape.Clockwise);
                using var vh=new VertexHelper();
                typeof(BoardIcon).GetMethod("OnPopulateMesh",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance,null,new[]{typeof(VertexHelper)},null).Invoke(icon,new object[]{vh});
                vh.FillMesh(mesh);var v=mesh.vertices;var t=mesh.triangles;float scale=Mathf.Min(width,height);
                Assert.That(v.Length,Is.EqualTo(201));Assert.That(t.Length,Is.EqualTo(201));
                var center=icon.rectTransform.rect.center;var origin=center+Vector2.down*.06f*scale;
                for(int i=0;i<32;i++)
                {
                    Vector2 a=(Vector2)v[i*6+1]-origin,b=(Vector2)v[i*6+2]-origin;
                    Assert.That(Vector2.SignedAngle(a,b),Is.LessThan(0),"Clockwise sweep must have negative signed angle");
                }
                Vector2 tip=v[193],baseCenter=(v[192]+v[194])*.5f,forward=tip-baseCenter;
                Assert.That(forward.y,Is.LessThan(0));Assert.That(Mathf.Abs(forward.x),Is.LessThan(.001f));
                Assert.That(((baseCenter-origin).x*forward.y-(baseCenter-origin).y*forward.x),Is.LessThan(0),"Head follows clockwise tangent");
                foreach(var vertex in v)
                {
                    Assert.That(float.IsNaN(vertex.x)||float.IsNaN(vertex.y)||float.IsInfinity(vertex.x)||float.IsInfinity(vertex.y),Is.False);
                    Assert.That(Mathf.Abs(vertex.x-center.x),Is.LessThanOrEqualTo(scale*.47f));
                    Assert.That(Mathf.Abs(vertex.y-center.y),Is.LessThanOrEqualTo(scale*.40f));
                }
                for(int i=0;i<t.Length;i+=3)
                {
                    Vector3 a=v[t[i+1]]-v[t[i]],b=v[t[i+2]]-v[t[i]];
                    Assert.That(Vector3.Cross(a,b).z,Is.LessThan(-.0001f),"All triangles must be nondegenerate with consistent winding");
                }
                Assert.That(icon.raycastTarget,Is.False);
            }
            finally{Object.DestroyImmediate(mesh);Object.DestroyImmediate(root);}
        }
        [UnityTest] public IEnumerator RotatorRepresentativeBoardsAndSmallSizeCapture()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/rotator"));
            foreach(int index in new[]{7,35,39})
            {
                Assert.That(game.SelectLevel(index),Is.True);yield return new WaitForSecondsRealtime(.2f);
                Assert.That(game.CurrentLevel.Placements.Any(p=>p.type==PieceType.Rotator),Is.True);
                var icons=Object.FindObjectsByType<BoardIcon>(FindObjectsSortMode.None).Where(i=>i.Shape==BoardIconShape.Clockwise).ToArray();
                Assert.That(icons.Length,Is.GreaterThan(0));Assert.That(icons.All(i=>!i.raycastTarget),Is.True);
                SprintPresentationTests.Capture("rotator/level"+(index+1)+".png");
            }
            var canvas=GameObject.Find("SHIFT UI").transform;
            var sheet=PlaceholderVisuals.Rect("Icon comparison",canvas,Vector2.zero,Vector2.one);
            sheet.gameObject.AddComponent<Image>().color=new Color32(19,32,49,255);
            var art=sheet.gameObject.AddComponent<BoardVisualResources>();
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            string[] labels={"ROTATOR","UNDO","RESTART"};
            for(int row=0;row<3;row++)
            {
                int size=new[]{128,64,32}[row];
                PlaceholderVisuals.Label("Size",sheet,font,size+" px tile",28,Color.white,new Vector2(.05f,.80f-row*.23f),new Vector2(.95f,.85f-row*.23f));
                for(int col=0;col<3;col++)
                {
                    float x=.2f+col*.3f,y=.72f-row*.23f;
                    var tile=PlaceholderVisuals.Rect("Sample",sheet,new Vector2(x,y),new Vector2(x,y));tile.sizeDelta=Vector2.one*size;
                    var surface=tile.gameObject.AddComponent<Image>();surface.sprite=art.Surface(BoardSurface.Raised,new Color32(123,100,171,255));surface.color=Color.white;
                    if(col==0)BoardIcon.Create("Rotator",tile,BoardIconShape.Clockwise,new Color32(248,244,255,255),Vector2.one*.09f,Vector2.one*.91f);
                    else IdentityIcon.Create("Control",tile,col==1?IdentitySymbol.Undo:IdentitySymbol.Restart,Color.white,Vector2.one*.09f,Vector2.one*.91f);
                    PlaceholderVisuals.Label("Caption",sheet,font,labels[col],24,Color.white,new Vector2(x-.13f,y-.08f),new Vector2(x+.13f,y-.045f));
                }
            }
            yield return null;SprintPresentationTests.Capture("rotator/comparison.png");
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator BoardSkinsPreserveInputsStatePlaybackAndTopology()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>(); game.UnlockAllLevels();
            foreach(int index in new[]{9,31,39})
            {
                Assert.That(game.SelectLevel(index),Is.True); yield return new WaitForSecondsRealtime(.3f);
                int playable=game.CurrentLevel.Width*game.CurrentLevel.Height;
                if(game.CurrentLevel.Design.sculptedTopology) playable-=game.CurrentLevel.Placements.Count(p=>p.type==PieceType.Wall);
                Assert.That(Object.FindObjectsByType<GridCell>(FindObjectsSortMode.None).Length,Is.EqualTo(playable));
                Assert.That(GameObject.Find("Inner Lip"),Is.Not.Null);
                var pieces=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None);
                foreach(var piece in pieces)
                {
                    var button=piece.GetComponentInChildren<Button>();
                    foreach(var graphic in piece.GetComponentsInChildren<Graphic>(true))
                        Assert.That(graphic.raycastTarget,Is.EqualTo(button!=null && graphic==button.targetGraphic),graphic.name);
                    if(button!=null)
                    {
                        var body=(RectTransform)button.transform;
                        Assert.That(body.anchorMin,Is.EqualTo(new Vector2(.12f,.12f))); Assert.That(body.anchorMax,Is.EqualTo(new Vector2(.88f,.88f)));
                        Assert.That(button.targetGraphic.raycastPadding,Is.EqualTo(Vector4.one*-8));
                        var icon=piece.GetComponentsInChildren<BoardIcon>().Single(i=>i.name=="Direction Icon");
                        Assert.That(icon.Facing,Is.EqualTo(piece.Data.Direction));
                        var original=piece.Data.Direction; piece.ShowDirection(Direction.Left);
                        Assert.That(icon.Facing,Is.EqualTo(Direction.Left)); Assert.That(piece.Data.Direction,Is.EqualTo(original)); piece.ShowDirection(original);
                        var pointer=new PointerEventData(EventSystem.current) { position=RectTransformUtility.WorldToScreenPoint(null,body.TransformPoint(Vector3.zero)) };
                        var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer,hits);
                        Assert.That(hits.Count,Is.GreaterThan(0)); Assert.That(hits[0].gameObject.GetComponentInParent<Piece>(),Is.EqualTo(piece));
                    }
                    if(piece.Data.Type==PieceType.Gate)
                    {
                        bool state=piece.Data.GateOpen;
                        foreach(bool open in new[]{false,true,state})
                        {
                            piece.ShowGate(open);
                            var barrier=piece.transform.Find("Body/Closed Barrier");
                            Assert.That(barrier.gameObject.activeSelf,Is.EqualTo(!open));
                            Assert.That(piece.transform.Find("Body/Open Passage").gameObject.activeSelf,Is.EqualTo(open));
                            Assert.That(piece.Data.GateOpen,Is.EqualTo(state),"Presentation must not change the model.");
                        }
                    }
                }
                foreach(var icon in Object.FindObjectsByType<BoardIcon>(FindObjectsSortMode.None)) Assert.That(icon.raycastTarget,Is.False);
                SprintPresentationTests.Capture($"board3-level{index+1}.png");
                if(index==31) SprintPresentationTests.Capture("board3-gate-closed-and-open.png");
                if(index==39)
                {
                    Assert.That(game.Board.GetOccupant(new GridPosition(3,2)).Color,Is.EqualTo(PieceColor.Blue));
                    foreach(var badge in GameObject.Find("Board").GetComponentsInChildren<Image>().Where(i=>i.name=="Channel Badge"))
                    { Assert.That(badge.raycastTarget,Is.False); Assert.That(badge.rectTransform.anchorMax.x-badge.rectTransform.anchorMin.x,Is.GreaterThan(.5f)); }
                    SprintPresentationTests.Capture("board3-occupied-switch.png");
                }
            }
            // Existing action playback, including the normal piece's redirected arrow and both gates.
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Type==PieceType.Normal && p.Data.Position==tap);
                piece.GetComponentInChildren<Button>().onClick.Invoke();
                double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
                foreach(var gate in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Where(p=>p.Data.Type==PieceType.Gate))
                    Assert.That(gate.GateShownOpen,Is.EqualTo(gate.Data.GateOpen));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won)); Assert.That(game.Telemetry.Result.perfectShift,Is.True);
            SprintPresentationTests.Capture("board3-perfect-shift.png");
            // Preserve the occupied-switch/box and two-channel playback regression on its original fixture.
            var fixture=UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Game/Tests/Editor/Fixtures/LegacyLevel40.asset");
            var serialized=new UnityEditor.SerializedObject(game);
            serialized.FindProperty("useLevelSet").boolValue=false;serialized.FindProperty("level").objectReferenceValue=fixture;
            serialized.ApplyModifiedPropertiesWithoutUndo();game.Restart();yield return new WaitForSecondsRealtime(.25f);
            Assert.That(game.Board.GetOccupant(new GridPosition(1,1)).Type,Is.EqualTo(PieceType.PushBlock));
            foreach(var badge in GameObject.Find("Board").GetComponentsInChildren<Image>().Where(i=>i.name=="Channel Badge"))
            {Assert.That(badge.raycastTarget,Is.False);Assert.That(badge.rectTransform.anchorMax.x-badge.rectTransform.anchorMin.x,Is.GreaterThan(.5f));}
            foreach(var tap in fixture.KnownSolution)
            {
                var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Type==PieceType.Normal&&p.Data.Position==tap);
                piece.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
                foreach(var gate in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Where(p=>p.Data.Type==PieceType.Gate))Assert.That(gate.GateShownOpen,Is.EqualTo(gate.Data.GateOpen));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));Assert.That(game.Telemetry.Result.switchActivations,Is.EqualTo(2));
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator GatePolishCampaignCapturesAndCertifiedPlayback()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            var guide=new MechanicGuideState(Prefix+"MechanicGuide.v1");
            guide.Present(MechanicGuideCatalog.Topics);
            yield return new EnterPlayMode(); yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();
            const string output="s11-08b-gates/after/";
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation",output));
            foreach(int number in new[]{21,23,25,30,35,38,40})
            {
                game.LoadLevel(number-1); yield return null; yield return null;
                var notice=Object.FindFirstObjectByType<MechanicNotice>(FindObjectsInactive.Include);
                if(notice!=null&&notice.IsVisible) GameObject.Find("Dismiss Mechanic").GetComponent<Button>().onClick.Invoke();
                SprintPresentationTests.Capture(output+"level"+number+".png");
                if(number==25) SprintPresentationTests.Capture(output+"small.png",720,1080);
                foreach(var tap in game.CurrentLevel.KnownSolution)
                {
                    var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Active&&p.Data.Type==PieceType.Normal&&p.Data.Position==tap);
                    piece.GetComponentInChildren<Button>().onClick.Invoke();
                    double deadline=Time.realtimeSinceStartupAsDouble+8;
                    while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartupAsDouble<deadline)
                    {
                        if(number==21)
                        {
                            var gate=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Type==PieceType.Gate);
                            float alpha=gate.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a;
                            if(alpha>0 && alpha<.19f && !File.Exists(Path.Combine(Application.dataPath,"../Validation",output,"transition-midpoint.png")))
                                SprintPresentationTests.Capture(output+"transition-midpoint.png");
                            var pad=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).First(p=>p.Data.Type==PieceType.Switch);
                            if(pad.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a>.2f && !File.Exists(Path.Combine(Application.dataPath,"../Validation",output,"switch-activation.png")))
                                SprintPresentationTests.Capture(output+"switch-activation.png");
                        }
                        yield return null;
                    }
                    Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
                    foreach(var gate in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Where(p=>p.Data.Type==PieceType.Gate))
                        Assert.That(gate.GateShownOpen,Is.EqualTo(gate.Data.GateOpen));
                }
                Assert.That(game.Board.State,Is.EqualTo(GameState.Won),"Level "+number);
            }
            game.LoadLevel(22); yield return null;
            game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();yield return null;
            SprintPresentationTests.Capture(output+"reduced-motion.png");
            yield return new ExitPlayMode();
        }
        [TestCase(BoardIconShape.Diamond,24,12)] [TestCase(BoardIconShape.DoubleDiamond,24,12)]
        [TestCase(BoardIconShape.Diamond,128,128)] [TestCase(BoardIconShape.DoubleDiamond,128,128)]
        [TestCase(BoardIconShape.Diamond,12,24)] [TestCase(BoardIconShape.DoubleDiamond,12,24)]
        public void ChannelGeometryFitsAndDiamondsRemainSeparated(BoardIconShape shape,int width,int height)
        {
            var root=new GameObject("Channel geometry",typeof(RectTransform));var mesh=new Mesh();
            try
            {
                var icon=root.AddComponent<BoardIcon>();icon.rectTransform.sizeDelta=new Vector2(width,height);icon.Configure(shape);
                using var vh=new VertexHelper();
                typeof(BoardIcon).GetMethod("OnPopulateMesh",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance,null,new[]{typeof(VertexHelper)},null).Invoke(icon,new object[]{vh});
                vh.FillMesh(mesh);var v=mesh.vertices;var t=mesh.triangles;
                Assert.That(v.Length,Is.EqualTo(shape==BoardIconShape.Diamond?24:48));
                foreach(var vertex in v) Assert.That(icon.rectTransform.rect.Contains(vertex),Is.True,"Mesh must fit its badge");
                for(int i=0;i<t.Length;i+=3) Assert.That(Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]).z,Is.LessThan(0));
                if(shape==BoardIconShape.DoubleDiamond) Assert.That(v.Take(24).Max(x=>x.x),Is.LessThan(v.Skip(24).Min(x=>x.x)-width*.02f));
                Assert.That(icon.raycastTarget,Is.False);
            }
            finally{Object.DestroyImmediate(mesh);Object.DestroyImmediate(root);}
        }
        [UnityTest] public IEnumerator GateFeedbackRestoreInterruptionAndReducedMotion()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.LoadLevel(22);yield return null;
            var view=Object.FindFirstObjectByType<BoardView>();
            var gates=view.GetComponentsInChildren<Piece>().Where(p=>p.Data.Type==PieceType.Gate).ToArray();
            Assert.That(gates.Select(p=>p.GateShownOpen).Distinct().Count(),Is.EqualTo(2));
            foreach(var gate in gates)
            {
                Assert.That(gate.transform.Find("Body/Gate Channel Header").gameObject.activeSelf,Is.False,"Board uses one overlaid gate badge");
                bool original=gate.Data.GateOpen;
                gate.ShowGate(!original);gate.Feedback(.1f);
                Assert.That(gate.GateShownOpen,Is.EqualTo(!original),"Playback state must not snap to final model state during feedback");
                Assert.That(gate.Data.GateOpen,Is.EqualTo(original));
                // Drive the envelope deterministically: batch rendering can take longer than 0.2 seconds per frame.
                typeof(Piece).GetMethod("UpdateFeedback",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(gate,new object[]{.5f});
                Assert.That(gate.GateShownOpen,Is.EqualTo(!original));
                Assert.That(gate.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a,Is.InRange(.001f,.299f));
                gate.ShowGate(original);gate.ShowGate(!original);gate.Feedback(.1f);view.RestoreFromModel();
                Assert.That(gate.GateShownOpen,Is.EqualTo(original));
                Assert.That(gate.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a,Is.Zero);
                gate.Feedback(.1f);gate.SendMessage("OnApplicationPause",true);
                Assert.That(gate.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a,Is.Zero);
                gate.Feedback(.1f);gate.gameObject.SetActive(false);gate.gameObject.SetActive(true);
                Assert.That(gate.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a,Is.Zero);
            }
            foreach(var marker in view.GetComponentsInChildren<RectTransform>().Where(t=>t.name=="Gate Channel"))
            {
                Assert.That(marker.GetSiblingIndex(),Is.GreaterThan(view.GetComponentsInChildren<Piece>().Max(p=>p.transform.GetSiblingIndex())));
                Assert.That(marker.GetComponentsInChildren<Graphic>().All(g=>!g.raycastTarget),Is.True);
            }
            // Dispose before the first playback action: final authoritative state must be restored.
            var tap=game.CurrentLevel.KnownSolution[0];Assert.That(game.Board.RequestMove(tap),Is.True);
            var playback=view.Play(game.Board,0);Assert.That(playback.MoveNext(),Is.True);
            foreach(var gate in gates) gate.ShowGate(!gate.Data.GateOpen);
            ((System.IDisposable)playback).Dispose();
            foreach(var gate in gates) Assert.That(gate.GateShownOpen,Is.EqualTo(gate.Data.GateOpen));
            game.Board.CompleteResolution();game.Restart();yield return null;
            game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();
            foreach(var gate in Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Where(p=>p.Data.Type==PieceType.Gate))
            {
                gate.ShowGate(!gate.Data.GateOpen);gate.Feedback(.1f);
                Assert.That(gate.GateShownOpen,Is.EqualTo(!gate.Data.GateOpen));
                Assert.That(gate.transform.Find("Body/Surface Feedback").GetComponent<Image>().color.a,Is.Zero);
                gate.RestoreFromModel();Assert.That(gate.GateShownOpen,Is.EqualTo(gate.Data.GateOpen));
            }
            yield return new ExitPlayMode();
        }
    }
}
