namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Checks the barrel as Unity imports it from Tools/asset_builders/builders/barrel.py:
    /// the size the builder's parameters give, the pivot at the base, upright, scale 1,
    /// and the builder's three material slots mapped to the project materials
    /// (docs/building_assets.md, "The Unity side").
    /// </summary>
    public sealed class BarrelModelTests
    {
        private const string ModelPath = "Assets/Art/Models/Barrel.fbx";

        // From barrel.py's PARAMS: height, and the bilge diameter plus the hoops standing off it.
        private const float Height = 0.95f;
        private const float Diameter = 0.72f;
        private const float SizeTolerance = 0.02f;
        private const float PivotTolerance = 0.005f;

        private GameObject _model;
        private MeshFilter _filter;

        [SetUp]
        public void SetUp()
        {
            _model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Assert.IsNotNull(_model, $"{ModelPath} must exist; build it with Tools/asset_builders");
            _filter = _model.GetComponentInChildren<MeshFilter>();
            Assert.IsNotNull(_filter, "The model must have a mesh");
        }

        [Test]
        public void Size_MatchesBuilderDimensions()
        {
            Bounds bounds = WorldBounds();
            Assert.AreEqual(Height, bounds.size.y, SizeTolerance, "Height is along Unity's Y axis");
            Assert.AreEqual(Diameter, bounds.size.x, SizeTolerance, "Diameter along X");
            Assert.AreEqual(Diameter, bounds.size.z, SizeTolerance, "Diameter along Z");
        }

        [Test]
        public void Pivot_IsAtBaseCentre()
        {
            Assert.AreEqual(0f, WorldBounds().min.y, PivotTolerance, "The base sits at y 0");

            // The centre of the base ring, not of the bounds: the bung stands out on one side.
            Matrix4x4 toModel = _model.transform.worldToLocalMatrix * _filter.transform.localToWorldMatrix;
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (Vector3 v in _filter.sharedMesh.vertices)
            {
                Vector3 p = toModel.MultiplyPoint3x4(v);
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

        [Test]
        public void Transforms_AreUnrotatedAtScaleOne()
        {
            foreach (Transform t in _model.GetComponentsInChildren<Transform>())
            {
                Assert.That(Quaternion.Angle(Quaternion.identity, t.localRotation), Is.LessThan(0.01f),
                    $"'{t.name}' must not carry a rotation: the export bakes the axis change");
                Assert.That((t.localScale - Vector3.one).magnitude, Is.LessThan(0.0001f),
                    $"'{t.name}' must be at scale 1: the export applies the units");
            }
        }

        [Test]
        public void Materials_AreMappedToProjectMaterials()
        {
            var renderer = _filter.GetComponent<MeshRenderer>();
            string[] paths = System.Array.ConvertAll(renderer.sharedMaterials, AssetDatabase.GetAssetPath);
            CollectionAssert.AreEquivalent(new[] { "Assets/Art/M_Wood.mat", "Assets/Art/M_Iron.mat", "Assets/Art/M_Cork.mat" }, paths);
        }

        private Bounds WorldBounds()
        {
            Mesh mesh = _filter.sharedMesh;
            Matrix4x4 toModel = _model.transform.worldToLocalMatrix * _filter.transform.localToWorldMatrix;
            var bounds = new Bounds(toModel.MultiplyPoint3x4(mesh.vertices[0]), Vector3.zero);
            foreach (Vector3 v in mesh.vertices)
            {
                bounds.Encapsulate(toModel.MultiplyPoint3x4(v));
            }
            return bounds;
        }
    }
}
