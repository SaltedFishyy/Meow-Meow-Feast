using UnityEngine;

namespace MMF.Data
{
    /// <summary>Controls when customers may appear and how many can be active.</summary>
    [TuningGroup("顾客", 6)]
    public sealed class CustomerTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("开局几秒来第一个"), Min(0f)] private float _firstDelay = 15f;
        [SerializeField, TuningLabel("每隔几秒来一个"), Min(0f)] private float _interval = 15f;
        [SerializeField, TuningLabel("场上最多几个"), Min(0)] private int _maxCount = 3;

        [SerializeField, TuningLabel("吃完几秒消失"), Min(0f)] private float _purifySeconds = 1f;

        /// <summary>Time from feeding to disappearance, in seconds.</summary>
        public float PurifySeconds => _purifySeconds;

        /// <summary>Delay before the first customer appears, in seconds.</summary>
        public float FirstDelay => _firstDelay;

        /// <summary>Delay between later customers, in seconds.</summary>
        public float Interval => _interval;

        /// <summary>Maximum number of customers allowed on the field.</summary>
        public int MaxCount => _maxCount;
    }
}
