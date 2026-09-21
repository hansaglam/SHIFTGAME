using UnityEngine;

namespace Shift.Game
{
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform rect;
        private Rect previous;
        private Vector2Int screenSize;
        private void Awake() { rect = (RectTransform)transform; Apply(); }
        private void Update()
        {
            if (previous != Screen.safeArea || screenSize.x != Screen.width || screenSize.y != Screen.height) Apply();
        }
        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            previous = Screen.safeArea; screenSize = new Vector2Int(Screen.width, Screen.height);
            rect.anchorMin = new Vector2(previous.xMin / Screen.width, previous.yMin / Screen.height);
            rect.anchorMax = new Vector2(previous.xMax / Screen.width, previous.yMax / Screen.height);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }
    }
}
