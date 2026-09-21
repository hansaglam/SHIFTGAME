using System;
using System.Collections.Generic;
using UnityEditor;

namespace Shift.Game.Editor
{
    public static class ChapterLevelWiring
    {
        private static readonly string[] Names =
        {
            "Level01_FirstTap", "Level02_FindTheExit", "Level03_ClearTheBox", "Level04_FirstChain",
            "Level05_BlockedIsFree", "Level06_TurnTheCorner", "Level07_MakeRoom", "Level08_Clockwise",
            "Level09_PushAndTurn", "Level10_SetTheChain", "Level11_OpenTheLane", "Level12_CrossingOrder",
            "Level13_SecondAct", "Level14_TwoCorners", "Level15_TheLongSetup", "Level16_TransformAhead",
            "Level17_SharedCorridor", "Level18_ClearTriggerFinish", "Level19_SharedDelivery", "Level20_TheFinalShift",
            "Level21_FirstSwitch", "Level22_OpenForRed", "Level23_ToggleTwice", "Level24_ClosedLane", "Level25_BeforeYouGo", "Level26_ThreeEntries", "Level27_BoxDoesNotPress", "Level28_TurnThrough", "Level29_ClockworkGate", "Level30_ClearThenOpen", "Level31_OnePadTwoGates", "Level32_OppositeStates", "Level33_SharedDeliveryGate", "Level34_ClearTheCorridor", "Level35_TwoChannels", "Level36_CrossBeforeClosing", "Level37_OpenTheSecondPad", "Level38_LiftAndTurn", "Level39_PrepareToggleRedirect", "Level40_TheStateOfShift"
        };

        public static LevelData[] LoadExpected()
        {
            var result = new LevelData[Names.Length];
            var unique = new HashSet<LevelData>();
            for (int i = 0; i < Names.Length; i++)
            {
                string path = "Assets/_Game/Data/Levels/" + Names[i] + ".asset";
                result[i] = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (result[i] == null || !unique.Add(result[i]))
                    throw new InvalidOperationException("Missing or duplicate chapter asset: " + path);
            }
            return result;
        }

        public static bool IsCorrect(PrototypeGame game)
        {
            var expected = LoadExpected();
            var list = new SerializedObject(game).FindProperty("levels");
            if (list.arraySize != expected.Length) return false;
            for (int i = 0; i < expected.Length; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue != expected[i]) return false;
            return true;
        }

        public static void Assign(PrototypeGame game)
        {
            // Resolve everything before mutation. Growing a Unity array copies its last element;
            // explicitly overwrite EVERY slot, including those that existed before the resize.
            var expected = LoadExpected();
            var serialized = new SerializedObject(game);
            var list = serialized.FindProperty("levels");
            list.arraySize = expected.Length;
            for (int i = 0; i < expected.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = expected[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
