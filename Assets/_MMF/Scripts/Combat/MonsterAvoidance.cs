using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Chooses a lateral direction without consulting physics or scene objects.</summary>
    public static class MonsterAvoidance
    {
        /// <summary>Prefers right; waits if a left fallback would follow the other monster's sidestep.</summary>
        public static Vector2 SelectDirection(Vector2 movementDirection, bool rightClear, bool leftClear,
            Vector2 otherAvoidanceDirection = default)
        {
            Vector2 right = new Vector2(movementDirection.y, -movementDirection.x).normalized;
            if (rightClear) return right;
            if (!leftClear || Vector2.Dot(-right, otherAvoidanceDirection) > 0f) return Vector2.zero;
            return -right;
        }
    }
}
