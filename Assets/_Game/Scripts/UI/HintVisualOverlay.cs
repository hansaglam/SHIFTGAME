using UnityEngine;
using UnityEngine.UI;
namespace Shift.Game
{
    // One cached, non-intercepting overlay on BoardView's normalized cell coordinate plane.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HintVisualOverlay : MaskableGraphic
    {
        private BoardManager board;
        private GameFeelSettings feel;
        private HintVisualResolver resolver;
        private HintBoardState shownState;
        private CanvasGroup opacity;
        private float elapsed;
        public HintVisualTarget Target { get; private set; }
        public bool IsVisible => Target != null;
        public bool IsStatic => feel == null || feel.reducedMotion;
        public void Initialize(BoardManager model, LevelData level, GameFeelSettings settings)
        {
            board = model; feel = settings; resolver = new HintVisualResolver(level);
            raycastTarget = false; opacity = gameObject.AddComponent<CanvasGroup>();
            opacity.blocksRaycasts = false; opacity.interactable = false; Clear();
        }
        public bool Show(int stage)
        {
            Clear(); Target = resolver?.Resolve(board, stage);
            if (Target == null) return false;
            shownState = new HintBoardState(board); elapsed = 0; opacity.alpha = 1; SetVerticesDirty(); return true;
        }
        public void Clear() { Target = null; shownState = null; if (opacity != null) opacity.alpha = 0; SetVerticesDirty(); }
        private void Update()
        {
            if (!IsVisible) return;
            if (!shownState.Matches(board)) { Clear(); return; }
            elapsed += Time.unscaledDeltaTime;
            // Two 1.5-second luminance pulses, then a static outline. No scale or gameplay delay.
            opacity.alpha = IsStatic || elapsed >= 3 ? 1 : .72f + .28f * (.5f + .5f * Mathf.Cos(elapsed * Mathf.PI * 2 / 1.5f));
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (Target == null || board == null) return;
            var rect = rectTransform.rect;
            float cell = Mathf.Min(rect.width / board.Width, rect.height / board.Height), radius = cell * .41f;
            var cyan = new Color32(83, 239, 255, 255); var glow = new Color32(59, 222, 255, 48);
            if (Target.Relationship)
                for (int i = 1; i < Target.Cells.Length; i++)
                {
                    var a = Center(Target.Cells[i-1], rect); var b = Center(Target.Cells[i], rect); var delta = b-a;
                    if (delta.magnitude <= radius * 2) continue;
                    a += delta.normalized * radius; b -= delta.normalized * radius;
                    for (int j = 0; j < 9; j++) Line(vh, Vector2.Lerp(a,b,j/9f), Vector2.Lerp(a,b,(j+.55f)/9f), 3, cyan);
                }
            foreach (var target in Target.Cells)
            {
                var c = Center(target, rect);
                Ring(vh,c,radius,11,glow); Ring(vh,c,radius,3.5f,cyan);
                if (Target.ExactMove)
                {
                    Ring(vh,c,radius-6,1.5f,new Color32(239,255,255,230));
                    // Small contact mark; icon-only and no gesture animation or future-move trail.
                    var contact = c + Vector2.down * radius;
                    Ring(vh,contact,8,5,new Color32(18,42,63,255)); Ring(vh,contact,5,4,cyan);
                }
            }
        }
        private Vector2 Center(GridPosition p, Rect rect) => new Vector2(rect.xMin+(p.x+.5f)*rect.width/board.Width,rect.yMin+(p.y+.5f)*rect.height/board.Height);
        private static void Ring(VertexHelper vh, Vector2 center, float radius, float width, Color32 tint)
        {
            const int segments = 64;
            for (int i = 0; i < segments; i++)
            {
                float a = i*Mathf.PI*2/segments, b = (i+1)*Mathf.PI*2/segments;
                var u = new Vector2(Mathf.Cos(a),Mathf.Sin(a)); var v = new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                Quad(vh,center+u*(radius-width/2),center+u*(radius+width/2),center+v*(radius+width/2),center+v*(radius-width/2),tint);
            }
        }
        private static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color32 tint)
        { var d = (b-a).normalized; var n = new Vector2(-d.y,d.x)*width/2; Quad(vh,a-n,a+n,b+n,b-n,tint); }
        private static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 tint)
        {
            int start=vh.currentVertCount; vh.AddVert(a,tint,Vector2.zero); vh.AddVert(b,tint,Vector2.zero); vh.AddVert(c,tint,Vector2.zero); vh.AddVert(d,tint,Vector2.zero);
            vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
        }
    }
}
