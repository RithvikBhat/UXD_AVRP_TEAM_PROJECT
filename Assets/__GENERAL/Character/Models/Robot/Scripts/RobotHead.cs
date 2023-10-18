using HCIG.Network;
using Photon.Pun;

using UnityEngine;

namespace HCIG.Avatar {

    public class RobotHead : MonoBehaviour {

        [Header("Offsets")]
        [SerializeField]
        private Vector3 _rotation;

        PhotonView _photonView;

        private void Awake() {
            _photonView = GetComponentInParent<PhotonView>();

            if(NetworkManager.Instance.InLobby || _photonView.IsMine) {
                transform.localPosition = new Vector3(-0.19f, -0.135f, 0);
                transform.localEulerAngles = new Vector3(0, 0, -50);
            }
        }

        private void FixedUpdate() {
            if(NetworkManager.Instance.InLobby || _photonView.IsMine) {
                return;
            }

            transform.eulerAngles = new Vector3(_photonView.transform.eulerAngles.z, _photonView.transform.eulerAngles.y, -_photonView.transform.eulerAngles.x) + _rotation;
        }
    }
}
