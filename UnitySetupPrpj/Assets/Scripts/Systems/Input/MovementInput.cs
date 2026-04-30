namespace Game.Systems
{
    /// <summary>Normalized movement intent for a single frame. Zero-allocation value type.</summary>
    public readonly struct MovementInput
    {
        /// <summary>Forward/backward axis normalized to [-1, +1]. Positive is forward.</summary>
        public readonly float Throttle;

        /// <summary>Left/right axis normalized to [-1, +1]. Positive is right.</summary>
        public readonly float Steering;

        /// <summary>Initializes a new <see cref="MovementInput"/>.</summary>
        public MovementInput(float throttle, float steering)
        {
            Throttle = throttle;
            Steering = steering;
        }
    }
}
