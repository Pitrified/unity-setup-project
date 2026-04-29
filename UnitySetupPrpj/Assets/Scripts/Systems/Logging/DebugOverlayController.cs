// The debug overlay is active only in Editor and Development builds.
// In Release the class body is empty so the component does nothing and
// IL2CPP can strip it entirely.
namespace Game.Systems
{
    using UnityEngine;
    using UnityEngine.UIElements;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    using System;
    using System.Text;
    using UnityEngine.InputSystem;
#endif

    /// <summary>
    /// Drives the in-game debug overlay UIDocument.
    /// Toggle: F1 in the Editor, three-finger tap on device.
    /// Attach to a GameObject that also has a UIDocument component pointed at
    /// DebugOverlay.uxml. The PanelSettings sort order should be higher than game UI.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DebugOverlayController : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        // ------------------------------------------------------------------ constants

        private const int FrameWindow = 60;
        private const float DisplayInterval = 0.1f;

        // ------------------------------------------------------------------ FPS tracking (pre-allocated, no-alloc in Update)

        private readonly float[] _frameTimes = new float[FrameWindow];
        private int _frameHead;
        private float _frameSum;
        private int _frameCount;

        // ------------------------------------------------------------------ GPU/CPU timing

        private readonly FrameTiming[] _framingBuf = new FrameTiming[1];
        private float _lastCpuMs;
        private float _lastGpuMs;

        // ------------------------------------------------------------------ log read buffer (pre-allocated)

        private readonly LogEntry[] _logReadBuf = new LogEntry[20];

        // ------------------------------------------------------------------ UI elements

        private VisualElement _overlayPanel;
        private Label _fpsLabel;
        private Label _frameTimeLabel;
        private Label _stateLabel;
        private Label _logLabel;

        // ------------------------------------------------------------------ state

        private bool _visible;
        private float _displayTimer;
        private bool _prevThreeFinger;

        // ------------------------------------------------------------------ string builder (reused at display rate)

        private readonly StringBuilder _sb = new StringBuilder(1024);

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            var doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;

            _overlayPanel   = root.Q<VisualElement>("overlay-panel");
            _fpsLabel        = root.Q<Label>("fps-label");
            _frameTimeLabel  = root.Q<Label>("frametime-label");
            _stateLabel      = root.Q<Label>("state-label");
            _logLabel        = root.Q<Label>("log-label");

            var btnReset  = root.Q<Button>("btn-reset-position");
            var btnReload = root.Q<Button>("btn-reload-scene");
            var btnSkip   = root.Q<Button>("btn-skip-loading");

            if (btnReset  != null) btnReset.RegisterCallback<ClickEvent>(_ => Log.OnDebugResetPosition?.Invoke());
            if (btnReload != null) btnReload.RegisterCallback<ClickEvent>(_ => Log.OnDebugReloadScene?.Invoke());
            if (btnSkip   != null) btnSkip.RegisterCallback<ClickEvent>(_ => Log.OnDebugSkipLoading?.Invoke());

            SetVisible(false);
        }

        private void Update()
        {
            SampleFrame();
            CheckToggle();

            if (!_visible)
                return;

            _displayTimer += Time.unscaledDeltaTime;
            if (_displayTimer >= DisplayInterval)
            {
                _displayTimer = 0f;
                RefreshDisplay();
            }
        }

        // ------------------------------------------------------------------ frame sampling (no allocation)

        private void SampleFrame()
        {
            float dt = Time.unscaledDeltaTime;

            if (_frameCount >= FrameWindow)
                _frameSum -= _frameTimes[_frameHead];

            _frameTimes[_frameHead] = dt;
            _frameSum += dt;
            _frameHead = (_frameHead + 1) % FrameWindow;
            if (_frameCount < FrameWindow)
                _frameCount++;
        }

        // ------------------------------------------------------------------ toggle detection (no allocation)

        private void CheckToggle()
        {
#if UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                ToggleOverlay();
                return;
            }
#endif
            CheckTouchToggle();
        }

        private void CheckTouchToggle()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return;

            int activeCount = 0;
            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var phase = touches[i].phase.ReadValue();
                if (phase != UnityEngine.InputSystem.TouchPhase.None
                    && phase != UnityEngine.InputSystem.TouchPhase.Ended
                    && phase != UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    activeCount++;
                }
            }

            bool threeFinger = activeCount >= 3;
            if (threeFinger && !_prevThreeFinger)
                ToggleOverlay();
            _prevThreeFinger = threeFinger;
        }

        private void ToggleOverlay()
        {
            SetVisible(!_visible);
        }

        private void SetVisible(bool show)
        {
            _visible = show;
            if (_overlayPanel != null)
                _overlayPanel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ------------------------------------------------------------------ display refresh (allocates strings at DisplayInterval rate, acceptable for dev tool)

        private void RefreshDisplay()
        {
            // FPS
            float avgFps = _frameCount > 0 ? _frameCount / _frameSum : 0f;

            // 1% low: with a 60-frame window, 1% worst = max frame time in the window
            float maxDt = 0f;
            int window = Math.Min(_frameCount, FrameWindow);
            for (int i = 0; i < window; i++)
            {
                if (_frameTimes[i] > maxDt)
                    maxDt = _frameTimes[i];
            }
            float p1LowFps = maxDt > 0f ? 1f / maxDt : 0f;

            // CPU/GPU frame times via FrameTimingManager (may return 0 on unsupported platforms)
            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, _framingBuf) > 0)
            {
                _lastCpuMs = (float)_framingBuf[0].cpuFrameTime;
                _lastGpuMs = (float)_framingBuf[0].gpuFrameTime;
            }

            if (_fpsLabel != null)
                _fpsLabel.text = $"FPS  avg {avgFps:F1}   1% low {p1LowFps:F1}";
            if (_frameTimeLabel != null)
                _frameTimeLabel.text = $"CPU {_lastCpuMs:F2} ms   GPU {_lastGpuMs:F2} ms";
            if (_stateLabel != null)
                _stateLabel.text = $"State: {(Log.GetGameState?.Invoke() ?? "?")}";

            // Log buffer
            int count = Log.GetRecentEntries(_logReadBuf);
            _sb.Clear();
            for (int i = 0; i < count; i++)
            {
                LogEntry e = _logReadBuf[i];
                _sb.Append('[').Append(e.Level).Append("][").Append(e.Category).Append("] ").AppendLine(e.Text);
            }
            if (_logLabel != null)
                _logLabel.text = _sb.ToString();
        }
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD
    }
}
