using UnityEngine;
using Photon.Pun;

namespace HCIG.Avatar {

    public class RobotBody : MonoBehaviour {

        [Header("Offsets")]
        [SerializeField]
        private Vector3 _position;

        Transform _head;

        private void Awake() {
            _head = GetComponentInChildren<RobotHead>().transform;

            transform.localPosition = -_position;
        }

        private void FixedUpdate() {
            
            // Calculate rotations
            transform.eulerAngles = new Vector3(0 , _head.eulerAngles.y + 90, 0);

            // Clear local Y-rotation
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, transform.localEulerAngles.z);
        }
    }
}

