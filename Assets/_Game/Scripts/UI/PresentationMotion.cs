using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class PresentationMotion : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private bool pressed;
        private Button button;
        public GameFeelSettings Settings { private get; set; }
        private void Awake() => button = GetComponent<Button>();
        public void OnPointerDown(PointerEventData e) { pressed = button != null && button.IsInteractable(); }
        public void OnPointerUp(PointerEventData e) => pressed = false;
        public void OnPointerExit(PointerEventData e) => pressed = false;
        private void OnDisable() { pressed = false; transform.localScale = Vector3.one; }
        private void Update()
        {
            if (Settings?.reducedMotion ?? false) { transform.localScale = Vector3.one; return; }
            float target = pressed && !(Settings?.reducedMotion ?? false) ? .96f : 1;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * target, 1 - Mathf.Exp(-30 * Time.unscaledDeltaTime));
        }
    }
}
