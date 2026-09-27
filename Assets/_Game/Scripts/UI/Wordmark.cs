using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Composed live type; no raster text or dependency on the level subtitle.
    public static class Wordmark
    {
        public static void Build(Transform parent, Font font)
        {
            var root = PlaceholderVisuals.Rect("SHIFT Wordmark",parent,new Vector2(.19f,.909f),new Vector2(.82f,.991f));
            var title = PlaceholderVisuals.Label("Title",root,font,"SHIFT",146,Color.white,Vector2.zero,new Vector2(.87f,1));
            title.fontStyle=FontStyle.BoldAndItalic;
            title.gameObject.AddComponent<WordmarkLight>();
            var shadow=title.gameObject.AddComponent<Shadow>(); shadow.effectColor=new Color(.025f,.12f,.24f,.28f); shadow.effectDistance=new Vector2(2,-5);
            var depth=title.gameObject.AddComponent<Shadow>(); depth.effectColor=new Color32(147,186,218,255); depth.effectDistance=new Vector2(0,-2.5f);
            IdentityIcon.Create("Brand Shift",root,IdentitySymbol.Forward,new Color32(110,241,255,255),new Vector2(.735f,.33f),new Vector2(.865f,.73f));
        }
    }
}
