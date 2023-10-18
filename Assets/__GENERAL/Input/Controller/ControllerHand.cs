using UnityEngine;
using UnityEngine.XR;

namespace HCIG.Input.Data {

    public class ControllerHand : Hand {

        [Header("Controller")]
        [SerializeField]
        private Animator _animator;

        private float _grabValue;

        [SerializeField]
        private float _pinchDetection = 0.95f;

        private bool _isValid = false;

        private void OffsetController() {

            OperatingSystem system = DeviceManager.Instance.System;

            Vector3 position = new Vector3(0, 0.01f, 0.02f);
            Vector3 rotation = new Vector3(10f, 0, 0);

            switch (system) {
                case OperatingSystem.Vive:

                    if (ApplicationManager.Instance.IsEditor) {
                        position = new Vector3(0.02f, 0.01f, -0.09f);
                    } 
                    break;

                case OperatingSystem.Pico:
                    position = new Vector3(-0.02f, -0.03f, 0.01f);
                    break;

                case OperatingSystem.Meta:

                    break;

            }

            _animator.transform.localPosition = position;
            _animator.transform.localEulerAngles = rotation;
        }

        private void Update() {

            InputDevice controller = DeviceManager.Instance.GetController(Chirality);

            if (!controller.isValid) {
                _isValid = false;
                return;
            }

            if (!_isValid) {
                _isValid = true;

                OffsetController();
            }

            // Trigger Button - Grab
            controller.TryGetFeatureValue(CommonUsages.trigger, out _grabValue);
            _animator.SetFloat("GrabValue", _grabValue);

            // Grip Button - Pick
            controller.TryGetFeatureValue(CommonUsages.grip, out float pickValue);
            _animator.SetFloat("PickValue", pickValue);
        }

        public override bool IsValid {
            get {
                return _isValid;
            }
        }

        public override bool IsPinching {
            get {
                return _isValid && _grabValue > _pinchDetection;
            }
        }
    }
}
