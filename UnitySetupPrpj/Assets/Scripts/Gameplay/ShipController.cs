namespace Game.Gameplay
{
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// Kinematic ship controller. Moves the Transform directly; no Rigidbody or physics queries.
    /// Reads movement input from an injected <see cref="IInputManager"/> each frame.
    /// </summary>
    public sealed class ShipController : MonoBehaviour
    {
        private const float DriftWarnDistanceSq = 10000f * 10000f;

        [SerializeField] private SO_ShipTuning _tuning;

        /// <summary>World-space Y position the ship is locked to (sea surface). Constant in v0.2.</summary>
        [SerializeField] private float _seaSurfaceY = 0f;

        private IInputManager _input;
        private Transform _transform;
        private float _currentSpeed;
        private float _smoothedSteering;
        private float _throttleInput;
        private float _steeringInput;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            if (_input != null)
            {
                MovementInput mi = _input.GetMovementInput();
                SetInput(mi.Throttle, mi.Steering);
            }
            Tick(Time.deltaTime);
        }

        // ------------------------------------------------------------------ public API

        /// <summary>
        /// Wires the input source. Called by the scene wiring script after the Game scene loads.
        /// </summary>
        public void Inject(IInputManager input)
        {
            _input = input;
        }

        /// <summary>
        /// Sets the current movement intent. Each axis is clamped to [-1, 1].
        /// NaN or Infinity values are clamped to 0 and a warning is logged.
        /// </summary>
        public void SetInput(float throttle, float steering)
        {
            if (float.IsNaN(throttle) || float.IsInfinity(throttle))
            {
                Log.Warn(LogCat.Gameplay, "ShipController: invalid throttle {0}, clamping to 0", throttle);
                throttle = 0f;
            }

            if (float.IsNaN(steering) || float.IsInfinity(steering))
            {
                Log.Warn(LogCat.Gameplay, "ShipController: invalid steering {0}, clamping to 0", steering);
                steering = 0f;
            }

            _throttleInput = Mathf.Clamp(throttle, -1f, 1f);
            _steeringInput = Mathf.Clamp(steering, -1f, 1f);
        }

        /// <summary>
        /// Instantly places the ship at <paramref name="position"/> facing <paramref name="yawDeg"/>.
        /// Called by SaveSystem on load. Resets accumulated speed and steering.
        /// </summary>
        public void Teleport(Vector3 position, float yawDeg)
        {
            if (_transform == null)
                _transform = transform;
            _transform.position = new Vector3(position.x, _seaSurfaceY, position.z);
            _transform.rotation = Quaternion.Euler(0f, yawDeg, 0f);
            _currentSpeed = 0f;
            _smoothedSteering = 0f;
        }

        /// <summary>Returns a snapshot of the ship's current world position and yaw.</summary>
        public ShipState GetState()
        {
            if (_transform == null)
                _transform = transform;
            return new ShipState(_transform.position, _transform.eulerAngles.y);
        }

        // ------------------------------------------------------------------ internal (testable)

        /// <summary>
        /// Applies one simulation step of <paramref name="dt"/> seconds.
        /// Called by <see cref="Update"/> with <see cref="Time.deltaTime"/>;
        /// exposed internally for EditMode tests.
        /// </summary>
        internal void Tick(float dt)
        {
            if (_transform == null)
                _transform = transform;

            if (_tuning == null)
                return;

            float targetSpeed = _throttleInput >= 0f
                ? _tuning.MaxSpeed * _throttleInput
                : _tuning.MaxSpeed * _tuning.ReverseFactor * _throttleInput;

            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _tuning.Acceleration * dt);

            _smoothedSteering = Mathf.Lerp(_smoothedSteering, _steeringInput, _tuning.SteeringResponseLerp * dt);

            Vector3 pos = _transform.position;
            pos += _transform.forward * (_currentSpeed * dt);
            pos.y = _seaSurfaceY;
            _transform.position = pos;

            _transform.Rotate(Vector3.up, _tuning.YawRateDeg * _smoothedSteering * dt);

            if (pos.sqrMagnitude > DriftWarnDistanceSq)
            {
                Log.Warn(LogCat.Gameplay, "ShipController: position drifted beyond +-10 km from origin");
            }
        }
    }
}
