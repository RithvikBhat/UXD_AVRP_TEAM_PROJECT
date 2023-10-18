using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

namespace HCIG {

    public enum Mode {
        PC,
        XR
    }

    public class ApplicationManager : Singleton<ApplicationManager> {

        public Action<Mode> OnModeChanged = (_) => { };

        public Mode Mode {
            get {
                return _mode;
            }
            set {
                if(_mode == value){
                    return;
                }

                if (value == Mode.XR) {
                    StartCoroutine(InitializeXR());
                } else {
                    InitializePC();
                }
            }
        }
        private Mode _mode = (Mode)(-1); // Null

        public bool IsEditor {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public bool IsAndroid {
            get {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }


        // DEBUG
        [Header("Debug")]
        [Tooltip("Just for Debugging situations during Editor-Mode")]
#if !UNITY_ANDROID
        [SerializeField]
#endif
        private Mode _desktopStartMode = Mode.PC;
        // DEBUG

        protected void Start() {

            if (IsAndroid) {
                Mode = Mode.XR;
            } else {
                if (IsEditor) {
                    Mode = _desktopStartMode;
                } else {
                    Mode = Mode.PC;
                }
            }
        }

        /// <summary>
        /// Enter/ Leave fullscreen
        /// </summary>
        private void Update() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
                Screen.fullScreen = !Screen.fullScreen;
            }
        }

        /// <summary>
        /// End application
        /// </summary>
        public void ExitApplication() {

            // Deinitialize XR, if active
            InitializePC();

            if (!IsEditor) {
                Application.Quit();
            } else {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            DeinitializeXR();
        }

        private IEnumerator InitializeXR() {

            if (IsEditor && XRGeneralSettings.Instance.Manager.activeLoader == null) {

                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

                if (XRGeneralSettings.Instance.Manager.activeLoader == null) {

                    LogManager.Instance.LogInfo("Fail XR...");

                    if (!IsEditor || !IsAndroid) {
                        Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
                        OnModeChanged.Invoke(_mode = Mode.PC);
                    }
                    yield break;
                }

                LogManager.Instance.LogInfo("Starting XR...");

                XRGeneralSettings.Instance.Manager.StartSubsystems();
                yield return null;
            }

            OnModeChanged.Invoke(_mode = Mode.XR);
        }

        private void InitializePC() {

            DeinitializeXR();

            OnModeChanged.Invoke(_mode = Mode.PC);
        }

        private void DeinitializeXR() {

            if (XRGeneralSettings.Instance.Manager.isInitializationComplete) {

                LogManager.Instance.LogInfo("Deinitialize...");

                XRGeneralSettings.Instance.Manager.StopSubsystems();
                //Camera.main.ResetAspect();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            } else {
                LogManager.Instance.LogInfo("Nothing to DEINITIALIZE");
            }
        }
    }
}
