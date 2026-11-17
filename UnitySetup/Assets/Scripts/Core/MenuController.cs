namespace Game.Core
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using Game.Systems;

    /// <summary>
    /// Wires the main menu buttons to <see cref="GameManager"/>.
    /// Lives only in <c>Menu.unity</c>. Destroyed when the Menu scene unloads.
    /// Calls no scene-loading code directly; all intents are routed through
    /// <see cref="GameManager.Instance"/>.
    /// </summary>
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _menuDocument;

        private Button _btnPlay;
        private Button _btnContinue;
        private Button _btnQuit;

        private void Start()
        {
            if (_menuDocument == null)
            {
                Log.Error(LogCat.UI, "MenuController: _menuDocument is not assigned.");
                return;
            }

            VisualElement root = _menuDocument.rootVisualElement;

            _btnPlay     = root.Q<Button>("btn-play");
            _btnContinue = root.Q<Button>("btn-continue");
            _btnQuit     = root.Q<Button>("btn-quit");

            if (_btnPlay     == null) Log.Error(LogCat.UI, "MenuController: btn-play not found in Menu.uxml.");
            if (_btnContinue == null) Log.Error(LogCat.UI, "MenuController: btn-continue not found in Menu.uxml.");
            if (_btnQuit     == null) Log.Error(LogCat.UI, "MenuController: btn-quit not found in Menu.uxml.");

            if (_btnPlay     != null) _btnPlay.clicked     += OnPlayClicked;
            if (_btnContinue != null) _btnContinue.clicked += OnContinueClicked;
            if (_btnQuit     != null) _btnQuit.clicked     += OnQuitClicked;

            // Reflect current save state so Continue is only enabled when a save exists.
            RefreshContinueButton();

            Log.Info(LogCat.UI, "MenuController initialized.");
        }

        private void OnDestroy()
        {
            if (_btnPlay     != null) _btnPlay.clicked     -= OnPlayClicked;
            if (_btnContinue != null) _btnContinue.clicked -= OnContinueClicked;
            if (_btnQuit     != null) _btnQuit.clicked     -= OnQuitClicked;
        }

        // ------------------------------------------------------------------ button handlers

        private void OnPlayClicked()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) { Log.Error(LogCat.UI, "MenuController: GameManager.Instance is null."); return; }
            // Do not pass destroyCancellationToken: MenuController is destroyed when Menu
            // unloads, which happens as part of this very transition. Passing it would
            // cancel the in-flight LoadGameAsync mid-way and log a spurious warning.
            _ = gm.StartNewGameAsync();
        }

        private void OnContinueClicked()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) { Log.Error(LogCat.UI, "MenuController: GameManager.Instance is null."); return; }
            _ = gm.ContinueAsync();
        }

        private void OnQuitClicked()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) { Log.Error(LogCat.UI, "MenuController: GameManager.Instance is null."); return; }
            gm.QuitGame();
        }

        // ------------------------------------------------------------------ helpers

        private void RefreshContinueButton()
        {
            if (_btnContinue == null) return;

            GameManager gm = GameManager.Instance;
            bool hasSave = gm != null && gm.SaveSystem != null && gm.SaveSystem.HasSave;
            _btnContinue.SetEnabled(hasSave);
        }
    }
}
