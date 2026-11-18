using MMF.Player;
using MMF.Truck;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MMF.UI
{
    /// <summary>Pauses a failed run and reloads its scene on restart input.</summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private TruckHealth _health = null;
        [SerializeField] private PlayerInputReader _playerInput = null;
        [SerializeField] private InputActionAsset _controls = null;
        [SerializeField] private GameObject _overlay = null;

        private InputAction _restart;
        private bool _failed;

        private void Awake()
        {
            _restart = _controls.FindAction("Gameplay/Restart", true).Clone();
            _restart.performed += Restart;
        }

        private void OnEnable()
        {
            _health.Destroyed += Show;
        }

        private void OnDisable()
        {
            _health.Destroyed -= Show;
            _restart.Disable();
        }

        private void Show()
        {
            _failed = true;
            Time.timeScale = 0f;
            _playerInput.enabled = false;
            _overlay.SetActive(true);
            _restart.Enable();
        }

        private void Restart(InputAction.CallbackContext context)
        {
            _restart.Disable();
            SceneManager.LoadSceneAsync(gameObject.scene.buildIndex);
        }

        private void OnDestroy()
        {
            _restart.performed -= Restart;
            _restart.Dispose();
            if (_failed) Time.timeScale = 1f;
        }
    }
}
