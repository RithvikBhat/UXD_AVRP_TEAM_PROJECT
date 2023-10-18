using UnityEngine;
using UnityEngine.UI;

using System;
using System.Runtime.InteropServices;

using HCIG.Network;
using HCIG.VisualEffects;
using TMPro;

namespace HCIG.UI {

    public class Bottom : MonoBehaviour {

        [Header("Left")]
        [SerializeField]
        private Button _menu;

        [Header("Middle")]
        [SerializeField]
        private Button _mode;

        [Header("right")]
        [SerializeField]
        private Button _mini;
        [SerializeField]
        private Button _maxi;
        [SerializeField]
        private Button _exit;

        private bool _triggered = false;

        //#region externs

        //[DllImport("user32.dll")]
        //private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        //[DllImport("user32.dll")]
        //private static extern IntPtr GetActiveWindow();

        //#endregion externs

        private void Awake() {

            _menu.onClick.AddListener(() => {
                if (NetworkManager.Instance.InRoom) {
                    if (MainMenu.Instance.Panel.Page != PageType.Room) {
                        MainMenu.Instance.Panel.Open(PageType.Room);
                    } else {
                        MainMenu.Instance.Panel.Close();
                    }
                } else {
                    if (MainMenu.Instance.Panel.Page != PageType.Lobby) {
                        MainMenu.Instance.Panel.Open(PageType.Lobby);
                    } else {
                        MainMenu.Instance.Panel.Close();
                    }
                }
            });

            // NOT IN USE YET
            //_mini.onClick.AddListener(HandleMinimize);
            //_maxi.onClick.AddListener(HandleMaximize);
            _exit.onClick.AddListener(() => HandleMenuPresence(PageType.Exit));

            if (!ApplicationManager.Instance.IsAndroid) {

                // Mode-Switcher 
                if (ApplicationManager.Instance.IsEditor) {

                    _mode.onClick.AddListener(() => {
                        _triggered = true;

                        _mode.interactable = false;

                        ScreenFadeManager.Instance.FadeOut();
                    });

                    // Buttons
                    ApplicationManager.Instance.OnModeChanged += UpdateButtons;

                    // Interactability 
                    EnvironmentManager.Instance.OnSwitchedEnvironment += (scene) => {
                        _mode.interactable = scene == Scene.Lobby;
                    };

                    // Switch-Calls
                    ScreenFadeManager.Instance.OnFadeState += (fadeState) => {

                        if (!fadeState) {
                            return;
                        }

                        if (!_triggered) {
                            return;
                        }

                        if (ApplicationManager.Instance.Mode == Mode.PC) {
                            ApplicationManager.Instance.Mode = Mode.XR;
                        } else {
                            ApplicationManager.Instance.Mode = Mode.PC;
                        }

                        _triggered = false;

                        ScreenFadeManager.Instance.FadeIn();
                    };

                } else {
                    // in build-version it hast to be deactivated, because OpenXR doesn't initialize in any circumstances right now
                    _mode.gameObject.SetActive(false);
                }
            }
        }

        private void Start() {
            UpdateButtons(ApplicationManager.Instance.Mode);
        }

        private void UpdateButtons(Mode mode) {

            bool isPC = mode == Mode.PC;

            _menu.interactable = isPC;
            _mode.interactable = EnvironmentManager.Instance.Scene == Scene.Lobby;
            _exit.interactable = isPC;

            _mode.GetComponentInChildren<TMP_Text>(true).text = isPC ? "XR" : "PC";
        }

        private void HandleMenuPresence(PageType pageType) {
            if (!MainMenu.Instance.Panel.Open(pageType)) {
                MainMenu.Instance.Panel.Close();
            }
        }

        //private void HandleMaximize() {
        //    Screen.fullScreen = !Screen.fullScreen;
        //}

        //private void HandleMinimize() {
        //    ShowWindow(GetActiveWindow(), 2);
        //}
    }
}
