using HCIG.Input;

using System;

using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace HCIG
{

    public enum OperatingSystem {
        None = -1,
        Vive,
        Pico,
        Meta,
        Varjo,
        OpenXR
    }

    public class DeviceManager : Singleton<DeviceManager>
    {
        public Action<OperatingSystem> OnOperatingSystemIdentified = (_) => { };

        public OperatingSystem System {
            get {
                return _system;
            }
        }
        private OperatingSystem _system = OperatingSystem.None;

        private InputDevice _controllerLeft;
        private InputDevice _controllerRight;

        protected override void Awake() {
            base.Awake();

            InputDevices.deviceConnected += DeviceConnected;
            InputDevices.deviceDisconnected += DeviceDisconnected;

            ApplicationManager.Instance.OnModeChanged += (_) => DetectDevice();
        }

        private void DetectDevice() {

            if(_system != OperatingSystem.None) {
                return;
            }

            if(ApplicationManager.Instance.Mode == Mode.PC) {
                return;
            }

            if (ApplicationManager.Instance.IsAndroid) {

               

                //UnityEngine.Debug.LogWarning("SYSTEM: " + UnityEngine.XR.OpenXR.OpenXRRuntime.name.ToUpper());

                //switch (UnityEngine.XR.OpenXR.OpenXRRuntime.name.ToUpper()) {
                //    case "VIVE WAVE":
                //        _system = OperatingSystem.Vive;
                //        break;

                //    case "PICOVR":
                //    case "PICO":
                //        _system = OperatingSystem.Pico;
                //        break;

                //    case "OCULUS":
                //    case "META":
                //        _system = OperatingSystem.Meta;
                //        break;

                //    default:
                //        _system = OperatingSystem.OpenXR;
                //        break;
                //}

                OnOperatingSystemIdentified.Invoke(_system);
            } else {
                if(XRGeneralSettings.Instance.Manager.activeLoader.name == "VarjoLoader") {
                    OnOperatingSystemIdentified.Invoke(_system = OperatingSystem.Varjo);
                } else {
                    OnOperatingSystemIdentified.Invoke(_system = OperatingSystem.OpenXR);
                }
            }
        }

        private void Update() {
            if (ApplicationManager.Instance.Mode == Mode.PC) {
                return;
            }

            if(!_controllerLeft.isValid && (_controllerLeft = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand)) != default) {
                UpdateOperatingSystem(_controllerLeft);
            } 
            
            if(!_controllerRight.isValid && (_controllerRight = InputDevices.GetDeviceAtXRNode(XRNode.RightHand)) != default) {
                UpdateOperatingSystem(_controllerRight);
            }
        }

        // Detects controllers and updates operating system (if assignable)
        private void DeviceConnected(InputDevice device) {

            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Controller)) {

                if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left)) {
                    _controllerLeft = device;
                } else {
                    _controllerRight = device;
                };

                UpdateOperatingSystem(device);
                return;
            } 
        }

        // kills controller availability when controller deactivated
        private void DeviceDisconnected(InputDevice device) {

            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Controller)) {

                if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left)) {
                    _controllerLeft = default;
                } else {
                    _controllerRight = default;
                };

                return;
            } 
        }

        private void UpdateOperatingSystem(InputDevice device) {

            OperatingSystem system = OperatingSystem.None;

            switch (device.manufacturer.ToUpper()) {
                case "HTC_RR":
                case "HTC":
                case "NA":
                    system = OperatingSystem.Vive;
                    break;

                case "PICOVR":
                case "PICO":
                    system = OperatingSystem.Pico;
                    break;

                case "META":
                case "OCULUS":
                    system = OperatingSystem.Meta;
                    break;
            }

            if(system == OperatingSystem.None) {
                return;
            }

            if (_system != system) {
                _system = system;
                OnOperatingSystemIdentified.Invoke(_system);
            }
        }

        public InputDevice GetController(Chirality chirality) {
            if (chirality == Chirality.Left) {
                return _controllerLeft;
            } else {
                return _controllerRight;
            }
        }
    }
}
