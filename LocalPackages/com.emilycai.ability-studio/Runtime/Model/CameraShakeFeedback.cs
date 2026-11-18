using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Requests camera-local positional shake without controlling a game camera.</summary>
    [Serializable, SkillModule("Feedback/CameraShake", DisplayName = "镜头震动")]
    public sealed class CameraShakeFeedback : HitFeedbackEffect
    {
        [SerializeField] private float strength = .15f;
        [SerializeField] private float duration = .2f;
        [SerializeField] private float frequency = 25;
        public CameraShakeFeedback() { TriggerMode = 1; }
        /// <summary>Maximum positional amplitude in world units.</summary>
        public float Strength { get => strength; set => strength = value; }
        /// <summary>Duration in seconds.</summary>
        public float Duration { get => duration; set => duration = value; }
        /// <summary>Oscillations per second.</summary>
        public float Frequency { get => frequency; set => frequency = value; }
    }
}
