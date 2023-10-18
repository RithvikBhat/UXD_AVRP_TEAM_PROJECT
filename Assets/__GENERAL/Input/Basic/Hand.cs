using UnityEngine;

namespace HCIG.Input {

    public enum Chirality { Left, Right };

    public abstract class Hand : MonoBehaviour {

        public virtual Chirality Chirality {
            get { return _chirality; }
        }
        [Header("Chirality")]
        [SerializeField]
        protected Chirality _chirality = (Chirality)(-1); // Null;

        [Header("Base")]
        [SerializeField]
        private Transform _wrist = null;
        private Pose _pWrist = default;
        /// <summary>
        /// Access the wrist information as pose data
        /// </summary>
        public Pose Wrist {
            get {
                if (_wrist != null) {
                    _pWrist.position = _wrist.position;
                    _pWrist.rotation = _wrist.rotation;
                }
                return _pWrist;
            }
            set {
                _pWrist = value;

                if (_wrist != null) {
                    _wrist.SetPositionAndRotation(_pWrist.position, _pWrist.rotation);
                }
            }
        }


        [SerializeField]
        private Transform _palm = null;
        private Pose _pPalm = default;
        /// <summary>
        /// Access the palm information as pose data
        /// </summary>
        public Pose Palm {
            get {
                if (_palm != null) {
                    _pPalm.position = _palm.position;
                    _pPalm.rotation = _palm.rotation;
                }
                return _pPalm;
            }
            set {
                _pPalm = value;

                if (_palm != null) {
                    _palm.SetPositionAndRotation(_pPalm.position, _pPalm.rotation);
                }
            }
        }

        [Header("Fingers")]
        [SerializeField]
        protected Finger Thumb = new();
        [SerializeField]
        protected Finger Index = new();
        [SerializeField]
        protected Finger Middle = new();
        [SerializeField]
        protected Finger Ring = new();
        [SerializeField]
        protected Finger Pinky = new();

        /// <summary>
        /// Returns the value pair of a desired joint on the desired finger.
        /// </summary>
        /// <param name="fingerT"></param>
        /// <param name="jointT"></param>
        /// <returns></returns>
        public Pose GetJoint(FingerType fingerT, JointType jointT) {

            switch (fingerT) {
                case FingerType.Thumb:
                    return Thumb.Joint(jointT);
                case FingerType.Index:
                    return Index.Joint(jointT);
                case FingerType.Middle:
                    return Middle.Joint(jointT);
                case FingerType.Ring:
                    return Ring.Joint(jointT);
                case FingerType.Pinky:
                    return Pinky.Joint(jointT);
            }
            return new();
        }

        /// <summary>
        /// Sets a new value pair to the desired joint on the desired finger.
        /// </summary>
        /// <param name="fingerT"></param>
        /// <param name="jointT"></param>
        /// <param name="pose"></param>
        public bool SetJoint(FingerType fingerT, JointType jointT, Pose pose) {

            Finger finger;

            // Finger
            switch (fingerT) {
                case FingerType.Thumb:
                    finger = Thumb;
                    break;
                case FingerType.Index:
                    finger = Index;
                    break;
                case FingerType.Middle:
                    finger = Middle;
                    break;
                case FingerType.Ring:
                    finger = Ring;
                    break;
                case FingerType.Pinky:
                    finger = Pinky;
                    break;
                default:
                    finger = new Finger();
                    break;
            }

            if ((int)finger.Type == -1) {
                return false;
            }

            // Joint
            switch (jointT) {
                case JointType.Metacarpal:
                    finger.Meta = pose;
                    break;
                case JointType.Proximal:
                    finger.Proxi = pose;
                    break;
                case JointType.Intermediate:
                    finger.Inter = pose;
                    break;
                case JointType.Distal:
                    finger.Distal = pose;
                    break;
                case JointType.Tip:
                    finger.Tip = pose;
                    break;
            }

            return true;
        }

        protected bool IsNullPose(Pose pose) {
            return pose.position == Vector3.zero && pose.rotation.normalized == Quaternion.identity;
        }

        public virtual bool IsValid {
            get {
                return false;
            }
        }

        public virtual bool IsPinching {
            get {
                return false;
            }
        }
    }
}


