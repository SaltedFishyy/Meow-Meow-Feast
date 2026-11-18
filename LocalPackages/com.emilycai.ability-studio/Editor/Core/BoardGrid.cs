using UnityEngine;
namespace AbilityStudio.Editor
{
    /// <summary>Board-space grid shared by placement and movement commands.</summary>
    public static class BoardGrid
    {
        /// <summary>Matches the GraphView background's minor grid spacing.</summary>
        public const float Spacing = 20;
        /// <summary>Rounds a canvas position to the nearest grid intersection, independent of zoom.</summary>
        public static Vector2 Snap(Vector2 point) => new Vector2(
            Mathf.Round(point.x / Spacing) * Spacing, Mathf.Round(point.y / Spacing) * Spacing);
    }
}
