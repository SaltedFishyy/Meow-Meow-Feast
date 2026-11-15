using MMF.Data;
using UnityEngine;

namespace MMF.Combat
{
    /// <summary>Owns one monster per fixed spawn point and restores each after its death delay.</summary>
    public sealed class MonsterSpawner : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition _definition = null;
        [SerializeField] private Monster[] _monsters = null;
        [SerializeField] private Transform[] _spawnPoints = null;
        [SerializeField] private Collider2D[] _doorBlockers = null;
        [SerializeField] private Collider2D[] _playerColliders = null;
        private float[] _remaining;

        private void Awake()
        {
            _remaining = new float[_monsters.Length];
            foreach (Collider2D door in _doorBlockers)
            foreach (Collider2D player in _playerColliders)
                Physics2D.IgnoreCollision(door, player);
            foreach (Monster monster in _monsters) monster.Died += OnDied;
        }

        private void Update()
        {
            for (int i = 0; i < _monsters.Length; i++)
            {
                if (_monsters[i].gameObject.activeSelf) continue;
                _remaining[i] -= Time.deltaTime;
                if (_remaining[i] > 0f) continue;
                _monsters[i].transform.position = _spawnPoints[i].position;
                _monsters[i].gameObject.SetActive(true);
            }
        }

        private void OnDied(Monster monster)
        {
            for (int i = 0; i < _monsters.Length; i++)
                if (_monsters[i] == monster) { _remaining[i] = _definition.RespawnDelay; return; }
        }

        private void OnDestroy()
        {
            foreach (Monster monster in _monsters)
                if (monster != null) monster.Died -= OnDied;
        }
    }
}
