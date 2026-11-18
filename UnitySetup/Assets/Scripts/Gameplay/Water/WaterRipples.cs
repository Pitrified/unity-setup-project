namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// Ripples on the water over a fixed square: a height field in two render textures, stepped a
    /// fixed number of times per second through the wave equation by blitting one into the other
    /// (WaterRippleSim.shader). <see cref="RippleSource"/>s add impulses; the water shader samples
    /// the field for normal bending and foam. Ripples are visual only: <see cref="WaveField"/>
    /// does not see them. Each step runs when it is blitted, so tests can step and read it at once.
    /// See docs/systems/water.md.
    /// </summary>
    public sealed class WaterRipples : MonoBehaviour
    {
        private const int MaxImpulses = 16;
        private const int MaxStepsPerFrame = 4;

        private static readonly int ImpulsesId = Shader.PropertyToID("_Impulses");
        private static readonly int ImpulseCountId = Shader.PropertyToID("_ImpulseCount");
        private static readonly int DampingId = Shader.PropertyToID("_Damping");
        private static readonly int RipplesId = Shader.PropertyToID("_WaterRipples");
        private static readonly int RippleRectId = Shader.PropertyToID("_WaterRippleRect");

        [SerializeField] private Material _simulation;

        [Tooltip("Centre of the rippling square, world X and Z.")]
        [SerializeField] private Vector2 _centre = new Vector2(2f, 40f);

        [Tooltip("Side of the rippling square, metres.")]
        [SerializeField] private float _size = 48f;

        [SerializeField] private int _resolution = 256;

        [Tooltip("Simulation steps per second; with the texel size this sets how fast ripples travel.")]
        [SerializeField] private float _stepsPerSecond = 30f;

        [Tooltip("Height kept per step, 0 to 1: lower dies out sooner (0.97 halves a ring in about 0.75 s at 30 steps/s).")]
        [SerializeField] private float _damping = 0.97f;

        private readonly Vector4[] _impulses = new Vector4[MaxImpulses];
        private int _impulseCount;
        private RenderTexture _current;
        private RenderTexture _next;
        private float _stepDebt;

        /// <summary>The ripple height field as it is now (R height, G the step before).</summary>
        public RenderTexture Field => _current;

        /// <summary>The world square the ripples cover: x min, z min, size, and one texel in uv.</summary>
        public Vector4 Rect => new Vector4(_centre.x - _size * 0.5f, _centre.y - _size * 0.5f, _size, 1f / _resolution);

        /// <summary>
        /// Adds a bump of <paramref name="strength"/> (metres of height, roughly; negative for a crater)
        /// and <paramref name="radius"/> metres at a world position, at the next step. Positions outside
        /// the square, and impulses beyond <see cref="MaxImpulses"/> in one step, are dropped.
        /// </summary>
        public void AddImpulse(Vector3 position, float radius, float strength)
        {
            Vector4 rect = Rect;
            float u = (position.x - rect.x) / rect.z;
            float v = (position.z - rect.y) / rect.z;
            if (_impulseCount >= MaxImpulses || u < 0f || u > 1f || v < 0f || v > 1f)
            {
                return;
            }
            _impulses[_impulseCount++] = new Vector4(u, v, radius / rect.z, strength);
        }

        /// <summary>Configures the area and steps in code (tests).</summary>
        internal void Configure(Material simulation, Vector2 centre, float size, int resolution, float stepsPerSecond)
        {
            _simulation = simulation;
            _centre = centre;
            _size = size;
            _resolution = resolution;
            _stepsPerSecond = stepsPerSecond;
            Release();
            Create();
        }

        /// <summary>Runs <paramref name="steps"/> simulation steps now; impulses go into the first.</summary>
        public void Step(int steps)
        {
            if (_current == null)
            {
                return;
            }
            _simulation.SetVectorArray(ImpulsesId, _impulses);
            _simulation.SetFloat(DampingId, _damping);
            for (int i = 0; i < steps; i++)
            {
                _simulation.SetFloat(ImpulseCountId, i == 0 ? _impulseCount : 0);
                Graphics.Blit(_current, _next, _simulation);
                (_current, _next) = (_next, _current);
            }
            _impulseCount = 0;
            Shader.SetGlobalTexture(RipplesId, _current);
        }

        private void OnEnable()
        {
            Create();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Update()
        {
            if (_current == null)
            {
                return;
            }
            _stepDebt += Time.deltaTime * _stepsPerSecond;
            int steps = Mathf.FloorToInt(_stepDebt);
            if (steps <= 0)
            {
                return;
            }
            _stepDebt -= steps;
            Step(Mathf.Min(steps, MaxStepsPerFrame));
        }

        private void Create()
        {
            if (_simulation == null || _current != null)
            {
                return;
            }
            _current = MakeTexture("WaterRipplesA");
            _next = MakeTexture("WaterRipplesB");
            Shader.SetGlobalTexture(RipplesId, _current);
            Shader.SetGlobalVector(RippleRectId, Rect);
        }

        private RenderTexture MakeTexture(string textureName)
        {
            var texture = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.RGHalf)
            {
                name = textureName,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.Create();
            // Start flat: a new render texture's contents are undefined.
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, Color.clear);
            RenderTexture.active = previous;
            return texture;
        }

        private void Release()
        {
            Shader.SetGlobalVector(RippleRectId, Vector4.zero);
            ReleaseTexture(_current);
            ReleaseTexture(_next);
            _current = null;
            _next = null;
        }

        private static void ReleaseTexture(RenderTexture texture)
        {
            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
            }
        }
    }
}
