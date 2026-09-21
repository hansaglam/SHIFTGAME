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
    }
}
