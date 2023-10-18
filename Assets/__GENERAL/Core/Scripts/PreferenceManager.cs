using UnityEngine;

namespace HCIG {

public class PreferenceManager : Singleton<PreferenceManager>
{

        #region User

        public string UserName {
            get {
                return PlayerPrefs.GetString(KEY_USER_NAME, "User-VR");
            }
            set {
                PlayerPrefs.SetString(KEY_USER_NAME, value);
            }
        }
        private static readonly string KEY_USER_NAME = "KEY_USER_NAME";

        #endregion User

        #region File Browser

        public string BrowserPathSlides {
            get {
                return PlayerPrefs.GetString(KEY_BROWSER_PATH_SLIDES, Application.dataPath);
            }
            set {
                PlayerPrefs.SetString(KEY_BROWSER_PATH_SLIDES, value);
            }
        }
        private static readonly string KEY_BROWSER_PATH_SLIDES = "BROWSER_PATH_SLIDES";


        public string BrowserPathNotes {
            get {
                return PlayerPrefs.GetString(KEY_BROWSER_PATH_NOTES, Application.dataPath);
            }
            set {
                PlayerPrefs.SetString(KEY_BROWSER_PATH_NOTES, value);
            }
        }
        private static readonly string KEY_BROWSER_PATH_NOTES = "BROWSER_PATH_NOTES";


        #endregion

    }
}
