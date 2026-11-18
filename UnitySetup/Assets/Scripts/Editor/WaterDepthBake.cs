namespace Game.Editor
{
    using System.Globalization;
    using System.IO;
    using Game.Gameplay.Water;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Bakes the seabed depth under a scene's water into a greyscale map the water shader reads
    /// for its colour, shore foam and edge fade: 0 at the still surface (or above it), 1 at the
    /// depth range or deeper. Rays are cast down against the scene's colliders, so the ground
    /// needs colliders (ModelSetup -colliders); colliders on a Rigidbody (things that float or
    /// move, a kinematic ship included) are not ground and are passed through. Bake again when
    /// the shore changes.
    /// See docs/systems/water.md.
    /// Batch mode: <c>-executeMethod Game.Editor.WaterDepthBake.BakeFromCommandLine
    /// [-scene Assets/Scenes/Game.unity] [-size 512] [-out Assets/Art/Textures/T_WaterDepth.png]</c>.
    /// </summary>
    public static class WaterDepthBake
    {
        private const string DefaultScene = "Assets/Scenes/Game.unity";
        private const string DefaultOut = "Assets/Art/Textures/T_WaterDepth.png";
        private const int DefaultSize = 512;
        private const float RayStart = 100f;
        private const float RayLength = 200f;

        /// <summary>Entry point for -executeMethod; exits with 0 when the map is written and assigned.</summary>
        public static void BakeFromCommandLine()
        {
            string scenePath = CommandLine.Value("-scene") ?? DefaultScene;
            string output = CommandLine.Value("-out") ?? DefaultOut;
            string sizeArg = CommandLine.Value("-size");
            int size = sizeArg == null ? DefaultSize : int.Parse(sizeArg, CultureInfo.InvariantCulture);

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            WaterSurface water = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                water = root.GetComponentInChildren<WaterSurface>(true);
                if (water != null)
                {
                    break;
                }
            }
            if (water == null)
            {
                Debug.LogError($"WaterDepthBake: no WaterSurface in {scenePath}.");
                EditorApplication.Exit(1);
                return;
            }

            Bake(water, size, output);
            water.SetDepthMap(AssetDatabase.LoadAssetAtPath<Texture2D>(output), water.DepthRect);
            EditorUtility.SetDirty(water);
            bool saved = EditorSceneManager.SaveScene(scene);
            EditorApplication.Exit(saved ? 0 : 3);
        }

        /// <summary>The highest hit below <paramref name="origin"/> on a collider with no Rigidbody.</summary>
        private static bool GroundBelow(Vector3 origin, out float groundY)
        {
            groundY = float.NegativeInfinity;
            foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, RayLength,
                         Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.attachedRigidbody == null && hit.point.y > groundY)
                {
                    groundY = hit.point.y;
                }
            }
            return !float.IsNegativeInfinity(groundY);
        }

        /// <summary>Casts a ray per texel over the water's depth rect and writes the map to <paramref name="output"/>.</summary>
        public static void Bake(WaterSurface water, int size, string output)
        {
            Physics.SyncTransforms();
            Vector4 rect = water.DepthRect;
            float seaLevel = water.SeaLevel;
            var image = new Texture2D(size, size, TextureFormat.RGB24, false, true);
            var pixels = new Color32[size * size];
            int hits = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float wx = rect.x + (x + 0.5f) / size * rect.z;
                    float wz = rect.y + (y + 0.5f) / size * rect.z;
                    float depth01 = 1f;
                    if (GroundBelow(new Vector3(wx, RayStart, wz), out float groundY))
                    {
                        depth01 = Mathf.Clamp01((seaLevel - groundY) / rect.w);
                        hits++;
                    }
                    byte v = (byte)Mathf.RoundToInt(depth01 * 255f);
                    pixels[y * size + x] = new Color32(v, v, v, 255);
                }
            }
            image.SetPixels32(pixels);
            image.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, image.EncodeToPNG());
            Object.DestroyImmediate(image);

            AssetDatabase.ImportAsset(output);
            var importer = (TextureImporter)AssetImporter.GetAtPath(output);
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;
            importer.SaveAndReimport();
            Debug.Log($"WaterDepthBake: {size}x{size} over {rect.z} m, {hits} of {size * size} rays hit ground -> {output}");
        }
    }
}
