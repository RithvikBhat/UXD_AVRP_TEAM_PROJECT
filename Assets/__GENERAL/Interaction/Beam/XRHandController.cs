using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using HCIG.Input;
using HCIG.Input.Data;

namespace HCIG.Interaction {

    public class XRHandController : XRBaseController {

        [Header("Hand")]
        [SerializeField]
        private Chirality _chirality;

        #region Input State

        protected override void UpdateInput(XRControllerState controllerState) {
            controllerState.ResetFrameDependentStates();

            if (InputDataManager.Instance.IsHandPinching(_chirality)) {

                //// Grab
                //if (!controllerState.selectInteractionState.active) {
                //    controllerState.selectInteractionState.activatedThisFrame = true;
                //    controllerState.selectInteractionState.active = true;
                //}

                // UI
                if (!controllerState.uiPressInteractionState.active) {
                    controllerState.uiPressInteractionState.activatedThisFrame = true;
                    controllerState.uiPressInteractionState.active = true;
                }
            } else {

                //// Grab
                //if (controllerState.selectInteractionState.active) {
                //    controllerState.selectInteractionState.activatedThisFrame = true;
                //    controllerState.selectInteractionState.active = false;
                //}

                // UI
                if (controllerState.uiPressInteractionState.active) {
                    controllerState.uiPressInteractionState.deactivatedThisFrame = true;
                    controllerState.uiPressInteractionState.active = false;
                }
            }
        }

        #endregion Input State

    }
}
