using Leap.Unity.Interaction;
using HCIG.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HCIG.UI {

    public class Settings : Page {

        [Header("Name")]
        [SerializeField]
        private TMP_InputField _inputName;
        [SerializeField]
        private GameObject _buttonName;


        private void Awake() {

            // Button
            if (ApplicationManager.Instance.Mode == Mode.XR) {
                _buttonName.GetComponentInChildren<InteractionButton>(true).OnPress += () => KeyboardManager.Instance.Open((text) => UpdateUserName(text));
            } else {
                _buttonName.GetComponentInChildren<Button>(true).onClick.AddListener(() => KeyboardManager.Instance.Open((text) => UpdateUserName(text)));
            }
        }

        private void Start() {
            UpdateUserName();
        }

        /// <summary>
        /// Updates the name in the settings and the visual component in one function
        /// </summary>
        /// <param name="name"></param>
        private void UpdateUserName(string name = "") {

            // Set new name
            if(name != "") {
                PreferenceManager.Instance.UserName = name;
            }

            // Get saved name
            _inputName.text = PreferenceManager.Instance.UserName;
        }
    }
}
