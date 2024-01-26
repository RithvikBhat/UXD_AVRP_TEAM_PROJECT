using Photon.Pun;
using Photon.Realtime;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace HCIG.Network {

    /// <summary>
    /// The NetworkManager handles connection to the master server, creation and joining of rooms.
    /// Additionally it handles network instantiation of player prefabs after joining,
    /// as well as runtime instantiation of networked systems (such as Tools instantiated by the Toolbar)
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>, IMatchmakingCallbacks, IConnectionCallbacks, ILobbyCallbacks, IInRoomCallbacks {

        // Lobby
        public Action OnLobbyJoined = delegate { };
        public Action OnLobbyJoinFailed = delegate { };
        public Action OnLobbyLeft = delegate { };

        public Action<List<RoomInfo>> OnRoomListUpdated = delegate { };

        // Room
        public Action OnRoomCreated = delegate { };
        public Action<short, string> OnRoomCreationFailed = delegate { };

        public Action OnRoomJoined = delegate { };
        public Action<short, string> OnRoomJoinFailed = delegate { };

        public Action OnRoomLeft = delegate { };

        // Connection
        public Action OnConnect = delegate { };
        public Action<DisconnectCause> OnDisconnect = delegate { };

        // Players
        public Action<Player> OnPlayerJoined = delegate { };
        public Action<Player> OnPlayerUpdated = delegate { };
        public Action<Player> OnPlayerLeft = delegate { };


        // Reconnection
        private string _roomState = "";
        private bool _isSwitching = false;

        //// Debug
        //[Header("Debug")]
        //[Tooltip("")]
        //[SerializeField]
        //private bool _startInOfflineMode = false;

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion
        /// (which allows you to make breaking changes).
        /// </summary>
        private static string GAME_VERSION = "1";

        private static RoomOptions ROOM_OPTIONS = new RoomOptions() {
            MaxPlayers = 20,
            IsOpen = true,
            IsVisible = true
        };

        public bool IsConnected {
            get {
                return PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer; //PhotonNetwork.IsConnectedAndReady;
            }
        }

        /// <summary>
        /// Returns true if we are currently disconnected.
        /// This returns false, when we are connecting or disconnecting.
        /// </summary>
        public bool IsDisconnected {
            get {
                return !PhotonNetwork.IsConnected;
            }
        }

        public bool InLobby {
            get {
                return PhotonNetwork.InLobby;
            }
        }

        public bool InRoom {
            get {
                return PhotonNetwork.InRoom;
            }
        }

        public bool IsMaster {
            get {
                return PhotonNetwork.IsMasterClient;
            }
        }
        private bool _wasCreator;

        public List<RoomInfo> RoomList {
            get {
                if (InLobby) {
                    return _roomList;
                } else {
                    return new List<RoomInfo>();
                }
            }
        }
        private List<RoomInfo> _roomList = new List<RoomInfo>();


        protected override void Awake() {
            base.Awake();

            PhotonNetwork.AddCallbackTarget(this);

            // Set Photons ServerSettings for networking.
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
            PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.WebSocketSecure;
            PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonindustries.io";
            PhotonNetwork.PhotonServerSettings.AppSettings.Port = 443;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = GAME_VERSION;

            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;

            PhotonNetwork.GameVersion = GAME_VERSION;
        }

        private void Start() {
            // Initial
            if (!_isSwitching) {
                EnterOfflineRoom();
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #region Offline

        public void EnterOfflineRoom() {
            StartCoroutine(OfflineRoomRoutine());
        }

        private IEnumerator OfflineRoomRoutine() {
            _isSwitching = true;

            if (PhotonNetwork.InRoom) {
                PhotonNetwork.LeaveRoom();
            }

            if (PhotonNetwork.IsConnected) {
                PhotonNetwork.Disconnect();
            }

            while (!IsDisconnected) {
                yield return null;
            }

            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null, new RoomOptions());
        }

        #endregion Offline

        #region Online

        /// <summary>
        /// Connects to the photon network with certain settings
        /// </summary>
        /// <returns></returns>
        private bool Connect() {
            if (!IsConnected) {
                // settings used for this session
                PhotonNetwork.NickName = PreferenceManager.Instance.UserName;

                return PhotonNetwork.ConnectUsingSettings();
            } else {
                return true;
            }
        }

        public void EnterLobby() {
            if (_isSwitching) {
                return;
            }

            StartCoroutine(ConnectAndJoinLobby());
        }

        private IEnumerator ConnectAndJoinLobby() {
            _isSwitching = true;

            if (PhotonNetwork.InLobby) {
                PhotonNetwork.LeaveLobby();
            }

            if (PhotonNetwork.InRoom) {
                PhotonNetwork.LeaveRoom();
            }

            if (PhotonNetwork.OfflineMode) {
                PhotonNetwork.OfflineMode = false;
            }

            if (Application.internetReachability != NetworkReachability.NotReachable) {
                Connect();

                while (!IsConnected) {
                    yield return null;
                }
            } else {
                EnterOfflineRoom();
                yield break;
            }

            if (!PhotonNetwork.JoinLobby()) {
                // In case there are server-side errors OnLobbyJoinFailed gets called instead.
                OnJoinLobbyFailed();
            }
        }

        public void EnterOnlineRoom(string room) {
            if (_isSwitching) {
                return;
            }

            StartCoroutine(ConnectAndCreateOrJoin(room.Trim()));
        }

        private IEnumerator ConnectAndCreateOrJoin(string room) {
            _isSwitching = true;

            if (PhotonNetwork.InLobby) {
                Debug.LogWarning("Leave Lobby - " + PhotonNetwork.LeaveLobby());
            }

            if (PhotonNetwork.InRoom) {
                Debug.LogWarning("Leave Room - " + PhotonNetwork.LeaveRoom());
            }

            if (PhotonNetwork.OfflineMode) {
                PhotonNetwork.OfflineMode = false;
            }

            if (Application.internetReachability != NetworkReachability.NotReachable) {
                Connect();

                while (!IsConnected) {
                    yield return null;
                }
            } else {
                EnterOfflineRoom();
                yield break;
            }

            if (!PhotonNetwork.JoinOrCreateRoom(room, ROOM_OPTIONS, null)) {
                // In case there are server-side errors OnJoinRoomFailed gets called instead.
                OnJoinRoomFailed(0, "Entering room failed locally.");
            }
        }

        private IEnumerator ConnectAndJoinRoom(string room) {
            _isSwitching = true;

            if (PhotonNetwork.InLobby) {
                PhotonNetwork.LeaveLobby();
            }

            if (PhotonNetwork.InRoom) {
                PhotonNetwork.LeaveRoom();
            }

            if (PhotonNetwork.OfflineMode) {
                PhotonNetwork.OfflineMode = false;
            }

            if (Application.internetReachability != NetworkReachability.NotReachable) {
                Connect();

                while (!IsConnected) {
                    yield return null;
                }
            } else {
                EnterOfflineRoom();
                yield break;
            }

            if (!PhotonNetwork.JoinRoom(room)) {
                // In case there are server-side errors OnJoinRoomFailed gets called instead.
                OnJoinRoomFailed(0, "Entering room failed locally.");
            }
        }

        #endregion Online

        #region Matchmaking callbacks

        public void OnJoinedRoom() {
            _isSwitching = false;

            if (PhotonNetwork.CurrentRoom.IsOffline) {
                _roomState = "OFFLINE";
            } else {
                _roomState = PhotonNetwork.CurrentRoom.Name;
            }

            Debug.Log("OnJoinedRoom() called by PUN. Now this client is in " + (PhotonNetwork.CurrentRoom.IsOffline ? "an offline" : "a network (" + PhotonNetwork.ServerAddress + ")") + " room.");
            OnRoomJoined.Invoke();
        }

        public void OnCreatedRoom() {
            _isSwitching = false;

            Debug.Log("Room created: " + PhotonNetwork.CurrentRoom?.Name);

            OnRoomCreated.Invoke();
        }

        public void OnCreateRoomFailed(short returnCode, string message) {
            _isSwitching = false;

            Debug.LogError("Creating room failed: " + returnCode);
            OnRoomCreationFailed.Invoke(returnCode, message);
        }

        public void OnJoinRoomFailed(short returnCode, string message) {
            _isSwitching = false;

            Debug.LogError("Joining room failed: " + returnCode);
            OnRoomJoinFailed.Invoke(returnCode, message);
        }

        public void OnLeftRoom() {
            OnRoomLeft.Invoke();
        }

        public void OnJoinRandomFailed(short returnCode, string message) {
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList) {
        }

        #endregion Matchmaking callbacks

        #region Connection Callbacks

        public void OnConnectedToMaster() {
            OnConnect.Invoke();
        }

        public void OnDisconnected(DisconnectCause cause) {
            if (_isSwitching) {
                // we had the intention to disconnect -> happens when we leave a online room and go offline or other stuff
                _wasCreator = false;
                return;
            }

            Debug.LogError("DISCONNECT");
            OnDisconnect.Invoke(cause);

            // Handle Reconnection
            switch (_roomState) {
                case "LOBBY":
                    EnterLobby();
                    break;
                case "OFFLINE":
                    EnterOfflineRoom();
                    break;
                default:
                    if (_wasCreator) {
                        EnterOnlineRoom(_roomState);
                    } else {
                        StartCoroutine(ConnectAndJoinRoom(_roomState));
                    }
                    break;
            }
        }

        public void OnConnected() {
        }

        public void OnRegionListReceived(RegionHandler regionHandler) {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
        }

        public void OnCustomAuthenticationFailed(string debugMessage) {
        }

        #endregion Connection Callbacks

        #region Lobby Callbacks

        public void OnJoinedLobby() {
            _isSwitching = false;

            _roomState = "LOBBY";

            _roomList.Clear();

            OnLobbyJoined.Invoke();
        }

        private void OnJoinLobbyFailed() {
            _isSwitching = false;

            OnLobbyJoinFailed.Invoke();
        }

        public void OnLeftLobby() {
            OnLobbyLeft.Invoke();
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList) {

            bool changed = false;

            foreach (RoomInfo room in roomList) {

                // lost old room
                if (room.RemovedFromList) {

                    if (_roomList.Contains(room)) {
                        _roomList.Remove(room);
                        changed = true;
                    }

                    continue;
                }

                if (_roomList.Contains(room)) {

                    int index = _roomList.IndexOf(room);

                    // updated existing room
                    if (_roomList[index].PlayerCount != room.PlayerCount) {
                        _roomList.RemoveAt(index);
                        _roomList.Insert(index, room);
                        changed = true;
                    }

                } else {

                    // received new room
                    _roomList.Add(room);
                    changed = true;
                }
            }

            if (changed) {
                OnRoomListUpdated.Invoke(_roomList);
            }
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) {
        }

        #endregion Lobby Callbacks

        #region Room Callbacks

        public void OnPlayerEnteredRoom(Player newPlayer) {
            OnPlayerJoined(newPlayer);
        }

        public void OnPlayerLeftRoom(Player otherPlayer) {
            OnPlayerLeft(otherPlayer);
        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
        }

        /// <summary>
        /// The Photon Server detects when a Master Client disconnects and assigns
        /// the actor with the lowest actor number as the new Master Client.
        /// 
        /// We use this as a way to determine when a master client left/disconnected.
        /// </summary>
        public void OnMasterClientSwitched(Player newMasterClient) {

            OnPlayerUpdated.Invoke(newMasterClient);
        }

        #endregion Room Callbacks
    }
}