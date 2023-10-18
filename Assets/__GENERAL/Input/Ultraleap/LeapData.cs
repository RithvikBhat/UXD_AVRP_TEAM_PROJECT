using UnityEngine;

using Leap.Unity;

namespace HCIG.Input.Data {

    public class LeapData : InputData {

        LeapXRServiceProvider _leapServiceProvider;

        protected override void Awake() {
            _leapServiceProvider = GetComponentInChildren<LeapXRServiceProvider>(true);
        }

        private void FixedUpdate() {

            if (!_leapServiceProvider.IsConnected()) {
                return;
            }

            _left = ExtractHandData(_leapServiceProvider.GetHand(Leap.Unity.Chirality.Left));
            _right = ExtractHandData(_leapServiceProvider.GetHand(Leap.Unity.Chirality.Right));
        }

        /// <summary>
        /// Connects the hand joints of the leap hand with our hand system
        /// </summary>
        /// <param name="data"></param>
        /// <param name="leap"></param>
        private Hand ExtractHandData(Leap.Hand leap) {

            if (leap == null) {
                return new VirtualHand();
            }

            Hand hand = new VirtualHand(leap.IsLeft ? Chirality.Left : Chirality.Right);

            // Wrist
            hand.Wrist = new Pose(leap.WristPosition - leap.Rotation * new Vector3((leap.IsLeft ? -1 : 1) * 0.01f, 0, 0.015f), leap.Rotation);

            // Palm
            hand.Palm = leap.GetPalmPose();

            // Fingers
            Pose joint = new Pose();

            for (int f = 0; f < (int)FingerType.MAX_FINGER_COUNT; f++) {
                for (int j = 0; j < (int)JointType.MAX_JOINT_COUNT; j++) {

                    // leap Data
                    if (f != (int)FingerType.Thumb) {
                        // normal 5 joints

                        if (j != (int)JointType.Tip) {
                            // in between
                            joint.position = leap.Finger(leap.Id * 10 + f).Bone((Leap.Bone.BoneType)j).PrevJoint;
                        } else {
                            // tip point
                            joint.position = leap.Finger(leap.Id * 10 + f).TipPosition;
                        }
                    } else {
                        // special 4 joint

                        switch (j) {
                            case < (int)JointType.Intermediate:
                                joint.position = leap.Finger(leap.Id * 10 + f).Bone((Leap.Bone.BoneType)j).NextJoint;
                                break;
                            case (int)JointType.Intermediate:
                                // skip the intermediate joint
                                continue;
                            case > (int)JointType.Intermediate:
                                joint.position = leap.Finger(leap.Id * 10 + f).Bone((Leap.Bone.BoneType)(j - 1)).NextJoint;
                                break;
                        }
                    }

                    // hand Data
                    hand.SetJoint((FingerType)f, (JointType)j, joint);
                }
            }

            return hand;
        }
    }
}
