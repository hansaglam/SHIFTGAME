using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class WordmarkLight : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount==0) return;
            var v = new UIVertex(); float low=float.MaxValue,high=float.MinValue;
            for(int i=0;i<vh.currentVertCount;i++) { vh.PopulateUIVertex(ref v,i);low=Mathf.Min(low,v.position.y);high=Mathf.Max(high,v.position.y); }
            for(int i=0;i<vh.currentVertCount;i++)
            { vh.PopulateUIVertex(ref v,i); var tint=Color.Lerp(new Color32(204,225,249,255),Color.white,Mathf.InverseLerp(low,high,v.position.y));v.color=(Color)v.color*tint;vh.SetUIVertex(v,i); }
        }
    }
}
