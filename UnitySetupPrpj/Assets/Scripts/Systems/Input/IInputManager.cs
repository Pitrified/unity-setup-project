namespace Game.Systems
{
    /// <summary>
    /// Contract for the input abstraction layer.
    /// Hides device differences (touch vs keyboard) behind a simple struct.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Returns the current normalized movement input for this frame.
        /// Zero-allocation; safe to call every frame.
        /// </summary>
        MovementInput GetMovementInput();

        /// <summary>
        /// Returns <c>true</c> exactly once per Pause press, then resets the flag.
        /// </summary>
        bool ConsumePausePressed();

        /// <summary>
        /// Enables or disables input processing.
        /// Pass <c>false</c> during scene transitions to prevent stale input reaching gameplay.
        /// </summary>
        void SetEnabled(bool enabled);
    }
}
