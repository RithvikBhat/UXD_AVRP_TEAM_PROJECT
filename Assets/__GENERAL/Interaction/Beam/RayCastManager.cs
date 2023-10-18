using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

using System;

using HCIG.Input;
using HCIG.Input.Data;
using HCIG.Input.Anchor;

namespace HCIG.Interaction {

    /// <summary>
    /// Sets which trajectory path Unity uses for the cast when detecting collisions.
    /// </summary>
    /// <seealso cref="lineType"/>
    public enum RayMode {

        /// <summary>
        /// Performs a single ray cast into the Scene with a set ray length.
        /// </summary>
        Interact,

        /// <summary>
        /// Samples the trajectory of a projectile to generate a projectile curve.
        /// </summary>
        Teleport,
    }

    public class RayCastManager : Singleton<RayCastManager> {

        public Action<Chirality, RaycastHit> OnObjectSelected = (_, _) => { };

        public Action<Chirality, RaycastResult> OnInterfaceSelected = (_, _) => { };

        [Header("Line")]
        public RayMode RayMode;

        [Header("Interactors")]
        [SerializeField]
        private XRRayInteractor _left;
        private bool _leftEnabled = false;

        [SerializeField]
        private XRRayInteractor _right;
        private bool _rightEnabled = false;

        [Header("Color")]
        [SerializeField]
        private Gradient _valid;
        [SerializeField]
        private Gradient _invalid;

        [Header("Offsets")]
        [SerializeField]
        private Vector3 _shoulder = new Vector3(0.2f, -0.2f, 0);

        public bool Activated {
            get {
                return _activated;
            }
            set {
                _activated = value;

                if (!_activated) {
                    // instant deactivation of every ray

                    if (_left.gameObject.activeSelf) {
                        _left.gameObject.SetActive(false);
                    }

                    if (_right.gameObject.activeSelf) {
                        _right.gameObject.SetActive(false);
                    }
                }
            }
        }
        private bool _activated = true;

        protected override void Awake() {
            EnvironmentManager.Instance.OnSwitchedEnvironment += (_) => ResetToDefaultRays();

            // Pinch - Object Hit
            InputDataManager.Instance.OnPinchTriggered += (chirality) => {
                if (TryGetCurrentObject(chirality, out RaycastHit raycastHit)) {
                    OnObjectSelected.Invoke(chirality, raycastHit);
                }
            };

            // Pinch - Interface Hit
            InputDataManager.Instance.OnPinchTriggered += (chirality) => {
                if (TryGetCurrentInterface(chirality, out RaycastResult raycastResult)) {
                    OnInterfaceSelected.Invoke(chirality, raycastResult);
                }
            };
        }

        private void Start() {
            ResetToDefaultRays();
        }

        private void Update() {

            if (_activated && !(GrabInteractionManager.Instance.IsGrabbing && RayMode == RayMode.Interact)) {

                if (_leftEnabled && InputDataManager.Instance.IsHandAvailable(Chirality.Left)) {

                    if (!_left.gameObject.activeSelf) {
                        _left.gameObject.SetActive(true);
                    }

                    // Ray (Position: Pinch & Direction is from left shoulder to pinch)
                    _left.transform.position = HandAnchorManager.Instance.GetAnchorPose(Chirality.Left, PoseType.Pinch).position;
                    _left.transform.rotation = Quaternion.LookRotation(_left.transform.position - (BaseManager.Instance.Camera.transform.position + Quaternion.Euler(0, BaseManager.Instance.Camera.transform.eulerAngles.y, 0) * Vector3.Scale(new(-1, 1, 1), _shoulder)));
                } else {
                    if (_left.gameObject.activeSelf) {
                        _left.gameObject.SetActive(false);
                    }
                }


                if (_rightEnabled && InputDataManager.Instance.IsHandAvailable(Chirality.Right)) {

                    if (!_right.gameObject.activeSelf) {
                        _right.gameObject.SetActive(true);
                    }

                    // Ray (Position: Pinch & Diorection is from right shoulder to pinch)
                    _right.transform.position = HandAnchorManager.Instance.GetAnchorPose(Chirality.Right, PoseType.Pinch).position;
                    _right.transform.rotation = Quaternion.LookRotation(_right.transform.position - (BaseManager.Instance.Camera.transform.position + Quaternion.Euler(0, BaseManager.Instance.Camera.transform.eulerAngles.y, 0) * _shoulder));
                } else {

                    if (_right.gameObject.activeSelf) {
                        _right.gameObject.SetActive(false);
                    }
                }
            } else {
                if (_left.gameObject.activeSelf) {
                    _left.gameObject.SetActive(false);
                }

                if (_right.gameObject.activeSelf) {
                    _right.gameObject.SetActive(false);
                }
            }
        }

