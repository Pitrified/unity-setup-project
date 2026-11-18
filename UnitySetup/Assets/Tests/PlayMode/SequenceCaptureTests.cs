namespace Game.Tests.PlayMode
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Gameplay;

    /// <summary>
    /// Plays the Game scene for a few seconds of fixed-step time and, when asked, writes frames
    /// from a camera pose: the sequence capture of docs/headless-unity.md, for things a still
    /// cannot show (waves, bobbing, ripples). Without the environment variables it is a plain
    /// test that the scene plays through without an error logged.
    /// Environment: CAPTURE_SEQUENCE_DIR (writes frames there), CAPTURE_SECONDS (default 4),
    /// CAPTURE_FPS (frames written per second, default 10), CAPTURE_EYE and CAPTURE_TARGET
    /// ("x,y,z"; default: the game's own chase camera), CAPTURE_SIZE ("960x540").
    /// Run alone with <c>-testFilter Game.Tests.PlayMode.SequenceCaptureTests</c>.
    /// </summary>
    public sealed class SequenceCaptureTests
    {
        private const int WaitFrames = 900;
        private const float SimulationFps = 30f;
        private const float DefaultSeconds = 4f;
        private const float DefaultCaptureFps = 10f;
        private const float WarmUpSeconds = 0.5f;

        [SetUp]
        public void SetUp()
        {
            // Fixed steps: the same frames come out however slowly the machine renders them.
            Time.captureDeltaTime = 1f / SimulationFps;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator GameScene_PlaysThrough_AndCapturesWhenAsked()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);

            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            yield return WaitForState(GameState.Playing);

            string dir = Environment.GetEnvironmentVariable("CAPTURE_SEQUENCE_DIR");
            float seconds = EnvFloat("CAPTURE_SECONDS", DefaultSeconds);
            float captureFps = EnvFloat("CAPTURE_FPS", DefaultCaptureFps);
            int every = Mathf.Max(1, Mathf.RoundToInt(SimulationFps / captureFps));

            Camera camera = SetUpCamera();
            RenderTexture target = null;
            Texture2D image = null;
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
                Vector2Int size = EnvSize("CAPTURE_SIZE", new Vector2Int(960, 540));
                target = new RenderTexture(size.x, size.y, 24);
                image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            }

            int warmUp = Mathf.RoundToInt(WarmUpSeconds * SimulationFps);
            for (int i = 0; i < warmUp; i++)
            {
                yield return null;
            }

            int frames = Mathf.RoundToInt(seconds * SimulationFps);
            int written = 0;
            for (int i = 0; i < frames; i++)
            {
                yield return null; // WaitForEndOfFrame never fires in batch mode; the camera renders on demand below
                if (target != null && i % every == 0)
                {
                    WriteFrame(camera, target, image, Path.Combine(dir, $"frame_{written:000}.png"));
                    written++;
                }
            }

            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState, "The game kept playing");
            if (target != null)
            {
                File.WriteAllText(Path.Combine(dir, "sequence.json"),
                    "{\"frames\": " + written + ", \"fps\": " + (SimulationFps / every).ToString(CultureInfo.InvariantCulture)
                    + ", \"seconds\": " + seconds.ToString(CultureInfo.InvariantCulture) + "}");
                Debug.Log($"SequenceCapture: {written} frames -> {dir}");
                target.Release();
            }
        }

        private static Camera SetUpCamera()
        {
            CameraController chase = UnityEngine.Object.FindAnyObjectByType<CameraController>();
            Camera camera = chase != null ? chase.GetComponent<Camera>() : Camera.main;
            string eye = Environment.GetEnvironmentVariable("CAPTURE_EYE");
            if (eye == null)
            {
                return camera;
            }
            if (chase != null)
            {
                chase.enabled = false; // hold the pose; the chase camera would move it every frame
            }
            camera.transform.position = ParseVector(eye);
            camera.transform.LookAt(ParseVector(Environment.GetEnvironmentVariable("CAPTURE_TARGET") ?? "0,0,0"));
            return camera;
        }

        private static void WriteFrame(Camera camera, RenderTexture target, Texture2D image, string path)
        {
            RenderTexture previous = camera.targetTexture;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            camera.targetTexture = previous;
            File.WriteAllBytes(path, image.EncodeToPNG());
        }

        private static IEnumerator WaitForState(GameState state)
        {
            for (int i = 0; i < WaitFrames; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == state)
                {
                    yield break;
                }
                yield return null;
            }
            Assert.Fail($"Timed out waiting for {state}");
        }

        private static float EnvFloat(string name, float fallback)
        {
            string value = Environment.GetEnvironmentVariable(name);
            return value == null ? fallback : float.Parse(value, CultureInfo.InvariantCulture);
        }

        private static Vector2Int EnvSize(string name, Vector2Int fallback)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (value == null)
            {
                return fallback;
            }
            string[] parts = value.Split('x');
            return new Vector2Int(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        private static Vector3 ParseVector(string text)
        {
            string[] p = text.Split(',');
            return new Vector3(
                float.Parse(p[0], CultureInfo.InvariantCulture),
                float.Parse(p[1], CultureInfo.InvariantCulture),
                float.Parse(p[2], CultureInfo.InvariantCulture));
        }
    }
}
