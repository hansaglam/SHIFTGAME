using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // One passive, dynamic rope per authored pair. Lives below pieces, draws
    // directly from their animated RectTransform anchors and never captures input.
    [RequireComponent(typeof(RectTransform))]
    public sealed class TetherLinkGraphic : MaskableGraphic
    {
        private RectTransform first, second, board;
        private Piece firstPiece, secondPiece;
        private float cellSize;

        public void Bind(Piece a, Piece b, RectTransform boardRect,
            int width, int height)
        {
            firstPiece = a;
            secondPiece = b;
            first = (RectTransform)a.transform;
            second = (RectTransform)b.transform;
            board = boardRect;
            cellSize = Mathf.Max(.008f, 1f / Mathf.Max(width, height));
            color = new Color32(250, 200, 109, 255);
            raycastTarget = false;
            SetVerticesDirty();
        }

        private Vector2 Center(RectTransform piece)
        {
            Rect bounds = board.rect;
            Vector2 anchor = (piece.anchorMin + piece.anchorMax) * .5f;
            return bounds.min + Vector2.Scale(anchor, bounds.size);
        }

        private static void Quad(VertexHelper helper, Vector2 a,
            Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int start = helper.currentVertCount;
            var vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = a;
            helper.AddVert(vertex);
            vertex.position = b;
            helper.AddVert(vertex);
            vertex.position = c;
            helper.AddVert(vertex);
            vertex.position = d;
            helper.AddVert(vertex);
            helper.AddTriangle(start, start + 1, start + 2);
            helper.AddTriangle(start, start + 2, start + 3);
        }

        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            if (first == null || second == null || board == null ||
                firstPiece == null || secondPiece == null ||
                !firstPiece.gameObject.activeInHierarchy ||
                !secondPiece.gameObject.activeInHierarchy)
                return;

            Vector2 a = Center(first), b = Center(second);
            Vector2 diff = b - a;
            if (diff.sqrMagnitude < 1f) return;
            Vector2 perpendicular = new Vector2(-diff.y, diff.x).normalized;
            float thickness = Mathf.Clamp(board.rect.height * cellSize * .038f,
                2.5f, 5.5f);
            var tint = color;
            tint.a = .20f;
            Quad(helper, a - perpendicular * thickness * 2f,
                a + perpendicular * thickness * 2f,
                b + perpendicular * thickness * 2f,
                b - perpendicular * thickness * 2f, tint);
            tint.a = .95f;
            Quad(helper, a - perpendicular * thickness * .5f,
                a + perpendicular * thickness * .5f,
                b + perpendicular * thickness * .5f,
                b - perpendicular * thickness * .5f, tint);
        }

        private void LateUpdate()
        {
            if (first != null && second != null &&
                firstPiece != null && secondPiece != null)
                SetVerticesDirty();
        }
    }
}
