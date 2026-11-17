namespace Game.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Game.Systems;

    /// <summary>
    /// Manages the persistent UI root: full-screen loading overlay and pause overlay.
    /// Lives in the Persistent scene on the <c>UIRoot</c> GameObject; owned by GameManager.
    /// All overlays default to hidden. Call <see cref="ShowLoading"/> or
    /// <see cref="ShowScreen"/> to reveal them.
    /// </summary>
    public sealed class UISystem : MonoBehaviour, IUISystem
    {
        // --------------------------------------------------------------------- element names

        private const string SafeAreaContainerName  = "safe-area-container";
        private const string BtnResumeName          = "btn-resume";
        private const string BtnReturnToMenuName    = "btn-return-to-menu";

        // --------------------------------------------------------------------- serialized

        [SerializeField] private UIDocument _loadingDocument;
        [SerializeField] private UIDocument _pauseDocument;

        // --------------------------------------------------------------------- private state

        private VisualElement _loadingRoot;
        private VisualElement _pauseRoot;

        private bool _loadingVisible;
        private bool _pauseVisible;

        private Rect       _lastSafeArea;
        private Vector2Int _lastScreenSize;

        // --------------------------------------------------------------------- IUISystem

        /// <inheritdoc/>
        public event Action OnResumeRequested;

        /// <inheritdoc/>
        public event Action OnReturnToMenuRequested;

        /// <inheritdoc/>
        public bool IsAnyOverlayOpen => _loadingVisible || _pauseVisible;

        // --------------------------------------------------------------------- Unity lifecycle

        private void Start()
        {
            _loadingRoot = BindDocument(_loadingDocument, "Loading");
            _pauseRoot   = BindDocument(_pauseDocument,   "Pause");

            RegisterPauseButtons();

            SetVisible(_loadingRoot, false);
            SetVisible(_pauseRoot,   false);

            _lastSafeArea   = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            ApplySafeArea();

            Log.Info(LogCat.UI, "UISystem initialized.");
        }

        private void Update()
        {
            // Safe area should only change when the device rotates or keyboard appears;
            // checking every frame is cheap (two struct comparisons, no allocations).
            Rect       currentArea = Screen.safeArea;
            Vector2Int currentSize = new Vector2Int(Screen.width, Screen.height);

            if (currentArea != _lastSafeArea || currentSize != _lastScreenSize)
            {
                _lastSafeArea   = currentArea;
                _lastScreenSize = currentSize;
                ApplySafeArea();
            }
        }

        // --------------------------------------------------------------------- IUISystem implementation

        /// <inheritdoc/>
        public void ShowLoading()
        {
            SetVisible(_loadingRoot, true);
            _loadingVisible = true;
            Log.Info(LogCat.UI, "Loading overlay shown.");
        }

        /// <inheritdoc/>
        public void HideLoading()
        {
            SetVisible(_loadingRoot, false);
            _loadingVisible = false;
        }

        /// <inheritdoc/>
        public void ShowScreen(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Pause:
                    SetVisible(_pauseRoot, true);
                    _pauseVisible = true;
                    Log.Info(LogCat.UI, "Pause screen shown.");
                    break;
                default:
                    Log.Warn(LogCat.UI, "ShowScreen: ScreenId {0} is not handled by UISystem.", id);
                    break;
            }
        }

        /// <inheritdoc/>
        public void HideScreen(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Pause:
                    SetVisible(_pauseRoot, false);
                    _pauseVisible = false;
                    break;
                default:
                    Log.Warn(LogCat.UI, "HideScreen: ScreenId {0} is not handled by UISystem.", id);
                    break;
            }
        }

        // --------------------------------------------------------------------- private helpers

        private VisualElement BindDocument(UIDocument doc, string debugName)
        {
            if (doc == null)
            {
                Log.Error(LogCat.UI, "{0} UIDocument is not assigned on UISystem.", debugName);
                return null;
            }

            VisualElement root = doc.rootVisualElement;
            if (root == null)
            {
                Log.Error(
                    LogCat.UI,
                    "{0} UIDocument has no rootVisualElement; UXML asset may be missing.",
                    debugName);
                return null;
            }

            return root;
        }

        private void RegisterPauseButtons()
        {
            if (_pauseRoot == null)
                return;

            Button btnResume = _pauseRoot.Q<Button>(BtnResumeName);
            if (btnResume != null)
                btnResume.clicked += () => OnResumeRequested?.Invoke();

            Button btnMenu = _pauseRoot.Q<Button>(BtnReturnToMenuName);
            if (btnMenu != null)
                btnMenu.clicked += () => OnReturnToMenuRequested?.Invoke();
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplySafeArea()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            Rect  safe         = Screen.safeArea;
            float paddingLeft  = safe.xMin;
            float paddingRight = Screen.width  - safe.xMax;
            float paddingTop   = Screen.height - safe.yMax;
            float paddingBot   = safe.yMin;

            ApplySafeAreaInsets(_loadingRoot, paddingLeft, paddingRight, paddingTop, paddingBot);
            ApplySafeAreaInsets(_pauseRoot,   paddingLeft, paddingRight, paddingTop, paddingBot);
        }

        private static void ApplySafeAreaInsets(
            VisualElement root,
            float left, float right, float top, float bottom)
        {
            if (root == null)
                return;

            VisualElement container = root.Q<VisualElement>(SafeAreaContainerName);
            if (container == null)
                return;

            container.style.paddingLeft   = left;
            container.style.paddingRight  = right;
            container.style.paddingTop    = top;
            container.style.paddingBottom = bottom;
        }
    }
}
