using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Spawns a visual prefab at a hit position for a bounded lifetime.</summary>
    [Serializable, SkillModule("Feedback/HitVfx", DisplayName = "命中特效")]
    public sealed class HitVfxFeedback : HitFeedbackEffect
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private float scale = 1;
        [SerializeField] private float lifetime = 1;
        [SerializeField] private Vector3 offset;
        /// <summary>Visual prefab; null leaves presentation silent.</summary>
        public GameObject Prefab { get => prefab; set => prefab = value; }
        /// <summary>Uniform multiplier of the prefab's authored scale.</summary>
        public float Scale { get => scale; set => scale = value; }
        /// <summary>Maximum presentation lifetime in seconds.</summary>
        public float Lifetime { get => lifetime; set => lifetime = value; }
        /// <summary>World-space offset from the hit point.</summary>
        public Vector3 Offset { get => offset; set => offset = value; }
    }
}
