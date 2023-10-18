using UnityEngine;

// Meta - OpenXR
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

// Vive - OpenXR - Schnittstelle
using Wave.OpenXR.Hand;
#if UNITY_EDITOR || UNITY_ANDROID
using Wave.OpenXR.Toolkit;
using Wave.OpenXR.Toolkit.Hand;
#endif
// https://developer.vive.com/resources/openxr/openxr-mobile/tutorials/unity/hand-tracking/see-your-hands-xr/


namespace HCIG.Input.Data {

    public class RealData : InputData {

        private bool _usedframeWork = false;

        private Vector3 _refPos = new();
        private Quaternion _refRot = new();

        protected override void Awake() {

            DeviceManager.Instance.OnOperatingSystemIdentified += (_) => {
                if (!ApplicationManager.Instance.IsAndroid) {
                    VIVE.HandTracking.HandManager.StartFrameWork(true);
                    VIVE.HandTracking.HandManager.StartFrameWork(false);
                    _usedframeWork = true;
                }
            };
        }

        private void OnDestroy() {
            if (_usedframeWork) {
                VIVE.HandTracking.HandManager.StopFrameWork(true);
                VIVE.HandTracking.HandManager.StopFrameWork(false);
            };
        }


        private void FixedUpdate() {

            // Get offset references
            _refPos = BaseXR.Instance.Offset.position;
            _refRot = BaseXR.Instance.Offset.rotation;

            // Calculate hands new 
            _left = SelectCorrectHand(Chirality.Left);
            _right = SelectCorrectHand(Chirality.Right);
        }

        private Hand SelectCorrectHand(Chirality chirality) {

            switch (DeviceManager.Instance.System) {
                case OperatingSystem.Vive:
                    return ViveHand(chirality);
                case OperatingSystem.Meta:
                    return MetaHand(chirality);
                case OperatingSystem.Pico:
                    return new VirtualHand();
                default:
                    Hand hand;

                    if (!(hand = MetaHand(chirality)).IsValid) {
                        hand = ViveHand(chirality);
                    }

                    return hand;
            }
        }

        #region META

        public XRHandSubsystem HandSystem {
            get {
                if (_handSystem == null && XRGeneralSettings.Instance.Manager != null && XRGeneralSettings.Instance.Manager.activeLoader != null) {
                    _handSystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
                }
                return _handSystem;
            }
        }
        private XRHandSubsystem _handSystem = null;

        /// <summary>
        /// Access the Meta Hand Data
        /// </summary>
        private Hand MetaHand(Chirality chirality) {

            if (HandSystem == null) {
                return new VirtualHand();
            }

            XRHand meta;

            if (chirality == Chirality.Left) {
                meta = HandSystem.leftHand;
            } else {
                meta = HandSystem.rightHand;
            }

            if (!meta.isTracked) {
                return new VirtualHand();
            }

            VirtualHand hand = new VirtualHand(chirality);

            int openID;

            // Wrist
            openID = (int)XRHandJointID.Wrist;

            meta.GetJoint((XRHandJointID)openID).TryGetPose(out Pose pose);

            hand.Wrist = new Pose(_refPos + _refRot * pose.position, _refRot * pose.rotation);

            // Palm
            openID = (int)XRHandJointID.Palm;

            meta.GetJoint((XRHandJointID)openID).TryGetPose(out pose);

            hand.Palm = new Pose(_refPos + _refRot * pose.position, Quaternion.identity);


            // Fingers
            int offset = (int)XRHandJointID.ThumbMetacarpal;

            for (int f = 0; f < 5; f++) {

                // Joints
                for (int j = 0; j < 5; j++) {

                    openID = f * (int)FingerType.MAX_FINGER_COUNT + j + offset;

                    if (meta.GetJoint((XRHandJointID)openID).TryGetPose(out pose)) {

                        if ((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                            --offset;
                        } else {
                            hand.SetJoint((FingerType)f, (JointType)j, new Pose(_refPos + _refRot * pose.position, Quaternion.identity));
                        }
                    }
                }
            }

            return hand;
        }

        #endregion META

        #region VIVE

        /// <summary>
        /// Access the Vive Hand Data
        /// </summary>
        private Hand ViveHand(Chirality chirality) {

#if UNITY_EDITOR || UNITY_ANDROID

            HandJoint[] joints = HandTracking.GetHandJointLocations(chirality == Chirality.Left ? HandFlag.Left : HandFlag.Right);

            int waveID;

            // Wrist
            waveID = (int)XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT;

            HandJoint wrist = joints[waveID];

            if (!wrist.isValid) {
                return new VirtualHand();
            }

            VirtualHand hand = new(chirality);

            hand.Wrist = new Pose(_refPos + _refRot * (wrist.position + wrist.rotation * new Vector3(0.0f, 0.01f, -0.01f)), _refRot * wrist.rotation);

            Debug.Log(_refPos);

            // Palm
            waveID = (int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT;

            hand.Palm = new Pose(_refPos + _refRot * joints[waveID].position, Quaternion.identity);


            // Fingers
            int offset = (int)XrHandJointEXT.XR_HAND_JOINT_THUMB_METACARPAL_EXT;

            for (int f = 0; f < 5; f++) {

                // Joints
                for (int j = 0; j < 5; j++) {

                    waveID = f * (int)FingerType.MAX_FINGER_COUNT + j + offset;

                    if ((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                        --offset;
                    } else {
                        hand.SetJoint((FingerType)f, (JointType)j, new Pose(_refPos + _refRot * joints[waveID].position, Quaternion.identity));
                    }
                }
            }

            return hand;
#else
           return new VirtualHand();
#endif
        }
    }

    #endregion VIVE
}
