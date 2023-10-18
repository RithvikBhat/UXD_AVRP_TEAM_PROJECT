using HCIG.Input.Data;

using UnityEngine;

namespace HCIG.Input.Anchor {

    public enum PoseType {
        Pinch,
        Select,
        Side,
        Wrist
    }

    public class HandAnchorManager : Singleton<HandAnchorManager> {

        private Vector3 _pinchOffset = new Vector3(0.02f, -0.08f, 0.11f);
        private Vector3 _sideOffset = new Vector3(-0.06f, -0.02f, 0.05f);

        protected override void Awake() {
            base.Awake();

            InputDataManager.Instance.OnInputTypeChanged += ChangeOffset;
            ApplicationManager.Instance.OnModeChanged += (_) => ChangeOffset(InputDataManager.Instance.InputType);
        }

        private void ChangeOffset(InputType type) {

            switch (type) {

                case InputType.Controllers:
                        _pinchOffset = new Vector3((ApplicationManager.Instance.IsAndroid ? -1 : 1) * 0.02f, -0.075f, 0.11f);
                    return;

                case InputType.Realistic:
                    switch(DeviceManager.Instance.System) {
                        case OperatingSystem.Meta:
                            _pinchOffset = new Vector3(0.02f, -0.08f, 0.11f);
                            return;
                        case OperatingSystem.Vive:
                            _pinchOffset = new Vector3(0.02f, -0.09f, 0.10f);
                            return;
                        case OperatingSystem.Varjo:
                            _pinchOffset = new Vector3(0.02f, -0.08f, 0.11f);
                            return;
                        default:
                            _pinchOffset = new Vector3(0.02f, -0.08f, 0.11f);
                            return;
                    }
            }
        }

        public Pose GetAnchorPose(Chirality chirality, PoseType poseType) {

            Pose pose = new();

            if (!InputDataManager.Instance.TryGetHand(chirality, out Hand hand)) {
                return pose;
            }

            Vector3 scale = new Vector3(chirality == Chirality.Left ? 1 : -1, 1, 1);

            Pose wrist = hand.Wrist;

            switch (poseType) {
                case PoseType.Pinch:

                    // TODO - CHECK TRANSFORMATION
                    pose.position = wrist.position + wrist.rotation * Vector3.Scale(_pinchOffset, scale);
                    pose.rotation = wrist.rotation;
                    break;

                case PoseType.Select:

                    pose = hand.GetJoint(FingerType.Index, JointType.Tip);
                    break;
                case PoseType.Side:

                    // TODO - CHECK TRANSFORMATION
                    pose.position = wrist.position + wrist.rotation * (Vector3.Scale(_sideOffset, scale));
                    pose.rotation = wrist.rotation;
                    break;
                case PoseType.Wrist:

                    pose = wrist;
                    break;
            }

            return pose;
        }
    }
}
