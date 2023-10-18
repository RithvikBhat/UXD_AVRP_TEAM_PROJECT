using Leap.Unity.Interaction;

using System.Collections.Generic;

using HCIG.UI;

public class ExampleHandPage : SubPage {

    private List<InteractionToggle> _toggles = new List<InteractionToggle>();

    private void Awake() {

        foreach (InteractionToggle toggle in GetComponentsInChildren<InteractionToggle>(true)) {

            toggle.OnToggle += () => UntoggleAllOtherToggles(toggle);

            _toggles.Add(toggle);
        }
    }

    /// <summary>
    /// Deactivates every other Toggle in the Hand
    /// </summary>
    /// <param name="caller"></param>
    private void UntoggleAllOtherToggles(InteractionToggle caller = null) {

        foreach (InteractionToggle toggle in _toggles) {

            if (toggle == caller) {
                // skips the caller toggle
                continue;
            }

            toggle.Untoggle();
        }
    }
}