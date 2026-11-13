using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shift.Game.Editor
{
    // Four small Chapter 6 mechanic pilots. Deliberately NOT a 20-level
    // production campaign; the experimental rule must pass engine/design QA.
    public static class Chapter6TetherPrototypes
    {
        public const string Folder =
            "Assets/_Game/Data/Levels/Chapter6_TetherPrototypes/";
        public const string PreviewScene =
            "Assets/_Game/Scenes/SHIFT_Tether_101_104_Preview.unity";
        public const string ProgressPrefix =
            "SHIFT.Preview.Chapter6Tether.v1.";
        public const string SettingsPrefix =
            "SHIFT.Preview.Chapter6Tether.Settings.v1.";

        private static readonly string[] Names =
        {
            "Level101_FirstBond",
            "Level102_BlockedTension",
            "Level103_TripleReaction",
            "Level104_TwoGateTether"
        };

        // Distinct *winning* experiments, not Perfect Shift certificates.
        // 102 is an equally short alternative decision order.
        private static readonly string[] LongerOrAlternateRoutes =
        {
            "01314151",           // 101: manually activate the blue switch first
            "012202",             // 102: choose red's exit before blue's
            "0131134151",         // 103: manually trigger Resonance before the bond
            "030111314151"        // 104: manually open the second gate with blue
        };

        public static LevelData Get(int number)
        {
            if (number < 101 || number > 104) return null;
            return AssetDatabase.LoadAssetAtPath<LevelData>(
                Folder + Names[number - 101] + ".asset");
        }

        public static GridPosition[] Alternative(int number)
        {
            if (number < 101 || number > 104) return null;
            string path = LongerOrAlternateRoutes[number - 101];
            var result = new GridPosition[path.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = new GridPosition(
                    path[2 * i] - '0', path[2 * i + 1] - '0');
            return result;
        }

        public static bool VerifyAlternative(LevelData level, int number,
            out string error)
        {
            error = null;
            var taps = Alternative(number);
            if (level == null || taps == null ||
                taps.Length < level.KnownSolutionLength ||
                taps.Length > level.MoveLimit)
            {
                error = "Invalid prototype alternative length.";
                return false;
            }
            var board = new BoardManager();
            board.Load(level);
            foreach (var tap in taps)
            {
                int before = board.MovesRemaining;
                if (!board.RequestMove(tap) ||
                    board.LastReactionWasCancelled ||
                    board.ReactionDepth == 0 ||
                    board.MovesRemaining != before - 1)
                {
                    error = "Alternative tap failed: " + tap;
                    return false;
                }
                board.CompleteResolution();
            }
            if (board.State != GameState.Won)
            {
                error = "Alternative did not reach victory.";
                return false;
            }
            return true;
        }

        public static LevelData[] Load104()
        {
            var first100 = ChapterJourneyScenePreview.Load100();
            if (first100.Length != 100)
                throw new InvalidOperationException("Levels 1-100 are incomplete.");
            var result = new LevelData[104];
            var seen = new HashSet<LevelData>();
            for (int i = 0; i < 100; i++)
            {
                result[i] = first100[i];
                if (result[i] == null || !seen.Add(result[i]))
                    throw new InvalidOperationException(
                        "Old campaign changed or repeated Level " + (i + 1));
            }
            for (int n = 101; n <= 104; n++)
            {
                result[n - 1] = Get(n);
                if (result[n - 1] == null || !seen.Add(result[n - 1]))
                    throw new InvalidOperationException(
                        "Missing/duplicated BAĞ pilot Level " + n);
            }
            return result;
        }

        [Serializable]
        public sealed class Check
        {
            public int level, minimum, known, alternative, visited,
                pulls, distinctMovedPieces, multiPieceTaps, resonanceEvents,
                gateOpenEvents;
            public bool passed;
            public string error;
        }

        [Serializable]
        public sealed class Report
        {
            public string status =
                "CHAPTER 6 MECHANIC PILOT — engine proof only, no release";
            public List<Check> levels = new List<Check>();
        }

        [MenuItem("SHIFT/Chapter 6/Validate BAĞ Prototype Levels 101-104")]
        public static void ValidateAll()
        {
            var report = new Report();
            int passes = 0;
            for (int n = 101; n <= 104; n++)
            {
                var record = new Check { level = n };
                report.levels.Add(record);
                var level = Get(n);
                try
                {
                    // Declare before short-circuit: when level is null,
                    // ValidatePlayable is skipped and cannot assign error.
                    string error = null;
                    if (level == null || !level.ValidatePlayable(
                        out error))
                        throw new InvalidOperationException(
                            error ?? "Missing LevelData");
                    if (!LevelValidation.VerifySolution(level,
                        out error, out int depth))
                        throw new InvalidOperationException(error);
                    if (!VerifyAlternative(level, n, out error))
                        throw new InvalidOperationException(error);
                    var proof = CampaignCertification.Prove(level, n);
                    if (!proof.complete ||
                        proof.minimum != level.KnownSolutionLength)
                        throw new InvalidOperationException(
                            "Optimality proof failed: " + proof.error +
                            ", minimum=" + proof.minimum);
                    if (level.VerifiedOptimalMoveCount.HasValue)
                        throw new InvalidOperationException(
                            "No speculative Perfect Shift certificates.");
                    if (!MechanicGuideCatalog.Contains(level,
                        MechanicTopic.Tether))
                        throw new InvalidOperationException(
                            "Tether guide not discoverable.");

                    var board = new BoardManager();
                    board.Load(level);
                    var moved = new HashSet<int>();
                    int pulls = 0, multiPiece = 0, pulses = 0, opened = 0;
                    foreach (var tap in level.KnownSolution)
                    {
                        if (!board.RequestMove(tap) ||
                            board.LastReactionWasCancelled)
                            throw new InvalidOperationException("Invalid tap.");
                        var movedThisTap = new HashSet<int>();
                        foreach (var action in board.Actions)
                        {
                            if (action.Type == BoardActionType.TetherPull)
                                pulls++;
                            if (action.Type == BoardActionType.ResonancePulse)
                                pulses++;
                            if (action.Type == BoardActionType.GateOpened)
                                opened++;
                            if (action.Type == BoardActionType.Move)
                            {
                                moved.Add(action.PieceId);
                                movedThisTap.Add(action.PieceId);
                            }
                        }
                        if (movedThisTap.Count >= 2) multiPiece++;
                        board.CompleteResolution();
                    }
                    if (board.State != GameState.Won || pulls == 0 ||
                        moved.Count < 2 || multiPiece == 0)
                        throw new InvalidOperationException(
                            "BAĞ has no useful multi-piece chain or route lost.");

                    record.passed = true;
                    record.minimum = proof.minimum;
                    record.known = level.KnownSolutionLength;
                    record.alternative = Alternative(n).Length;
                    record.visited = proof.states;
                    record.pulls = pulls;
                    record.distinctMovedPieces = moved.Count;
                    record.multiPieceTaps = multiPiece;
                    record.resonanceEvents = pulses;
                    record.gateOpenEvents = opened;
                    passes++;
                    Debug.Log("BAĞ Level " + n + ": " + pulls +
                        " pulls, " + proof.minimum + " minimum, " +
                        record.alternative + " alternate, " + proof.states +
                        " solver states.", level);
                }
                catch (Exception ex)
                {
                    record.error = ex.Message;
                    Debug.LogError("BAĞ Level " + n + ": " + ex.Message,
                        level);
                }
            }
            string folder = Path.Combine(Application.dataPath, "../Validation");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,
                "chapter6_tether_unity_proofs.json"),
                JsonUtility.ToJson(report, true));
            Debug.Log("BAĞ pilot: " + passes + "/4 engine checks. " +
                "Human play tests and Chapter 6 integration remain pending.");
            if (passes != 4)
                Debug.LogError("Chapter 6 prototype is NOT cleared for release.");
        }

        // Rebuild the LevelData references even when an earlier preview
        // scene already exists. Merely reopening a stale copy can display
        // "Campaign unavailable" despite all four puzzle proofs passing.
        private static void RewirePreviewScene(Scene scene, LevelData[] levels)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException(
                    "BAĞ preview scene is not loaded.");
            PrototypeGame game = null;
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var candidate in
                    root.GetComponentsInChildren<PrototypeGame>(true))
                {
                    game = candidate;
                    count++;
                }
            if (count != 1)
                throw new InvalidOperationException(
                    "Expected exactly one PrototypeGame, found " + count);
            var serialized = new SerializedObject(game);
            serialized.Update();
            var slots = serialized.FindProperty("levels");
            var enabled = serialized.FindProperty("useLevelSet");
            var selected = serialized.FindProperty("selectedLevel");
            var progressKey = serialized.FindProperty("saveKeyPrefix");
            var settingsKey = serialized.FindProperty("settingsKeyPrefix");
            if (slots == null || enabled == null || selected == null ||
                progressKey == null || settingsKey == null)
                throw new InvalidOperationException(
                    "PrototypeGame campaign fields are missing.");
            slots.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
                slots.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
            enabled.boolValue = true;
            selected.intValue = 0;
            progressKey.stringValue = ProgressPrefix;
            settingsKey.stringValue = SettingsPrefix;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException(
                    "Could not save the repaired BAĞ preview scene.");

            // Inspect serialized values, not just the in-memory level set.
            var verify = new SerializedObject(game);
            var saved = verify.FindProperty("levels");
            if (saved.arraySize != 104 ||
                verify.FindProperty("saveKeyPrefix").stringValue !=
                    ProgressPrefix ||
                !verify.FindProperty("useLevelSet").boolValue)
                throw new InvalidOperationException(
                    "BAĞ preview wiring did not persist.");
            for (int i = 0; i < levels.Length; i++)
                if (saved.GetArrayElementAtIndex(i).objectReferenceValue
                    != levels[i])
                    throw new InvalidOperationException(
                        "BAĞ preview level reference mismatch at " +
                        (i + 1));
        }

        [MenuItem("SHIFT/Chapter 6/Create or Open Isolated BAĞ Preview")]
        public static void CreateOrOpenPreview()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning(
                    "Stop Play Mode before opening the BAĞ preview.");
                return;
            }
            var levels = Load104();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                PreviewScene) != null)
            {
                var opened = EditorSceneManager.OpenScene(
                    PreviewScene, OpenSceneMode.Single);
                RewirePreviewScene(opened, levels);
                Debug.Log("Existing BAĞ preview repaired and reopened. " +
                    "104 LevelData references verified; shipped scene unchanged.");
                return;
            }
            if (!AssetDatabase.CopyAsset(PrototypeSetup.ScenePath,
                PreviewScene))
                throw new InvalidOperationException(
                    "Cannot make an isolated copy of the shipped scene.");
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(
                    PreviewScene, OpenSceneMode.Single);
                RewirePreviewScene(scene, levels);
                var save = new SaveService(ProgressPrefix);
                var previous = save.Load(104);
                save.Save(new ProgressData(103, 100,
                    Math.Max(99, previous.HighestCompleted)));
                Debug.Log("Isolated BAĞ preview created with 104 verified " +
                    "level references. Original scene and saves unchanged.");
            }
            catch
            {
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.NewScene(
                        NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AssetDatabase.DeleteAsset(PreviewScene);
                throw;
            }
        }

        [MenuItem("SHIFT/Chapter 6/Create or Open Isolated BAĞ Preview", true)]
        public static bool CanOpenPreview() =>
            !EditorApplication.isPlayingOrWillChangePlaymode;

        private static void OpenLevel(int level)
        {
            if (level < 101 || level > 104)
                throw new ArgumentOutOfRangeException(nameof(level));
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScene) == null)
            {
                Debug.LogError("Create the isolated BAĞ preview scene first.");
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning(
                    "Stop Play Mode before selecting a BAĞ pilot level.");
                return;
            }
            var levels = Load104();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var opened = EditorSceneManager.OpenScene(
                PreviewScene, OpenSceneMode.Single);
            RewirePreviewScene(opened, levels);
            var save = new SaveService(ProgressPrefix);
            var previous = save.Load(104);
            // This is a developer-only jump. Keep the test save's
            // completed frontier coherent with the selected level index;
            // none of these synthetic completions touch real players.
            save.Save(new ProgressData(103, level - 1,
                Math.Max(level - 2, previous.HighestCompleted)));
            Debug.Log("BAĞ preview level " + level +
                " selected; 104 scene references verified. " +
                "Now press Play in Unity.");
        }

        [MenuItem("SHIFT/Chapter 6/Preview Level 101")]
        public static void Preview101() => OpenLevel(101);
        [MenuItem("SHIFT/Chapter 6/Preview Level 102")]
        public static void Preview102() => OpenLevel(102);
        [MenuItem("SHIFT/Chapter 6/Preview Level 103")]
        public static void Preview103() => OpenLevel(103);
        [MenuItem("SHIFT/Chapter 6/Preview Level 104")]
        public static void Preview104() => OpenLevel(104);

        [MenuItem("SHIFT/Chapter 6/Preview Level 101", true)]
        public static bool CanPreview101() => CanOpenPreview();
        [MenuItem("SHIFT/Chapter 6/Preview Level 102", true)]
        public static bool CanPreview102() => CanOpenPreview();
        [MenuItem("SHIFT/Chapter 6/Preview Level 103", true)]
        public static bool CanPreview103() => CanOpenPreview();
        [MenuItem("SHIFT/Chapter 6/Preview Level 104", true)]
        public static bool CanPreview104() => CanOpenPreview();
    }
}
