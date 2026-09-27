using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public enum IdentitySymbol { Restart, Levels, Forward, Gear, Spark, Undo, Hint }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class IdentityIcon : MaskableGraphic
    {
        public IdentitySymbol Symbol { get; private set; }
        public static IdentityIcon Create(string name, Transform parent, IdentitySymbol symbol, Color tint, Vector2 min, Vector2 max)
        {
            var icon = PlaceholderVisuals.Rect(name, parent, min, max).gameObject.AddComponent<IdentityIcon>();
            icon.Symbol = symbol; icon.color = tint; icon.raycastTarget = false; return icon;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = rectTransform.rect; float scale = Mathf.Min(r.width, r.height);
            void Tri(Vector2 a, Vector2 b, Vector2 c)
            {
                int i = vh.currentVertCount;
                vh.AddVert(r.center + a * scale, color, Vector2.zero);
                vh.AddVert(r.center + b * scale, color, Vector2.zero);
                vh.AddVert(r.center + c * scale, color, Vector2.zero); vh.AddTriangle(i, i + 1, i + 2);
            }
            void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) { Tri(a, b, c); Tri(a, c, d); }
            void Bar(Vector2 a, Vector2 b, float width)
            { var n = new Vector2(-(b-a).y, (b-a).x).normalized * width / 2; Quad(a-n,a+n,b+n,b-n); }
            Vector2 P(float a, float radius) => new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            if (Symbol == IdentitySymbol.Hint)
            {
                for(int i=0;i<40;i++) Tri(new Vector2(0,.12f),new Vector2(0,.12f)+P(i*Mathf.PI/20,.28f),new Vector2(0,.12f)+P((i+1)*Mathf.PI/20,.28f));
                Bar(new Vector2(-.13f,-.17f),new Vector2(.13f,-.17f),.15f);
                Bar(new Vector2(-.11f,-.32f),new Vector2(.11f,-.32f),.08f);
            }
            else if (Symbol == IdentitySymbol.Undo)
            {
                for(int i=0;i<32;i++) {float a=Mathf.Lerp(-110,100,i/32f)*Mathf.Deg2Rad,b=Mathf.Lerp(-110,100,(i+1)/32f)*Mathf.Deg2Rad;Quad(P(a,.20f),P(a,.34f),P(b,.34f),P(b,.20f));}
                Bar(new Vector2(0,.28f),new Vector2(-.29f,.28f),.13f);
                Tri(new Vector2(-.42f,.28f),new Vector2(-.18f,.47f),new Vector2(-.18f,.09f));
            }
            else if (Symbol == IdentitySymbol.Forward)
            {
                for (int i = 0; i < 2; i++)
                { float x = -.32f + i * .35f; Bar(new Vector2(x,.28f),new Vector2(x+.24f,0),.12f); Bar(new Vector2(x+.24f,0),new Vector2(x,-.28f),.12f); }
            }
            else if (Symbol == IdentitySymbol.Levels)
            {
                for (int x=0; x<2; x++) for (int y=0; y<2; y++)
                { var p = new Vector2(-.36f+x*.43f,-.36f+y*.43f); Quad(p,p+new Vector2(0,.29f),p+Vector2.one*.29f,p+new Vector2(.29f,0)); }
            }
            else if (Symbol == IdentitySymbol.Spark)
            {
                for (int i=0; i<4; i++) { float a=i*Mathf.PI/2; Tri(Vector2.zero,P(a,.45f),P(a+Mathf.PI/4,.12f)); Tri(Vector2.zero,P(a+Mathf.PI/4,.12f),P(a+Mathf.PI/2,.45f)); }
            }
            else if (Symbol == IdentitySymbol.Gear)
            {
                const int segments=96;
                for (int i=0; i<segments; i++)
                {
                    float a=i*Mathf.PI*2/segments, b=(i+1)*Mathf.PI*2/segments;
                    float outer=(i%12>=3 && i%12<9) ? .44f : .35f;
                    Quad(P(a,.17f),P(a,outer),P(b,outer),P(b,.17f));
                }
            }
            else
            {
                const int segments=48;
                for(int i=0;i<segments;i++)
                { float a=Mathf.Lerp(65,365,(float)i/segments)*Mathf.Deg2Rad,b=Mathf.Lerp(65,365,(float)(i+1)/segments)*Mathf.Deg2Rad; Quad(P(a,.23f),P(a,.37f),P(b,.37f),P(b,.23f)); }
                Tri(new Vector2(.23f,.29f),new Vector2(-.03f,.31f),new Vector2(.18f,.50f));
            }
        }
    }
}
