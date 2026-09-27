using UnityEngine;
namespace Shift.Game
{
    // Additive presentation history, separate from authoritative campaign progress.
    public sealed class CampaignEndingState
    {
        public const string SaveKey = "SHIFT.CampaignEnding.Seen.v1";
        private readonly string key;
        public CampaignEndingState(string key = SaveKey) { this.key = key; }
        public bool Seen => PlayerPrefs.GetInt(key, 0) == 1;
        public bool Eligible(int index, bool won, bool daily, bool debug, bool campaignComplete) =>
            index == 39 && won && !daily && !debug && campaignComplete && !Seen;
        public bool MarkSeen()
        {
            if (Seen) return false;
            PlayerPrefs.SetInt(key, 1); PlayerPrefs.Save(); return true;
        }
    }
}
