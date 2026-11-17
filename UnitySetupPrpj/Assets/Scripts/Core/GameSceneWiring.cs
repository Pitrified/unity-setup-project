namespace Game.Core
{
    using UnityEngine;
    using Game.Systems;
    using Game.Gameplay;

    /// <summary>
    /// Wires Game scene components (AvatarController, CameraController) with Persistent scene
    /// systems (InputManager). Place this on any root object in Game.unity and assign
    /// the serialized references in the Inspector.
    /// </summary>
    public sealed class GameSceneWiring : MonoBehaviour
    {
        [SerializeField] private AvatarController _avatar;
        [SerializeField] private CameraController _camera;

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: GameManager.Instance is null - avatar/camera not wired");
                return;
            }

            if (_avatar == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: AvatarController not assigned");
            }
            else
            {
                _avatar.Inject(gm.InputManager);
                gm.RegisterAvatar(_avatar);
                Log.Info(LogCat.Gameplay, "GameSceneWiring: AvatarController injected with InputManager");
            }

            if (_camera == null)
            {
                Log.Warn(LogCat.Gameplay, "GameSceneWiring: CameraController not assigned");
            }
            else if (_avatar != null)
            {
                _camera.SetTarget(_avatar.transform);
                _camera.SnapToTarget();
                Log.Info(LogCat.Gameplay, "GameSceneWiring: CameraController wired to avatar");
            }
        }
    }
}
