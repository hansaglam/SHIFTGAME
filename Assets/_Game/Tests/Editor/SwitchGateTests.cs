using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class SwitchGateTests
    {
        private LevelData level;
        private BoardManager board;
        private static PiecePlacement P(PieceType t,int x,int y,Direction d=Direction.None,PieceColor c=PieceColor.None,int channel=0,bool open=false)
            => new PiecePlacement(t,c,d,new GridPosition(x,y),channel,open);
        [SetUp] public void Setup() { level=ScriptableObject.CreateInstance<LevelData>(); board=new BoardManager(); }
        [TearDown] public void Cleanup() => Object.DestroyImmediate(level);
        private void Load(params PiecePlacement[] pieces) { level.Configure(6,6,10,PieceColor.Red,pieces); board.Load(level); }
        private void Tap(int x,int y) { Assert.That(board.RequestMove(new GridPosition(x,y)),Is.True); board.CompleteResolution(); }
        [Test] public void EntryTogglesOnceAndRestartRestoresInitialState()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Switch,1,1,channel:1),P(PieceType.Gate,3,1,channel:1));
            Tap(0,1); Assert.That(board.GetTerrain(new GridPosition(3,1)).GateOpen,Is.True);
            Assert.That(board.ReactionDepth,Is.EqualTo(3));
            CollectionAssert.AreEqual(new[]{BoardActionType.Move,BoardActionType.SwitchActivated,BoardActionType.GateOpened},board.Actions.Select(a=>a.Type));
            Tap(1,1); Assert.That(board.ReactionDepth,Is.EqualTo(1));
            board.Load(level); Assert.That(board.GetTerrain(new GridPosition(3,1)).GateOpen,Is.False);
        }
        [Test] public void StartingOnSwitchDoesNotActivate()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Switch,0,1,channel:1),P(PieceType.Gate,3,1,channel:1));
            Tap(0,1); Assert.That(board.ReactionDepth,Is.EqualTo(1)); Assert.That(board.Pieces[2].GateOpen,Is.False);
        }
        [Test] public void RepeatedEntriesToggleAllLinkedGatesButNotOtherChannel()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Switch,1,1,channel:1),P(PieceType.Switch,2,1,channel:1),
                P(PieceType.Gate,4,1,channel:1),P(PieceType.Gate,4,2,channel:1,open:true),P(PieceType.Switch,0,4,channel:2),P(PieceType.Gate,4,4,channel:2));
            Tap(0,1); Assert.That(board.Pieces[3].GateOpen,Is.True); Assert.That(board.Pieces[4].GateOpen,Is.False); Assert.That(board.Pieces[6].GateOpen,Is.False);
            Tap(1,1); Assert.That(board.Pieces[3].GateOpen,Is.False); Assert.That(board.Pieces[4].GateOpen,Is.True);
        }
        [TestCase(false)] [TestCase(true)] public void GateBlocksOrPassesBothMovableKinds(bool open)
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.PushBlock,1,1),P(PieceType.Gate,2,1,channel:1,open:open),P(PieceType.Switch,0,4,channel:1));
            Tap(0,1); Assert.That(board.Pieces[1].Position,Is.EqualTo(new GridPosition(open?2:1,1)));
            Load(P(PieceType.Normal,1,1,Direction.Right,PieceColor.Red),P(PieceType.Gate,2,1,channel:1,open:open),P(PieceType.Switch,0,4,channel:1));
            Tap(1,1); Assert.That(board.Pieces[0].Position,Is.EqualTo(new GridPosition(open?2:1,1)));
        }
        [Test] public void PushBlockDoesNotActivatePad()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.PushBlock,1,1),P(PieceType.Switch,2,1,channel:1),P(PieceType.Gate,4,1,channel:1));
            Tap(0,1); Assert.That(board.Pieces[3].GateOpen,Is.False); Assert.That(board.Actions.Any(a=>a.Type==BoardActionType.SwitchActivated),Is.False);
        }
        [Test] public void PushedNormalActivatesSwitch()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Normal,1,1,Direction.Up,PieceColor.Yellow),P(PieceType.Switch,2,1,channel:1),P(PieceType.Gate,4,1,channel:1));
            Tap(0,1); Assert.That(board.Pieces[3].GateOpen,Is.True); Assert.That(board.Actions.Count(a=>a.Type==BoardActionType.SwitchActivated),Is.EqualTo(1));
        }
        [Test] public void UnsafePushRestoresGateOccupantsAndBudget()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Normal,1,1,Direction.Right,PieceColor.Yellow),
                P(PieceType.Gate,1,1,channel:1,open:true),P(PieceType.Switch,2,1,channel:1));
            Tap(0,1); Assert.That(board.LastReactionWasCancelled,Is.True); Assert.That(board.Actions,Is.Empty);
            Assert.That(board.Pieces[2].GateOpen,Is.True); Assert.That(board.Pieces[1].Position,Is.EqualTo(new GridPosition(1,1))); Assert.That(board.MovesRemaining,Is.EqualTo(10));
        }
        [Test] public void OccupiedGateCanCloseAndOccupantCanLeave()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Switch,1,1,channel:1),P(PieceType.Normal,3,2,Direction.Right,PieceColor.Blue),P(PieceType.Gate,3,2,channel:1,open:true));
            Tap(0,1); Assert.That(board.Pieces[3].GateOpen,Is.False); Tap(3,2); Assert.That(board.Pieces[2].Position,Is.EqualTo(new GridPosition(4,2)));
        }
        [TestCase(0)] [TestCase(3)] public void InvalidChannelsAreRejected(int channel)
        {
            level.Configure(4,4,3,PieceColor.Red,new[]{P(PieceType.Normal,0,0,Direction.Right,PieceColor.Red),P(PieceType.Switch,1,1,channel:channel),P(PieceType.Gate,2,1,channel:channel)});
            Assert.That(level.Validate(out _),Is.False);
        }
        [Test] public void MissingRelationshipsAndDuplicateTerrainAreRejected()
        {
            level.Configure(4,4,3,PieceColor.Red,new[]{P(PieceType.Normal,0,0,Direction.Right,PieceColor.Red),P(PieceType.Gate,2,1,channel:1)}); Assert.That(level.Validate(out _),Is.False);
            level.Configure(4,4,3,PieceColor.Red,new[]{P(PieceType.Normal,0,0,Direction.Right,PieceColor.Red),P(PieceType.Gate,2,1,channel:1),P(PieceType.Switch,2,1,channel:1)}); Assert.That(level.Validate(out _),Is.False);
        }
        [Test] public void TelemetryCountsOnlyCommittedStateActions()
        {
            Load(P(PieceType.Normal,0,1,Direction.Right,PieceColor.Red),P(PieceType.Switch,1,1,channel:1),P(PieceType.Switch,2,1,channel:1),P(PieceType.Gate,4,1,channel:1));
            foreach(var provider in new IAnalyticsService[]{new LocalAnalyticsService(),new NullAnalyticsService()})
            {
                board.Load(level); var t=new TelemetryTracker(provider,()=>0); t.StartLevel(level,20,null);
                Tap(0,1); t.StateActions(board.Actions); t.AcceptedTap(board.ReactionDepth,board.MovesRemaining,false);
                Tap(1,1); t.StateActions(board.Actions); t.AcceptedTap(board.ReactionDepth,board.MovesRemaining,false);
                var a=t.Snapshot(); Assert.That(a.switchActivations,Is.EqualTo(2)); Assert.That(a.gateOpens,Is.EqualTo(1)); Assert.That(a.gateCloses,Is.EqualTo(1)); Assert.That(a.totalDepth,Is.EqualTo(6));
            }
        }
    }
}
