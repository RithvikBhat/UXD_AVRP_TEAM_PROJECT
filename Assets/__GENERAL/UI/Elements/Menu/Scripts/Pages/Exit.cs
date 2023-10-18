using Leap.Unity.Interaction;
using HCIG.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HCIG.UI {

    public class Exit : Page {

        [Header("Components")]
        [SerializeField]
        private TMP_Text _message;
        [SerializeField]
        private GameObject _exit;


        private void Awake() {

            // Message
            EnvironmentManager.Instance.OnSwitchedEnvironment += (_) => UpdateMessage();

            // Button
            if (ApplicationManager.Instance.Mode == Mode.XR) {
                _exit.GetComponentInChildren<InteractionButton>(true).OnPress += LeaveEnvironment;
            } else {
                _exit.GetComponentInChildren<Button>(true).onClick.AddListener(LeaveEnvironment);
            }
        }

        private void Start() {
            UpdateMessage();
        }

        private void LeaveEnvironment() {
            if (ApplicationManager.Instance.Mode == Mode.XR) {
                if (EnvironmentManager.Instance.Scene == Scene.Lobby) {
                    ApplicationManager.Instance.ExitApplication();
                } else {
                    EnvironmentManager.Instance.LoadEnvironment(Scene.Lobby);
                }
            } else {
                ApplicationManager.Instance.ExitApplication();
            }

        }

        private void UpdateMessage() {
            if (ApplicationManager.Instance.Mode == Mode.XR) {
                if (EnvironmentManager.Instance.Scene == Scene.Lobby) {
                    _message.text = "Do you want to leave the Application?";
                } else {
                    _message.text = "Do you want to leave the current room?";
                }
            } else {
                _message.text = "Do you want to leave the Application?";
            }
        }
    }
}
