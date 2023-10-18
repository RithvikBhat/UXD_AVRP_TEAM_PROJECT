using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace HCIG.Network {

    public class NetworkStatus : MonoBehaviour {

        private void Start() {
            if (!ApplicationManager.Instance.IsEditor) {
                gameObject.SetActive(false);
            }
        }

        void OnGUI() {
            if (!PhotonNetwork.InRoom) {
                return;
            }

            string status = "Photon-Version: " + PhotonNetwork.GameVersion + "\n";

            status += "-------------------------------------------------------\n";
            status += "VR Raum:         " + (PhotonNetwork.OfflineMode ? "OFFLINE" : PhotonNetwork.CurrentRoom.Name) + "\n";
            status += "-------------------------------------------------------\n";

            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++) {
                Player player = PhotonNetwork.PlayerList[i];

                status += "Client-Name:     " + (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber ? "MYSELF" : player.NickName) + "\n";
                status += "Client-Number:  " + player.ActorNumber + "\n";
                status += "Client-State:      " + (player.IsMasterClient ? "Master" : "Joiner") + "\n";

                if (i != PhotonNetwork.CurrentRoom.PlayerCount - 1) {
                    status += "- - - - - - - - - - - - - - - - - - - - - - - - - - - -\n";
                }
            }

            status += "-------------------------------------------------------\n";
            status += "Player-Count:    " + PhotonNetwork.CurrentRoom.PlayerCount + "\n";
            status += "-------------------------------------------------------\n";

            GUI.TextField(new Rect(10, 10, 210, 155 + (PhotonNetwork.CurrentRoom.PlayerCount - 1) * 60), status);
        }
    }
}
