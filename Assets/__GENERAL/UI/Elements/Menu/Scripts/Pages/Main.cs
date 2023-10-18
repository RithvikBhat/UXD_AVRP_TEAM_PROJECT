using UnityEngine;

using Leap.Unity.Interaction;

using HCIG.Teleport;

using System.Collections.Generic;

namespace HCIG.UI {

    /// <summary>
    /// is the main page for the (HAND-Menu). Handles all other sub-pages
    /// </summary>
    public class Main : Page {

        [Header("Components")]
        [SerializeField]
        private InteractionToggle _tele;
        [SerializeField]
        private InteractionToggle _room;
        [SerializeField]
        private InteractionToggle _settings;
        [SerializeField]
        private InteractionToggle _exit;

        private List<InteractionToggle> _toggles = new List<InteractionToggle>();

        private void Awake() {

            // Add References
            foreach(InteractionToggle toggle in GetComponentsInChildren<InteractionToggle>(true)) {

                toggle.OnToggle += () => Untoggle(toggle);

                _toggles.Add(toggle);
            }

            _tele.OnToggle += () => TeleportManager.Instance.Activated = true;
            _tele.OnUntoggle += () => TeleportManager.Instance.Activated = false;

            _room.OnToggle += () => Panel.Open(PageType.Room);
            _room.OnUntoggle += () => Panel.Close();

            _settings.OnToggle += () => Panel.Open(PageType.Settings);
            _settings.OnUntoggle += () => Panel.Close();     

            _exit.OnToggle += () => Panel.Open(PageType.Exit);
            _exit.OnUntoggle += () => Panel.Close();
        }

        private void Untoggle(InteractionToggle caller = null) {

            foreach(InteractionToggle toggle in _toggles) {

                if(toggle == caller) {
                    continue;
                }

                if (toggle.isToggled) {
                    toggle.isToggled = false;
                }
            }
        }
    }
}

