using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared cooking tuning read by components.</summary>
    [TuningGroup("煎锅", 3)]
    [CreateAssetMenu(menuName = "MMF/Cooking Tuning")]
    public sealed class CookingTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("互动距离"), Min(0f)] private float _interactionDistance = 1f;
        [SerializeField, TuningLabel("一次放几个"), Min(1)] private int _itemsPerBatch = 1;
        [SerializeField, TuningLabel("每轮几个字母（下次开始煎时生效）"), Min(1)] private int _lettersPerRound = 4;
        [SerializeField, TuningLabel("每轮几秒（下次开始煎时生效）"), Min(0.01f)] private float _roundSeconds = 10f;
        [SerializeField, TuningLabel("翻面动画几秒"), Min(0.01f)] private float _flipSeconds = 0.4f;

        /// <summary>InteractionDistance used by components.</summary>
        public float InteractionDistance => _interactionDistance;
        /// <summary>ItemsPerBatch used by components.</summary>
        public int ItemsPerBatch => _itemsPerBatch;
        /// <summary>LettersPerRound used by components.</summary>
        public int LettersPerRound => _lettersPerRound;
        /// <summary>RoundSeconds used by components.</summary>
        public float RoundSeconds => _roundSeconds;
        /// <summary>FlipSeconds used by components.</summary>
        public float FlipSeconds => _flipSeconds;
    }
}
