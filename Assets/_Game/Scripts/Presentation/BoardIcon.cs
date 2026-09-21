using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public enum BoardIconShape { Arrow, Clockwise, Diamond, DoubleDiamond, Crate }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BoardIcon : MaskableGraphic
    {
        public BoardIconShape Shape { get; private set; }
        public Direction Facing { get; private set; }
        public void Configure(BoardIconShape shape, Direction facing = Direction.Right)
        { Shape=shape; Facing=facing; raycastTarget=false; SetVerticesDirty(); }
        public static BoardIcon Create(string name,Transform parent,BoardIconShape shape,Color color,Vector2 min,Vector2 max,Direction facing=Direction.Right)
        {
            var icon=PlaceholderVisuals.Rect(name,parent,min,max).gameObject.AddComponent<BoardIcon>();
            icon.color=color; icon.Configure(shape,facing); return icon;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r=rectTransform.rect; float size=Mathf.Min(r.width,r.height); var center=r.center;
            if(Shape==BoardIconShape.Diamond) size=Mathf.Min(r.width/.60f,r.height/.80f);
            else if(Shape==BoardIconShape.DoubleDiamond) size=Mathf.Min(r.width/.96f,r.height/.66f);
            Vector2 Map(Vector2 p)
            {
                if(Shape==BoardIconShape.Arrow)
                    p=Facing==Direction.Up?new Vector2(-p.y,p.x):Facing==Direction.Down?new Vector2(p.y,-p.x):Facing==Direction.Left?-p:p;
                return center+p*size;
            }
            void Tri(Vector2 a,Vector2 b,Vector2 c)
            {
                int i=vh.currentVertCount; vh.AddVert(Map(a),color,Vector2.zero); vh.AddVert(Map(b),color,Vector2.zero); vh.AddVert(Map(c),color,Vector2.zero); vh.AddTriangle(i,i+1,i+2);
            }
            void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d) { Tri(a,b,c); Tri(a,c,d); }
            void Bar(Vector2 a,Vector2 b,float width)
            {
                var v=(b-a).normalized; var n=new Vector2(-v.y,v.x)*width*.5f; Quad(a-n,a+n,b+n,b-n);
            }
            void Diamond(float x,float scale)
            {
                Vector2 a=new Vector2(x,scale), b=new Vector2(x+scale*.72f,0), c=new Vector2(x,-scale), d=new Vector2(x-scale*.72f,0);
                Bar(a,b,.055f); Bar(b,c,.055f); Bar(c,d,.055f); Bar(d,a,.055f);
            }
            if(Shape==BoardIconShape.Arrow)
            {
                if(Facing==Direction.None) return;
                Quad(new Vector2(-.39f,-.11f),new Vector2(-.39f,.11f),new Vector2(.07f,.11f),new Vector2(.07f,-.11f));
                Tri(new Vector2(.01f,.30f),new Vector2(.40f,0),new Vector2(.01f,-.30f));
            }
            else if(Shape==BoardIconShape.Clockwise)
            {
                const int segments=44; const float start=140, end=-130, outer=.40f, inner=.25f;
                Vector2 P(float degrees,float radius) { float a=degrees*Mathf.Deg2Rad;return new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius; }
                for(int i=0;i<segments;i++)
                {
                    float a=Mathf.Lerp(start,end,(float)i/segments), b=Mathf.Lerp(start,end,(float)(i+1)/segments);
                    Quad(P(a,inner),P(a,outer),P(b,outer),P(b,inner));
                }
                float angle=end*Mathf.Deg2Rad; var tangent=new Vector2(Mathf.Sin(angle),-Mathf.Cos(angle));
                var normal=new Vector2(-tangent.y,tangent.x); var endpoint=P(end,.325f);
                Tri(endpoint+normal*.17f-tangent*.04f,endpoint+tangent*.20f,endpoint-normal*.17f-tangent*.04f);
            }
            else if(Shape==BoardIconShape.Diamond) Diamond(0,.34f);
            else if(Shape==BoardIconShape.DoubleDiamond) { Diamond(-.24f,.27f); Diamond(.24f,.27f); }
            else
            {
                Bar(new Vector2(-.33f,-.33f),new Vector2(.33f,.33f),.13f);
                Bar(new Vector2(-.33f,.33f),new Vector2(.33f,-.33f),.13f);
            }
        }
    }
}
