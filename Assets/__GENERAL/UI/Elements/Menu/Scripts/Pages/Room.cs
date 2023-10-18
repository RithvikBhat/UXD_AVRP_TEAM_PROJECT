using System.Linq;

using HCIG.Network;

namespace HCIG.UI {

    public class Room : Page {

        SubPage _currentPage = null;

        private void Awake() {

            NetworkManager.Instance.OnLobbyJoined += () => {
                if (isActiveAndEnabled) {
                    Panel.Open(PageType.Lobby);
                }
            };

            EnvironmentManager.Instance.OnSwitchedEnvironment += (_) => UpdateCoursePage();
        }

        /// <summary>
        /// opens the for the current environment relevant page, so we have for each room individual hand UI
        /// </summary>
        /// <param name="scene"></param>
        private void OnEnable() {
            UpdateCoursePage();
        }

        private void UpdateCoursePage() {

            if (!isActiveAndEnabled) {
                return;
            }

            _currentPage = null;
            SubPage[] scenePages = GetComponentsInChildren<SubPage>(true);

            foreach (SubPage scenePage in scenePages) {
                // find correct page + close all other opened
                if (scenePage.Type == EnvironmentManager.Instance.Scene) {
                    _currentPage = scenePage;
                } else {
                    scenePage.gameObject.SetActive(false);
                }
            }

            if (_currentPage == null) {
                // find default page
                _currentPage = scenePages.First(scenePage => scenePage.Type == Scene.None);
            }

            if(_currentPage != null) {
                _currentPage.gameObject.SetActive(true);
            }
        }

        protected override void DeactivateBeforeClose() {
            if(_currentPage != null) {
                _currentPage.DeactivateBeforeClose();
            }
        }
    }
}
