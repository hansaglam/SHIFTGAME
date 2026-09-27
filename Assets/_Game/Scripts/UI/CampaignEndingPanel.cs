using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Shift.Game
{
    public sealed class CampaignEndingPanel : MonoBehaviour
    {
        public bool IsOpen => gameObject.activeSelf;
        public bool Mastered { get; private set; }
        public float RevealDuration => .32f;
        private CanvasGroup opacity;
        private GameFeelSettings feel;
        public void Build(Font font, Sprite rounded, Sprite circle, GameFeelSettings settings, int perfectCount,
            Action mastery, Action daily, Action levels)
        {
            feel = settings; Mastered = perfectCount == 40;
            // Existing scenery and wordmark; the final board is retained underneath, never rebuilt.
            gameObject.AddComponent<Image>().color = IdentityStyle.Navy;
            VisualTheme.Background(transform, circle); Wordmark.Build(transform, font);
            opacity = gameObject.AddComponent<CanvasGroup>();
            var card = VisualTheme.Surface("Ending Card", transform, rounded, IdentityStyle.Cream, new Vector2(.07f,.035f), new Vector2(.93f,.865f));
            IdentityStyle.Material(card); VisualTheme.Shadow(card, 8);
            Label("Ending Title", Mastered ? "ending.mastered" : "ending.complete", 46, .75f,.83f, true);
            Label("Ending Quote", "ending.quote", 30, .64f,.725f);
            var rule = VisualTheme.Surface("Ending Gold Rule", transform, rounded, new Color32(185,144,60,255),new Vector2(.35f,.615f),new Vector2(.65f,.617f));
            rule.raycastTarget = false;
            var summary = Label("Ending Summary", "ending.summary", 32, .455f,.59f);
            LocalizedLabel.Bind(summary, "ending.summary", perfectCount);
            if (Mastered) summary.color = new Color32(135,96,28,255);
            Label("Ending Body", Mastered ? "ending.mastered_body" : "ending.body", 28, .275f,.425f);
            var primary = ChapterSelect.CreateButton("Ending Primary",font,rounded,transform,new Vector2(.15f,.178f),new Vector2(.85f,.245f),Mastered ? daily : mastery,out var primaryText);
            primary.targetGraphic.color = IdentityStyle.Teal;
            LocalizedLabel.Bind(primaryText, Mastered ? "ending.daily" : "ending.mastery"); primaryText.fontStyle = FontStyle.Bold;
            ChapterSelect.CreateButton("Ending Secondary",font,rounded,transform,new Vector2(.15f,.098f),new Vector2(.85f,.158f),Mastered ? levels : daily,out var secondaryText);
            LocalizedLabel.Bind(secondaryText, Mastered ? "ending.replay" : "ending.daily");
            if (!Mastered)
            {
                ChapterSelect.CreateButton("Ending Levels",font,rounded,transform,new Vector2(.24f,.049f),new Vector2(.76f,.087f),levels,out var back);
                LocalizedLabel.Bind(back,"ending.levels"); back.fontSize = 25;
            }
            foreach (var motion in GetComponentsInChildren<PresentationMotion>()) motion.Settings = feel;
            gameObject.SetActive(false);
            Text Label(string name, string key, int size, float bottom, float top, bool bold = false)
            {
                var text = PlaceholderVisuals.Label(name,transform,font,"",size,VisualTheme.Ink,new Vector2(.13f,bottom),new Vector2(.87f,top));
                text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                LocalizedLabel.Bind(text,key);return text;
            }
        }
        public void Open()
        {
            if (IsOpen) return;
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            opacity.alpha = feel.reducedMotion ? 1 : 0; transform.localScale = Vector3.one;
            if (!feel.reducedMotion) StartCoroutine(Reveal());
        }
        private IEnumerator Reveal()
        {
            for(float t=0;t<RevealDuration;t+=Time.unscaledDeltaTime)
            { opacity.alpha=t/RevealDuration; transform.localScale=Vector3.one*Mathf.Lerp(.985f,1,t/RevealDuration); yield return null; }
            opacity.alpha=1;transform.localScale=Vector3.one;
        }
        public void Close() { StopAllCoroutines(); gameObject.SetActive(false); }
    }
}
