using TMPro;

using UnityEngine;

namespace HCIG.UI {

    public class DefaultPage : SubPage {

        [Header("Components")]
        [SerializeField]
        private TMP_Text _message;

        [SerializeField]
        private GameObject _leave;

        private void Awake() {

            if (_leave != null) {
                AddContentToButton(_leave, LeaveCourse);
            }
        }

        private void OnEnable() {

            if (_leave != null) {
                if (Application.internetReachability != NetworkReachability.NotReachable) {
                    _leave.SetActive(true);
                } else {
                    _leave.SetActive(false);
                }
            }
        }
    }
}