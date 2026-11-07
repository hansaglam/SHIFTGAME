using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Collects evidence from the actual Unity board rules without changing a
    /// shipping registry. A Level 41+ Perfect Shift can only be enabled after
    /// a human-reviewed, fingerprint-bound certificate is approved.
    /// </summary>
    public static class Chapter3QualityReport
    {
        [Serializable]
        public sealed class Record
        {
            public int number;
            public string level, fingerprint, error;
            public bool certified;
            public int shortestMoves, moveLimit, states, maxReaction,
                meaningfulDecisions, efficiencyDecisions;
            public long winningPaths;
            public double randomSolveProbability, optimalDiscoveryProbability;
            public bool allowsLongerWin;
        }

        [Serializable] public sealed class Report
        {
            public string status = "UNITY CANDIDATE QA - NOT A SHIPPING CERTIFICATE";
            public List<Record> levels = new List<Record>();
        }

        [MenuItem("SHIFT/Chapter 3/Export Quality and Perfect Candidate Report")]
        public static void Export()
        {
            var report = new Report();
            int pass = 0;
            for (int number = 41; number <= 60; number++)
            {
                var row = new Record { number = number };
                report.levels.Add(row);
                var level = Chapter3DraftAudit.Get(number);
                if (level == null)
                {
                    row.error = "Missing LevelData asset.";
                    continue;
                }
                row.level = level.name;
                row.fingerprint = VerifiedOptimality.Fingerprint(level);
                row.moveLimit = level.MoveLimit;
                try
                {
                    if (!LevelValidation.VerifySolution(level, out string error, out _))
                        throw new InvalidOperationException(error);
                    var proof = CampaignCertification.Prove(level, number);
                    if (!proof.complete)
                        throw new InvalidOperationException(proof.error);
                    if (proof.minimum != level.KnownSolutionLength)
                        throw new InvalidOperationException("Recorded route is not shortest.");
                    var quality = new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
                    row.certified = true;
                    row.shortestMoves = proof.minimum;
                    row.states = proof.states;
                    row.maxReaction = quality.routeMaxDepth;
                    row.meaningfulDecisions = quality.meaningfulDecisions;
                    row.efficiencyDecisions = quality.efficiencyDecisions;
                    row.winningPaths = quality.solutionPaths;
                    row.randomSolveProbability = quality.randomSolveProbability;
                    row.optimalDiscoveryProbability = quality.optimalDiscoveryProbability;

                    if (number >= 51)
                    {
                        var alternate = Chapter3AlternateSolutions.Get(number);
                        if (alternate == null || alternate.Length != proof.minimum + 1 ||
                            alternate.Length > level.MoveLimit)
                            throw new InvalidOperationException("Missing/invalid longer route contract.");
                        var board = new BoardManager(); board.Load(level);
                        foreach (var tap in alternate)
                        {
                            if (!board.RequestMove(tap) || board.LastReactionWasCancelled ||
                                board.ReactionDepth == 0)
                                throw new InvalidOperationException("Invalid alternate tap: " + tap);
                            board.CompleteResolution();
                        }
                        if (board.State != GameState.Won)
                            throw new InvalidOperationException("Alternate route did not win.");
                        row.allowsLongerWin = true;
                    }
                    pass++;
                }
                catch (Exception e)
                {
                    row.certified = false;
                    row.error = e.Message;
                }
            }
            // Evidence-only JSON. Nothing in this menu affects runtime Perfect rewards.
            var directory = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "chapter3-unity-candidate-quality-report.json");
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log("Chapter 3 candidate quality report: " + pass +
                "/20 passed. Exported to " + path +
                ". Do not publish or enable Perfect until reviewed.");
            if (pass < 20)
                Debug.LogError("Chapter 3 quality gate FAILED for " + (20 - pass) + " draft levels.");
        }
    }
}
