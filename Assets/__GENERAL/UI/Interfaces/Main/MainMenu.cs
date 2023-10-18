using UnityEngine;

using HCIG.VisualEffects;
using System.Collections;

namespace HCIG.UI {

    public class MainMenu : Singleton<MainMenu> {

        #region Components

        public Panel Panel {
            get {
                if (_panel == null) {
                    _panel = GetComponentInChildren<Panel>(true);
                }
                return _panel;
            }
        }
        private Panel _panel = null;
        private Canvas _canvas;

        public Bottom Bottom {
            get {
                if (_bottom == null) {
                    _bottom = GetComponentInChildren<Bottom>(true);
                }
                return _bottom;
            }
        }
        private Bottom _bottom = null;

        #endregion Components

        private bool _initialized = false;

        public bool IsOpen {
            get {
                return Panel.Page != PageType.None;
            }
        }

        protected override void Awake() {
            base.Awake();

            EnvironmentManager.Instance.OnSwitchedEnvironment += UpdateBoardPresence;
            ApplicationManager.Instance.OnModeChanged += UpdateBoardStyle;

            if (!ApplicationManager.Instance.IsAndroid) {
                ScreenFadeManager.Instance.OnFadeState += UpdateCameraStyle;
            }

            _canvas = GetComponentInChildren<Canvas>(true);
        }

        private IEnumerator InitializeMenu() {

            // Set Up Menu
            if (ApplicationManager.Instance.IsAndroid) {

                Panel.Open(PageType.Start);

                yield return new WaitForSeconds(2.5f);

                Panel.Open(PageType.Lobby);
            } else {
                Panel.Open(PageType.Start, 2.5f);
            }

            _initialized = true;
        }

        /// <summary>
        /// When we joint the lobby, we need the lobby board... otherwise we close it because no need of it
        /// </summary>
        private void UpdateBoardPresence(Scene scene) {

            if (_initialized) {

                if (!ApplicationManager.Instance.IsAndroid && !Bottom.gameObject.activeSelf) {
                    Bottom.gameObject.SetActive(true);
                }

                if (scene == Scene.Lobby) {
                    Panel.Open(PageType.Lobby);
                } else {
                    Panel.Close();
                }
            } else {

                if(scene == Scene.Lobby) {
                    // we start in the actual lobby
                    StartCoroutine(InitializeMenu());

                } else {
                    // we start in a debugging scene
                    _initialized = true;
                }

                if (!ApplicationManager.Instance.IsAndroid && !Bottom.gameObject.activeSelf) {
                    Bottom.gameObject.SetActive(true);
                }
            }
        }


        /// <summary>
        /// Update and repositioning of the Panel-Board (Lobby) depending on the activated mode.
        /// </summary>
        private void UpdateBoardStyle(Mode mode) {

            _canvas.worldCamera = BaseManager.Instance.Camera;

            if (mode == Mode.PC) {

                _canvas.renderMode = RenderMode.ScreenSpaceCamera;

                _canvas.planeDistance = 0.5f;

                Panel.Close();
            } else {
                _canvas.renderMode = RenderMode.WorldSpace;

                _canvas.transform.SetPositionAndRotation(new Vector3(2, 1.5f, 2), Quaternion.Euler(0, 45, 0));
                _canvas.transform.localScale = 2 * Mathf.Pow(10, -3) * Vector3.one;

                if(EnvironmentManager.Instance.Scene == Scene.Lobby) {
                    Panel.Open(PageType.Lobby);
                }
            }
        }

        /// <summary>
        /// Relevant for PC camera, because when we fade in/ out we would overlay our fade and that is ugly
        /// </summary>
        /// <param name="fadeState"></param>
        private void UpdateCameraStyle(bool fadeState) {

            if (fadeState) {
                return;
            }

            switch (_canvas.renderMode) {
                case RenderMode.ScreenSpaceOverlay:
                    _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    break;

                case RenderMode.ScreenSpaceCamera:
                    _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    break;
            }
        }
    }
}
