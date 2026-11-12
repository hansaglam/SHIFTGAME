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
            "232101212211213133",
            "615141303133144215",
            "544765464442416362",
            "606231564463462111",
            "654523324253515251",
            "12133224114234445452",
            "06163625244443414222",
            "436062312144634611",
            "5141211233341314155035",
            "17151436463331665666",
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

        [Serializable]
        public sealed class MotifPair
        {
            public int first, second, sharedTapPattern, firstLength, secondLength;
            public float normalizedOverlap;
            public int sharedOpening;
        }

        [Serializable]
        public sealed class MotifReport
        {
            public string status = "EDITOR DESIGN REVIEW - similarity is not a failure by itself";
            public List<MotifPair> candidatePairs = new List<MotifPair>();
        }

        // Coordinates are deliberately excluded. Different-looking boards can
        // repeat the very same player decision sequence, hence include color
        // and action consequences on each committed tap.
        public static List<string> DecisionMotif(LevelData level)
        {
            var board = new BoardManager();
            board.Load(level);
            var pattern = new List<string>();
            foreach (var tap in level.KnownSolution)
            {
                var piece = board.GetOccupant(tap);
                if (piece == null || piece.Type != PieceType.Normal)
                    throw new InvalidOperationException("No normal piece at " + tap);
                var color = piece.Color;
                if (!board.RequestMove(tap) || board.LastReactionWasCancelled)
                    throw new InvalidOperationException("Invalid optimal motif tap.");
                var moved = new HashSet<int>();
                int remote = 0, gates = 0, delivered = 0;
                foreach (var action in board.Actions)
                {
                    if (action.Type == BoardActionType.Move) moved.Add(action.PieceId);
                    if (action.Type == BoardActionType.ResonancePulse) remote++;
                    if (action.Type == BoardActionType.GateOpened) gates++;
                    if (action.Type == BoardActionType.Deliver) delivered++;
                }
                pattern.Add(((int)color).ToString() + ":" +
                    (moved.Count > 1 ? "chain" : "solo") + ":" +
                    (remote > 0 ? "signal" : "quiet") + ":" +
                    (gates > 0 ? "gate" : "closed") + ":" +
                    (delivered > 0 ? "exit" : "moving"));
                board.CompleteResolution();
            }
            if (board.State != GameState.Won)
                throw new InvalidOperationException("Motif requires winning route.");
            return pattern;
        }

        private static int MotifLcs(List<string> a, List<string> b)
        {
            var previous = new int[b.Count + 1];
            for (int i = 0; i < a.Count; i++)
            {
                var current = new int[b.Count + 1];
                for (int j = 0; j < b.Count; j++)
                    current[j + 1] = a[i] == b[j] ? previous[j] + 1 :
                        Math.Max(previous[j + 1], current[j]);
                previous = current;
            }
            return previous[b.Count];
        }

        public static MotifReport ScanMotifSimilarities(float threshold = .75f)
        {
            var profiles = new List<string>[20];
            for (int n = 81; n <= 100; n++)
            {
                var level = Get(n);
                if (level == null)
                    throw new InvalidOperationException("Missing level " + n);
                profiles[n - 81] = DecisionMotif(level);
            }
            var result = new MotifReport();
            for (int i = 0; i < profiles.Length; i++)
                for (int j = i + 1; j < profiles.Length; j++)
                {
                    var a = profiles[i]; var b = profiles[j];
                    int common = MotifLcs(a, b);
                    float overlap = (float)common / Math.Min(a.Count, b.Count);
                    int opening = 0;
                    while (opening < a.Count && opening < b.Count &&
                        a[opening] == b[opening]) opening++;
                    if (overlap < threshold && opening < 4) continue;
                    result.candidatePairs.Add(new MotifPair
                    {
                        first = 81 + i, second = 81 + j,
                        sharedTapPattern = common,
                        firstLength = a.Count, secondLength = b.Count,
                        normalizedOverlap = overlap, sharedOpening = opening
                    });
                }
            return result;
        }

        [MenuItem("SHIFT/Chapter 5/Export Decision Motif Similarities 81-100")]
        public static void ExportMotifSimilarities()
        {
            var report = ScanMotifSimilarities();
            string folder = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(folder);
            string output = Path.Combine(folder, "chapter5_motif_similarity.json");
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            foreach (var pair in report.candidatePairs)
                Debug.LogWarning("Chapter 5 design review: Level " + pair.first +
                    " / " + pair.second + " pattern overlap " +
                    (pair.normalizedOverlap * 100f).ToString("F0") +
                    "%; common opening taps=" + pair.sharedOpening +
                    ". Review insight, not just rotated board shape.");
            Debug.Log("Chapter 5 motif review exported: " + output);
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
