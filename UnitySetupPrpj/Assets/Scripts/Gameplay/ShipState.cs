namespace Game.Gameplay
{
    using UnityEngine;

    /// <summary>Snapshot of the ship's world position and heading at a point in time.</summary>
    public readonly struct ShipState
    {
        /// <summary>World-space position.</summary>
        public readonly Vector3 Position;

        /// <summary>Yaw angle in degrees (world Y rotation).</summary>
        public readonly float YawDeg;

        /// <summary>Initializes a new <see cref="ShipState"/>.</summary>
        public ShipState(Vector3 position, float yawDeg)
        {
            Position = position;
            YawDeg = yawDeg;
        }
    }
}
