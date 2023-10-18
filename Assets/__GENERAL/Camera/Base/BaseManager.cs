using HCIG.Teleport;

using UnityEngine;

namespace HCIG {

    /// <summary>
    /// Management of our two camera base systems (XR / PC)
    /// </summary>
    public class BaseManager : Singleton<BaseManager> {

        /// <summary>
        /// Returns the current active Camera (PC or XR)
        /// </summary>
        public Camera Camera {
            get {
                if (ApplicationManager.Instance.Mode == Mode.XR) {
                    return BaseXR.Instance.Camera;
                } else {
                    return BasePC.Instance.Camera;
                }
            }
        }

        protected override void Awake() {
            base.Awake();

            // Entering the environment on position...
            EnvironmentManager.Instance.OnSwitchedEnvironment += ShouldStartWhere;

            // Activate the correct camera 
            ApplicationManager.Instance.OnModeChanged += SwitchCameraSystem;

            // Instantiations... PC (relevant for Varjo XR-3
            if (!ApplicationManager.Instance.IsAndroid){
                UnityEngine.Rendering.TextureXR.maxViews = 2;
            }
        }


        /// <summary>
        /// for the specific environment maybe there could be interesting start positions 
        /// </summary>
        /// <param name="scene"></param>
        private void ShouldStartWhere(Scene scene) {

            if (ApplicationManager.Instance.Mode == Mode.XR) {

                TeleportManager.Instance.TeleportWithoutFade(Vector3.zero, BaseXR.Instance.Camera.transform.eulerAngles);
            } else {
                if (scene == Scene.Lobby) {
                    BasePC.Instance.MovementAllowed = false;
                    TeleportManager.Instance.TeleportWithoutFade(new Vector3(-20.5f, 0.4f, -0.125f), new Vector3(-15.5f, 50.5f, 0));
                } else {
                    BasePC.Instance.MovementAllowed = true;
                    TeleportManager.Instance.TeleportWithoutFade(new Vector3(0, 2, -4.5f), new Vector3(20, 0, 0));
                }
            }
        }

        /// <summary>
        /// switch between the two variants (XR or PC) of our camera system
        /// </summary>
        private void SwitchCameraSystem(Mode mode)
        {
            if(mode == Mode.XR)
            {
                BasePC.Instance.gameObject.SetActive(false);
                BaseXR.Instance.gameObject.SetActive(true);
            }
            else
            {
                BaseXR.Instance.gameObject.SetActive(false);
                BasePC.Instance.gameObject.SetActive(true);
            }
        }
    }
}
