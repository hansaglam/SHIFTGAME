using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    public enum PuzzleArchetype { Corridor, Rooms, Crossroads, StateMachine, Cascade, Optimization }
    public enum DifficultyBand { Intro, Easy, Planning, Advanced, Mastery, Finale }
    [Flags] public enum ReasoningStyle { None = 0, Setup = 1, Sequence = 2, State = 4, Space = 8, Cascade = 16, Optimization = 32 }
    public enum CreativeHookType { None, WhichOneWouldYouTap, OneTapChain, LooksImpossible, WrongChoice, PerfectShift }

    [Serializable]
    public sealed class LevelDesign
    {
        public LevelData level;
        public PuzzleArchetype primary;
        public List<PuzzleArchetype> secondary = new List<PuzzleArchetype>();
        public DifficultyBand difficultyBand;
        public ReasoningStyle reasoningStyle;
        public bool sculptedTopology;
        public bool hasPayoffMove;
        [Min(0)] public int expectedPayoffDepth;
        public bool creativeCandidate;
        public CreativeHookType creativeHook;
        [TextArea] public string designRationale;
    }

    // Separate authoring data preserves the shipped tutorial asset bytes.
    // Read once per level visit; the simulation never consults these labels.
    [CreateAssetMenu(fileName = "LevelDesignCatalog", menuName = "SHIFT/Level Design Catalog")]
    public sealed class LevelDesignCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelDesign> entries = new List<LevelDesign>();
        public IReadOnlyList<LevelDesign> Entries => entries;
        private static LevelDesignCatalog cached;
        public static LevelDesignCatalog Current
        {
            get { if (cached == null) cached = Resources.Load<LevelDesignCatalog>("LevelDesignCatalog"); return cached; }
        }
        public LevelDesign Find(LevelData level) => entries.Find(entry => entry.level == level);
    }
}
