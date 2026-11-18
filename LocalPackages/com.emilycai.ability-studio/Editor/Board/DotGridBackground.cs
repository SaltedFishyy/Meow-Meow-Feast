using UnityEngine;
using UnityEngine.UIElements;
using Canvas = UnityEditor.Experimental.GraphView.GraphView;

namespace AbilityStudio.Editor.Board.GraphView
{
    /// <summary>Viewport dot grid aligned with the board's snapping coordinates.</summary>
    internal sealed class DotGridBackground : VisualElement
    {
        private readonly Canvas _board;
        internal DotGridBackground(Canvas board)
        {
            _board = board;
            pickingMode = PickingMode.Ignore;
            style.backgroundColor = (Color)new Color32(30, 30, 30, 255);
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            var origin = _board.contentViewContainer.ChangeCoordinatesTo(this, Vector2.zero);
            var next = _board.contentViewContainer.ChangeCoordinatesTo(this, Vector2.one * BoardGrid.Spacing);
            var step = next - origin;
            if (!float.IsFinite(step.x) || !float.IsFinite(step.y) || step.x < 1 || step.y < 1) return;
            float width = contentRect.width, height = contentRect.height;
            if (!float.IsFinite(width) || !float.IsFinite(height) || width <= 0 || height <= 0) return;
            float firstX = Mathf.Repeat(origin.x, step.x), firstY = Mathf.Repeat(origin.y, step.y);
            int columns = Mathf.FloorToInt((width - firstX) / step.x) + 1;
            if (columns <= 0) return;
            // Allocate each row separately to keep vertex indices below the mesh's ushort limit.
            float radius = Mathf.Clamp(step.x / BoardGrid.Spacing, .65f, 1.2f);
            var color = new Color32(71, 71, 71, 255);
            for (float y = firstY; y < height; y += step.y)
            {
                var mesh = context.Allocate(columns * 8, columns * 18);
                for (int col = 0; col < columns; col++)
                {
                    float x = firstX + col * step.x;
                    for (int v = 0; v < 8; v++)
                    {
                        float angle = v * Mathf.PI / 4;
                        mesh.SetNextVertex(new Vertex {
                            position = new Vector3(Mathf.Clamp(x + Mathf.Cos(angle) * radius, 0, width),
                                Mathf.Clamp(y + Mathf.Sin(angle) * radius, 0, height), Vertex.nearZ), tint = color });
                    }
                    for (int triangle = 1; triangle < 7; triangle++)
                    {
                        mesh.SetNextIndex((ushort)(col * 8));
                        mesh.SetNextIndex((ushort)(col * 8 + triangle));
                        mesh.SetNextIndex((ushort)(col * 8 + triangle + 1));
                    }
                }
            }
        }
    }
}
