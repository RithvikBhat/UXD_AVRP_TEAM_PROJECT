using UnityEngine.SceneManagement;

using System;

using HCIG.VisualEffects;
using HCIG.Network;
using UnityEngine;

namespace HCIG {

    /// <summary>
    /// all NEGATIV Environments are special environments and the POSITIV are predefined (environment = session) ones
    /// </summary>
    public enum Scene {

        Lobby = -1,
        None = 0,

        CHANGE_NAME_OF_THIS = 1,    // The AVRP-Student Scene -> has to be named the same way like your scene in the "__STUDENTS"-Folder, so the system can correctly load the scene when selected

        Example = 2,
    }

    public class EnvironmentManager : Singleton<EnvironmentManager> {

        public Action<Scene> OnSwitchedEnvironment = (_) => { };

        /// <summary>
        /// this is the current loaded scene
        /// </summary>
        public Scene Scene {
            get {
                return _scene;
            }
            private set {
                Debug.LogWarning("SCEEN: " + value.ToString());
                OnSwitchedEnvironment.Invoke(_scene = value);
            }
        }
        private Scene _scene = (Scene)(-1);

        /// <summary>
        /// To this scene we want to switch
        /// </summary>
        private Scene _change = Scene.None;

        // DEBUG
        [Header("Debug")]
        [Tooltip("Just for Debugging situations during Editor-Mode")]
        [SerializeField]
        private bool _JumpToInitialScene = false;
        // DEBUG


        private string _session = "";

        protected override void Awake() {
            base.Awake();

            // Events
            ScreenFadeManager.Instance.OnFadeState += PerformSceneTransition;

            NetworkManager.Instance.OnLobbyJoined += () => ScreenFadeManager.Instance.FadeIn();
            NetworkManager.Instance.OnRoomJoined += () => ScreenFadeManager.Instance.FadeIn();

            NetworkManager.Instance.OnRoomJoinFailed += (_, _) => EnterLobby();

        }

        /// <summary>
        /// Loads the scene, we have set in the "_scene" value for start up
        /// </summary>
        private void Start() {
            if (ApplicationManager.Instance.IsEditor && !_JumpToInitialScene) {

                for (int i = 0; i < SceneManager.loadedSceneCount; i++) {

                    string name = SceneManager.GetSceneAt(i).name;

                    Debug.Log("SCENE - " + name);

                    if (name == Scene.CHANGE_NAME_OF_THIS.ToString()) {
                        Debug.Log(1);
                        Scene = Scene.CHANGE_NAME_OF_THIS;
                        return;
                    }

                    if (name == Scene.Lobby.ToString()) {
                        Debug.Log(2);
                        Scene = Scene.Lobby;
                        return;
                    }

                    if (name == Scene.Example.ToString()) {
                        Debug.Log(3);
                        Scene = Scene.Example;
                        return;
                    }
                }

                Scene = Scene.None;
            } else {
                EnterLobby();
            }
        }

        /// <summary>
        /// Enter lobby by start, or when a join process failed
        /// </summary>
        private void EnterLobby() {
            _change = Scene.Lobby;

            PerformSceneTransition(ScreenFadeManager.Instance.IsActive);
        }

        /// <summary>
        /// Enter one of our predefined contents
        /// </summary>
        /// <param name="scene"></param>
        public void LoadEnvironment(Scene scene) {
            if (_change != Scene.None) {
                return;
            }

            _change = scene;

            ScreenFadeManager.Instance.FadeOut();
        }

        private void PerformSceneTransition(bool state) {

            if (!state) {
                return;
            }

            if (_change == Scene.None) {
                return;
            }

            // Load Scene
            SceneManager.LoadScene(_change.ToString(), LoadSceneMode.Single);

            // Enter Room
            if (_session != "") {
                // dynamic course session

                NetworkManager.Instance.EnterOnlineRoom(_session);

                _session = "";
            } else {
                // predefined session

                switch (_change) {
                    case Scene.Lobby:
                        NetworkManager.Instance.EnterLobby();
                        break;
                    default: // Lecture
                        NetworkManager.Instance.EnterOnlineRoom(_change.ToString());
                        break;
                }
            }

            // transition finished
            Scene = _change;

            // reset "_change" for next switch
            _change = Scene.None;
        }
    }
}