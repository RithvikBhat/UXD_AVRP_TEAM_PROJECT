using UnityEngine;

namespace HCIG.UI {

    public enum PageType { None = -1, Start, Lobby, Room, Settings, Exit };

    public abstract class Page : MonoBehaviour {

        /// <summary>
        /// returns my parental managing panel
        /// </summary>
        protected Panel Panel {
            get {
                if (_panel == null) {
                    _panel = GetComponentInParent<Panel>(true);
                }
                return _panel;
            }
        }
        private Panel _panel = null;


        public PageType Type {
            get {
                return _type;
            }
        }
        [Header("Page")]
        [SerializeField]
        private PageType _type = PageType.None;

        public void Close() {
            DeactivateBeforeClose();

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Can be overwritten, so we can for a special page clean up some stuff
        /// </summary>
        protected virtual void DeactivateBeforeClose() { }
    }
}
