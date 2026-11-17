namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Designer-tunable data for the avatar kinematic model.
    /// Create an instance via Assets > Create > unity-setup > Avatar Tuning
    /// and assign it to the AvatarController on PF_Avatar.
    /// </summary>
    [CreateAssetMenu(menuName = "unity-setup/Avatar Tuning", fileName = "SO_AvatarTuning")]
    public sealed class SO_AvatarTuning : ScriptableObject
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
