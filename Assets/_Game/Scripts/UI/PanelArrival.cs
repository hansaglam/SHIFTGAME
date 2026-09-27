using UnityEngine;

namespace Shift.Game
{
    public sealed class PanelArrival : MonoBehaviour
    {
        private CanvasGroup group;
        private GameFeelSettings feel;
        private float elapsed;
        public void Initialize(GameFeelSettings settings)
        { feel = settings; group = gameObject.AddComponent<CanvasGroup>(); group.alpha = .7f; }
        private void Update()
        {
            if (feel == null) return;
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.01f, feel.panelDuration));
            group.alpha = Mathf.Lerp(.7f, 1, t);
            transform.localScale = Vector3.one * (feel.reducedMotion ? 1 : Mathf.Lerp(.985f, 1, 1 - Mathf.Pow(1-t,3)));
            if (t >= 1) enabled = false;
        }
    }
}
