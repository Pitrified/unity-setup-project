namespace Game.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Checks each model built by Tools/asset_builders as Unity imports it: the size its builder's
    /// parameters give, the pivot at the centre of its base, no rotation or scale on any transform
    /// (the export bakes the axis change and applies the units), and its material slots mapped to
    /// the project materials (docs/building_assets.md, "The Unity side"). A new model adds a case.
    /// </summary>
    public sealed class BuiltModelTests
    {
        private const float SizeTolerance = 0.02f;
        private const float PivotTolerance = 0.005f;

        /// <summary>A built model and what its builder's PARAMS say it should be.</summary>
        public sealed class Expected
        {
            /// <summary>The FBX's asset path.</summary>
            public string Path;

            /// <summary>Bounds size in Unity axes: X width, Y height, Z depth, in metres.</summary>
            public Vector3 Size;

            /// <summary>The project materials its slots map to.</summary>
            public string[] Materials;

            /// <summary>The model's name, for the test runner.</summary>
            public override string ToString() => System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        private static IEnumerable<Expected> Models()
        {
            // barrel.py: 0.72 m at the bilge (hoops and bung stand a little off it), 0.95 m tall.
            yield return new Expected
            {
                Path = "Assets/Art/Models/Barrel.fbx",
                Size = new Vector3(0.72f, 0.95f, 0.72f),
                Materials = new[] { "Assets/Art/M_Wood.mat", "Assets/Art/M_Iron.mat", "Assets/Art/M_Cork.mat" },
            };
            // crate.py: 0.27 x 0.265 m with the lid's overhangs (0.005 m a side, 0.01 m front and back), 0.135 m tall.
            yield return new Expected
            {
                Path = "Assets/Art/Models/Crate.fbx",
                Size = new Vector3(0.28f, 0.135f, 0.285f),
                Materials = new[] { "Assets/Art/M_CrateWood.mat" },
            };
        }

        [TestCaseSource(nameof(Models))]
        public void Size_MatchesBuilderDimensions(Expected model)
        {
            Bounds bounds = ModelBounds(Load(model), out _);
            Assert.AreEqual(model.Size.x, bounds.size.x, SizeTolerance, "Width along X");
            Assert.AreEqual(model.Size.y, bounds.size.y, SizeTolerance, "Height along Unity's Y axis");
            Assert.AreEqual(model.Size.z, bounds.size.z, SizeTolerance, "Depth along Z");
        }

        [TestCaseSource(nameof(Models))]
        public void Pivot_IsAtBaseCentre(Expected model)
        {
            Bounds bounds = ModelBounds(Load(model), out List<Vector3> points);
            Assert.AreEqual(0f, bounds.min.y, PivotTolerance, "The base sits at y 0");

            // The centre of the base, not of the bounds: a feature on one side (a bung) moves the bounds.
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (Vector3 p in points)
            {
                if (p.y < PivotTolerance)
                {
                    sum += p;
                    count++;
                }
            }
            Assert.That(count, Is.GreaterThan(0), "The model has vertices at its base");
            Vector3 centre = sum / count;
            Assert.AreEqual(0f, centre.x, PivotTolerance, "The base is centred on X");
            Assert.AreEqual(0f, centre.z, PivotTolerance, "The base is centred on Z");
        }

        [TestCaseSource(nameof(Models))]
        public void Transforms_AreUnrotatedAtScaleOne(Expected model)
        {
            foreach (Transform t in Load(model).GetComponentsInChildren<Transform>())
            {
                Assert.That(Quaternion.Angle(Quaternion.identity, t.localRotation), Is.LessThan(0.01f),
                    $"'{t.name}' must not carry a rotation");
                Assert.That((t.localScale - Vector3.one).magnitude, Is.LessThan(0.0001f),
                    $"'{t.name}' must be at scale 1");
            }
        }

        [TestCaseSource(nameof(Models))]
        public void Materials_AreMappedToProjectMaterials(Expected model)
        {
            var renderer = Load(model).GetComponentInChildren<MeshRenderer>();
            string[] paths = System.Array.ConvertAll(renderer.sharedMaterials, AssetDatabase.GetAssetPath);
            CollectionAssert.AreEquivalent(model.Materials, paths);
        }

        private static GameObject Load(Expected model)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(model.Path);
            Assert.IsNotNull(go, $"{model.Path} must exist; build it with Tools/asset_builders");
            return go;
        }

        private static Bounds ModelBounds(GameObject root, out List<Vector3> points)
        {
            points = new List<Vector3>();
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Matrix4x4 toModel = root.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                foreach (Vector3 v in filter.sharedMesh.vertices)
                {
                    points.Add(toModel.MultiplyPoint3x4(v));
                }
            }
            Assert.That(points.Count, Is.GreaterThan(0), $"'{root.name}' has a mesh");
            var bounds = new Bounds(points[0], Vector3.zero);
            foreach (Vector3 p in points)
            {
                bounds.Encapsulate(p);
            }
            return bounds;
        }
    }
}
