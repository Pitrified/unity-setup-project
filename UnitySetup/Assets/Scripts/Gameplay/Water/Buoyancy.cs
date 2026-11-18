namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// Floats a rigidbody on a <see cref="WaterSurface"/>. Each sample point is the centre of an
    /// equal share of the body's volume, a slice <see cref="_columnHeight"/> tall: it is half wet
    /// when the water surface (from <see cref="WaveField"/>, the same waves the shader draws) is at
    /// the point, and pushes up by the weight of the water its wet part displaces. Spread the
    /// points through the whole body, top and bottom: points only below the centre of mass make a
    /// body roll over, and once they are on top it has no lift and sinks. Mass comes from the
    /// density, so a body lighter than water floats at the matching draft and a denser one sinks.
    /// Drag rises with how much of the body is wet. See docs/systems/water.md.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Buoyancy : MonoBehaviour
    {
        private const float WaterDensity = 1000f; // kg per cubic metre
        private const float Gravity = 9.81f;

        [SerializeField] private WaterSurface _water;

        [Tooltip("Body density relative to water: under 1 floats, over 1 sinks.")]
        [SerializeField] private float _relativeDensity = 0.5f;

        [Tooltip("Volume of the body, cubic metres.")]
        [SerializeField] private float _volume = 0.1f;

        [Tooltip("Centres of equal slices of the volume, local space, spread through the whole body.")]
        [SerializeField] private Vector3[] _points = { new Vector3(0f, 0f, 0f) };

        [Tooltip("Height of each slice, metres: wet from half its height below the point to half above.")]
        [SerializeField] private float _columnHeight = 0.5f;

        [SerializeField] private float _airDrag = 0.05f;
        [SerializeField] private float _waterDrag = 1.2f;
        [SerializeField] private float _airAngularDrag = 0.05f;
        [SerializeField] private float _waterAngularDrag = 1.0f;

        private Rigidbody _body;
        private float _wetFraction;

        /// <summary>Fraction of the sample points under the surface at the last physics step, 0 to 1.</summary>
        public float WetFraction => _wetFraction;

        /// <summary>Density relative to water: under 1 floats.</summary>
        public float RelativeDensity => _relativeDensity;

        /// <summary>The water this body floats on.</summary>
        public WaterSurface Water => _water;

        /// <summary>Wires the water (for bodies spawned at runtime) and sets the mass from the density.</summary>
        public void Initialise(WaterSurface water)
        {
            _water = water;
            ApplyMass();
        }

        /// <summary>Sets shape and density in code (bodies spawned at runtime, tests).</summary>
        public void Configure(float relativeDensity, float volume, Vector3[] points, float columnHeight)
        {
            _relativeDensity = relativeDensity;
            _volume = volume;
            _points = points;
            _columnHeight = columnHeight;
            ApplyMass();
        }

        private void Awake()
        {
            ApplyMass();
        }

        private void ApplyMass()
        {
            _body = GetComponent<Rigidbody>();
            _body.mass = Mathf.Max(_relativeDensity * WaterDensity * _volume, 0.01f);
        }

        private void FixedUpdate()
        {
            if (_water == null || _points.Length == 0)
            {
                return;
            }

            WaveField waves = _water.Waves;
            float now = _water.Now;
            float share = WaterDensity * Gravity * _volume / _points.Length;
            int wet = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                Vector3 p = transform.TransformPoint(_points[i]);
                float submerged = Mathf.Clamp01((waves.HeightAt(p.x, p.z, now) - p.y) / _columnHeight + 0.5f);
                if (submerged <= 0f)
                {
                    continue;
                }
                wet++;
                _body.AddForceAtPosition(Vector3.up * (share * submerged), p, ForceMode.Force);
            }

            _wetFraction = (float)wet / _points.Length;
            _body.linearDamping = Mathf.Lerp(_airDrag, _waterDrag, _wetFraction);
            _body.angularDamping = Mathf.Lerp(_airAngularDrag, _waterAngularDrag, _wetFraction);
        }
    }
}
