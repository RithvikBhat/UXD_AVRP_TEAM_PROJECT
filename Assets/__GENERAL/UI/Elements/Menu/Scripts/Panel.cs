using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HCIG.UI {

    public class Panel : MonoBehaviour {

        /// <summary>
        /// Returns the current active page
        /// </summary>
        public PageType Page {
            get {
                return _currentPage;
            }
        }
        private PageType _currentPage = PageType.None;

        private Dictionary<PageType, Page> _pages = new Dictionary<PageType, Page>();

        private bool _initialized = false;
        private Coroutine _coroutine = null;


        private void Initialize() {

            // collect all pages of our menu
            foreach (Page page in GetComponentsInChildren<Page>(true)) {

                if (page.Type == PageType.None) {
                    Debug.LogWarning("We have a unallocated page here - " + page.name);
                    continue;
                }

                if (_pages.ContainsKey(page.Type)) {
                    Debug.LogWarning("We have a duplicated page here - " + page.name);
                }

                page.gameObject.SetActive(false);

                _pages.Add(page.Type, page);
            }

            _initialized = true;
        }

        /// <summary>
        /// Activates the desired page if created in the panel
        /// </summary>
        /// <param name="type"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Open(PageType type, float time = 0) {

            if (!_initialized) {
                Initialize();
            }

            if (!_pages.ContainsKey(type)) {
                return false;
            }

            if (_currentPage == type) {
                return false;
            }

            // close current page
            if (_currentPage != PageType.None) {
                Close();
            }

            // open our panel
            if (!gameObject.activeInHierarchy) {
                gameObject.SetActive(true);
            }

            // open new page
            _currentPage = type;
            _pages[_currentPage].gameObject.SetActive(true);


            if(time != 0) {
                _coroutine = StartCoroutine(CloseAfterTime(time));
            }

            return true;
        }

        /// <summary>
        /// Deactivates the current page
        /// </summary>
        /// <returns></returns>
        public bool Close() {

            if (_currentPage == PageType.None) {
                return false;
            }

            // Stop coroutine
            if(_coroutine != null) {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            // close current page
            _pages[_currentPage].Close();

            // close our panel
            if (gameObject.activeInHierarchy) {
                gameObject.SetActive(false);
            }

            _currentPage = PageType.None;
            return true;
        }

        /// <summary>
        /// If we had a routine running, we stop it 
        /// </summary>
        private void OnDisable() {
            StopAllCoroutines();
        }

        /// <summary>
        /// The timed close routine
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private IEnumerator CloseAfterTime(float time) {

            yield return new WaitForSeconds(time);

            Close();
        }
    }
}
