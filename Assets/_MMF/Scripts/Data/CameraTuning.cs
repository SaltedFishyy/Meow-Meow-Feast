using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared camera tuning read by components.</summary>
    [CreateAssetMenu(menuName = "MMF/Camera Tuning")]
    public sealed class CameraTuning : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _insideSize = 5f;
        [SerializeField, Min(0.01f)] private float _outsideSize = 7f;
        [SerializeField, Min(0f)] private float _zoomDuration = 0.5f;
        [SerializeField, Min(0f)] private float _followTime = 0.2f;

        /// <summary>InsideSize used by components.</summary>
        public float InsideSize => _insideSize;
        /// <summary>OutsideSize used by components.</summary>
        public float OutsideSize => _outsideSize;
        /// <summary>ZoomDuration used by components.</summary>
        public float ZoomDuration => _zoomDuration;
        /// <summary>FollowTime used by components.</summary>
        public float FollowTime => _followTime;
    }
}
