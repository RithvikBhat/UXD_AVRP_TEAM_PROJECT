using System;
using System.Collections.Generic;

using UnityEngine;

using Leap.Unity;
using Leap.Unity.Interaction;
using Leap.Unity.Attributes;

using HCIG.Input.Data;
using HCIG.Input.Anchor;

namespace HCIG.Interaction {

    /// <summary>
    /// This class allows you to use hand tracking as the controller for the interaction engine.
    /// </summary>
    [DisallowMultipleComponent]
    public class PrimitivInteractionController : InteractionController {

        #region Inspector

        [Header("Controller Configuration")]

        [Tooltip("Which hand will hold this controller? This property cannot be changed "
               + "at runtime.")]
        [SerializeField, EditTimeOnly]
        private Input.Chirality _chirality;
        public Input.Chirality Chirality { get { return _chirality; } }

        [Header("Hover Configuration")]

        [Tooltip("This is the point used to determine the distance to objects for the "
               + "purposes of their 'hovered' state. Generally, it should be somewhere "
               + "between the tip of the controller and the controller's center of mass.")]
        [SerializeField]
        private Transform _hoverPoint;

        [Tooltip("These points refine the hover point when determining distances to "
               + "interaction objects for evaluating which object should be the primary hover "
               + "of this interaction controller. An object's proximity to one of these "
               + "points is interpreted as the user's intention to interact specifically "
               + "with that object, and is important when building less accident-prone user "
               + "interfaces. For example, hands place their primary hover points on the "
               + "thumb, index finger, and middle finger by default. Controllers generally "
               + "should have a primary hover point at any tip of the controller you expect "
               + "users might use to hit a button. Warning: Each point costs distance checks "
               + "against nearby objects, so making this list large is costly!")]
        [SerializeField]
        private new List<Transform> primaryHoverPoints;

        [Header("Grasping Configuration")]

        [Tooltip("The point around which to check objects eligible for being grasped. Only "
              + "objects with an InteractionBehaviour component with ignoreGrasping disabled "
              + "are eligible for grasping. Upon attempting to grasp with a controller, the "
              + "object closest to the grasp point is chosen for grasping.")]
        public Transform graspPoint;

        [Tooltip("The distance around the set graspPoint to check objects eligible for being grasped.")]
        public float maxGraspDistance = 0.06F;

        [Tooltip("The duration of time in seconds beyond initially pressing the grasp button "
               + "that the user can move the grasp point within range of a graspable "
               + "interaction object and still trigger a grasp. With a value of zero, objects "
               + "can only be grasped if they are already within the grasp distance of the "
               + "grasp point.")]
        public float graspTimingSlop = 0.10F;

        #endregion // Inspector

        #region Unity Events

        protected override void Reset() {
            base.Reset();

            hoverEnabled = true;
            contactEnabled = true;
            graspingEnabled = true;

            _hoverPoint = null;
            primaryHoverPoints.Clear();
            graspPoint = null;

            maxGraspDistance = 0.06F;
            graspTimingSlop = 0.1F;
        }

        protected override void Start() {
            base.Start();
        }

        protected override void fixedUpdateController() {

            // Colliders
            RefreshControllerTrackingData(HandAnchorManager.Instance.GetAnchorPose(Chirality, PoseType.Wrist));

            // Hover
            _hoverPoint.position = HandAnchorManager.Instance.GetAnchorPose(Chirality, PoseType.Select).position;

            // Grasp
            graspPoint.position = HandAnchorManager.Instance.GetAnchorPose(Chirality, PoseType.Pinch).position;
        }

        #endregion

        #region Controller Tracking

        private bool _hasTrackedPositionLastFrame = false;
        private Vector3 _trackedPositionLastFrame = Vector3.zero;
        private Quaternion _trackedRotationLastFrame = Quaternion.identity;

