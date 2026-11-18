namespace Game.Tests.PlayMode
{
    using System;
    using System.Collections;
    using System.IO;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Game.Core;
    using Game.Systems;
    using Game.UI;

    /// <summary>
    /// The Diagnostics panel in the running game: it opens over the menu, its buttons reach the
    /// exporter with the report, and Close hides it. With PANEL_CAPTURE set it writes a picture of
    /// the panel (UI Toolkit draws to the screen, so it is rendered into a texture for the shot).
    /// </summary>
    public sealed class DiagnosticsPanelTests
    {
        private const int WaitFrames = 900;

        private sealed class FakeExport : IDiagnosticsExport
        {
            public string Copied;
            public string Shared;

            public void Copy(string text) => Copied = text;

            public void Share(string subject, string text) => Shared = text;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator Panel_CopiesSharesAndCloses()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            for (int i = 0; i < WaitFrames && (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Menu); i++)
            {
                yield return null;
            }
            yield return null;

            var ui = UnityEngine.Object.FindAnyObjectByType<UISystem>();
            Assert.IsNotNull(ui.Diagnostics, "UISystem built the diagnostics panel");
            var fake = new FakeExport();
            ui.Diagnostics.UseExport(fake);

            ui.ShowScreen(ScreenId.Diagnostics);
            Assert.IsTrue(ui.IsAnyOverlayOpen, "the panel counts as an overlay");
            yield return null;
            VisualElement root = PanelRoot();
            StringAssert.Contains("kB", ui.Diagnostics.Status, "the status gives the report's size");

            Press(root, "btn-diag-copy");
            StringAssert.Contains(Application.productName, fake.Copied, "Copy hands over the report");
            StringAssert.DoesNotContain("detail:", fake.Copied);
            StringAssert.StartsWith("copied", ui.Diagnostics.Status);

            Press(root, "btn-diag-detailed");
            StringAssert.Contains("detail:", fake.Copied, "Copy detailed adds the per-second series");

            Press(root, "btn-diag-share");
            StringAssert.Contains("detail:", fake.Shared, "Share hands over the detailed report");

            string capture = Environment.GetEnvironmentVariable("PANEL_CAPTURE");
            if (!string.IsNullOrEmpty(capture))
            {
                yield return Capture(capture);
            }

            Press(root, "btn-diag-close");
            Assert.IsFalse(ui.IsAnyOverlayOpen, "Close hides the panel");
        }

        private static VisualElement PanelRoot()
        {
            foreach (UIDocument doc in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude))
            {
                if (doc.name == "DiagnosticsUI")
                {
                    return doc.rootVisualElement;
                }
            }
            Assert.Fail("no DiagnosticsUI document");
            return null;
        }

        private static void Press(VisualElement root, string name)
        {
            Button button = root.Q<Button>(name);
            Assert.IsNotNull(button, name);
            using (NavigationSubmitEvent e = NavigationSubmitEvent.GetPooled())
            {
                e.target = button;
                button.SendEvent(e);
            }
        }

        private static IEnumerator Capture(string path)
        {
            UIDocument doc = null;
            foreach (UIDocument d in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude))
            {
                if (d.name == "DiagnosticsUI") doc = d;
            }
            PanelSettings original = doc.panelSettings;
            var shot = UnityEngine.Object.Instantiate(original);
            var target = new RenderTexture(1920, 1080, 24);
            shot.targetTexture = target;
            doc.panelSettings = shot;
            for (int i = 0; i < 3; i++)
            {
                yield return null;
            }
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(path, image.EncodeToPNG());
            doc.panelSettings = original;
            UnityEngine.Object.Destroy(shot);
            target.Release();
        }
    }
}
