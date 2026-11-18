using UnityEngine;

namespace MMF.Data
{
    /// <summary>Live dimensions of the prototype map.</summary>
    [TuningGroup("场地", 11)]
    public sealed class MapTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("场地宽"), Min(1f)] private float _width = 32f;
        [SerializeField, TuningLabel("场地高"), Min(1f)] private float _height = 18f;

        /// <summary>Map width in world units.</summary>
        public float Width => _width;
        /// <summary>Map height in world units.</summary>
        public float Height => _height;
    }
}
