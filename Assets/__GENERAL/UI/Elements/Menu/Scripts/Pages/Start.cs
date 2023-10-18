using TMPro;

using UnityEngine;

namespace HCIG.UI {

    public class Start : Page {

        [Header("Components")]
        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _version;

        private void Awake() {

            _name.text = Application.productName;
            _version.text = Application.version;
        }
    }
}
