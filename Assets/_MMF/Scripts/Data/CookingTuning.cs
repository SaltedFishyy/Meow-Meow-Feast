using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared cooking tuning read by components.</summary>
    [TuningGroup("煎锅", 3)]
    [CreateAssetMenu(menuName = "MMF/Cooking Tuning")]
    public sealed class CookingTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("互动距离"), Min(0f)] private float _interactionDistance = 1f;
        [SerializeField, TuningLabel("翻面动画几秒"), Min(0.01f)] private float _flipSeconds = 0.4f;

        [SerializeField, TuningLabel("每面煎几秒"), Min(0.01f)] private float _sideSeconds = 3f;
        [SerializeField, TuningLabel("可以翻以后几秒焦"), Min(0.01f)] private float _burnSeconds = 5f;
        [SerializeField, TuningLabel("快焦提示提前几秒"), Min(0f)] private float _burnWarningSeconds = 2f;

        [SerializeField, TuningLabel("锅画面在右边（试验）")] private bool _panViewOnRight = true;

        /// <summary>Whether the pan is shown in the right-side panel.</summary>
        public bool PanViewOnRight => _panViewOnRight;

        /// <summary>InteractionDistance used by components.</summary>
        public float InteractionDistance => _interactionDistance;
        /// <summary>FlipSeconds used by components.</summary>
        public float FlipSeconds => _flipSeconds;
        /// <summary>Cooking duration sampled when a side starts.</summary>
        public float SideSeconds => _sideSeconds;
        /// <summary>Burn delay sampled when a side starts.</summary>
        public float BurnSeconds => _burnSeconds;
        /// <summary>Time remaining before burning at which the visual warning starts.</summary>
        public float BurnWarningSeconds => _burnWarningSeconds;
    }
}