        private void RefreshControllerTrackingData(Pose pose) {

            refreshIsBeingMoved(pose.position, pose.rotation);

            if (_hasTrackedPositionLastFrame) {
                _trackedPositionLastFrame = transform.position;
                _trackedRotationLastFrame = transform.rotation;
            }

            transform.position = pose.position;
            transform.rotation = pose.rotation;

            refreshContactBoneTargets();

            if (!_hasTrackedPositionLastFrame) {
                _hasTrackedPositionLastFrame = true;
                _trackedPositionLastFrame = transform.position;
                _trackedRotationLastFrame = transform.rotation;
            }
        }

        #endregion

        #region Movement Detection

        private const float RIG_LOCAL_MOVEMENT_SPEED_THRESHOLD = 00.07F;
        private const float RIG_LOCAL_MOVEMENT_SPEED_THRESHOLD_SQR = RIG_LOCAL_MOVEMENT_SPEED_THRESHOLD * RIG_LOCAL_MOVEMENT_SPEED_THRESHOLD;
        private const float RIG_LOCAL_ROTATION_SPEED_THRESHOLD = 10.00F;
        private const float BEING_MOVED_TIMEOUT = 0.5F;

        private float _lastTimeMoved = 0F;
        private bool _isBeingMoved = false;
        private void refreshIsBeingMoved(Vector3 position, Quaternion rotation) {
            var isMoving = false;
            var baseTransform = manager.transform;

            // Check translation speed, relative to the Interaction Manager.
            var baseLocalPos = baseTransform.InverseTransformPoint(position);
            var baseLocalPosLastFrame = baseTransform.InverseTransformPoint(_trackedPositionLastFrame);
            var baseLocalSqrSpeed = ((baseLocalPos - baseLocalPosLastFrame) / Time.fixedDeltaTime).sqrMagnitude;

            if (baseLocalSqrSpeed > RIG_LOCAL_MOVEMENT_SPEED_THRESHOLD_SQR) {
                isMoving = true;
            }

            // Check rotation speed, relative to the Interaction Manager.
            var baseLocalRot = baseTransform.InverseTransformRotation(rotation);
            var baseLocalRotLastFrame = baseTransform.InverseTransformRotation(_trackedRotationLastFrame);
            var baseLocalAngularSpeed = Quaternion.Angle(baseLocalRot, baseLocalRotLastFrame) / Time.fixedDeltaTime;

            if (baseLocalAngularSpeed > RIG_LOCAL_ROTATION_SPEED_THRESHOLD) {
                isMoving = true;
            }

            if (isMoving) {
                _lastTimeMoved = Time.fixedTime;
            }

            // "isMoving" lasts for a bit after the controller stops moving, to avoid
            // rapid oscillation of the value.
            var timeSinceLastMoving = Time.fixedTime - _lastTimeMoved;
            _isBeingMoved = timeSinceLastMoving < BEING_MOVED_TIMEOUT;
        }

        #endregion

        #region General InteractionController Implementation

        /// <summary>
        /// Gets whether or not the underlying controller is currently tracked
        /// </summary>
        public override bool isTracked {
            get {
                return isActiveAndEnabled && InputDataManager.Instance.IsHandAvailable(Chirality);
            }
        }

        /// <summary>
        /// Gets whether or not the underlying controller is currently being moved in world
        /// space, but relative to the Interaction Manager's transform. The Interaction
        /// Manager is usually a sibling of the main camera beneath the camera rig transform,
        /// so that if your application is only translating the player rig in space, this
        /// method won't incorrectly return true.
        /// </summary>
        public override bool isBeingMoved {
            get {
                return _isBeingMoved;
            }
        }


        /// <summary>
        /// Gets whether the controller is a left-hand controller.
        /// </summary>
        public override bool isLeft {
            get { return Chirality == Input.Chirality.Left; }
        }

        /// <summary>
        /// Gets the last-tracked position of the controller.
        /// </summary>
        public override Vector3 position {
            get {
                return transform.position;
            }
        }

        /// <summary>
        /// Gets the last-tracked rotation of the controller.
        /// </summary>
        public override Quaternion rotation {
            get {
                return transform.rotation;
            }
        }

