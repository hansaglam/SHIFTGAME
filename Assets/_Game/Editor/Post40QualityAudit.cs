using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Read-only design audit. It NEVER modifies level assets, saves or Perfect Shift
    /// certificates. A valid solution is not the same thing as good player difficulty.
    /// </summary>
    public static class Post40QualityAudit
    {
        [Serializable]
        public sealed class Row
        {
            public int level, gridWidth, gridHeight, moveLimit, shortestMoves, exploredStates;
            public int meaningfulDecisions, efficiencyDecisions, pulseTaps;
            public int multiPieceTaps, gateOpenings, gateClosings, simpleMoveTaps;
            public int distinctPulseChannels, totalActions, longestReaction;
            public int pulseEvents, occupiedRemotePulseEvents, emptyRemotePulseEvents;
            public bool solvedAndProven, needsDesignReview;
            public string title, error, fingerprint;
        }

        [Serializable]
        public sealed class Matrix
        {
            public string status = "EXPERIMENTAL / EDITOR ONLY - not a shipping certificate";
            public int attempted, passed, flagged;
            public List<Row> rows = new List<Row>();
        }

        private static LevelData Get(int number)
        {
            if (number >= 41 && number <= 60) return Chapter3DraftAudit.Get(number);
            if (number >= 61 && number <= 65) return Chapter4ResonanceDrafts.Get(number);
            return number >= 66 && number <= 80
                ? Chapter4ResonanceExtendedDrafts.Get(number) : null;
        }

        // Counts whether a remote Resonance receiver was actually waiting at
        // the partner node when the pulse happened. Merely emitting a pulse is
        // not proof of a meaningful remote action. Replays BoardManager events
        // in order so occupants arriving earlier within the tap are included.
        public static void CountResonanceReceivers(LevelData level,
            IReadOnlyList<GridPosition> route, out int pulses, out int occupied)
        {
            pulses = 0;
            occupied = 0;
            var board = new BoardManager();
            board.Load(level);
            foreach (var tap in route)
            {
                var timeline = new Dictionary<int, GridPosition>();
                foreach (var piece in board.Pieces)
                    if (piece.Type == PieceType.Normal && piece.Active)
                        timeline[piece.Id] = piece.Position;
                if (!board.RequestMove(tap) || board.LastReactionWasCancelled ||
                    board.Actions.Count == 0)
                    throw new InvalidOperationException("Invalid route for Resonance audit: " + tap);
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move &&
                        board.Pieces[action.PieceId].Type == PieceType.Normal)
                        timeline[action.PieceId] = action.To;
                    else if (action.Type == BoardActionType.Deliver)
                        timeline.Remove(action.PieceId);
                    else if (action.Type == BoardActionType.ResonancePulse)
                    {
                        pulses++;
                        foreach (var remote in timeline)
                            if (remote.Value == action.To)
                            {
                                occupied++;
                                break;
                            }
                    }
                }
                board.CompleteResolution();
            }
            if (board.State != GameState.Won)
                throw new InvalidOperationException("Resonance audit route is not winning.");
        }

        [MenuItem("SHIFT/Quality/Export Level 41-80 Decision Matrix")]
        public static void Export()
        {
            var matrix = new Matrix();
            for (int number = 41; number <= 80; number++)
            {
                matrix.attempted++;
                var row = new Row { level = number };
                matrix.rows.Add(row);
                try
                {
                    var level = Get(number);
                    if (level == null) throw new InvalidOperationException("Missing LevelData.");
                    row.title = level.DisplayTitle;
                    row.gridWidth = level.Width;
                    row.gridHeight = level.Height;
                    row.moveLimit = level.MoveLimit;
                    row.fingerprint = VerifiedOptimality.Fingerprint(level);
                    if (!level.ValidatePlayable(out string error))
                        throw new InvalidOperationException(error);
                    if (!LevelValidation.VerifySolution(level, out error, out _))
                        throw new InvalidOperationException(error);
                    var proof = CampaignCertification.Prove(level, number);
                    if (!proof.complete || proof.minimum != level.KnownSolutionLength)
                        throw new InvalidOperationException("Unity shortest-path proof failed: " +
                            proof.error + " / minimum " + proof.minimum);
                    row.shortestMoves = proof.minimum;
                    row.exploredStates = proof.states;

                    var board = new BoardManager();
                    board.Load(level);
                    var channels = new HashSet<int>();
                    foreach (var tap in level.KnownSolution)
                    {
                        if (!board.RequestMove(tap) || board.LastReactionWasCancelled)
                            throw new InvalidOperationException("Recorded tap canceled: " + tap);
                        var moved = new HashSet<int>();
                        bool pulsed = false;
                        foreach (var action in board.Actions)
                        {
                            row.totalActions++;
                            if (action.Type == BoardActionType.Move) moved.Add(action.PieceId);
                            if (action.Type == BoardActionType.ResonancePulse)
                            {
                                pulsed = true;
                                channels.Add(board.Pieces[action.PieceId].Channel);
                            }
                            if (action.Type == BoardActionType.GateOpened) row.gateOpenings++;
                            if (action.Type == BoardActionType.GateClosed) row.gateClosings++;
                        }
                        row.longestReaction = Math.Max(row.longestReaction, board.Actions.Count);
                        if (moved.Count >= 2) row.multiPieceTaps++;
                        if (moved.Count == 1) row.simpleMoveTaps++;
                        if (pulsed) row.pulseTaps++;
                        board.CompleteResolution();
                    }
                    if (board.State != GameState.Won)
                        throw new InvalidOperationException("Recorded route is not a win.");
                    row.distinctPulseChannels = channels.Count;
                    if (number >= 61)
                    {
                        CountResonanceReceivers(level, level.KnownSolution,
                            out row.pulseEvents, out row.occupiedRemotePulseEvents);
                        row.emptyRemotePulseEvents =
                            row.pulseEvents - row.occupiedRemotePulseEvents;
                    }
                    var quality = new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
                    row.meaningfulDecisions = quality.meaningfulDecisions;
                    row.efficiencyDecisions = quality.efficiencyDecisions;
                    row.solvedAndProven = true;

                    // Review flags are not proofs of poor design; only a human
                    // playtest can decide whether a pattern feels repetitive.
                    row.needsDesignReview = number >= 66 &&
                        (row.simpleMoveTaps > row.shortestMoves / 2 ||
                         row.multiPieceTaps < 2 && number >= 76 ||
                         row.pulseEvents > 0 && row.occupiedRemotePulseEvents == 0);
                    if (row.needsDesignReview) matrix.flagged++;
                    matrix.passed++;
                }
                catch (Exception e)
                {
                    row.error = e.Message;
                    Debug.LogError("Quality audit level " + number + ": " + e.Message);
                }
            }
            var folder = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(folder);
            var output = Path.Combine(folder, "level41_80_unity_quality_matrix.json");
            File.WriteAllText(output, JsonUtility.ToJson(matrix, true));
            Debug.Log("SHIFT 41-80 audit: " + matrix.passed + "/40 proved; " +
                matrix.flagged + " flagged for human design review. " +
                "JSON: " + output + ". No Perfect rewards were enabled.");
            if (matrix.passed != 40)
                Debug.LogError("SHIFT 41-80 audit FAILED. Keep the candidate branch isolated.");
        }
    }
}
