using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Fade only the approved gold rich-text Perfect line; keep the existing text layout/localization intact.
    public sealed class PerfectStatusFade : BaseMeshEffect
    {
        private float opacity = 1;
        public float Opacity
        {
            get => opacity;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(opacity, value)) return;
                opacity = value;
                if (graphic != null) graphic.SetVerticesDirty();
            }
        }
        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive() || opacity >= 1) return;
            var vertex = new UIVertex();
            for (int i = 0; i < vertices.currentVertCount; i++)
            {
                vertices.PopulateUIVertex(ref vertex, i);
                var color = vertex.color;
                if (color.r != 144 || color.g != 100 || color.b != 20) continue;
                color.a = (byte)Mathf.RoundToInt(color.a * opacity);
                vertex.color = color;
                vertices.SetUIVertex(vertex, i);
            }
        }
    }
}
