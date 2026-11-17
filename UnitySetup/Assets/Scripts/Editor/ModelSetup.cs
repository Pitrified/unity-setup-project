namespace Game.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Sets up a model exported by a builder in Tools/asset_builders: import settings for a
    /// static prop, and each material slot of the FBX mapped to the project material
    /// <c>Assets/Art/M_&lt;slot&gt;.mat</c> (URP Lit, created grey if missing). The mapping is
    /// stored in the model's .meta, so a rebuilt FBX keeps it. See docs/building_assets.md.
    /// Batch mode: <c>-executeMethod Game.Editor.ModelSetup.SetUpFromCommandLine -modelPath Assets/Art/Models/X.fbx</c>.
    /// </summary>
    public static class ModelSetup
    {
        private const string MaterialFolder = "Assets/Art";
        private const string LitShader = "Universal Render Pipeline/Lit";

        /// <summary>Entry point for -executeMethod; reads -modelPath and exits with 0 on success.</summary>
        public static void SetUpFromCommandLine()
        {
            string path = CommandLine.Value("-modelPath");
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("ModelSetup: -modelPath <Assets/...fbx> is required.");
                EditorApplication.Exit(2);
                return;
            }
            bool ok = SetUp(path);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        /// <summary>Applies the import settings and material mapping to the model at <paramref name="path"/>.</summary>
        public static bool SetUp(string path)
        {
            if (AssetImporter.GetAtPath(path) is not ModelImporter importer)
            {
                Debug.LogError($"ModelSetup: no model at {path}.");
                return false;
            }

            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.isReadable = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is not Material source)
                {
                    continue;
                }
                Material target = FindOrCreateMaterial(source.name);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), source.name), target);
                Debug.Log($"ModelSetup: {path} slot '{source.name}' -> {AssetDatabase.GetAssetPath(target)}");
            }

            importer.SaveAndReimport();
            return true;
        }

        private static Material FindOrCreateMaterial(string slot)
        {
            string matPath = $"{MaterialFolder}/M_{slot}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null)
            {
                return existing;
            }
            Directory.CreateDirectory(MaterialFolder);
            var material = new Material(Shader.Find(LitShader)) { name = $"M_{slot}" };
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, matPath);
            Debug.Log($"ModelSetup: created {matPath}; set its colour in Unity (docs/building_assets.md).");
            return material;
        }
    }

    /// <summary>Reads <c>-name value</c> pairs from the editor's command line.</summary>
    internal static class CommandLine
    {
        /// <summary>The value after <paramref name="name"/>, or null when it is absent.</summary>
        public static string Value(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
