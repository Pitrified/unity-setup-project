namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// Makes ripples where its object meets the water: a splash ring when it enters the water fast
    /// from above, small rings while it bobs or moves in the water. Speed comes from the change in
    /// position, so it works on rigidbodies and on kinematic movers alike. Its object counts as in
    /// the water when the surface is within <see cref="_reach"/> of its pivot. See docs/systems/water.md.
    /// </summary>
    public sealed class RippleSource : MonoBehaviour
    {
        [SerializeField] private WaterSurface _water;
        [SerializeField] private WaterRipples _ripples;

        [Tooltip("Metres from the pivot within which the surface counts as touching the object.")]
        [SerializeField] private float _reach = 0.5f;

        [Tooltip("Radius of the rings it makes, metres.")]
        [SerializeField] private float _ringRadius = 0.6f;

        [Tooltip("Seconds between rings while it moves in the water.")]
        [SerializeField] private float _interval = 0.25f;

        [Tooltip("Speed in m/s (horizontal plus vertical) under which it makes no rings.")]
        [SerializeField] private float _minSpeed = 0.25f;

        [Tooltip("Ring height per m/s of movement.")]
        [SerializeField] private float _strength = 0.02f;

        [Tooltip("Downward speed in m/s above which entering the water splashes.")]
        [SerializeField] private float _splashSpeed = 2f;

        [Tooltip("Splash depth per m/s of downward speed.")]
        [SerializeField] private float _splashStrength = 0.012f;

        private Vector3 _last;
        private bool _hasLast;
        private bool _wasIn;
        private float _timer;

        /// <summary>Wires the water and ripples (sources made at runtime).</summary>
        public void Initialise(WaterSurface water, WaterRipples ripples, float reach, float ringRadius)
        {
            _water = water;
            _ripples = ripples;
            _reach = reach;
            _ringRadius = ringRadius;
            _last = transform.position;
            _hasLast = true;
            _wasIn = false;
        }

        private void OnEnable()
        {
            // The first position is taken in the first Update: something placed in Start (a hull
            // put on the water) is not a movement.
            _hasLast = false;
            _wasIn = false;
            _timer = 0f;
        }

        private void Update()
        {
            if (_water == null || _ripples == null || Time.deltaTime <= 0f)
            {
                return;
            }
            Vector3 position = transform.position;
            if (!_hasLast)
            {
                _last = position;
                _hasLast = true;
                return;
            }
            Vector3 velocity = (position - _last) / Time.deltaTime;
            _last = position;

            float surface = _water.HeightAt(position);
            bool isIn = Mathf.Abs(position.y - surface) < _reach;
            if (isIn && !_wasIn && velocity.y < -_splashSpeed)
            {
                // A crater; the wave equation turns it into a ring.
                _ripples.AddImpulse(position, _ringRadius * 1.5f, velocity.y * _splashStrength);
            }
            else if (isIn)
            {
                _timer -= Time.deltaTime;
                float speed = velocity.magnitude;
                if (speed > _minSpeed && _timer <= 0f)
                {
                    _timer = _interval;
                    _ripples.AddImpulse(position, _ringRadius, -speed * _strength);
                }
            }
            _wasIn = isIn;
        }
    }
}
