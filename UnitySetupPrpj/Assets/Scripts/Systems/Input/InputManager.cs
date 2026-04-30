namespace Game.Systems
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Reads raw input from the InputActions asset, normalizes it via a radial deadzone,
    /// and exposes it to gameplay through <see cref="IInputManager"/>.
    /// Lives in the Persistent scene; owned and referenced by GameManager.
    /// </summary>
    public sealed class InputManager : MonoBehaviour, IInputManager
    {
        /// <summary>Radial deadzone applied to the Move stick before magnitude remapping.</summary>
        internal const float DeadzoneRadius = 0.15f;

        [SerializeField] private InputActionAsset _inputActions;

        private InputAction _moveAction;
        private InputAction _pauseAction;
        private bool _enabled = true;
        private bool _pauseTriggered;

        private void Awake()
        {
            _moveAction = _inputActions.FindAction("Gameplay/Move", throwIfNotFound: true);
            _pauseAction = _inputActions.FindAction("Gameplay/Pause", throwIfNotFound: true);
            _pauseAction.performed += OnPausePerformed;
            Log.Info(LogCat.Input, "InputManager initialized.");
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void OnDestroy()
        {
            _pauseAction.performed -= OnPausePerformed;
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            if (_enabled)
                _pauseTriggered = true;
        }

        /// <inheritdoc/>
        public MovementInput GetMovementInput()
        {
            if (!_enabled)
                return default;

            Vector2 raw = _moveAction.ReadValue<Vector2>();
            Vector2 normalized = ApplyDeadzone(raw, DeadzoneRadius);
            return new MovementInput(
                throttle: Mathf.Clamp(normalized.y, -1f, 1f),
                steering: Mathf.Clamp(normalized.x, -1f, 1f));
        }

        /// <inheritdoc/>
        public bool ConsumePausePressed()
        {
            if (!_pauseTriggered)
                return false;

            _pauseTriggered = false;
            return true;
        }

        /// <inheritdoc/>
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled)
                _pauseTriggered = false;
            Log.Info(LogCat.Input, "InputManager enabled: {0}", enabled);
        }

        /// <summary>
        /// Applies a radial deadzone and remaps the surviving range to [0, +1].
        /// Values whose magnitude is below <paramref name="deadzone"/> map to zero.
        /// Exposed as internal for EditMode tests.
        /// </summary>
        /// <param name="raw">Raw Vector2 read from the Move action.</param>
        /// <param name="deadzone">Deadzone radius in the [0, 1) range.</param>
        internal static Vector2 ApplyDeadzone(Vector2 raw, float deadzone)
        {
            float magnitude = raw.magnitude;
            if (magnitude < deadzone)
                return Vector2.zero;

            float remapped = (magnitude - deadzone) / (1f - deadzone);
            return raw.normalized * Mathf.Min(remapped, 1f);
        }
    }
}
