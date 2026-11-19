using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Creates customers at alternating points and removes each customer on death or purification.</summary>
    public sealed class CustomerSpawner : MonoBehaviour
    {
        [SerializeField] private CustomerTuning _tuning = null;
        [SerializeField] private Customer _template = null;
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
            Customer customer = Instantiate(_template, _spawnPoints[_nextPoint].position,
                _template.transform.rotation, transform);
            customer.name = "Customer";
            customer.Purified += RemoveCustomer;
            _nextPoint = (_nextPoint + 1) % _spawnPoints.Length;
            _activeCount++;
            customer.gameObject.SetActive(true);
            customer.Monster.Died += RemoveCustomer;
        }

        private void RemoveCustomer(Monster monster)
        {
            if (_activeCount >= _tuning.MaxCount) _elapsed = 0f;
            _activeCount--;
            monster.Died -= RemoveCustomer;
            monster.GetComponent<Customer>().Purified -= RemoveCustomer;
            Destroy(monster.gameObject);
        }
    }
}
