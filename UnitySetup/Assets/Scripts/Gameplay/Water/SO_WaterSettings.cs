namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>One Gerstner wave: a travelling crest with a direction, a length and a steepness.</summary>
    [System.Serializable]
    public struct GerstnerWave
    {
        /// <summary>Travel direction in degrees, 0 along +X, 90 along +Z.</summary>
        public float directionDeg;

        /// <summary>Crest-to-crest distance in metres.</summary>
        public float wavelength;

        /// <summary>0 flat to 1 pointed crests; the sum over all waves must stay at or under 1.</summary>
        [Range(0f, 1f)] public float steepness;
    }

    /// <summary>
    /// Everything the water looks and moves like: the waves (read by both the shader and
    /// <see cref="WaveField"/>), the colours and the foam. One asset per body of water.
    /// See docs/systems/water.md.
    /// </summary>
    [CreateAssetMenu(menuName = "unity-setup/Water Settings", fileName = "SO_WaterSettings")]
    public sealed class SO_WaterSettings : ScriptableObject
    {
        [SerializeField] private float _seaLevel = -0.35f;
        [SerializeField] private GerstnerWave[] _waves =
        {
            new GerstnerWave { directionDeg = 20f, wavelength = 12f, steepness = 0.10f },
            new GerstnerWave { directionDeg = 60f, wavelength = 7.5f, steepness = 0.09f },
            new GerstnerWave { directionDeg = -15f, wavelength = 4.5f, steepness = 0.08f },
            new GerstnerWave { directionDeg = 100f, wavelength = 3.4f, steepness = 0.06f },
        };

        [Header("Colour")]
        [SerializeField] private Color _shallow = new Color(0.20f, 0.62f, 0.62f);
        [SerializeField] private Color _deep = new Color(0.03f, 0.20f, 0.33f);
        [SerializeField] private Color _sky = new Color(0.62f, 0.76f, 0.88f);
        [SerializeField] private Color _foam = new Color(0.92f, 0.95f, 0.96f);
        [SerializeField] private float _deepAt = 4f;

        [Header("Surface detail")]
        [SerializeField] private float _normalTiling = 0.12f;
        [SerializeField] private float _normalStrength = 0.2f;
        [SerializeField] private float _normalScroll = 0.03f;

        [Header("Foam")]
        [SerializeField] private float _shoreFoamDepth = 0.9f;
        [SerializeField] private float _shoreBandSpeed = 0.35f;
        [SerializeField] private float _crestFoam = 0.4f;
        [SerializeField] private float _calmDepth = 2f;

        /// <summary>Height of the still water surface, in metres.</summary>
        public float SeaLevel => _seaLevel;

        /// <summary>The waves; at most <see cref="WaveField.MaxWaves"/> are used.</summary>
        public GerstnerWave[] Waves => _waves;

        /// <summary>Colour over sand just under the surface.</summary>
        public Color Shallow => _shallow;

        /// <summary>Colour once the water is <see cref="DeepAt"/> deep or more.</summary>
        public Color Deep => _deep;

        /// <summary>The sky colour the surface reflects at grazing angles.</summary>
        public Color Sky => _sky;

        /// <summary>Foam colour, at the shore, on crests and in ripples.</summary>
        public Color Foam => _foam;

        /// <summary>Depth in metres at which the water reaches its deep colour.</summary>
        public float DeepAt => _deepAt;

        /// <summary>Normal map repeats per metre.</summary>
        public float NormalTiling => _normalTiling;

        /// <summary>How much the normal map bends the surface normal, 0 to 1.</summary>
        public float NormalStrength => _normalStrength;

        /// <summary>Normal map scroll in tiles per second.</summary>
        public float NormalScroll => _normalScroll;

        /// <summary>Depth in metres under which the shore foam appears.</summary>
        public float ShoreFoamDepth => _shoreFoamDepth;

        /// <summary>How fast the shore foam bands run up the beach, in bands per second.</summary>
        public float ShoreBandSpeed => _shoreBandSpeed;

        /// <summary>How much foam the wave crests get, 0 none to 1 a lot.</summary>
        public float CrestFoam => _crestFoam;

        /// <summary>
        /// Depth in metres under which the drawn waves flatten toward the shore. The CPU field does not
        /// flatten, so floaters in very shallow water ride slightly higher than the drawn surface.
        /// </summary>
        public float CalmDepth => _calmDepth;
    }
}
