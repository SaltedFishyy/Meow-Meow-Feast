using System;
using UnityEngine;
namespace AbilityStudio.Model
{
    /// <summary>Plays an audio clip at the impact point.</summary>
    [Serializable, SkillModule("Feedback/HitAudio", DisplayName = "命中音效")]
    public sealed class HitAudioFeedback : HitFeedbackEffect
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private float volume = 1;
        [SerializeField] private float pitch = 1;
        /// <summary>Audio asset; null leaves presentation silent.</summary>
        public AudioClip Clip { get => clip; set => clip = value; }
        /// <summary>Volume from zero to one.</summary>
        public float Volume { get => volume; set => volume = value; }
        /// <summary>Playback pitch from 0.1 to 3.</summary>
        public float Pitch { get => pitch; set => pitch = value; }
    }
}
