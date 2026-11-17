using UnityEngine;

namespace MMF.Data
{
    /// <summary>Shared backpack and ground item tuning.</summary>
    [TuningGroup("背包", 2)]
    [CreateAssetMenu(menuName = "MMF/Backpack Tuning")]
    public sealed class BackpackTuning : ScriptableObject
    {
        [SerializeField, TuningLabel("背包格数（下次 Play 生效）"), Min(1)] private int _slotCount = 5;
        [SerializeField, TuningLabel("每格最多叠几个（下次 Play 生效）"), Min(1)] private int _stackLimit = 3;
        [SerializeField, TuningLabel("自动捡起距离"), Min(0f)] private float _pickupRadius = 0.5f;
        [SerializeField, TuningLabel("丢掉后几秒才能捡回"), Min(0f)] private float _dropPickupDelay = 2f;
        [SerializeField, TuningLabel("掉落物几秒后消失"), Min(0f)] private float _worldLifetime = 60f;
        [SerializeField, TuningLabel("头上\"x\"显示几秒"), Min(0f)] private float _failureSeconds = 1f;

        /// <summary>Number of slots, read when the backpack awakens.</summary>
        public int SlotCount => _slotCount;
        /// <summary>Items per slot, read when the backpack awakens.</summary>
        public int StackLimit => _stackLimit;
        /// <summary>Maximum automatic pickup distance.</summary>
        public float PickupRadius => _pickupRadius;
        /// <summary>Pickup cooldown for items dropped by the player.</summary>
        public float DropPickupDelay => _dropPickupDelay;
        /// <summary>Ground item lifetime shared with monster drops.</summary>
        public float WorldLifetime => _worldLifetime;
        /// <summary>Duration of the failure marker above the player.</summary>
        public float FailureSeconds => _failureSeconds;
    }
}
