using UnityEngine;
using UnityEngine.InputSystem;

namespace HCIG {

    /// <summary>
    /// All relevant informations that could be interesting for others + Movement control of the PC-View
    /// </summary>
    public class BasePC : Singleton<BasePC> {

        public bool MovementAllowed {
            get; set;
        } = true;

        public Camera Camera {
            get {
                if (_camera == null) {
                    _camera = GetComponentInChildren<Camera>(true);
                }
                return _camera;
            }
        }
        private Camera _camera = null;


        public Transform Offset {
            // only for nicer query from outside
            get {
                return transform;
            }
        }


        private float _movementSpeed = 5f;
        private float _rotationSpeed = 5;

        private Vector2 _mousePosition;


        void Update() {

            // Other Scripts can suspend Movement for different cases
            if (!MovementAllowed) {
                return;
            }

            PositionCamera();
            RotateCamera();
        }

        private void RotateCamera() {

            // Visibility of cursor
            if (!UnityEngine.Input.GetMouseButton(1)) {
                if (!Cursor.visible) {
                    Mouse.current.WarpCursorPosition(_mousePosition);
                    Cursor.visible = true;
                }

                return;
            } else {
                if (Cursor.visible) {
                    _mousePosition = UnityEngine.Input.mousePosition;
                    Cursor.visible = false;
                }
            }

            Vector3 origin = transform.eulerAngles;
            Vector3 destination = origin;

            destination.x -= UnityEngine.Input.GetAxis("Mouse Y") * _rotationSpeed;
            destination.y += UnityEngine.Input.GetAxis("Mouse X") * _rotationSpeed;

            //if a change in position is detected perform the necessary update
            if (destination != origin) {
                transform.eulerAngles = destination;
            }
        }

        private void PositionCamera() {

            Vector3 movement = new Vector3(0, 0, 0);

            // Sideways
            if (UnityEngine.Input.GetKey(KeyCode.A)) {
                movement.x -= _movementSpeed;
            }

            if (UnityEngine.Input.GetKey(KeyCode.D)) {
                movement.x += _movementSpeed;
            }

            // Forward
            if (UnityEngine.Input.GetKey(KeyCode.S)) {
                movement.z -= _movementSpeed;
            }

            if (UnityEngine.Input.GetKey(KeyCode.W)) {
                movement.z += _movementSpeed;
            }

            // Upwards
            if (UnityEngine.Input.GetKey(KeyCode.Q)) {
                movement.y -= _movementSpeed;
            }

            if (UnityEngine.Input.GetKey(KeyCode.E)) {
                movement.y += _movementSpeed;
            }

            if (UnityEngine.Input.GetKey(KeyCode.LeftShift)) {
                movement *= 2;
            }

            //calculate desired camera position based on received input
            Vector3 destination = transform.position + transform.TransformDirection(movement);

            //if a change in position is detected perform the necessary update
            if (destination != transform.position) {
                transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * _movementSpeed);
            }
        }
    }
}