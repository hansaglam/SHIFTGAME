using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public static class IdentityStyle
    {
        public static readonly Color Navy = new Color32(49, 77, 110, 255);
        public static readonly Color Teal = new Color32(37, 160, 183, 255);
        public static readonly Color Cream = new Color32(255, 244, 218, 255);
        public static void Material(Image image, bool round = false)
        {
            image.sprite = IdentityResources.Surface(image.transform, round);
            image.type = round ? Image.Type.Simple : Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1.5f;
        }
        public static void Action(Button button, Text label, IdentitySymbol symbol, bool primary)
        {
            button.targetGraphic.color = primary ? Teal : Navy;
            label.fontStyle = FontStyle.Bold;
            label.rectTransform.anchorMin = new Vector2(.25f, .12f);
            label.rectTransform.anchorMax = new Vector2(.92f, .88f);
            IdentityIcon.Create("Action Icon", button.transform, symbol, new Color32(237, 251, 255, 255),
                new Vector2(.08f, .27f), new Vector2(.24f, .73f));
        }
        public static void Modal(Transform parent, Sprite rounded, string name)
        {
            var card = VisualTheme.Surface(name, parent, rounded, Cream, new Vector2(.055f, .065f), new Vector2(.945f, .94f));
            Material(card); VisualTheme.Shadow(card, 8);
        }
    }
}
