using UnityEngine;

namespace Shift.Game
{
    // Decorative only: no coroutine, input lock or gameplay timing dependency.
    public sealed class RewardGleam : MonoBehaviour
    {
        private CanvasGroup group;
        private GameFeelSettings settings;
        private float remaining;
        public void Initialize(GameFeelSettings feel)
        {
            settings=feel;
            var root=PlaceholderVisuals.Rect("Reward Gleam",transform,Vector2.zero,Vector2.one);
            group=root.gameObject.AddComponent<CanvasGroup>(); group.alpha=0;group.blocksRaycasts=false;group.interactable=false;
            IdentityIcon.Create("Left Spark",root,IdentitySymbol.Spark,new Color32(174,126,44,255),new Vector2(.016f,.36f),new Vector2(.052f,.64f));
            IdentityIcon.Create("Right Spark",root,IdentitySymbol.Spark,new Color32(174,126,44,255),new Vector2(.948f,.36f),new Vector2(.984f,.64f));
        }
        public void Reveal() { remaining=.6f; group.alpha=1; }
        private void Update()
        {
            if(group==null || remaining<=0) return;
            remaining=Mathf.Max(0,remaining-Time.unscaledDeltaTime);
            float t=remaining/.6f;
            group.alpha=Mathf.SmoothStep(0,1,t);
            foreach(Transform child in group.transform)
                child.localScale=Vector3.one*(settings.reducedMotion?1:1+.12f*Mathf.Sin(t*Mathf.PI));
        }
    }
}
