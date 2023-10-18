using UnityEngine;

namespace HCIG.Input {

    public enum FingerType { Thumb, Index, Middle, Ring, Pinky, MAX_FINGER_COUNT }

    public enum JointType { Metacarpal, Proximal, Intermediate, Distal, Tip, MAX_JOINT_COUNT }

    public class Finger : MonoBehaviour {

        public FingerType Type = (FingerType)(-1); // Null;

        [Header("Joints")]
        [SerializeField]
        private Transform _meta = null;
        private Pose _pMeta = default;
        public Pose Meta {
            get {
                if(_meta != null) {
                    _pMeta.position = _meta.position;
                    _pMeta.rotation = _meta.rotation;
                }
                return _pMeta;
            }
            set {
                _pMeta = value;

                if (_meta != null) {
                    _meta.SetPositionAndRotation(_pMeta.position, _pMeta.rotation);
                }
            }
        }

        [SerializeField]
        private Transform _proxi = null;
        private Pose _pProxi = default;
        public Pose Proxi {
            get {
                if (_proxi != null) {
                    _pProxi.position = _proxi.position;
                    _pProxi.rotation = _proxi.rotation;
                }
                return _pProxi;
            }
            set {
                _pProxi = value;

                if (_proxi != null) {
                    _proxi.SetPositionAndRotation(_pProxi.position, _pProxi.rotation);
                }
            }
        }

        [SerializeField]
        private Transform _inter = null;
        private Pose _pInter = default;
        public Pose Inter {
            get {
                if (_inter != null) {
                    _pInter.position = _inter.position;
                    _pInter.rotation = _inter.rotation;
                }
                return _pInter;
            }
            set {
                _pInter = value;

                if (_inter != null) {
                    _inter.SetPositionAndRotation(_pInter.position, _pInter.rotation);
                }
            }
        }

        [SerializeField]
        private Transform _distal = null;
        private Pose _pDistal = default;
        public Pose Distal {
            get {
                if (_distal != null) {
                    _pDistal.position = _distal.position;
                    _pDistal.rotation = _distal.rotation;
                }
                return _pDistal;
            }
            set {
                _pDistal = value;

                if (_distal != null) {
                    _distal.SetPositionAndRotation(_pDistal.position, _pDistal.rotation);
                }
            }
        }

        [SerializeField]
        private Transform _tip = null;
        private Pose _pTip = default;
        public Pose Tip {
            get {
                if (_tip != null) {
                    _pTip.position = _tip.position;
                    _pTip.rotation = _tip.rotation;
                }
                return _pTip;
            }
            set {
                _pTip = value;

                if (_tip != null) {
                    _tip.SetPositionAndRotation(_pTip.position, _pTip.rotation);
                }
            }
        }

        public Pose Joint(JointType joint) {

            return joint switch {
                JointType.Metacarpal => Meta,
                JointType.Proximal => Proxi,
                JointType.Intermediate => Inter,
                JointType.Distal => Distal,
                JointType.Tip => Tip,
                _ => default,
            };
        }

        /// <summary>
        /// Creates a non-valid finger reference
        /// </summary>
        public Finger() {
        }

        /// <summary>
        /// Creates a valid finger reference for data storing
        /// </summary>
        public Finger(FingerType type) {
            Type = type;
        }
    }
}
