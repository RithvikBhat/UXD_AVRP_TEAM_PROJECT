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

    public class RealisticHand : Hand {

        private bool _usedframeWork = false;

        private void Awake() {
            DeviceManager.Instance.OnOperatingSystemIdentified += (_) => {
                if (!ApplicationManager.Instance.IsAndroid) {
                    VIVE.HandTracking.HandManager.StartFrameWork(Chirality == Chirality.Left);
                    _usedframeWork = true;
                }
            };
        }

        private void OnDestroy() {
            if (_usedframeWork) {
                VIVE.HandTracking.HandManager.StopFrameWork(Chirality == Chirality.Left);
            };
        }

        void Update() {

            switch (DeviceManager.Instance.System) {
                case OperatingSystem.Vive:
                    UpdateViveHand();
                    break;

                case OperatingSystem.Meta:
                    UpdateMetaHand();
                    break;

                //case OperatingSystem.Pico:
                    
                //    break;

                default:
                    if (!UpdateMetaHand()) {
                        UpdateViveHand();
                    }
                    break;
            }
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
        /// <returns>returns if the data was accessible</returns>
        private bool UpdateMetaHand() {

            if (HandSystem == null) {
                return false;
            }

            XRHand xrHand;

            if (Chirality == Chirality.Left) {
                xrHand = HandSystem.leftHand;
            } else {
                xrHand = HandSystem.rightHand;
            }

            if (!(_isValid = xrHand.isTracked)) {
                return true;
            }


            int openID;
            Pose pose;

            // Wrist
            openID = (int)XRHandJointID.Wrist;

            xrHand.GetJoint((XRHandJointID)openID).TryGetPose(out pose);

            Wrist = pose;

            //Wrist.localRotation = pose.rotation;
            //Wrist.localPosition = pose.position;

            // Palm
            openID = (int)XRHandJointID.Palm;

            xrHand.GetJoint((XRHandJointID)openID).TryGetPose(out pose);

            Palm = pose;
            
            //Palm.localPosition = Wrist.InverseTransformVector(pose.position - Wrist.TransformVector(Wrist.localPosition));


            // Fingers
            int offset = (int)XRHandJointID.ThumbMetacarpal;

            for (int f = 0; f < 5; f++) {

                //Vector3 refPos = BaseXR.Instance.Offset.InverseTransformVector(Wrist.localPosition);

                // Joints
                for (int j = 0; j < 5; j++) {

                    openID = f * (int)FingerType.MAX_FINGER_COUNT + j + offset;

                    if (xrHand.GetJoint((XRHandJointID)openID).TryGetPose(out pose)) {

                        //Transform joint = GetJoint((FingerType)f, (JointType)j);

                        if ((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                            --offset;

                            //joint.localPosition = Wrist.InverseTransformVector(pose.position - refPos);
                            //refPos = pose.position;
                        } else {
                            SetJoint((FingerType)f, (JointType)j, pose);
                        }
                    }
                }
            }

            return true;
        }

        #endregion META

        #region VIVE

        /// <summary>
        /// Access the Vive Hand Data
        /// </summary>
        private void UpdateViveHand() {

#if UNITY_EDITOR || UNITY_ANDROID

            HandJoint[] joints = HandTracking.GetHandJointLocations(Chirality == Chirality.Left ? HandFlag.Left : HandFlag.Right);

            int waveID;

            // Wrist
            waveID = (int)XrHandJointEXT.XR_HAND_JOINT_WRIST_EXT;

            HandJoint wrist = joints[waveID];

            if (!wrist.isValid) {
                _isValid = false;
                return;
            }

            Wrist = new Pose(wrist.position + wrist.rotation * new Vector3(0.0f, 0.01f, -0.01f), wrist.rotation);

            //Wrist.localRotation = wrist.rotation;
            //Wrist.localPosition = wrist.position + Wrist.TransformVector(new Vector3(0.0f, 0.01f, -0.01f));

            // Palm
            waveID = (int)XrHandJointEXT.XR_HAND_JOINT_PALM_EXT;

            Palm = new Pose(joints[waveID].position, wrist.rotation);

            //Palm.localPosition = Wrist.InverseTransformVector(joints[waveID].position - Wrist.localPosition);

            // Fingers
            int offset = (int)XrHandJointEXT.XR_HAND_JOINT_THUMB_METACARPAL_EXT;

            for (int f = 0; f < 5; f++) {

                //Vector3 refPos = Wrist.localPosition;

                // Joints
                for (int j = 0; j < 5; j++) {

                    waveID = f * (int)FingerType.MAX_FINGER_COUNT + j + offset;

                    //Transform joint = GetJoint((FingerType)fingerID, (JointType)jointID);

                    //if (joint != null) {

                    //    // Vector3 position = new Vector3(joints[waveID].pose.position.x, joints[waveID].pose.position.y, joints[waveID].pose.position.z);
                    //    Vector3 position = joints[waveID].position;

                    //    joint.localPosition = Wrist.InverseTransformVector(position - refPos);
                    //    refPos = position;
                    //} else {
                    //    --offset;
                    //}

                    if ((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                        --offset;

                        //joint.localPosition = Wrist.InverseTransformVector(pose.position - refPos);
                        //refPos = pose.position;
                    } else {
                        SetJoint((FingerType)f, (JointType)j, new Pose(joints[waveID].position, joints[waveID].rotation));
                    }
                }
            }

            _isValid = true;
            return;
#endif
        }
    }

    #endregion VIVE
}

