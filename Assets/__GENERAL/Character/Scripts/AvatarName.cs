using Photon.Pun;

using UnityEngine;

using TMPro;

using HCIG.Network;

namespace HCIG.Avatar {

    public class AvatarName : MonoBehaviour {

        PhotonView _photonView;

        TMP_Text _name;

        private void Awake() {

            _photonView = GetComponentInParent<PhotonView>();
            _name = GetComponentInChildren<TMP_Text>(true);

            if (NetworkManager.Instance.InLobby || _photonView.IsMine) {
                gameObject.SetActive(false);
            } else {
                if ((bool)_photonView.InstantiationData[0]) {
                    // XR
                    _name.text = _photonView.Owner.NickName;
                } else {
                    // PC
                    gameObject.SetActive(false);
                }
            }
        }

        void Update() {
            transform.LookAt(BaseManager.Instance.Camera.transform, Vector3.up);
        }
    }
}
