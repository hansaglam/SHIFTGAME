using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class RewardedHintPanel : MonoBehaviour
    {
        private Button watch;
        private Text message, title, primary, secondary;
        private RewardReason reason;
        private int amount = 1;
        private bool ready, failed;
        private bool pending;
        public bool IsOpen => gameObject.activeSelf;
        public void Build(Font font, Sprite rounded, GameFeelSettings feel, Action request, Action close)
        {
            gameObject.AddComponent<Image>().color = new Color(.055f,.13f,.23f,.88f);
            var card = VisualTheme.Surface("Hint Card", transform, rounded, IdentityStyle.Cream, new Vector2(.08f,.30f), new Vector2(.92f,.68f));
            IdentityStyle.Material(card); VisualTheme.Shadow(card, 8);
            title = PlaceholderVisuals.Label("Hint Title", transform, font, "Need another hint?", 50, VisualTheme.Ink, new Vector2(.12f,.57f), new Vector2(.88f,.65f));
            message = PlaceholderVisuals.Label("Hint Availability", transform, font, "", 30, VisualTheme.Ink, new Vector2(.16f,.48f), new Vector2(.84f,.56f));
            watch = ChapterSelect.CreateButton("Watch Ad", font, rounded, transform, new Vector2(.16f,.39f), new Vector2(.84f,.46f), request, out primary);
            primary.text = "Watch Ad for +1 Hint";
            var colors = watch.colors; colors.disabledColor = new Color(1,1,1,.4f); watch.colors = colors;
            watch.GetComponent<Image>().color = new Color32(198,143,37,255);
            ChapterSelect.CreateButton("Not now", font, rounded, transform, new Vector2(.27f,.32f), new Vector2(.73f,.37f), close, out secondary);
            secondary.text = "Not now";
            foreach (var motion in GetComponentsInChildren<PresentationMotion>()) motion.Settings = feel;
            Close();
        }
        public void Open(bool available, RewardReason reason = RewardReason.Hint, int amount = 1, HintLanguage language = HintLanguage.English)
        {
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            this.reason = reason; this.amount = amount; pending = false;
            failed = false; ready = available;
            watch.interactable = available;
            Render();
        }
        public void RefreshAvailability(bool available, HintLanguage value)
        {
            if (available == ready) return;
            ready = available;
            if (ready) failed = false;
            watch.interactable = ready; Render();
        }
        private void Render()
        {
            bool undo = reason == RewardReason.Undo;
            LocalizedLabel.Bind(title, undo ? "reward.undo_title" : "reward.hint_title");
            LocalizedLabel.Bind(primary, undo ? "reward.undo_primary" : "reward.hint_primary", amount);
            LocalizedLabel.Bind(secondary, "reward.cancel");
            LocalizedLabel.Bind(message, pending ? "reward.pending" : failed ? "reward.failed" : ready ? undo ? "reward.undo_ready" : "reward.hint_ready" : "reward.unavailable", amount);
        }
        public void Pending() { pending = true; watch.interactable = false; Render(); }
        public void Unavailable() { pending = false; failed = true; ready = false; watch.interactable = false; Render(); }
        public void Close() => gameObject.SetActive(false);
    }
}
