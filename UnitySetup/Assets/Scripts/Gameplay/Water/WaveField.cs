namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// The water's Gerstner waves on the CPU: the same sum the water shader displaces its
    /// vertices with (Water.shader, <c>GerstnerSum</c>), so floating things sit on the surface
    /// that is drawn. Pure math, no allocations after construction. See docs/systems/water.md.
    /// </summary>
    public sealed class WaveField
    {
        /// <summary>Waves the shader evaluates; settings beyond this are ignored.</summary>
        public const int MaxWaves = 4;

        /// <summary>Iterations that undo the horizontal displacement when looking up a height.</summary>
        public const int HeightIterations = 3;

        private const float Gravity = 9.81f;

        private readonly Vector4[] _waves = new Vector4[MaxWaves];
        private readonly float[] _speeds = new float[MaxWaves];
        private int _count;

        /// <summary>Height of the still surface, in metres.</summary>
        public float SeaLevel { get; private set; }

        /// <summary>Builds the field from a settings asset.</summary>
        public WaveField(SO_WaterSettings settings)
        {
            Configure(settings.SeaLevel, settings.Waves);
        }

        /// <summary>Builds the field from a sea level and waves (tests, tools).</summary>
        public WaveField(float seaLevel, GerstnerWave[] waves)
        {
            Configure(seaLevel, waves);
        }

        /// <summary>
        /// Per wave, what the shader receives: (direction x, direction z, steepness, wavelength).
        /// </summary>
        public Vector4 PackedWave(int index) => index < _count ? _waves[index] : Vector4.zero;

        /// <summary>Re-reads the waves, e.g. after the settings asset changed.</summary>
        public void Configure(float seaLevel, GerstnerWave[] waves)
        {
            SeaLevel = seaLevel;
            _count = Mathf.Min(waves.Length, MaxWaves);
            for (int i = 0; i < MaxWaves; i++)
            {
                if (i >= _count)
                {
                    _waves[i] = Vector4.zero;
                    _speeds[i] = 0f;
                    continue;
                }
                GerstnerWave w = waves[i];
                float rad = w.directionDeg * Mathf.Deg2Rad;
                float wavelength = Mathf.Max(w.wavelength, 0.01f);
                _waves[i] = new Vector4(Mathf.Cos(rad), Mathf.Sin(rad), w.steepness, wavelength);
                _speeds[i] = Mathf.Sqrt(Gravity / (2f * Mathf.PI / wavelength));
            }
        }

        /// <summary>
        /// Where the surface point that starts at (x, sea level, z) is at <paramref name="time"/>:
        /// an offset from that rest position, horizontal and vertical.
        /// </summary>
        public Vector3 Displacement(float x, float z, float time)
        {
            Vector3 d = Vector3.zero;
            for (int i = 0; i < _count; i++)
            {
                Vector4 w = _waves[i];
                float k = 2f * Mathf.PI / w.w;
                float a = w.z / k;
                float f = k * (w.x * x + w.y * z - _speeds[i] * time);
                float cos = Mathf.Cos(f);
                d.x += w.x * a * cos;
                d.y += a * Mathf.Sin(f);
                d.z += w.y * a * cos;
            }
            return d;
        }

        /// <summary>
        /// The surface height above the world point (x, z) at <paramref name="time"/>.
        /// Gerstner waves move points sideways, so the rest position under (x, z) is found by
        /// undoing the horizontal offset a few times; three iterations land within millimetres.
        /// </summary>
        public float HeightAt(float x, float z, float time)
        {
            float px = x;
            float pz = z;
            Vector3 d = Vector3.zero;
            for (int i = 0; i < HeightIterations; i++)
            {
                d = Displacement(px, pz, time);
                px = x - d.x;
                pz = z - d.z;
            }
            d = Displacement(px, pz, time);
            return SeaLevel + d.y;
        }

        /// <summary>The surface normal at the rest position (x, z), as the shader computes it.</summary>
        public Vector3 NormalAtRest(float x, float z, float time)
        {
            Vector3 tangent = new Vector3(1f, 0f, 0f);
            Vector3 binormal = new Vector3(0f, 0f, 1f);
            for (int i = 0; i < _count; i++)
            {
                Vector4 w = _waves[i];
                float k = 2f * Mathf.PI / w.w;
                float f = k * (w.x * x + w.y * z - _speeds[i] * time);
                float s = w.z * Mathf.Sin(f);
                float c = w.z * Mathf.Cos(f);
                tangent += new Vector3(-w.x * w.x * s, w.x * c, -w.x * w.y * s);
                binormal += new Vector3(-w.x * w.y * s, w.y * c, -w.y * w.y * s);
            }
            return Vector3.Cross(binormal, tangent).normalized;
        }
    }
}
