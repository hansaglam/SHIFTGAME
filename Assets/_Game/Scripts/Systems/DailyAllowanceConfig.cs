using UnityEngine;

namespace Shift.Game
{
    [CreateAssetMenu(menuName = "SHIFT/Daily Allowance Config")]
    public sealed class DailyAllowanceConfig : ScriptableObject
    {
        [Min(0)] public int DailyFreeHints = 3;
        [Min(0)] public int DailyFreeUndos = 5;
        [Min(1)] public int RewardedHintAmount = 1;
        [Min(1)] public int RewardedUndoAmount = 1;
        public bool EnableRewardedHintRecovery = true;
        public bool EnableRewardedUndoRecovery = true;
    }
}
