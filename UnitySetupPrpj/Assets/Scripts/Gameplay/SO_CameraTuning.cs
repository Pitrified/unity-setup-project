namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>
    /// Designer-tunable data for the third-person chase camera.
    /// Create an instance via Assets > Create > artificial-pi > Camera Tuning
    /// and assign it to the CameraController on MainCamera.
    /// </summary>
    [CreateAssetMenu(menuName = "artificial-pi/Camera Tuning", fileName = "SO_CameraTuning")]
    public sealed class SO_CameraTuning : ScriptableObject
    {
        [SerializeField] private Vector3 _localOffset = new Vector3(0f, 3f, -6f);
        [SerializeField] private float _positionLerp = 5f;
        [SerializeField] private float _rotationLerp = 5f;
        [SerializeField] private bool _lookAtTarget = true;

        /// <summary>Camera offset in the target's local space.</summary>
        public Vector3 LocalOffset => _localOffset;

        /// <summary>Per-second position smoothing coefficient for framerate-independent Lerp.</summary>
        public float PositionLerp => _positionLerp;

        /// <summary>Per-second rotation smoothing coefficient for framerate-independent Slerp.</summary>
        public float RotationLerp => _rotationLerp;

        /// <summary>When true the camera continuously rotates to look at the target.</summary>
        public bool LookAtTarget => _lookAtTarget;
    }
}
