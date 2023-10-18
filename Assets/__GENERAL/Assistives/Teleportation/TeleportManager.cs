using System;

using UnityEngine;

using HCIG.Input;
using HCIG.Interaction;
using HCIG.VisualEffects;
using HCIG.UI;
using HCIG.Input.Anchor;

namespace HCIG.Teleport {

    public class TeleportManager : Singleton<TeleportManager> {

        public Action OnTeleported = () => { };

        [Header("Tele-Side")]
        [SerializeField]
        private Chirality _chirality = Chirality.Right;

        [Header("Components")]
        [SerializeField]
        private GameObject _ball;
        [SerializeField]
        private GameObject _mark;

        [Header("Gradients")]
        [SerializeField]
        private Gradient _valid;
        [SerializeField]
        private Gradient _invalid;

        [Header("Materials")]
        [SerializeField]
        private Material _teleportMaterial;

        private int _angleDeviation = 30;

        private bool _teleportable = false;

        private bool _triggered = false;
        private Vector3 _position = Vector3.zero;
        private Vector3 _rotation = Vector3.zero;

        /// <summary>
        /// SET: activates/ deactivates the teleportation functionality
        /// GET: Returns the current activation state of the teleport
        /// </summary>
        public bool Activated {
            get {
                return _activated;
            }
            set {
                if (value == _activated) {
                    return;
                }

                // Activation Teleportation
                _activated = value;

                // Activate Raycast-System
                if (_activated) {
                    RayCastManager.Instance.ChangeLine(RayMode.Teleport);

                    RayCastManager.Instance.ChangeColor(true, _invalid);
                    RayCastManager.Instance.ChangeColor(false, _invalid);

                    ChangeSide(_chirality);

                    _teleportable = false;

                    RayCastManager.Instance.Activated = !HandMenu.Instance.IsOpen;

                    _ball.SetActive(true);
                    _mark.SetActive(false);
                } else {
                    RayCastManager.Instance.ResetToDefaultRays();

                    _ball.SetActive(false);
                    _mark.SetActive(false);
                }
            }
        }
        private bool _activated = false;

        protected override void Awake() {
            base.Awake();

            // Events
            ScreenFadeManager.Instance.OnFadeState += PerformTeleportation;
            RayCastManager.Instance.OnObjectSelected += CheckTeleportable;
        }

        #region Ray Cast

        private void FixedUpdate() {

            if (!_activated) {
                return;
            }

            _ball.transform.position = HandAnchorManager.Instance.GetAnchorPose(_chirality, PoseType.Pinch).position;

            // Raycast-Activation
            if (HandMenu.Instance.IsOpen) {
                if (RayCastManager.Instance.Activated) {
                    RayCastManager.Instance.Activated = false;

                    _teleportMaterial.color = _invalid.colorKeys[0].color;

                    _teleportable = false;

                    _mark.transform.position = new Vector3(0, -5000, 0);
                    _mark.SetActive(false);
                }

                return;
            } else {
                if (!RayCastManager.Instance.Activated) {
                    RayCastManager.Instance.Activated = true;
                }
            }

            if (RayCastManager.Instance.TryGetCurrentObject(_chirality, out RaycastHit raycastHit)) {
                if (!_mark.activeSelf) {
                    _mark.SetActive(true);
                }

                _mark.transform.position = raycastHit.point;
            } else {
                if (_mark.activeSelf) {
                    _mark.SetActive(false);
                }
            }

            // RayCast-Coloration
            if (CheckNormal(raycastHit.normal) && _mark.activeSelf) {
                if (!_teleportable) {
                    RayCastManager.Instance.ChangeColor(false, _valid);

                    _teleportMaterial.color = _valid.colorKeys[0].color;

                    _teleportable = true;
                }
            } else {
                if (_teleportable) {
                    RayCastManager.Instance.ChangeColor(false, _invalid);

                    _teleportMaterial.color = _invalid.colorKeys[0].color;

                    _teleportable = false;
                }
            }
        }

        /// <summary>
        /// switches the side of the teleport ray
        /// </summary>
        /// <param name="chirality"></param>
        public void ChangeSide(Chirality chirality) {

            // Deactivates the other chirality hand
            RayCastManager.Instance.DisableRay((Chirality)Mathf.Abs((int)_chirality - 1));

            if (_activated) {
                RayCastManager.Instance.ChangeLine(RayMode.Teleport);
                RayCastManager.Instance.EnableRay(chirality);
            }
        }

        #endregion Ray Cast

        #region Checks

        private void CheckTeleportable(Chirality chirality, RaycastHit raycastHit) {

            if (!_activated) {
                return;
            }

            if(_chirality != chirality) {
                return;
            }

            if (HandMenu.Instance.IsOpen) {
                return;
            }

            if (CheckNormal(raycastHit.normal)) {
                Teleport(raycastHit.point);
            }
        }

        private bool CheckNormal(Vector3 normal) {
            if(normal == Vector3.zero) {
                return false;
            }

            return Vector3.Angle(normal, Vector3.up) < _angleDeviation;
        }

        #endregion Checks

        #region Teleportation

        /// <summary>
        /// Starts the teleport process with a fade out/ in
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(Vector3 position) {
            if (ScreenFadeManager.Instance.IsActive) {
                return;
            }

            _triggered = true;

            _position = position;
            _rotation = BaseXR.Instance.Camera.transform.eulerAngles;

            ScreenFadeManager.Instance.Fade();
        }

        /// <summary>
        /// The final teleportation process        
        /// </summary>
        /// <param name="fadeState"></param>
        private void PerformTeleportation(bool fadeState) {

            if (!fadeState) {
                return;
            }

            if(_triggered) {

                TeleportWithoutFade(_position, BaseXR.Instance.Camera.transform.eulerAngles);

                _position = Vector3.zero;
                _triggered = false;
            }
        }

        /// <summary>
        /// Teleports the rig (camera) directly to the specified entrance point without a fade (e.g. we are already in fade process or someting else)
        /// </summary>
        /// <param name="position"></param>
        public void TeleportWithoutFade(Vector3 position = default, Vector3 rotation = default) {

            if (ApplicationManager.Instance.Mode == Mode.XR) {

                // Reference Camera
                Vector3 cameraPos = BaseXR.Instance.Camera.transform.localPosition;
                Vector3 cameraRot = BaseXR.Instance.Camera.transform.localEulerAngles;

                // Calculate correct rotation (in VR we only allow horizontal rotations)
                BaseXR.Instance.Offset.eulerAngles = new Vector3(0, rotation.y - cameraRot.y, 0);

                Debug.Log(rotation.y - cameraRot.y);

                // Clear position to zero, so the reference calibration is correct
                BaseXR.Instance.Offset.position = Vector3.zero;

                // Calculate the desired position in reference to the current camera position
                BaseXR.Instance.Offset.position = position - BaseXR.Instance.Offset.transform.TransformPoint(cameraPos.x, 0, cameraPos.z);
            } else {
                BasePC.Instance.Offset.position = position;
                BasePC.Instance.Offset.eulerAngles = rotation;
            }

            OnTeleported.Invoke();
        }

        #endregion Teleportation
    }
}
