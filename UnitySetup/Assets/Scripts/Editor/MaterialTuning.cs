namespace Game.Editor
{
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Sets a URP Lit material's base colour, metallic and smoothness without the Editor, for the
    /// palette passes of docs/building_assets.md. Colours are sRGB, 0 to 1, as the Inspector shows them.
    /// Batch mode: <c>-executeMethod Game.Editor.MaterialTuning.SetFromCommandLine
    /// -material Assets/Art/M_Wood.mat [-baseColor r,g,b] [-metallic m] [-smoothness s]</c>.
    /// </summary>
    public static class MaterialTuning
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int Metallic = Shader.PropertyToID("_Metallic");
        private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");

        /// <summary>Entry point for -executeMethod; exits with 0 when the material is saved.</summary>
        public static void SetFromCommandLine()
        {
            string path = CommandLine.Value("-material");
            var material = path == null ? null : AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Debug.LogError($"MaterialTuning: no material at '{path}' (-material Assets/...mat).");
                EditorApplication.Exit(2);
                return;
            }

            string color = CommandLine.Value("-baseColor");
            if (color != null)
            {
                string[] c = color.Split(',');
                material.SetColor(BaseColor, new Color(Parse(c[0]), Parse(c[1]), Parse(c[2])));
            }
            string metallic = CommandLine.Value("-metallic");
            if (metallic != null)
            {
                material.SetFloat(Metallic, Parse(metallic));
            }
            string smoothness = CommandLine.Value("-smoothness");
            if (smoothness != null)
            {
                material.SetFloat(Smoothness, Parse(smoothness));
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            Color now = material.GetColor(BaseColor);
            Debug.Log($"MaterialTuning: {path} base {now.r:F3},{now.g:F3},{now.b:F3} "
                + $"metallic {material.GetFloat(Metallic):F2} smoothness {material.GetFloat(Smoothness):F2}");
            EditorApplication.Exit(0);
        }

        private static float Parse(string text) => float.Parse(text, CultureInfo.InvariantCulture);
    }
}
