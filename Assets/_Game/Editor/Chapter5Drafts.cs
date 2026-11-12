using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Source-controlled Level 81-100 design candidates.
    // Does not mutate campaign scenes, PlayerPrefs or Perfect Shift registry.
    public static class Chapter5Drafts
    {
        public const string Folder =
            "Assets/_Game/Data/Levels/Chapter5_ConfluenceDrafts/";

        public static readonly string[] Names =
        {
            "Level81_FirstLink",
            "Level82_Countercurrent",
            "Level83_SplitEcho",
            "Level84_DistantPreparation",
            "Level85_CrossedRoute",
            "Level86_HeldInMotion",
            "Level87_BorrowedPush",
            "Level88_SharedPassage",
            "Level89_ReturnPath",
            "Level90_TwoKeys",
            "Level91_ThreeCurrents",
            "Level92_SilentRelay",
            "Level93_HiddenCrossing",
            "Level94_SecondChance",
            "Level95_Interlocked",
            "Level96_BackThrough",
            "Level97_LateHandoff",
            "Level98_TheLastReturn",
            "Level99_ConvergenceTest",
            "Level100_TheOpenHorizon"
        };

        // Editor QA only: independently found valid +1-tap winning routes.
        // Real players may find other routes. These are not in-game hints.
        private static readonly string[] LongerWins =
        {
            "5141303132341415",
            "1525354342520451",
            "0111212313234315",
            "5141312214121415",
            "5051534235322212",
            "6473635351312111",
            "646251413143112123",
            "514121121333161435",
            "1627262546454342",
            "544765464442416362",
            "606231564446112111",
            "654523324253515251",
            "12133224114234445452",
            "06163625244443414222",
            "60623121441156453646",
            "5141211233341314155035",
            "34171546563366213231",
            "75453323135354452515",
            "4253646251533341312123",
            "01213143161516625363"
        };

        public static LevelData Get(int number)
        {
            if (number < 81 || number > 100) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(
                Folder + Names[number - 81] + ".asset");
        }

        public static GridPosition[] AlternateWinningRoute(int number)
        {
            if (number < 81 || number > 100) return null;
            string value = LongerWins[number - 81];
            var positions = new GridPosition[value.Length / 2];
            for (int i = 0; i < positions.Length; i++)
                positions[i] = new GridPosition(value[2 * i] - '0',
                    value[2 * i + 1] - '0');
            return positions;
        }

        public static bool CheckAlternate(LevelData level, int number,
            out string error)
        {
            error = null;
            var longer = AlternateWinningRoute(number);
            if (level == null || longer == null ||
                longer.Length != level.KnownSolutionLength + 1 ||
                longer.Length > level.MoveLimit)
            {
                error = "Alternative must be exactly one tap longer, under move limit.";
                return false;
            }
            var board = new BoardManager();
            board.Load(level);
            foreach (var tap in longer)
            {
                int before = board.MovesRemaining;
                if (!board.RequestMove(tap) || board.LastReactionWasCancelled ||
                    board.ReactionDepth <= 0 ||
                    board.MovesRemaining != before - 1)
                {
                    error = "Invalid extra-winning-route tap: " + tap;
                    return false;
                }
                board.CompleteResolution();
            }
            if (board.State != GameState.Won)
            {
                error = "Alternative route did not win.";
                return false;
            }
            return true;
        }

        [Serializable]
        public sealed class CandidateProof
        {
            public int number, shortest, longer, explored, multiPieceTaps,
                productiveRemote, openedGates, longestSoloRun;
            public bool passed;
            public string error;
        }

        [Serializable]
        public sealed class CandidateReport
        {
            public string status = "DESIGN CANDIDATES - Unity proof only, no player QA";
            public List<CandidateProof> levels = new List<CandidateProof>();
        }

        [MenuItem("SHIFT/Chapter 5/Validate All Draft Levels 81-100")]
        public static void ValidateAll()
        {
            var report = new CandidateReport();
            int passed = 0;
            for (int n = 81; n <= 100; n++)
            {
                var record = new CandidateProof { number = n };
                report.levels.Add(record);
                var level = Get(n);
                if (level == null)
                {
                    record.error = "Missing LevelData asset.";
                    Debug.LogError("Level " + n + ": " + record.error);
                    continue;
                }
                try
                {
                    if (!level.ValidatePlayable(out string error))
                        throw new InvalidOperationException(error);
                    if (!LevelValidation.VerifySolution(level, out error,
                        out int reactionDepth))
                        throw new InvalidOperationException(error);
                    var proof = CampaignCertification.Prove(level, n);
                    if (!proof.complete || proof.minimum != level.KnownSolutionLength)
                        throw new InvalidOperationException("Minimum not proven: " +
                            proof.error + " / minimum=" + proof.minimum);
                    if (!CheckAlternate(level, n, out error))
                        throw new InvalidOperationException(error);
                    if (level.VerifiedOptimalMoveCount.HasValue)
                        throw new InvalidOperationException(
                            "Do not release Perfect certificates from draft levels.");

                    var rhythm = Post40QualityAudit.AnalyzeRhythm(level,
                        level.KnownSolution);
                    var sim = new BoardManager();
                    sim.Load(level);
                    int multi = 0, opened = 0;
                    foreach (var tap in level.KnownSolution)
                    {
                        if (!sim.RequestMove(tap) ||
                            sim.LastReactionWasCancelled)
                            throw new InvalidOperationException("Uncommitted tap.");
                        var moved = new HashSet<int>();
                        foreach (var action in sim.Actions)
                        {
                            if (action.Type == BoardActionType.Move)
                                moved.Add(action.PieceId);
                            if (action.Type == BoardActionType.GateOpened)
                                opened++;
                        }
                        if (moved.Count >= 2) multi++;
                        sim.CompleteResolution();
                    }
                    if (sim.State != GameState.Won)
                        throw new InvalidOperationException("Route failed to win.");
                    int remote = Post40QualityAudit.CountProductiveResonanceMoves(
                        level, level.KnownSolution);
                    if (remote < 1 || multi < 2 || rhythm.longestSinglePieceRun > 3)
                        throw new InvalidOperationException(
                            "Weak candidate: empty signal, insufficient chain, " +
                            "or four-plus solo taps.");

                    record.passed = true;
                    record.shortest = proof.minimum;
                    record.longer = AlternateWinningRoute(n).Length;
                    record.explored = proof.states;
                    record.multiPieceTaps = multi;
                    record.productiveRemote = remote;
                    record.openedGates = opened;
                    record.longestSoloRun = rhythm.longestSinglePieceRun;
                    passed++;
                    Debug.Log("Level " + n + " draft proof: " + proof.minimum +
                        " optimal, " + record.longer + " longer win, " +
                        multi + " multi-piece taps and " + remote +
                        " productive remote moves.", level);
                }
                catch (Exception ex)
                {
                    record.error = ex.Message;
                    Debug.LogError("Level " + n + " failed draft proof: " +
                        ex.Message, level);
                }
            }

            var directory = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,
                "chapter5_unity_candidate_proofs.json"),
                JsonUtility.ToJson(report, true));
            Debug.Log("Chapter 5 candidate validation: " + passed +
                "/20. Level 1-40 assets and release Perfect state unchanged.");
            if (passed != 20)
                Debug.LogError("Chapter 5 quality gate OPEN. No production merge.");
        }
    }
}
