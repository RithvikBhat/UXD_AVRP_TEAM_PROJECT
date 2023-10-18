using Photon.Pun;
using Photon.Realtime;

using System.Collections.Generic;

using UnityEngine;

namespace HCIG.Network {

    public class PlayerManager : Singleton<PlayerManager> {

        [Header("Instantiation")]
        [SerializeField]
        private GameObject _avatar;

        private GameObject _local = null;

        public int MyActorNumber {
            get {
                if (NetworkManager.Instance.InRoom) {
                    return PhotonNetwork.LocalPlayer.ActorNumber;
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// other network users, who are in the same room 
        /// </summary>
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();

        protected override void Awake() {
            base.Awake();

            // myself
            NetworkManager.Instance.OnRoomJoined += InstantiateAvatar;
            NetworkManager.Instance.OnLobbyJoined += InstantiateAvatar;
            NetworkManager.Instance.OnRoomLeft += _players.Clear;

            // others
            NetworkManager.Instance.OnPlayerJoined += AppendPlayer;
            NetworkManager.Instance.OnPlayerUpdated += UpdatePlayer;
            NetworkManager.Instance.OnPlayerLeft += RemovePlayer;
        }

        /// <summary>
        /// Instantiates our avatar model when we join the lobby or a room
        /// </summary>
        private void InstantiateAvatar() {
            if(_local != null) {
                Destroy(_local);
            }

            if (NetworkManager.Instance.InLobby) {
                _local = Instantiate(_avatar, Vector3.zero, Quaternion.identity);
            } else {
                _local = PhotonNetwork.Instantiate(_avatar.name, Vector3.zero, Quaternion.identity, 0, new object[] { ApplicationManager.Instance.Mode == Mode.XR });
            }
        }

        /// <summary>
        /// new player joined
        /// </summary>
        /// <param name="player"></param>
        private void AppendPlayer(Player player) {
            _players.Add(player.ActorNumber, player);
        }

        /// <summary>
        /// This player is now master
        /// </summary>
        /// <param name="player"></param>
        private void UpdatePlayer(Player player) {
            _players[player.ActorNumber] = player;
        }

        /// <summary>
        /// This player left
        /// </summary>
        /// <param name="player"></param>
        private void RemovePlayer(Player player) {
            _players.Remove(player.ActorNumber);
        }


        /// <summary>
        /// Returns the requested player if available, instead it returns NULL 
        /// </summary>
        /// <param name="actorNumber"></param>
        /// <returns></returns>
        public Player GetPlayer(int actorNumber) {
            if (!_players.ContainsKey(actorNumber)){
                return null;
            }

            return _players[actorNumber];
        }
    }
}
