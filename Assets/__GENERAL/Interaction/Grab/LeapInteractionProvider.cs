using UnityEngine;

using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;

using HCIG.Input.Data;
using HCIG.Input;

namespace HCIG.Interaction.LeapMotion {

    public class LeapInteractionProvider : LeapProvider {

        [Header("Wrist")]
        [SerializeField]
        [Tooltip("Offset from our wrist position")]
        private Vector3 position = new Vector3(0.01f, 0, 0.07f);

        [SerializeField]
        [Tooltip("Offset from our wrist rotation")]
        private Vector3 rotation = new Vector3(0, 15f, 0);

        public override Frame CurrentFrame {
            get {
                return _currentFrame;
            }
        }
        private Frame _currentFrame = new Frame();


        public override Frame CurrentFixedFrame {
            get {
                return _currentFixedFrame;
            }
        }
        private Frame _currentFixedFrame = new Frame();


        private void OnEnable() {
            Application.onBeforeRender += OnBeforeRenderUpdate;
        }

        private void OnDisable() {
            Application.onBeforeRender -= OnBeforeRenderUpdate;
        }


        /// <summary>
        /// Normal - Frame
        /// </summary>
        private void OnBeforeRenderUpdate() {

            UpdateFrame(_currentFrame);
            DispatchUpdateFrameEvent(_currentFrame);
        }


        /// <summary>
        /// Fixed - Frame
        /// </summary>
        private void FixedUpdate() {

            UpdateFrame(_currentFixedFrame);
            DispatchFixedFrameEvent(_currentFixedFrame);
        }


        /// <summary>
        /// fill leap hands with data to get access to interaction engine
        /// </summary>
        /// <param name="frame"></param>
        void UpdateFrame(Frame frame) {

            frame.Hands = new();

            if (!InputDataManager.Instance.IsDataAvailable) {
                return;
            }

            Input.Hand hand;

            // Left Hand
            if (InputDataManager.Instance.TryGetHand(Input.Chirality.Left, out hand)) {
                frame.Hands.Add(ConvertHandData(hand));
            }

            // Right Hand
            if (InputDataManager.Instance.TryGetHand(Input.Chirality.Right, out hand)) {
                frame.Hands.Add(ConvertHandData(hand));
            }
        }


        /// <summary>
        /// Converts our data into leap hands
        /// </summary>
        private Leap.Hand ConvertHandData(Input.Hand hand) {

            Leap.Hand leapHand = new();
            VectorHand vecHand = new();
            
            // Chirality
            vecHand.isLeft = hand.Chirality == Input.Chirality.Left;

            // Palm
            // TODO - CHECK INVERSE TRANSFORMATION
            vecHand.palmPos = hand.Wrist.position + hand.Wrist.rotation * position;
            vecHand.palmRot = hand.Wrist.rotation * Quaternion.Euler(rotation * (vecHand.isLeft ? -1 : 1));

            int jointID = 0;

            // Fingers
            for (int f = 0; f < (int)FingerType.MAX_FINGER_COUNT; f++) {

                if (f == (int)FingerType.Thumb) {
                    vecHand.jointPositions[jointID++] = VectorHand.ToLocal(hand.GetJoint(FingerType.Thumb, JointType.Metacarpal).position, vecHand.palmPos, vecHand.palmRot);
                }

                for (int j = 0; j < (int)JointType.MAX_JOINT_COUNT; j++) {

                    if ((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                        continue;
                    }

                    vecHand.jointPositions[jointID++] = VectorHand.ToLocal(hand.GetJoint((FingerType)f, (JointType)j).position, vecHand.palmPos, vecHand.palmRot);
                }
            }

            vecHand.Decode(leapHand);

            return leapHand;
        }
    }
}