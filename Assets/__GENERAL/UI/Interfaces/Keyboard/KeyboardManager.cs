using System;

using UnityEngine;

using Leap.Unity.Interaction.Keyboard;

namespace HCIG.UI {

    public class KeyboardManager : Singleton<KeyboardManager> {

        public Keyboard Keyboard {
            get {
                return _keyboard;
            }
        }
        private Keyboard _keyboard = null;

        private Vector3 _offset = Vector3.zero;

        private string _text;

        // Handle Parameters                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     a
        private Action<string> _feedbackAction;
        private bool _updateOnEveryChange = false;

        protected override void Awake() {
            base.Awake();

            _keyboard = GetComponentInChildren<Keyboard>(true);

            _offset = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            if (_keyboard != null) {
                _keyboard.OnUpdate += UpdateInput;
            }
        }

        private void Update() {


            if (!_keyboard.gameObject.activeSelf) {
                return;
            }
        }

        private void UpdateInput(byte[] key) {
            string keyDecoded = System.Text.Encoding.UTF8.GetString(key);

            switch (keyDecoded) {
                case "\u0008":
                    // Reverse

                    if (_text.Length > 0) {
                        _keyboard.UpdatePreview(_text = _text.Substring(0, _text.Length - 1));
                    }
                    break;
                case "\n":
                    // Return;

                    Close();
                    break;
                default:
                    // New Char

                    _keyboard.UpdatePreview(_text += keyDecoded);

                    if (_updateOnEveryChange) {
                        _feedbackAction.Invoke(_text);
                    }
                    break;
            }
        }

        /// <summary>
        /// Opens the keyboard and allows us to type down some text 
        /// </summary>
        /// <param name="handle">This is the action that will called, when we are finished with our text</param>
        /// <param name="updateOnEveryChange">When enabled, the action gets called every single time when a change made</param>
        public void Open(Action<string> handle, bool updateOnEveryChange = false) {

            if (_keyboard == null) {
                return;
            }

            if (_keyboard.gameObject.activeSelf) {
                Close();
            }

            Transform head = BaseManager.Instance.Camera.transform;

            transform.rotation = Quaternion.Euler(0, head.eulerAngles.y, 0);
            transform.position = head.position + transform.TransformVector(_offset);

            if (!_keyboard.gameObject.activeSelf) {
                _keyboard.gameObject.SetActive(true);
            } 

            if (!HandMenu.Instance.Suspended) {
                HandMenu.Instance.Suspended = true;
            } 

            _updateOnEveryChange = updateOnEveryChange;

            _keyboard.UpdatePreview(_text = "");
            _feedbackAction = handle;
        }

        /// <summary>
        /// Closes the current board and updates the handle once
        /// </summary>
        public void Close() {

            if (_text != "") {
                _feedbackAction.Invoke(_text);
            }

            if (_keyboard.gameObject.activeSelf) {
                _keyboard.gameObject.SetActive(false);
            }

            if (HandMenu.Instance.Suspended) {
                HandMenu.Instance.Suspended = false;
            }
        }
    }
}
