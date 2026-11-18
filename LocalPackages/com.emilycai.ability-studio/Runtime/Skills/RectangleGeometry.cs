using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Shared XY geometry for rectangular targeting and presentation.</summary>
    public static class RectangleGeometry
    {
        /// <summary>Tests circular target bounds against a rectangle starting at origin. Direction must be normalized on XY.</summary>
        public static bool Intersects(Vector3 origin, Vector3 direction, float length, float width, Vector3 target, float radius)
        {
            var offset = target - origin; offset.z = 0;
            var right = new Vector3(direction.y, -direction.x, 0);
            float forward = Vector3.Dot(offset, direction), side = Vector3.Dot(offset, right);
            float dz = forward - Mathf.Clamp(forward, 0, length);
            float dx = side - Mathf.Clamp(side, -width * 0.5f, width * 0.5f);
            radius = Mathf.Max(0, radius);
            return dx * dx + dz * dz <= radius * radius + 0.000001f;
        }
        /// <summary>Returns one of four perimeter corners using the same dimensions as targeting.</summary>
        public static Vector3 Corner(Vector3 origin, Vector3 direction, float length, float width, int index)
        {
            var right = new Vector3(direction.y, -direction.x, 0) * (width * 0.5f);
            return origin + (index == 1 || index == 2 ? direction * length : Vector3.zero) + (index < 2 ? -right : right);
        }
    }
}

