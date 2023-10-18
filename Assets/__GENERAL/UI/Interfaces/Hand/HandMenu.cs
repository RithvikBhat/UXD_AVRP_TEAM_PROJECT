using UnityEngine;

using HCIG.Input;
using HCIG.Input.Anchor;
using HCIG.Input.Data;
using HCIG.Network;

namespace HCIG.UI {

    /// <summary>
    /// A on our left hand anchord menu
    /// </summary>
    public class HandMenu : Singleton<HandMenu> {

        public bool IsOpen {
            get {
                return _isOpen;
            }
        }
        private bool _isOpen;

        public Panel Panel {
            get {
                if (_panel == null) {
                    _panel = GetComponentInChildren<Panel>(true);
                }
                return _panel;
            }
        }
        private Panel _panel = null;

        public bool Suspended {
            get {
                return _suspended;
            }
            set {
                _suspended = value;
            }
        }
        private bool _suspended = false;

        private void Update() {

            if (!_suspended && InputDataManager.Instance.IsHandAvailable(Chirality.Left)) {

                Pose hand = HandAnchorManager.Instance.GetAnchorPose(Chirality.Left, PoseType.Side);

                transform.SetPositionAndRotation(hand.position, hand.rotation);

                if (!Panel.gameObject.activeSelf) {
                    if (CheckActivation(hand)) {
                        Panel.gameObject.SetActive(true);
                        _isOpen = true;
                    }
                } else {
                    if (CheckDeactivation(hand)) {
                        Panel.gameObject.SetActive(false);
                        _isOpen = false;
                    }
                }
            } else {
                if (Panel.gameObject.activeSelf) {
                    Panel.gameObject.SetActive(false);
                    _isOpen = false;
                }
            }
        }

        /// <summary>
        /// Checks if the menu should be activated
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private bool CheckActivation(Pose hand) {

            //Debug.DrawLine(CameraManager.Instance.Camera.transform.position, hand.position);
            //Debug.DrawRay(hand.position, - hand.up, Color.red);
            //Debug.DrawRay(CameraManager.Instance.Camera.transform.position, CameraManager.Instance.Camera.transform.forward, Color.cyan);

            // Check: Hand looks at head
            if (Vector3.Angle(- hand.up, (BaseManager.Instance.Camera.transform.position - hand.position).normalized) > 25) {
                return false;
            }

            // Check: Head looks at hand
            if (Vector3.Angle(BaseManager.Instance.Camera.transform.forward, (hand.position - BaseManager.Instance.Camera.transform.position).normalized) > 25) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the menu should be deactivated
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private bool CheckDeactivation(Pose hand) {

            //Debug.DrawLine(CameraManager.Instance.Camera.transform.position, hand.position);
            //Debug.DrawRay(hand.position, - hand.up, Color.red);
            //Debug.DrawRay(CameraManager.Instance.Camera.transform.position, CameraManager.Instance.Camera.transform.forward, Color.cyan);

            // Check: Hand looks at head
            if (Vector3.Angle(-hand.up, (BaseManager.Instance.Camera.transform.position - hand.position).normalized) > 25) {
                return true;
            }

            return false;
        }
    }
}
