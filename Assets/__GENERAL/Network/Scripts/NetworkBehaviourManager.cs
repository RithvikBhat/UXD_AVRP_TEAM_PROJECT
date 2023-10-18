using Leap.Unity.Interaction;

using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;

namespace HCIG.Network {

    public class NetworkBehaviourManager : Singleton<NetworkBehaviourManager> {

        private PhotonView _photonView;

        private int _instanceID = 0;
        private Dictionary<int, InteractionBehaviour> _networkBehaviours = new();

        protected override void Awake() {
            base.Awake();

            _photonView = GetComponent<PhotonView>();

            NetworkManager.Instance.OnRoomLeft += () => {
                // Clean up all notes in the scene when we leave the room

                foreach (int id in new Dictionary<int, InteractionBehaviour>(_networkBehaviours).Keys) {
                    DestroyNetworkBehaviourRPC(id);
                }

                _instanceID = 0;
            };

            NetworkManager.Instance.OnPlayerJoined += (player) => {
                // Sync current _instanceID to newly joined user

                if (!NetworkManager.Instance.IsMaster) {
                    return;
                }

                _photonView.RPC(nameof(SyncInstanceCounterRPC), player, _instanceID);
            };
        }

        /// <summary>
        /// Network-Call: the user gets the current _instanceID communicated, so he can create also objects with the newly number
        /// </summary>
        /// <param name="id"></param>
        [PunRPC]
        private void SyncInstanceCounterRPC(int id) {

            if(_instanceID < id) {
                _instanceID = id;
            }
        }

        /// <summary>
        /// Returns the ID this gameobject is registered with. Returns -1 when not registered.
        /// </summary>
        /// <param name="networkObject"></param>
        /// <returns></returns>
        public int GetNetworkBehaviourID(GameObject networkObject) {

            if (!networkObject.TryGetComponent(out InteractionBehaviour networkBehaviour)) {
                return -1;
            }

            if (!_networkBehaviours.ContainsValue(networkBehaviour)) {
                return -1;
            }

            return _networkBehaviours.First(x => x.Value == networkBehaviour).Key;
        }

        /// <summary>
        /// Returns the under this ID is registered NetworkBehaviour. Creates a new one local when none found.
        /// </summary>
        /// <param name="networkObject"></param>
        /// <returns></returns>
        public InteractionBehaviour GetNetworkBehaviour(int id) {

            if (!_networkBehaviours.ContainsKey(id)) {
                CreateNetworkObjectRPC(id);
            }

            return _networkBehaviours[id];
        }

        /// <summary>
        /// Creates a GameObject AND InteractionBehvaiour (for all networkers), so it's position & rotation and the interactions are synchronized with all network members 
        /// </summary>
        /// <param name="newObject"></param>
        /// <param name="graspable"></param>
        public InteractionBehaviour CreateNetworkBehaviour() {

            int id = _instanceID;

            _photonView.RPC(nameof(CreateNetworkObjectRPC), RpcTarget.All, id);

            return _networkBehaviours[id];
        }

        /// <summary>
        /// The local creation call who will be called on every device
        /// </summary>
        /// <param name="i"></param>
        [PunRPC]
        private void CreateNetworkObjectRPC(int id) {

            if (_networkBehaviours.ContainsKey(id)) {
                return;
            } else {
                // count up the instance id, so we can create a new networkBehaviour with a unique id
                _instanceID = id + 1;
            }

            GameObject networkObject = new GameObject("NetworkObject_" + id);

            networkObject.transform.parent = transform;

            // Interaction
            InteractionBehaviour networkBehaviour = networkObject.AddComponent<InteractionBehaviour>();

            _networkBehaviours.Add(id, networkBehaviour);

            networkBehaviour.OnGraspBegin += () => _photonView.RPC(nameof(SyncGraspBeginRPC), RpcTarget.Others, id);
            networkBehaviour.OnGraspStay += () => _photonView.RPC(nameof(SyncGraspStayRPC), RpcTarget.Others, id, networkBehaviour.transform.position, networkBehaviour.transform.rotation);
            networkBehaviour.OnGraspEnd += () => _photonView.RPC(nameof(SyncGraspEndRPC), RpcTarget.Others, id);

            // Physics
            Rigidbody rigidbody = networkObject.GetComponent<Rigidbody>();

            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
        }

        [PunRPC]
        private void SyncGraspBeginRPC(int id) {
            _networkBehaviours[id].ignoreGrasping = true;
        }

        [PunRPC]
        private void SyncGraspStayRPC(int id, Vector3 position, Quaternion rotation) {
            _networkBehaviours[id].transform.SetPositionAndRotation(position, rotation);
        }

        [PunRPC]
        private void SyncGraspEndRPC(int id) {
            _networkBehaviours[id].ignoreGrasping = false;
        }


        public void DestroyNetworkBehaviour(int id) {
            _photonView.RPC(nameof(DestroyNetworkBehaviourRPC), RpcTarget.All, id);
        }

        [PunRPC]
        private void DestroyNetworkBehaviourRPC(int id) {

            InteractionBehaviour networkBehaviour = GetNetworkBehaviour(id);

            InteractionManager.instance.UnregisterInteractionBehaviour(networkBehaviour);

            _networkBehaviours.Remove(id);

            Destroy(networkBehaviour.gameObject);
        }
    }
}
