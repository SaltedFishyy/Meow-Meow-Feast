using UnityEngine;
namespace AbilityStudio.Runtime
{
    /// <summary>Shared sector geometry for ground-plane hits and preview outlines.</summary>
    public static class ConeGeometry
    {
        /// <summary>Point along the arc from its left edge (t=0) to its right edge (t=1). Direction must be normalized on XY.</summary>
        public static Vector3 ArcPoint(Vector3 origin, Vector3 direction, float range, float angleDegrees, float t)
        {
            float angle = (t - 0.5f) * angleDegrees * Mathf.Deg2Rad;
            var right = new Vector3(direction.y, -direction.x, 0);
            return origin + (direction * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * range;
        }
        /// <summary>Exact circle-sector intersection, including arc and radial edges, for normalized XY direction.</summary>
        public static bool Intersects(Vector3 origin, Vector3 direction, float range, float angleDegrees, Vector3 target, float radius)
        {
            var offset = target - origin; offset.z = 0; radius = Mathf.Max(0, radius);
            var right = new Vector3(direction.y, -direction.x, 0);
            float angle = Mathf.Atan2(Mathf.Abs(Vector3.Dot(offset, right)), Vector3.Dot(offset, direction)) * Mathf.Rad2Deg;
            if (angleDegrees >= 360 || angle <= angleDegrees * 0.5f)
                return offset.sqrMagnitude <= (range + radius) * (range + radius) + 0.000001f;
            var left = ArcPoint(Vector3.zero, direction, 1, angleDegrees, 0);
            var edgeRight = ArcPoint(Vector3.zero, direction, 1, angleDegrees, 1);
            return Mathf.Min(DistanceToSegmentSquared(offset, left, range), DistanceToSegmentSquared(offset, edgeRight, range)) <= radius * radius + 0.000001f;
        }
        private static float DistanceToSegmentSquared(Vector3 point, Vector3 direction, float range)
        {
            float t = Mathf.Clamp(Vector3.Dot(point, direction), 0, range);
            return (point - direction * t).sqrMagnitude;
        }
    }
}

