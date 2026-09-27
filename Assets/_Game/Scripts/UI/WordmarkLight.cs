using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class WordmarkLight : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount==0) return;
            var source = new List<UIVertex>();
            vh.GetUIVertexStream(source);
            float low=float.MaxValue,high=float.MinValue,left=float.MaxValue,right=float.MinValue;
            foreach(var vertex in source)
            {
                low=Mathf.Min(low,vertex.position.y);high=Mathf.Max(high,vertex.position.y);
                left=Mathf.Min(left,vertex.position.x);right=Mathf.Max(right,vertex.position.x);
            }
            float centerX=(left+right)*.5f,centerY=(low+high)*.5f;
            for(int i=0;i<source.Count;i++)
            {
                var vertex=source[i];var p=vertex.position;
                // Keep live italic type, with a tighter silhouette and a gentler lean.
                p.x=centerX+(p.x-centerX)*.95f-(p.y-centerY)*.045f;
                vertex.position=p;
                var tint=Color.Lerp(new Color32(232,243,255,255),Color.white,Mathf.InverseLerp(low,high,p.y));
                vertex.color=(Color)vertex.color*tint;source[i]=vertex;
            }
            // Circular expansion adds weight and softens corners without a contrasting outline.
            var weighted=new List<UIVertex>(source.Count*9);
            for(int sample=0;sample<8;sample++)
            {
                float angle=sample*Mathf.PI/4;
                var offset=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*2.2f;
                foreach(var original in source)
                {var vertex=original;vertex.position+=offset;weighted.Add(vertex);}
            }
            weighted.AddRange(source);vh.Clear();vh.AddUIVertexTriangleStream(weighted);
        }
    }
}