        /// <summary>
        /// Gets the current velocity of the controller.
        /// </summary>
        public override Vector3 velocity {
            get {
                if (_hasTrackedPositionLastFrame) {
                    return (transform.position - _trackedPositionLastFrame) / Time.fixedDeltaTime;
                } else {
                    return Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Gets the type of controller this is. For InteractionVRController, the type is
        /// always ControllerType.VRController.
        /// </summary>
        public override ControllerType controllerType {
            get { return ControllerType.XRController; }
        }

        /// <summary>
        /// This implementation of InteractionControllerBase represents just a virtual Leap hand,
        /// so the Interaction Manager gets no problems with his inspector editor script.
        /// </summary>
        public override InteractionHand intHand {
            get {
                if (_intHand == null && ApplicationManager.Instance.IsEditor && !Application.isPlaying) {
                    _intHand = new InteractionHand();
                }
                return _intHand;
            }
        }
        InteractionHand _intHand = null;

        /// <summary>
        /// InteractionVRController doesn't need to do anything when an object is
        /// unregistered.
        /// </summary>
        protected override void onObjectUnregistered(IInteractionBehaviour intObj) { }

        #endregion

        #region Hover Implementation

        /// <summary>
        /// Gets the center point used for hover distance checking.
        /// </summary>
        public override Vector3 hoverPoint {
            get { return _hoverPoint == null ? Vector3.zero : _hoverPoint.position; }
        }

        /// <summary>
        /// Gets the list of points to be used to perform higher-fidelity "primary hover"
        /// checks. Only one interaction object may be the primary hover of an interaction
        /// controller (Leap hand or otherwise) at a time. Interface objects such as buttons
        /// can only be pressed when they are primarily hovered by an interaction controller,
        /// so it's best to return points on whatever you expect to be able to use to push
        /// buttons with the controller.
        /// </summary>
        protected override List<Transform> _primaryHoverPoints {
            get { return primaryHoverPoints; }
        }

        #endregion

        #region Contact Implementation

        private Vector3[] _contactBoneLocalPositions;
        private Quaternion[] _contactBoneLocalRotations;

        private Vector3[] _contactBoneTargetPositions;
        private Quaternion[] _contactBoneTargetRotations;

        private ContactBone[] _contactBones;
        public override ContactBone[] contactBones {
            get { return _contactBones; }
        }

        private GameObject _contactBoneParent;
        protected override GameObject contactBoneParent {
            get { return _contactBoneParent; }
        }

        protected override bool initContact() {
            initContactBones();

            if (_contactBoneParent == null) {
                _contactBoneParent = new GameObject("VR Controller Contact Bones "
                                                  + (isLeft ? "(Left)" : "(Right"));
            }

            foreach (var contactBone in _contactBones) {
                contactBone.transform.parent = _contactBoneParent.transform;
            }

            return true;
        }

        private void refreshContactBoneTargets() {
            if (_wasContactInitialized) {

                if (!InputDataManager.Instance.TryGetHand(Chirality, out Input.Hand hand)) {
                    return;
                }

                Pose jointPrev;
                Pose jointNext;

                int fingerID = (int)Input.JointType.Proximal;

                for (int i = 0; i < _contactBones.Length; i++) {

                    // Palm
                    if (_contactBones[i].collider is BoxCollider) {

                        _contactBoneTargetPositions[i] = hand.Wrist.position;
                        _contactBoneTargetRotations[i] = hand.Wrist.rotation;

                        continue;
                    }

                    // Joints
                    jointPrev = hand.GetJoint(Input.FingerType.Index, (Input.JointType)fingerID);
                    jointNext = hand.GetJoint(Input.FingerType.Index, (Input.JointType)(fingerID + 1));

                    // Height
                    (_contactBones[i].collider as CapsuleCollider).height = Vector3.Distance(jointPrev.position, jointNext.position) + 0.02f;

                    // The middle collider is handled as a trigger - so that it does not irritate the other two colliders
                    //(_contactBones[i].collider as CapsuleCollider).isTrigger = (Input.JointType)fingerID == Input.JointType.Intermediate;

                    // Localization
                    _contactBoneTargetPositions[i] = (jointPrev.position + jointNext.position) / 2;
                    _contactBoneTargetRotations[i] = Quaternion.LookRotation(jointNext.position - jointPrev.position);

                    // Increase fingerID
                    fingerID += 2;
                }
            }
        }

        private List<ContactBone> _contactBoneBuffer = new List<ContactBone>();
        private List<Collider> _colliderBuffer = new List<Collider>();

        private void initContactBones() {

            _colliderBuffer.Clear();
            _contactBoneBuffer.Clear();

            // Scan for existing colliders and construct contact bones out of them.
            Utils.FindColliders<Collider>(gameObject, _colliderBuffer, includeInactiveObjects: true);

            foreach (var collider in _colliderBuffer) {
                if (collider.isTrigger) continue; // Contact Bones are for "contacting" colliders.

                ContactBone contactBone = collider.gameObject.AddComponent<ContactBone>();
                Rigidbody body = collider.gameObject.GetComponent<Rigidbody>();
                if (body == null) {
                    body = collider.gameObject.AddComponent<Rigidbody>();
                }

                body.freezeRotation = true;
                body.useGravity = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.mass = 1F;

                contactBone.interactionController = this;
                contactBone.rigidbody = body;
                contactBone.collider = collider;

                _contactBoneBuffer.Add(contactBone);
            }

            //The number of bones is equal to the colliders found that are not set to trigger
            int numBones = _contactBoneBuffer.Count;
            _contactBones = new ContactBone[numBones];
            _contactBoneLocalPositions = new Vector3[numBones];
            _contactBoneLocalRotations = new Quaternion[numBones];
            _contactBoneTargetPositions = new Vector3[numBones];
            _contactBoneTargetRotations = new Quaternion[numBones];

            for (int i = 0; i < numBones; i++) {
                _contactBones[i] = _contactBoneBuffer[i];

                _contactBoneLocalPositions[i] = _contactBoneTargetPositions[i] = transform.InverseTransformPoint(_contactBones[i].transform.position);
                _contactBoneLocalRotations[i] = _contactBoneTargetRotations[i] = transform.InverseTransformRotation(_contactBones[i].transform.rotation);
            }
        }

        protected override void getColliderBoneTargetPositionRotation(int contactBoneIndex, out Vector3 targetPosition, out Quaternion targetRotation) {
            targetPosition = _contactBoneTargetPositions[contactBoneIndex];
            targetRotation = _contactBoneTargetRotations[contactBoneIndex];
        }

        #endregion

        #region Grasping Implementation

        /// <summary>
        /// Gets a list returning this controller's hoverPoint. Because the
        /// InteractionVRController represents a rigid controller, any two points that
        /// rigidly move with the controller position and orientation will provide enough
        /// information.
        /// </summary>
        public override List<Vector3> graspManipulatorPoints {
            get {
                _graspManipulatorPointsBuffer.Clear();
                _graspManipulatorPointsBuffer.Add(hoverPoint);
                _graspManipulatorPointsBuffer.Add(hoverPoint + this.transform.rotation * Vector3.forward * 0.05F);
                _graspManipulatorPointsBuffer.Add(hoverPoint + this.transform.rotation * Vector3.right * 0.05F);
                return _graspManipulatorPointsBuffer;
            }
        }
        private List<Vector3> _graspManipulatorPointsBuffer = new List<Vector3>();

        private IInteractionBehaviour _closestGraspableObject = null;

        private bool _graspButtonLastFrame = false;
        private bool _graspButtonDown = false;
        private bool _graspButtonUp = false;
        private float _graspButtonDownSlopTimer = 0F;

        public override Vector3 GetGraspPoint() {
            return graspPoint.transform.position;
        }

        protected override void fixedUpdateGraspingState() {
            refreshClosestGraspableObject();

            fixedUpdateGraspButtonState();
        }

        private void refreshClosestGraspableObject() {
            _closestGraspableObject = null;

            float closestGraspableDistance = float.PositiveInfinity;
            foreach (var intObj in graspCandidates) {
                float testDist = intObj.GetHoverDistance(graspPoint.position);
                if (testDist < maxGraspDistance && testDist < closestGraspableDistance) {
                    _closestGraspableObject = intObj;
                    closestGraspableDistance = testDist;
                }
            }
        }

        private void fixedUpdateGraspButtonState(bool ignoreTemporal = false) {
            _graspButtonDown = false;
            _graspButtonUp = false;

            bool graspButton = InputDataManager.Instance.IsHandPinching(Chirality);

            if (!_graspButtonLastFrame) {
                if (graspButton) {
                    // Grasp button was _just_ depressed this frame.
                    _graspButtonDown = true;
                    _graspButtonDownSlopTimer = graspTimingSlop;
                }
            } else {
                if (!graspButton) {
                    // Grasp button was _just_ released this frame.
                    _graspButtonUp = true;
                    _graspButtonDownSlopTimer = 0F;
                }
            }

            if (_graspButtonDownSlopTimer > 0F) {
                _graspButtonDownSlopTimer -= Time.fixedDeltaTime;
            }

            _graspButtonLastFrame = graspButton;
        }

        protected override bool checkShouldGrasp(out IInteractionBehaviour objectToGrasp) {
            bool shouldGrasp = !isGraspingObject && (_graspButtonDown || _graspButtonDownSlopTimer > 0F) && _closestGraspableObject != null;

            objectToGrasp = null;

            if (shouldGrasp) {
                objectToGrasp = _closestGraspableObject;
            }

            return shouldGrasp;
        }

        /// <summary>
        /// If the provided object is within range of this VR controller's grasp point and
        /// the grasp button is currently held down, this method will manually initiate a
        /// grasp and return true. Otherwise, the method returns false.
        /// </summary>
        protected override bool checkShouldGraspAtemporal(IInteractionBehaviour intObj) {
            bool shouldGrasp = !isGraspingObject && _graspButtonLastFrame && intObj.GetHoverDistance(graspPoint.position) < maxGraspDistance;

            if (shouldGrasp) {
                var tempControllers = Pool<List<InteractionController>>.Spawn();
                try {
                    intObj.BeginGrasp(tempControllers);
                } finally {
                    tempControllers.Clear();
                    Pool<List<InteractionController>>.Recycle(tempControllers);
                }
            }

            return shouldGrasp;
        }

        protected override bool checkShouldRelease(out IInteractionBehaviour objectToRelease) {
            bool shouldRelease = _graspButtonUp && isGraspingObject;

            if (shouldRelease) {
                objectToRelease = graspedObject;
            } else {
                objectToRelease = null;
            }

            return shouldRelease;
        }

        #endregion

        #region Gizmos

        public override void OnDrawRuntimeGizmos(Leap.Unity.RuntimeGizmos.RuntimeGizmoDrawer drawer) {
            base.OnDrawRuntimeGizmos(drawer);

            // Grasp Point
            float graspAmount = 0F;
            try {
                graspAmount = InputDataManager.Instance.IsHandPinching(Chirality) ? 1 : 0;
            } catch (ArgumentException) { }

            drawer.color = Color.Lerp(GizmoColors.GraspPoint, Color.white, graspAmount);
            drawer.DrawWireSphere(GetGraspPoint(), maxGraspDistance);

            // Nearest graspable object
            if (_closestGraspableObject != null) {
                drawer.color = Color.Lerp(GizmoColors.Graspable, Color.white, Mathf.Sin(Time.time * 2 * Mathf.PI * 2F));
                drawer.DrawWireSphere(_closestGraspableObject.rigidbody.position, maxGraspDistance * 0.75F);
            }
        }

        #endregion

    }
}
