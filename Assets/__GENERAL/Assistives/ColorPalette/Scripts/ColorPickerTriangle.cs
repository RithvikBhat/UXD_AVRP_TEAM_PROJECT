using UnityEngine;

using Leap.Unity.Interaction;

namespace HCIG.UsefulTools  {

    [RequireComponent(typeof(InteractionBehaviour))]
    public class ColorPickerTriangle : MonoBehaviour {

        private enum Type {
            None = -1,
            Triangle,
            Circle
        }

        const float CIRCLE_RADIUS = .5f;
        const float CIRCLE_WIDTH = .1f;
        const float TRIANGLE_RADIUS = .4f;

        [Header("Triangle")]
        [SerializeField]
        private Transform _tPointer;
        [SerializeField]
        private MeshFilter _tMeshFilter;

        private Vector3[] _tEdges = new Vector3[3];

        [Header("Circle")]
        [SerializeField]
        private Transform _cPointer;
        private Color _cColor = Color.red;

        private Vector3 _curBary = Vector3.up;

        private Vector3 _curLocalPos;
        private Type _mode = Type.None;

        void Awake() {

            // Get triangle edges for barycentric calculations;
            float c = Mathf.Sin(Mathf.Deg2Rad * 30);
            float s = Mathf.Cos(Mathf.Deg2Rad * 30);

            float factor = _tMeshFilter.transform.localScale.x;

            // Up
            _tEdges[0] = factor * TRIANGLE_RADIUS * Vector3.up;
            // Left
            _tEdges[1] = factor * TRIANGLE_RADIUS * new Vector3(s, -c, 0);
            // Right
            _tEdges[2] = factor * TRIANGLE_RADIUS * new Vector3(-s, -c, 0);

            // Default Color
            SetNewColor(ColorPaletteManager.Instance.Color);
        }

        public void SetNewColor(Color NewColor) {

            Color.RGBToHSV(NewColor, out float h, out float s, out float v);

            _cColor = Color.HSVToRGB(h, 1, 1);

            ChangeTriangleColor(_cColor);

            // Circle
            _cPointer.localEulerAngles = Vector3.back * (h * 360f);

            // Triangle
            _curBary.y = 1f - v;
            _curBary.x = v * s;
            _curBary.z = 1f - _curBary.y - _curBary.x;

            _curLocalPos = _tEdges[0] * _curBary.x + _tEdges[1] * _curBary.y + _tEdges[2] * _curBary.z;

            _tPointer.localPosition = _curLocalPos;
        }

        private bool CheckCircle() {
            if (Mathf.Abs(_curLocalPos.magnitude - CIRCLE_RADIUS) > CIRCLE_WIDTH / 2f) {
                return false;
            }

            float a = Vector3.Angle(Vector3.left, _curLocalPos);

            if (_curLocalPos.y < 0) {
                a = 360f - a;
            }

            _cColor = Color.HSVToRGB(a / 360, 1, 1);

            ChangeTriangleColor(_cColor);

            _cPointer.localEulerAngles = Vector3.back * a;

            SetColor();

            return true;
        }

        private bool CheckTriangle() {

            // Barycentric calculations
            Vector3 bary = Vector3.zero;

            Vector3 v0 = _tEdges[1] - _tEdges[0];
            Vector3 v1 = _tEdges[2] - _tEdges[0];
            Vector3 v2 = _curLocalPos - _tEdges[0];

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            bary.y = (d11 * d20 - d01 * d21) / denom;
            bary.z = (d00 * d21 - d01 * d20) / denom;

            bary.x = 1.0f - bary.y - bary.z;

            // Check if we are inside the triangle
            if (bary.x >= 0f && bary.y >= 0f && bary.z >= 0f) {
                _curBary = bary;
                _tPointer.localPosition = _curLocalPos;
                SetColor();
                return true;
            } else {
                return false;
            }
        }

        private void SetColor() {

            Color.RGBToHSV(_cColor, out float h, out float v, out float s);

            Color c = (_curBary.y > .9999) ? Color.black : Color.HSVToRGB(h, _curBary.x / (1f - _curBary.y), 1f - _curBary.y);

            c.a = 1f;

            ColorPaletteManager.Instance.Color = c;
        }

        private void ChangeTriangleColor(Color circle) {

            Color[] colors = new Color[_tMeshFilter.mesh.colors.Length];

            colors[0] = Color.black;
            colors[1] = circle;
            colors[2] = Color.white;

            _tMeshFilter.mesh.colors = colors;
        }

        private void OnCollisionStay(Collision collision) {

            if (!collision.gameObject.TryGetComponent<ContactBone>(out _)) {
                return;
            }

            if (collision.contactCount == 0) {
                return;
            }

            // Hit-Point
            _curLocalPos = transform.worldToLocalMatrix.MultiplyPoint(collision.GetContact(0).point);
            _curLocalPos.z = 0;

            // Update Selection
            switch (_mode) {
                case Type.Triangle:
                    CheckTriangle();
                    return;
                case Type.Circle:
                    CheckCircle();
                    return;
                default:
                    if (CheckTriangle()) {
                        _mode = Type.Triangle;
                        return;
                    }

                    if (CheckCircle()) {
                        _mode = Type.Circle;
                        return;
                    }

                    return;
            }
        }

        private void OnCollisionExit(Collision collision) {

            if (!collision.gameObject.TryGetComponent<ContactBone>(out _)) {
                return;
            }

            _mode = Type.None;
        }
    }
}
