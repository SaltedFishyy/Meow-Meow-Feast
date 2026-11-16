using System;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Point containment rules for a melee sector, independent of scene state.</summary>
    public static class WeaponArc
    {
        /// <summary>Includes range and angle boundaries; a zero aim direction never hits.</summary>
        public static bool Contains(Vector2 origin, Vector2 aimDirection, Vector2 target,
            float range, float angle)
        {
            if (aimDirection.sqrMagnitude == 0f) return false;
            Vector2 offset = target - origin;
            if (offset.sqrMagnitude > range * range) return false;
            if (offset.sqrMagnitude == 0f) return true;
            double cross = (double)aimDirection.x * offset.y - (double)aimDirection.y * offset.x;
            double dot = (double)aimDirection.x * offset.x + (double)aimDirection.y * offset.y;
            double degrees = Math.Atan2(Math.Abs(cross), dot) * (180.0 / Math.PI);
            return degrees <= angle * 0.5;
        }
    }
}
