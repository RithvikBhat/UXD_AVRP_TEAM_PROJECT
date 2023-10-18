using TMPro;

using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HCIG.Network;

namespace HCIG.UI {

    public class Lobby : Page {

        [Header("Prefab")]
        [SerializeField]
        private GameObject _listItemPrefab;

        [Header("Listing")]
        [SerializeField]
        private RectTransform _container;

        [Header("Notification")]
        [SerializeField]
        private TMP_Text _message;

        [Header("Creation")]
        [SerializeField]
        private Button _create;

        [SerializeField]
        private TMP_InputField _inputfield;

        private void Awake() {

            // Events
            NetworkManager.Instance.OnRoomListUpdated += (_) => {
                if (!isActiveAndEnabled) {
                    return;
                }

                // clear old list 
                ClearList();

                // and update with new one
                CreateList();
            };

            NetworkManager.Instance.OnRoomJoined += () => {
                if (isActiveAndEnabled) {
                    Panel.Open(PageType.Room);
                }
            };
        }

        #region List

        private void OnEnable() {

            // clear old list 
            ClearList();

            // and update with new one
            CreateList();

            if (_inputfield != null) {
                _inputfield.text = "";
            }

            if(_message != null) {
                _message.enabled = false;
            }

        }

        /// <summary>
        /// destroys the current list
        /// -> ANDROID: we dont have the create section
        /// -> DESKTOP: we always hold create section alive 
        /// </summary>
        private void ClearList() {

            int defChilds = /*ApplicationManager.Instance.Mode == Mode.XR ?*/ 0 /*: 1*/;

            while (_container.childCount > defChilds) {

                DeleteContainerChild();
            }

            _container.sizeDelta = new Vector2(_container.rect.width, defChilds * 80);
        }

        /// <summary>
        /// creats a new list with available rooms
        /// </summary>
        private void CreateList() {

            List<Photon.Realtime.RoomInfo> roomInfos = NetworkManager.Instance.RoomList;

            if (ApplicationManager.Instance.Mode == Mode.XR || ApplicationManager.Instance.IsEditor) {

                // first create always accessible rooms for SA devices
                foreach (Scene scene in System.Enum.GetValues(typeof(Scene))) {

                    if ((int)scene <= 0) {
                        // we dont want the special environments
                        continue;
                    }

                    if (roomInfos.Any(room => scene.ToString().Contains(room.Name))) {
                        // we have already the predefined environment in our lobby list
                        continue;
                    }

                    // Instantiate list of available rooms
                    RoomListItem listItem = Instantiate(_listItemPrefab, _container).GetComponent<RoomListItem>();

                    listItem.Initialize(scene, 0, 20, EnvironmentManager.Instance.LoadEnvironment);
                    listItem.transform.SetAsFirstSibling();
                }
            }


            // instantiate currently opened online rooms 
            foreach (Photon.Realtime.RoomInfo roominfo in NetworkManager.Instance.RoomList) {

                // Instantiate list of available rooms
                RoomListItem listItem = Instantiate(_listItemPrefab, _container).GetComponent<RoomListItem>();

                Scene selection = Scene.None;

                // Check if it is a specified scene 
                foreach (Scene scene in System.Enum.GetValues(typeof(Scene))) {
                    if (scene.ToString().Contains(roominfo.Name.Trim())){
                        selection = scene;
                        break;
                    }
                }

                if (selection != Scene.None) {
                    // Specified Course
                    listItem.Initialize(selection, roominfo.PlayerCount, roominfo.MaxPlayers, EnvironmentManager.Instance.LoadEnvironment);
                } else {
                    // Custom Course
                    //listItem.Initialize(roominfo.Name, roominfo.PlayerCount, roominfo.MaxPlayers, EnvironmentManager.Instance.LoadEnvironment);
                }

                listItem.transform.SetAsFirstSibling();
            }

            _container.sizeDelta = new Vector2(0, _container.childCount * _container.sizeDelta.y);
        }

        /// <summary>
        /// Destroys the first child in our container list
        /// </summary>
        private void DeleteContainerChild() {
            Transform child = _container.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        #endregion List

        #region Message

        private void ShowMessage(string message, float cycle = 1f, int laps = 3) {
            StopAllCoroutines();

            StartCoroutine(MessageRoutine(message, cycle, laps));
        }

        /// <summary>
        /// Shows the message in a blinking style for a set duration
        /// </summary>
        /// <param name="message"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator MessageRoutine(string message, float cycle, int laps) {

            _message.text = message;

            Color color = _message.color;

            _message.enabled = true;

            float start = Time.realtimeSinceStartup;
            bool flank = false;

            while (laps > 0) {

                float time = (Time.realtimeSinceStartup - start) % cycle;

                if (time < cycle / 2) {
                    // increase
                    color.a = time * 2 / cycle;

                    if (flank) {
                        flank = false;
                        laps--;
                    }
                } else {
                    // decrease 
                    color.a = 1 - (time * 2 - cycle) / cycle;

                    if (!flank) {
                        flank = true;
                    }
                }

                _message.color = color;

                yield return null;
            }

            _message.enabled = false;
        }

        #endregion Message
    }
}


