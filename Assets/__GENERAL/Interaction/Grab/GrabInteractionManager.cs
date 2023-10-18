using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using HCIG.Input;
using HCIG.Input.Data;
using HCIG.Input.Anchor;

using Leap.Unity.Interaction;

namespace HCIG.Interaction {

    public class GrabInteractionManager : Singleton<GrabInteractionManager> {

        [Header("Grab Physics")]
        [SerializeField]
        private GameObject _realistic;
        [SerializeField]
        private GameObject _controller;

        public bool IsGrabbing {
            get {
                foreach(InteractionController controller in InteractionManager.instance.interactionControllers) {
                    if (controller.isGraspingObject) return true;
                }

                return false;            
            }
        }

        protected override void Awake() {
            base.Awake();

            InputDataManager.Instance.OnInputTypeChanged += SwitchPhysics;

            RayCastManager.Instance.OnObjectSelected += CheckRayGrabability;

            // Initial
            SwitchPhysics(InputType.None);
        }

        /// <summary>
        /// Enables/ Disables the physic for the specified InputType
        /// </summary>
        /// <param name="inputType"></param>
        private void SwitchPhysics(InputType inputType) {

            // Deactivation
            if(inputType != InputType.Controllers && _controller.activeSelf) {
                _controller.SetActive(false);
            }

            if (inputType != InputType.Realistic && _realistic.activeSelf) {
                _realistic.SetActive(false);
            }


            // Activation
            switch (inputType) {
                case InputType.Controllers:

                    if (!_controller.activeSelf) {
                        _controller.SetActive(true);
                    }
                        break;

                case InputType.Realistic:

                    if (!_realistic.activeSelf) {
                        _realistic.SetActive(true);
                    }
                    break;
            }
        }

        /// <summary>
        /// WIP - NOT WORKABLE YET
        /// </summary>
        /// <param name="chirality"></param>
        /// <param name="raycastHit"></param>
        private void CheckRayGrabability(Chirality chirality, RaycastHit raycastHit) {

            if (raycastHit.rigidbody == null) {
                return;
            }

            GameObject hitObject = raycastHit.rigidbody.gameObject;

            if (!hitObject.TryGetComponent(out XRSimpleInteractable _)) {
                return;
            }

            // Position
            hitObject.transform.position = HandAnchorManager.Instance.GetAnchorPose(chirality, PoseType.Pinch).position + hitObject.transform.position - raycastHit.point;

            // Interaction
            hitObject.GetComponent<InteractionBehaviour>().OnGraspBegin.Invoke();
        }
    }
}
