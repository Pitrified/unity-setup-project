namespace Game.Gameplay.Water
{
    using UnityEngine;

    /// <summary>
    /// Sample only: lobs a cannonball at the water near a target every few seconds, so the water's
    /// reactions (splash, ripples, a struck floater, a sinking ball) can be seen without a player.
    /// Balls come from a small pool made at start; each floats or sinks through its own
    /// <see cref="Buoyancy"/> (iron: it sinks) and splashes through its <see cref="RippleSource"/>.
    /// </summary>
    public sealed class CannonballLauncher : MonoBehaviour
    {
        private const float IronDensity = 7.8f;
        private const float RecycleDepth = 6f;

        [SerializeField] private WaterSurface _water;
        [SerializeField] private WaterRipples _ripples;
        [SerializeField] private Material _ballMaterial;
        [SerializeField] private Vector3 _target = new Vector3(2f, -0.35f, 40f);
        [SerializeField] private float _spread = 5f;
        [SerializeField] private float _interval = 3f;
        [SerializeField] private float _flightTime = 1.6f;
        [SerializeField] private float _ballRadius = 0.12f;
        [SerializeField] private int _poolSize = 4;
        [SerializeField] private int _seed = 3;

        private Rigidbody[] _balls;
        private int _next;
        private float _timer;
        private System.Random _random;

        /// <summary>The balls, for tests.</summary>
        internal Rigidbody[] Balls => _balls;

        /// <summary>Configures and builds the pool in code (tests).</summary>
        internal void Configure(WaterSurface water, WaterRipples ripples, Vector3 target, float interval)
        {
            _water = water;
            _ripples = ripples;
            _target = target;
            _interval = interval;
            BuildPool();
        }

        /// <summary>Fires the next ball now (tests).</summary>
        internal void FireNow() => Fire();

        private void Start()
        {
            if (_balls == null)
            {
                BuildPool();
            }
        }

        private void BuildPool()
        {
            _random = new System.Random(_seed);
            _balls = new Rigidbody[_poolSize];
            float volume = 4f / 3f * Mathf.PI * _ballRadius * _ballRadius * _ballRadius;
            for (int i = 0; i < _poolSize; i++)
            {
                GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.name = $"Cannonball{i}";
                // In the launcher's scene, not the active one, so the balls go when the scene unloads.
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ball, gameObject.scene);
                ball.transform.localScale = Vector3.one * (2f * _ballRadius);
                if (_ballMaterial != null)
                {
                    ball.GetComponent<MeshRenderer>().sharedMaterial = _ballMaterial;
                }
                Rigidbody body = ball.AddComponent<Rigidbody>();
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Buoyancy buoyancy = ball.AddComponent<Buoyancy>();
                buoyancy.Configure(IronDensity, volume, new[] { Vector3.zero }, 2f * _ballRadius);
                buoyancy.Initialise(_water);
                RippleSource source = ball.AddComponent<RippleSource>();
                source.Initialise(_water, _ripples, _ballRadius * 2f, 0.6f);
                ball.SetActive(false);
                _balls[i] = body;
            }
        }

        private void Update()
        {
            if (_balls == null || _water == null)
            {
                return;
            }
            for (int i = 0; i < _balls.Length; i++)
            {
                Rigidbody ball = _balls[i];
                if (ball.gameObject.activeSelf && ball.position.y < _water.SeaLevel - RecycleDepth)
                {
                    ball.gameObject.SetActive(false);
                }
            }
            _timer += Time.deltaTime;
            if (_timer >= _interval)
            {
                _timer = 0f;
                Fire();
            }
        }

        private void Fire()
        {
            Rigidbody ball = _balls[_next];
            _next = (_next + 1) % _balls.Length;

            float angle = (float)_random.NextDouble() * 2f * Mathf.PI;
            float distance = (float)_random.NextDouble() * _spread;
            Vector3 target = _target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            Vector3 start = transform.position;

            // Ballistic: reach the target after the flight time under gravity.
            Vector3 velocity = (target - start - 0.5f * Physics.gravity * _flightTime * _flightTime) / _flightTime;

            // Move first: the ripple source reads its start position when the ball is enabled.
            ball.transform.position = start;
            ball.gameObject.SetActive(true);
            ball.position = start;
            ball.linearVelocity = velocity;
            ball.angularVelocity = Vector3.zero;
        }
    }
}
