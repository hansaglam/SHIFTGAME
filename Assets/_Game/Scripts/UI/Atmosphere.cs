using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class Atmosphere : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r = rectTransform.rect;
            var top = new Color32(57,109,164,255); var bottom = new Color32(159,208,209,255);
            vh.AddVert(new Vector3(r.xMin,r.yMin), bottom, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin,r.yMax), top, Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMax), top, Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMin), bottom, Vector2.zero);
            vh.AddTriangle(0,1,2); vh.AddTriangle(2,3,0);
        }
    }
}
