using System;
using UnityEngine;

namespace Shift.Game
{
    public sealed class DailyAllowanceService
    {
        public const string SaveKey = "SHIFT.DailyAllowance.v1";
        [Serializable] private sealed class State
        {
            public int version;
            public string dateKey;
            public int hintsRemaining = -1, undosRemaining = -1;
        }
        private readonly DailyAllowanceConfig config;
        private readonly Func<DateTime> now;
        private readonly string key;
        private State state;
        private string lastSaved;
        // A null key is an isolated in-memory store for unit tests; game always supplies a key.
        public DailyAllowanceService(DailyAllowanceConfig config, Func<DateTime> now, string key = null)
        { this.config = config; this.now = now; this.key = key; Refresh(); }
        public string DateKey { get { Refresh(); return state.dateKey; } }
        public int HintsRemaining { get { Refresh(); return state.hintsRemaining; } }
        public int UndosRemaining { get { Refresh(); return state.undosRemaining; } }
        public bool RecoveryEnabled(RewardReason reason) => reason == RewardReason.Hint ? config.EnableRewardedHintRecovery : config.EnableRewardedUndoRecovery;
        public int RewardAmount(RewardReason reason) => Math.Max(1, reason == RewardReason.Hint ? config.RewardedHintAmount : config.RewardedUndoAmount);
        public bool Refresh()
        {
            string date = DailyPool.Key(now());
            if (key != null)
            {
                string saved = PlayerPrefs.GetString(key, "");
                if (saved != lastSaved)
                {
                    try { state = JsonUtility.FromJson<State>(saved); } catch { state = null; }
                    lastSaved = saved;
                }
            }
            if (state != null && state.version == 1 && state.dateKey == date &&
                state.hintsRemaining >= 0 && state.undosRemaining >= 0) return false;
            state = new State { version = 1, dateKey = date, hintsRemaining = Math.Max(0, config.DailyFreeHints), undosRemaining = Math.Max(0, config.DailyFreeUndos) };
            Save(); return true;
        }
        private void Save()
        {
            if (key == null) return;
            lastSaved = JsonUtility.ToJson(state); PlayerPrefs.SetString(key, lastSaved); PlayerPrefs.Save();
        }
        public bool TryConsumeHint()
        {
            Refresh(); if (state.hintsRemaining == 0) return false;
            state.hintsRemaining--; Save(); return true;
        }
        public bool TryUndo(Func<bool> authoritativeUndo)
        {
            Refresh(); if (state.undosRemaining == 0 || authoritativeUndo == null || !authoritativeUndo()) return false;
            state.undosRemaining--; Save(); return true;
        }
        public bool Grant(RewardReason reason, string requestDate)
        {
            Refresh();
            if ((reason != RewardReason.Hint && reason != RewardReason.Undo) || !RecoveryEnabled(reason) || state.dateKey != requestDate) return false;
            int amount = RewardAmount(reason);
            if (reason == RewardReason.Hint) state.hintsRemaining = (int)Math.Min(int.MaxValue, (long)state.hintsRemaining + amount);
            else state.undosRemaining = (int)Math.Min(int.MaxValue, (long)state.undosRemaining + amount);
            Save(); return true;
        }
    }
}