        public void ResetToDefaultRays() {

            // Color
            ChangeColor(true, _valid);
            ChangeColor(false, _invalid);

            // Line
            ChangeLine(RayMode.Interact);

            // Rays
            EnableRay(Chirality.Left);
            EnableRay(Chirality.Right);

            // Start
            _activated = true;
        }

        #region Activation / Deactivation

        public void EnableRay(Chirality chirality) {

            if (chirality == Chirality.Left) {
                _leftEnabled = true;
            } else {
                _rightEnabled = true;
            }
        }

        public void DisableRay(Chirality chirality) {

            if (chirality == Chirality.Left) {
                _leftEnabled = false;

                if (_left.gameObject.activeSelf) {
                    _left.gameObject.SetActive(false);
                }
            } else {
                _rightEnabled = false;

                if (_right.gameObject.activeSelf) {
                    _right.gameObject.SetActive(false);
                }
            }
        }

        #endregion Activation / Deactivation

        #region Visualization

        /// <summary>
        /// Changes the color of both rays
        /// </summary>
        /// <param name="validLine"></param>
        /// <param name="gradient"></param>
        public void ChangeColor(bool validLine, Gradient gradient) {

            if (_left != null) {
                XRInteractorLineVisual lineVisual = _left.GetComponent<XRInteractorLineVisual>();

                if (validLine) {
                    lineVisual.validColorGradient = gradient;
                } else {
                    lineVisual.invalidColorGradient = gradient;
                }
            }


            if (_right != null) {
                XRInteractorLineVisual lineVisual = _right.GetComponent<XRInteractorLineVisual>();

                if (validLine) {
                    lineVisual.validColorGradient = gradient;
                } else {
                    lineVisual.invalidColorGradient = gradient;
                }
            }
        }

        /// <summary>
        /// Changes the line type of both rays
        /// </summary>
        /// <param name="type"></param>
        public void ChangeLine(RayMode type) {

            if (_left != null) {
                _left.lineType = (XRRayInteractor.LineType)type;

                if (type == RayMode.Interact) {
                    _left.enableUIInteraction = true;
                } else {
                    _left.enableUIInteraction = false;
                }
            }

            if (_right != null) {
                _right.lineType = (XRRayInteractor.LineType)type;

                if (type == RayMode.Interact) {
                    _right.enableUIInteraction = true;
                } else {
                    _right.enableUIInteraction = false;
                }
            }

            RayMode = type;
        }

        #endregion Visualization

        #region Target

        public bool TryGetCurrentObject(Chirality chirality, out RaycastHit raycastHit) {

            if (chirality == Chirality.Left) {
                return _left.TryGetCurrent3DRaycastHit(out raycastHit);
            } else {
                return _right.TryGetCurrent3DRaycastHit(out raycastHit);
            }
        }

        public bool TryGetCurrentInterface(Chirality chirality, out RaycastResult raycastResult) {

            if (chirality == Chirality.Left) {
                return _left.TryGetCurrentUIRaycastResult(out raycastResult);
            } else {
                return _right.TryGetCurrentUIRaycastResult(out raycastResult);
            }
        }

        #endregion
    }
}
