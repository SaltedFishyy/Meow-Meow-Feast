using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Presentation-only response to connected hit targets.</summary>
    [Serializable]
    public abstract class HitFeedbackEffect : EffectModule
    {
        [SerializeField] private int triggerMode;
        [SerializeField] private float minInterval;
        /// <summary>0: each hit; 1: first hit of this node per attack cast.</summary>
        public int TriggerMode { get => triggerMode; set => triggerMode = value; }
        /// <summary>Minimum seconds between requests from this caster, skill and node, across casts.</summary>
        public float MinInterval { get => minInterval; set => minInterval = value; }
        /// <summary>Copies presentation parameters and asset references for an in-flight cast.</summary>
        public HitFeedbackEffect Snapshot() => (HitFeedbackEffect)MemberwiseClone();
    }
}
