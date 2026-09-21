using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public static class LayoutControls
    {
        public static void Settings(Transform parent, Sprite circle, GameFeelSettings feel, Action open)
        {
            var rect = PlaceholderVisuals.Rect("Open Settings",parent,Vector2.one,Vector2.one);
            rect.pivot = Vector2.one; rect.sizeDelta = new Vector2(112,112); rect.anchoredPosition = new Vector2(-20,-20);
            var hit = rect.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var face = VisualTheme.Surface("Settings Face",rect,circle,new Color32(40,68,102,255),new Vector2(.08f,.08f),new Vector2(.92f,.92f));
            face.type = Image.Type.Simple; VisualTheme.Shadow(face,5);
            IdentityStyle.Material(face, true);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = face;
            var colors = button.colors; colors.pressedColor = new Color(.65f,.82f,.95f); colors.fadeDuration = .08f; button.colors = colors;
            rect.gameObject.AddComponent<PresentationMotion>().Settings = feel; button.onClick.AddListener(() => open());
            IdentityIcon.Create("Gear", face.transform, IdentitySymbol.Gear, new Color32(240,249,255,255),
                new Vector2(.24f,.24f), new Vector2(.76f,.76f));
        }
    }
}
