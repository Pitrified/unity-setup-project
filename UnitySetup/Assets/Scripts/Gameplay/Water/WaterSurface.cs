namespace Game.Gameplay.Water
{
    using UnityEngine;
    using Game.Systems;

    /// <summary>
    /// One body of water: owns the <see cref="WaveField"/> and its clock, and hands the shader
    /// the same waves, the same time and the baked seabed depth as globals every frame, so what
    /// floats and what is drawn agree. Things that float hold a reference to it.
    /// Runs in the Editor too, so a scene capture sees the waves. See docs/systems/water.md.
    /// </summary>
    [ExecuteAlways]
    public sealed class WaterSurface : MonoBehaviour
    {
        private static readonly int WaveA = Shader.PropertyToID("_WaterWaveA");
        private static readonly int WaveB = Shader.PropertyToID("_WaterWaveB");
        private static readonly int WaveC = Shader.PropertyToID("_WaterWaveC");
        private static readonly int WaveD = Shader.PropertyToID("_WaterWaveD");
        private static readonly int WaterTime = Shader.PropertyToID("_WaterTime");
        private static readonly int SeaLevelId = Shader.PropertyToID("_WaterSeaLevel");
        private static readonly int Shallow = Shader.PropertyToID("_WaterShallow");
        private static readonly int Deep = Shader.PropertyToID("_WaterDeep");
        private static readonly int Sky = Shader.PropertyToID("_WaterSky");
        private static readonly int Foam = Shader.PropertyToID("_WaterFoam");
        private static readonly int Detail = Shader.PropertyToID("_WaterDetail");
        private static readonly int FoamParams = Shader.PropertyToID("_WaterFoamParams");
        private static readonly int DepthMap = Shader.PropertyToID("_WaterDepthMap");
        private static readonly int DepthRectId = Shader.PropertyToID("_WaterDepthRect");

        [SerializeField] private SO_WaterSettings _settings;

        [Tooltip("Seabed depth under the sea level, baked by Game.Editor.WaterDepthBake: 0 at the surface, 1 at Depth Range.")]
        [SerializeField] private Texture2D _depthMap;

        [Tooltip("World area the depth map covers: x min, z min, size (m), depth range (m).")]
        [SerializeField] private Vector4 _depthRect = new Vector4(-90f, -90f, 180f, 8f);

        private WaveField _field;
        private bool _pinned;
        private float _pinnedTime;

        /// <summary>The waves, for anything that needs the surface on the CPU.</summary>
        public WaveField Waves => _field ??= new WaveField(_settings);

        /// <summary>The water clock in seconds: the level's time, unless a capture pinned it.</summary>
        public float Now => _pinned ? _pinnedTime : Time.timeSinceLevelLoad;

        /// <summary>Height of the still surface.</summary>
        public float SeaLevel => _settings.SeaLevel;

        /// <summary>The settings this water draws and moves with.</summary>
        public SO_WaterSettings Settings => _settings;

        /// <summary>The world area and depth range the seabed depth map covers.</summary>
        public Vector4 DepthRect => _depthRect;

        /// <summary>The surface height above a world position, now.</summary>
        public float HeightAt(Vector3 position) => Waves.HeightAt(position.x, position.z, Now);

        /// <summary>Freezes the water clock at <paramref name="time"/>, for repeatable captures and tests.</summary>
        public void PinClock(float time)
        {
            _pinned = true;
            _pinnedTime = time;
            Push();
        }

        /// <summary>Lets the clock run with the level's time again.</summary>
        public void ReleaseClock()
        {
            _pinned = false;
            Push();
        }

        /// <summary>Uses <paramref name="settings"/> (water built in code: tests).</summary>
        internal void UseSettings(SO_WaterSettings settings)
        {
            _settings = settings;
            _field = new WaveField(settings);
            Push();
        }

        /// <summary>Points the depth map at a baked texture (the bake tool calls this).</summary>
        public void SetDepthMap(Texture2D map, Vector4 rect)
        {
            _depthMap = map;
            _depthRect = rect;
            Push();
        }

        private void OnEnable()
        {
            if (_settings == null)
            {
                Log.Warn(LogCat.Gameplay, "WaterSurface: no settings assigned");
                return;
            }
            _field = new WaveField(_settings);
            Push();
        }

        private void OnValidate()
        {
            if (_settings != null)
            {
                _field = new WaveField(_settings);
                Push();
            }
        }

        private void Update()
        {
            if (_settings != null)
            {
                Push();
            }
        }

        private void Push()
        {
            if (_settings == null)
            {
                return;
            }
            WaveField field = Waves;
            Shader.SetGlobalVector(WaveA, field.PackedWave(0));
            Shader.SetGlobalVector(WaveB, field.PackedWave(1));
            Shader.SetGlobalVector(WaveC, field.PackedWave(2));
            Shader.SetGlobalVector(WaveD, field.PackedWave(3));
            Shader.SetGlobalFloat(WaterTime, Now);
            Shader.SetGlobalFloat(SeaLevelId, _settings.SeaLevel);
            Shader.SetGlobalColor(Shallow, _settings.Shallow);
            Shader.SetGlobalColor(Deep, _settings.Deep);
            Shader.SetGlobalColor(Sky, _settings.Sky);
            Shader.SetGlobalColor(Foam, _settings.Foam);
            Shader.SetGlobalVector(Detail, new Vector4(
                _settings.NormalTiling, _settings.NormalStrength, _settings.NormalScroll, _settings.DeepAt));
            Shader.SetGlobalVector(FoamParams, new Vector4(
                _settings.ShoreFoamDepth, _settings.ShoreBandSpeed, _settings.CrestFoam, _settings.CalmDepth));
            if (_depthMap != null)
            {
                Shader.SetGlobalTexture(DepthMap, _depthMap);
            }
            Shader.SetGlobalVector(DepthRectId, _depthRect);
        }
    }
}
