
using UnityEngine;

namespace HCIG.Input.Data {

    /// <summary>
    /// Simulated Hand that can be fed with external hand data (e.g. from leap hand)
    /// </summary>
    public class VirtualHand : Hand {

        /// <summary>
        /// Creates a valid hand reference for data storing
        /// </summary>
        /// <param name="chirality"></param>
        public VirtualHand(Chirality chirality) {
            _chirality = chirality;

            Thumb = new(FingerType.Thumb);
            Index = new(FingerType.Index);
            Middle = new(FingerType.Middle);
            Ring = new(FingerType.Ring);
            Pinky = new(FingerType.Pinky);

            _isValid = true;
        }

        /// <summary>
        /// Creates a non-valid hand reference -> just for checks OR as first placeholder
        /// </summary>
        public VirtualHand() {
            _isValid = false;
        }

        public override bool IsValid {
            get {
                return _isValid;
            }
        }
        private bool _isValid = false;

        public override bool IsPinching {
            get {
                Vector3 thumb = GetJoint(FingerType.Thumb, JointType.Tip).position;
                Vector3 index = GetJoint(FingerType.Index, JointType.Tip).position;

                return Vector3.Distance(thumb, index) <= _pinchDetection;
            }
        }
        private readonly float _pinchDetection = 0.03f;
    }
}
