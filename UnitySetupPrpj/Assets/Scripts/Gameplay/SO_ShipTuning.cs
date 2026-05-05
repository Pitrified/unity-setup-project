namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Designer-tunable data for the ship kinematic model.
    /// Create an instance via Assets > Create > artificial-pi > Ship Tuning
    /// and assign it to the ShipController on PF_Ship.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pi/Ship Tuning", fileName = "SO_ShipTuning")]
    public sealed class SO_ShipTuning : ScriptableObject
    {
        [SerializeField] private float _maxSpeed = 8f;
        [SerializeField] private float _acceleration = 4f;
        [SerializeField] private float _reverseFactor = 0.3f;
        [SerializeField] private float _yawRateDeg = 60f;
        [SerializeField] private float _steeringResponseLerp = 6f;

        /// <summary>Maximum forward speed in metres per second.</summary>
        public float MaxSpeed => _maxSpeed;

        /// <summary>Acceleration rate in metres per second squared.</summary>
        public float Acceleration => _acceleration;

        /// <summary>Multiplier on MaxSpeed applied to reverse throttle.</summary>
        public float ReverseFactor => _reverseFactor;

        /// <summary>Yaw rotation rate in degrees per second at full steering input.</summary>
        public float YawRateDeg => _yawRateDeg;

        /// <summary>Per-second Lerp factor for smoothing the raw steering input.</summary>
        public float SteeringResponseLerp => _steeringResponseLerp;
    }
}
