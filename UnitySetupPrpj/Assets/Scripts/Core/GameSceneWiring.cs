namespace Game.Core
{
    using UnityEngine;
    using Game.Systems;
    using Game.Gameplay;

    /// <summary>
    /// Wires Game scene components (ShipController, CameraController) with Persistent scene
    /// systems (InputManager). Place this on any root object in Game.unity and assign
    /// the serialized references in the Inspector.
    /// </summary>
    public sealed class GameSceneWiring : MonoBehaviour
    {
        [SerializeField] private ShipController _ship;
        [SerializeField] private CameraController _camera;

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: GameManager.Instance is null - ship/camera not wired");
                return;
            }

            if (_ship == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: ShipController not assigned");
            }
            else
            {
                _ship.Inject(gm.InputManager);
                gm.RegisterShip(_ship);
                Log.Info(LogCat.Gameplay, "GameSceneWiring: ShipController injected with InputManager");
            }

            if (_camera == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: CameraController not assigned");
            }
            else if (_ship != null)
            {
                _camera.SetTarget(_ship.transform);
                _camera.SnapToTarget();
                Log.Info(LogCat.Gameplay, "GameSceneWiring: CameraController wired to ship");
            }
        }
    }
}
