using UnityEngine;

using Photon.Pun;
using Photon.Voice.PUN;

using HCIG.Network;

using System.Collections.Generic;
using System;

namespace HCIG.Avatar {

    public class Avatar : MonoBehaviour, IPunObservable {

        PhotonView _photonView;

        [Serializable]
        private struct KeyValuePair {
            public Scene Scene;
            public GameObject Avatar;
        }

        [SerializeField]
        private List<KeyValuePair> _characterBinding;

        private GameObject _currentCharacter = null;

        private void Awake() {
            _photonView = GetComponent<PhotonView>();

            InitializeAvatar(EnvironmentManager.Instance.Scene);

            if (NetworkManager.Instance.InLobby) {
                //Local

                if (ApplicationManager.Instance.Mode == Mode.PC) {
                    // PC
                    _currentCharacter.SetActive(false);
                }
            } else {
                // Network

                if (!(bool)_photonView.InstantiationData[0]) {
                    // PC
                    _currentCharacter.SetActive(false);
                }
            }

            transform.SetParent(PlayerManager.Instance.transform);

            // Collect all observable components for the PhotonView
            _photonView.ObservedComponents.Clear();
            _photonView.FindObservables(true);

            if (_photonView.IsMine || NetworkManager.Instance.InLobby) {
                
                // Voice 
                GetComponent<PhotonVoiceView>().enabled = false;

                // Character
                ApplicationManager.Instance.OnModeChanged += SetCharacterActive;
            }
        }

        private void OnDestroy() {
            ApplicationManager.Instance.OnModeChanged -= SetCharacterActive;
        }


        private void InitializeAvatar(Scene scene) {

            if (_currentCharacter != null) {
                Destroy(_currentCharacter);
                _currentCharacter = null;
            }

            GameObject newAvatar = null;

            foreach (KeyValuePair pair in _characterBinding) {

                if (pair.Avatar != null) {

                    if (pair.Scene == scene) {
                        newAvatar = pair.Avatar;
                        break;
                    }

                    if (pair.Scene == Scene.None && newAvatar == null) {
                        newAvatar = pair.Avatar;
                        continue;
                    }
                }
            }

            if (newAvatar != null) {
                _currentCharacter = Instantiate(newAvatar, transform);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

            if (stream.IsWriting) {
                // Sender

                stream.SendNext(transform.position);
                stream.SendNext(transform.eulerAngles);

            } else {
                // Receiver

                transform.position = (Vector3)stream.ReceiveNext();
                transform.eulerAngles = (Vector3)stream.ReceiveNext();
            }
        }

        private void FixedUpdate() {
            if (!NetworkManager.Instance.InLobby && !_photonView.IsMine) {
                return;
            }

            transform.SetPositionAndRotation(BaseManager.Instance.Camera.transform.position, BaseManager.Instance.Camera.transform.rotation);
        }

        #region Character

        private void SetCharacterActive(Mode mode) {

            if (NetworkManager.Instance.InLobby) {
                SetCharacterActiveRPC(mode == Mode.XR);
            } else {
                _photonView.RPC(nameof(SetCharacterActiveRPC), RpcTarget.All, mode == Mode.XR);
            }
        }

        [PunRPC]
        private void SetCharacterActiveRPC(bool active) {

            if (_currentCharacter != null) {
                _currentCharacter.SetActive(active);
            }
        }

        #endregion
    }
}
