using Photon.Pun;
using Photon.Voice.PUN;

using UnityEngine;

using HCIG.Network;
using Photon.Voice.Unity;

namespace HCIG.Avatar {

    public class AvatarAudio : MonoBehaviour {

        PhotonView _photonView;
        PhotonVoiceView _voiceView;

        [Header("Visual")]
        [SerializeField]
        private GameObject _symbol;

        private void Awake() {

            _photonView = GetComponentInParent<PhotonView>();
            _voiceView = GetComponentInParent<PhotonVoiceView>();

            if (NetworkManager.Instance.InLobby || _photonView.IsMine) {
                gameObject.SetActive(false);
            } else {
                _symbol.SetActive(false);
            }
        }

        void Update() {
            // Rotation
            transform.LookAt(BaseManager.Instance.Camera.transform, Vector3.up);

            if (_voiceView.IsSpeaking) {
                if (!_symbol.activeSelf) {
                    _symbol.SetActive(true);
                }
            } else {
                if (_symbol.activeSelf) {
                    _symbol.SetActive(false);
                }
            }
        }
    }
}
