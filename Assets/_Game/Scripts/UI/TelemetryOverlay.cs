#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class TelemetryOverlay : MonoBehaviour
    {
        private TelemetryTracker tracker;
        private Text label;
        public void Initialize(TelemetryTracker tracker, Font font)
        {
            this.tracker = tracker;
            var group = gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            label = PlaceholderVisuals.Label("Telemetry Text", transform, font, "", 24, Color.white, Vector2.zero, Vector2.one);
            var bg = gameObject.AddComponent<Image>(); bg.color = new Color(0,0,0,.85f); bg.raycastTarget = false;
            StartCoroutine(Refresh());
        }
        private IEnumerator Refresh()
        {
            var interval = new WaitForSecondsRealtime(.5f);
            while (true)
            {
                var a = tracker.Snapshot();
                if (a != null) label.text = $"DEV · Level {a.levelNumber} · Attempt {a.attemptNumber} · {a.duration:0}s\nMoves {a.successfulMoves}/{a.budget} · Taps {a.taps} · Blocked {a.blockedTaps}\nRestarts {a.restarts} · Max chain {a.maxDepth} · Optimal {(a.hasOptimal ? a.verifiedOptimalMoves.ToString() : "unknown")}";
                yield return interval;
            }
        }
    }
}
#endif
