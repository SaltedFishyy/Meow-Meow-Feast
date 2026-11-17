using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared cooking tuning read by components.</summary>
    [CreateAssetMenu(menuName = "MMF/Cooking Tuning")]
    public sealed class CookingTuning : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _interactionDistance = 1f;
        [SerializeField, Min(1)] private int _itemsPerBatch = 1;
        [SerializeField, Min(1), Tooltip("Read when frying starts; changes apply the next time frying starts.")] private int _lettersPerRound = 4;
        [SerializeField, Min(0.01f), Tooltip("Read when frying starts; changes apply the next time frying starts.")] private float _roundSeconds = 10f;
        [SerializeField, Min(0.01f)] private float _flipSeconds = 0.4f;

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
