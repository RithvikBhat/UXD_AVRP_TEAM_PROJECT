using System;
using UnityEngine;

namespace HCIG.Input.Data {

    public enum InputType { None = -1, Controllers, Realistic }

    public class InputDataManager : Singleton<InputDataManager> {

        public Action<InputType> OnInputTypeChanged = (_) => { };

        public Action<Chirality> OnPinchTriggered = (_) => { };

        public bool IsDataAvailable {
            get {
                if (_currentData == null) {
                    return false;
                }

                return _currentData.HandCount > 0;
            }
        }
        private InputData _currentData = null;

        [Header("Data - Sources")]
        [SerializeField, Tooltip("Hand data, that got actually tracked by a ultraleap sensor (especially used on Varjo XR3")]
        private InputData _leapData;
        [SerializeField, Tooltip("Hand data, that got actually tracked by the internal tracking systems (Focus 3, Quest 2, etc.)")]
        private InputData _handData;
        [SerializeField, Tooltip("Hand data, that got simulated by the controller inputs and an hand animator.")]
        private InputData _contData;

        public InputType InputType {
            get {
                return _currentType;
            }
            private set {
                if (_currentType == value) {
                    return;
                }

                OnInputTypeChanged(_currentType = value);
            }
        }
        private InputType _currentType = InputType.None;

        private bool _lastPinchLeft = false;
        private bool _lastPinchRight = false;

        #region Data - Management

        private void FixedUpdate() {

            // Select the currently relevant/ accessible data
            if (!SourceDataSelection()) {
                return;
            }

            // Check Pinch-States
            if (TryGetHand(Chirality.Left, out Hand hand)) {

                bool value = hand.IsPinching;

                if (value && !_lastPinchLeft) {
                    OnPinchTriggered.Invoke(Chirality.Left);
                }

                _lastPinchLeft = value;
            } else {
                _lastPinchLeft = false;
            }

            if (TryGetHand(Chirality.Right, out hand)) {

                bool value = hand.IsPinching;

                if (value && !_lastPinchRight) {
                    OnPinchTriggered.Invoke(Chirality.Right);
                }

                _lastPinchRight = value;
            } else {
                _lastPinchRight = false;
            }
        }

        /// <summary>
        /// Checks the input data and sets the first available dataset as the current one to use after a specified sequence.
        /// </summary>
        /// <returns>Returns true or false dependent on if we got data or not.</returns>
        private bool SourceDataSelection() {

            if (_leapData.HandCount > 0) {
                _currentData = _leapData;
                InputType = InputType.Realistic;
                return true;
            }

            if (_handData.HandCount > 0) {
                _currentData = _handData;
                InputType = InputType.Realistic;
                return true;
            }

            if (_contData.HandCount > 0) {
                _currentData = _contData;
                InputType = InputType.Controllers;
                return true;
            }

            if (InputType != InputType.None) {

                _lastPinchLeft = false;
                _lastPinchRight = false;

                _currentData = null;
                InputType = InputType.None;
            }
            return false;
        }

        #endregion Data - Management

        public Hand GetHand(Chirality chirality) {
            if (_currentData == null) {
                return new VirtualHand();
            }

            return _currentData.GetHand(chirality);
        }

        public bool TryGetHand(Chirality chirality, out Hand hand) {

            hand = GetHand(chirality);

            return hand.IsValid;
        }

        public bool IsHandAvailable(Chirality chirality) {

            return GetHand(chirality).IsValid;
        }

        public bool IsHandPinching(Chirality chirality) {
            Hand hand = GetHand(chirality);

            if (!hand.IsValid) {
                return false;
            }

            return hand.IsPinching;
        }
    }
}
