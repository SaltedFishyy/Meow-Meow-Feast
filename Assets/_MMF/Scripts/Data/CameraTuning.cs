using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared camera tuning read by components.</summary>
    [TuningGroup("镜头", 4)]
    [CreateAssetMenu(menuName = "MMF/Camera Tuning")]
    public sealed class CameraTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("车里镜头大小"), Min(0.01f)] private float _insideSize = 5f;
        [SerializeField, TuningLabel("车外镜头大小"), Min(0.01f)] private float _outsideSize = 9f;
        [SerializeField, TuningLabel("车外镜头固定")] private bool _fixedOutside = true;
        [SerializeField, TuningLabel("拉近拉远时间（秒）"), Min(0f)] private float _zoomDuration = 0.5f;
        [SerializeField, TuningLabel("跟随延迟（秒）"), Min(0f)] private float _followTime = 0.2f;

        /// <summary>InsideSize used by components.</summary>
        public float InsideSize => _insideSize;
        /// <summary>OutsideSize used by components.</summary>
        public float OutsideSize => _outsideSize;
        /// <summary>Whether the outdoor camera stays centered on the map.</summary>
        public bool FixedOutside => _fixedOutside;
        /// <summary>ZoomDuration used by components.</summary>
        public float ZoomDuration => _zoomDuration;
        /// <summary>FollowTime used by components.</summary>
        public float FollowTime => _followTime;
    }
}
