using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Creates customers at alternating points and removes each customer on death.</summary>
    public sealed class CustomerSpawner : MonoBehaviour
    {
        [SerializeField] private CustomerTuning _tuning = null;
        [SerializeField] private Monster _template = null;
        [SerializeField] private Transform[] _spawnPoints = null;
        private int _activeCount;
        private int _nextPoint;
        private float _elapsed;
        private bool _hasSpawned;

        private void Update()
        {
            if (_activeCount >= _tuning.MaxCount)
            {
                _elapsed = 0f;
                return;
            }

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;
            _elapsed += deltaTime;
            if (_elapsed < (_hasSpawned ? _tuning.Interval : _tuning.FirstDelay)) return;

            _elapsed = 0f;
            _hasSpawned = true;
            Monster monster = Instantiate(_template, _spawnPoints[_nextPoint].position,
                _template.transform.rotation, transform);
            monster.name = "Customer";
            monster.Died += OnDied;
            _nextPoint = (_nextPoint + 1) % _spawnPoints.Length;
            _activeCount++;
            monster.gameObject.SetActive(true);
        }

        private void OnDied(Monster monster)
        {
            if (_activeCount >= _tuning.MaxCount) _elapsed = 0f;
            _activeCount--;
            monster.Died -= OnDied;
            Destroy(monster.gameObject);
        }
    }
}
