using TMPro;

using System.IO;

using UnityEngine;
using UnityEngine.UI;

namespace HCIG.UI {

    public class FileItem : MonoBehaviour {

        [Header("Components")]
        [SerializeField]
        private TMP_Text _path;
        [SerializeField]
        private Button _kill;

        public void Initialize(string path, System.Action<string> selectionFeedback) {

            _path.text = Path.GetFileName(path);

            _kill.onClick.AddListener(() => {

                // give feedback...
                selectionFeedback.Invoke(path);

                // ...and kill yourself
                transform.parent = null;
                Destroy(gameObject);
            });
        }
    }
}

