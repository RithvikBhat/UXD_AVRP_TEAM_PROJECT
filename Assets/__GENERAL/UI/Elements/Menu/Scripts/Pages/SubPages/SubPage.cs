using UnityEngine;
using UnityEngine.UI;

using System;
using Leap.Unity.Interaction;

namespace HCIG.UI {

    public abstract class SubPage : MonoBehaviour {

        public Scene Type {
            get {
                return _type;
            }
        }
        [Header("Course")]
        [SerializeField]
        protected Scene _type = Scene.None;

        protected void LeaveCourse() {
            EnvironmentManager.Instance.LoadEnvironment(Scene.Lobby);
        }

        protected void AddContentToButton(GameObject interactable, Action content) {
            if (ApplicationManager.Instance.Mode == Mode.XR) {
                interactable.GetComponentInChildren<InteractionButton>(true).OnPress += content.Invoke;
            } else {
                interactable.GetComponentInChildren<Button>(true).onClick.AddListener(content.Invoke);
            }
        }

        /// <summary>
        /// Untoggles every activated toggle, so we don't have any function remaining opened
        /// </summary>
        public virtual void DeactivateBeforeClose() {

            if (ApplicationManager.Instance.Mode == Mode.XR) {
                foreach (InteractionToggle toggle in GetComponentsInChildren<InteractionToggle>(true)) {
                    if (toggle.isToggled) {
                        toggle.Untoggle();
                    }
                }
            } else {
                foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true)) {
                    if (toggle.isOn) {
                        toggle.isOn = false;
                    }
                }
            }
        }
    }
}