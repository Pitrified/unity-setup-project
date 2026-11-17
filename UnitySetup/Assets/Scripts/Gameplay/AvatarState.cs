namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>Snapshot of the avatar's world position and heading at a point in time.</summary>
    public readonly struct AvatarState
    {
        /// <summary>World-space position.</summary>
        public readonly Vector3 Position;

        /// <summary>Yaw angle in degrees (world Y rotation).</summary>
        public readonly float YawDeg;

        /// <summary>Initializes a new <see cref="AvatarState"/>.</summary>
        public AvatarState(Vector3 position, float yawDeg)
        {
            Position = position;
            YawDeg = yawDeg;
        }
    }
}
