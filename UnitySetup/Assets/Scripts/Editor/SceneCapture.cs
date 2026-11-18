namespace Game.Editor
{
    using System.Globalization;
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Renders a scene through its camera to a PNG, for checking visual changes without a screen
    /// (docs/headless-unity.md). The camera can be moved for the shot; the scene is not saved.
    /// Batch mode, under xvfb-run on Linux, without -nographics:
    /// <c>-executeMethod Game.Editor.SceneCapture.CaptureFromCommandLine -captureOut shot.png
    /// [-captureScene Assets/Scenes/Game.unity] [-captureEye x,y,z -captureTarget x,y,z]
    /// [-captureFov 50] [-captureSize 960x540] [-captureGreyCard x,y,z]</c>.
    /// A grey card is a temporary 50% grey square at a point, facing the camera, lit like the scene:
    /// the neutral patch for measuring a palette when the scene has nothing white or grey.
    /// </summary>
    public static class SceneCapture
    {
        private const string DefaultScene = "Assets/Scenes/Game.unity";
        private const int DefaultWidth = 960;
        private const int DefaultHeight = 540;
        private const float GreyCardSize = 0.12f;
        private const float GreyCardValue = 0.5f;

        /// <summary>Entry point for -executeMethod; exits with 0 when the PNG is written.</summary>
        public static void CaptureFromCommandLine()
        {
            string output = CommandLine.Value("-captureOut");
            if (string.IsNullOrEmpty(output))
            {
                Debug.LogError("SceneCapture: -captureOut <file.png> is required.");
                EditorApplication.Exit(2);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(CommandLine.Value("-captureScene") ?? DefaultScene, OpenSceneMode.Single);
            Camera camera = FindCamera(scene);
            if (camera == null)
            {
                Debug.LogError($"SceneCapture: no camera in {scene.path}.");
                EditorApplication.Exit(1);
                return;
            }

            string eye = CommandLine.Value("-captureEye");
            if (eye != null)
            {
                camera.transform.position = ParseVector(eye);
                camera.transform.LookAt(ParseVector(CommandLine.Value("-captureTarget") ?? "0,0,0"));
            }
            string fov = CommandLine.Value("-captureFov");
            if (fov != null)
            {
                camera.fieldOfView = float.Parse(fov, CultureInfo.InvariantCulture);
            }

            int width = DefaultWidth;
            int height = DefaultHeight;
            string size = CommandLine.Value("-captureSize");
            if (size != null)
            {
                string[] parts = size.Split('x');
                width = int.Parse(parts[0], CultureInfo.InvariantCulture);
                height = int.Parse(parts[1], CultureInfo.InvariantCulture);
            }

            GameObject card = null;
            string cardAt = CommandLine.Value("-captureGreyCard");
            if (cardAt != null)
            {
                card = MakeGreyCard(ParseVector(cardAt), camera.transform.position);
            }

            Render(camera, width, height, output);
            if (card != null)
            {
                Object.DestroyImmediate(card.GetComponent<MeshRenderer>().sharedMaterial);
                Object.DestroyImmediate(card);
            }
            Debug.Log($"SceneCapture: {scene.path} through '{camera.name}' -> {output}");
            EditorApplication.Exit(0);
        }

        /// <summary>
        /// Renders <paramref name="camera"/> into a PNG. Renders twice: the first frame after a
        /// scene opens has no realtime light (docs/headless-unity.md §5).
        /// </summary>
        public static void Render(Camera camera, int width, int height, string output)
        {
            var target = new RenderTexture(width, height, 24);
            RenderTexture previous = camera.targetTexture;
            camera.targetTexture = target;
            camera.Render();
            camera.Render();

            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            camera.targetTexture = previous;

            string folder = Path.GetDirectoryName(Path.GetFullPath(output));
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(output, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            target.Release();
        }

        private static GameObject MakeGreyCard(Vector3 at, Vector3 eye)
        {
            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Quad);
            card.name = "GreyCard";
            Object.DestroyImmediate(card.GetComponent<Collider>());
            card.transform.position = at;
            card.transform.localScale = Vector3.one * GreyCardSize;
            // A quad faces -Z; look away from the eye so its front faces the camera.
            card.transform.rotation = Quaternion.LookRotation(at - eye);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", new Color(GreyCardValue, GreyCardValue, GreyCardValue));
            material.SetFloat("_Smoothness", 0f);
            card.GetComponent<MeshRenderer>().sharedMaterial = material;
            return card;
        }

        private static Camera FindCamera(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Camera camera = root.GetComponentInChildren<Camera>(true);
                if (camera != null)
                {
                    return camera;
                }
            }
            return null;
        }

        private static Vector3 ParseVector(string text)
        {
            string[] parts = text.Split(',');
            return new Vector3(
                float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture));
        }
    }
}
