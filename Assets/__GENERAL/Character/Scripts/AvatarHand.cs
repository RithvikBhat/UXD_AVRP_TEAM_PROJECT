using UnityEngine;

using HCIG.Input;
using HCIG.Input.Data;
using HCIG.Network;

using System;
using System.Collections.Generic;

using Photon.Pun;

namespace HCIG.Avatar {

    public class AvatarHand : Hand, IPunObservable {

        PhotonView _photonView;

        [Header("Wrist")]
        [Tooltip("positional offset of the model wrist to our data wrist position")]
        [SerializeField]
        private Vector3 _position = Vector3.zero;
        [Tooltip("rotational offset of the model wrist to our data wrist rotation")]
        [SerializeField]
        private Vector3 _rotation = Vector3.zero;

        [Header("Offset")]
        [Tooltip("Offset of the forward of the current model to the normally forward (blue axis) of a hand")]
        [SerializeField]
        private Vector3 _forward = Vector3.zero;

        private bool _isValid = false;


        // Safety-Parameter: helps us with the finger rotation calculation.
        // It is used to locally offset the last finger point down so that if the fingers are overstretched,
        // the segments do not rotate away because the angle of extension then becomes negative
        private float _safetyOffset = 0.01f;

        private void Start() {
            _photonView = GetComponentInParent<PhotonView>();

            if (Chirality == Chirality.Right) {

                _position = Vector3.Scale(_position, -Vector3.one);

                _rotation = Vector3.Scale(_rotation, -Vector3.one);
                _rotation += new Vector3(180, 0, 0);

                _forward = Vector3.Scale(_forward, -Vector3.one);
                _forward += new Vector3(180, 0, 0);
            }
        }

        #region Synchro

        /// <summary>
        /// Sync the visual hand with our current available hand data (realistic or controller), when his is our hand
        /// </summary>
        private void FixedUpdate() {
            if (!NetworkManager.Instance.InLobby && !_photonView.IsMine) {
                return;
            }

            Hand data = InputDataManager.Instance.GetHand(Chirality);

            if (!data.IsValid) {
                Wrist = new Pose(new Vector3(0, -100, 0), Quaternion.identity);
                _isValid = false;
                return;
            }

            // Wrist
            Quaternion rotation = data.Wrist.rotation * Quaternion.Euler(_rotation);
            Vector3 position = data.Wrist.position + rotation * _position;

            Wrist = new Pose(position, rotation);

            // Fingers
            SyncFingers(data);

            _isValid = true;
        }

        public override bool IsValid {
            get {
                return _isValid;
            }
        }

        private void SyncFingers(Hand data) {

            for (int f = 0; f < (int)FingerType.MAX_FINGER_COUNT; f++) {

                // References to the two relevant data joints
                Pose thisJoint = default;
                Pose nextJoint = default;

                // References to finger- & Joint-Types to determine wich joint should be synced next
                FingerType finger = (FingerType)f;
                JointType joint = (JointType)(-1);

                // Reference upward for the finger rotation calibration
                Vector3 upward = Vector3.zero;

                for (int j = 0; j < (int)JointType.MAX_JOINT_COUNT; j++) {

                    // Get the paring data joint...
                    if (IsNullPose(thisJoint)) {
                        if (IsNullPose(thisJoint = data.GetJoint(finger, (JointType)j))) {
                            continue;
                        } else {
                            // for this joint we have data, so we sync the next hand joint with this joint
                            joint = (JointType)j;

                            // calculate first reference direction
                            if (upward == Vector3.zero) {

                                // Tip
                                Vector3 tip;

                                if (finger == FingerType.Thumb) {
                                    // tip (slightly raised wrist)
                                    tip = Wrist.position - (Chirality == Chirality.Left ? Wrist.up : -Wrist.up).normalized * 2 * _safetyOffset;
                                } else {
                                    // tip (slightly raised virtual metacarpal)
                                    if (Chirality == Chirality.Left) {
                                        tip = thisJoint.position - _safetyOffset * 2 * Wrist.up.normalized + _safetyOffset * 5 * Wrist.right.normalized;
                                    } else {
                                        tip = thisJoint.position + _safetyOffset * 2 * Wrist.up.normalized - _safetyOffset * 5 * Wrist.right.normalized;
                                    }
                                }

                                // Root
                                Vector3 root = thisJoint.position;

                                //Debug.DrawLine(root, tip);

                                upward = tip - root;
                            }
                        }
                    }

                    Vector3 position = thisJoint.position;
                    Quaternion rotation;

                    if (j + 1 == (int)JointType.MAX_JOINT_COUNT) {
                        // we have already reached the tip

                        rotation = Quaternion.identity;
                    } else {
                        // we are in the middle of the finger

                        // get the next data joint for direction calculation 
                        if (IsNullPose(nextJoint) && IsNullPose(nextJoint = data.GetJoint(finger, (JointType)(j + 1)))) {
                            continue;
                        }

                        // new forward direction of our current bone 
                        Vector3 forward = (nextJoint.position - thisJoint.position).normalized;

                        // calculate with old reference direction new right vector
                        Vector3 right = Vector3.Cross(forward, upward);

                        // calculate new correct upwards vector
                        upward = Vector3.Cross(forward, right);

                        // calulate the correct rotation of the bone (in the matched model coordinate system)
                        rotation = Quaternion.LookRotation(forward, upward) * Quaternion.Euler(_forward);

                        // calculate reference direction for next bone
                        upward = (thisJoint.position - upward.normalized * 2 * _safetyOffset - nextJoint.position).normalized;

                        // Preperations for next bone
                        thisJoint = default;
                        nextJoint = default;
                    }

                    // Update hand joint with data
                    SetJoint(finger, joint, new Pose(position, rotation));
                }
            }
        }

        #endregion Synchro

        #region Network

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

            if (!gameObject.activeInHierarchy) {
                return;
            }

            if (stream.IsWriting) {
                // Sender

                if (!IsValid) {
                    stream.SendNext(new byte[0]);
                } else {
                    stream.SendNext(Serialize());
                }
            } else {
                // Receiver

                int listCount = stream.Count;
                int tryCount = 0;
                List<byte> data;

                do {
                    try {
                        data = new List<byte>((byte[])stream.ReceiveNext());

                        if (data.Count == 0) {
                            // Hide hand
                            Wrist = new Pose(new Vector3(0, -100, 0), Quaternion.identity);
                        } else {
                            // Show hand
                            Deserialize(data);
                        }
                        return;
                    } catch {
                        tryCount++;
                    }
                } while (tryCount <= listCount);
            }
        }

        /// <summary>
        /// Writes all relevant information into a byte array for network transfer
        /// </summary>
        protected byte[] Serialize() {

            List<byte> bytes = new List<byte>();

            // Wrist Position - 12 Byte
            bytes.AddRange(VecToBytes(Wrist.position));

            // Wrist rotation -  4 Byte
            bytes.AddRange(QuatToBytes(Wrist.rotation));

            // Joint positions - 72 Byte (in relation to the wrist)
            for (int f = 0; f < (int)FingerType.MAX_FINGER_COUNT; f++) {

                for (int j = 0; j < (int)JointType.MAX_JOINT_COUNT; j++) {

                    if ((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                        // skip impossible joints
                        continue;
                    }

                    bytes.AddRange(VecToBytes(GetJoint((FingerType)f, (JointType)j).position - Wrist.position, true));
                }
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Reads byte array and fills all relevant values with network data 
        /// </summary>
        /// <param name="bytes"></param>
        protected void Deserialize(List<byte> bytes) {

            VirtualHand data = new(Chirality);

            // Wrist Position - 12 Byte
            Vector3 position = BytesToVec(GetSegment(ref bytes, 12));

            // Wrist Rotation -  4 Byte
            Quaternion rotation = BytesToQuat(GetSegment(ref bytes, 4));

            // Joint positions - var Byte - up to 72 (in relation to the wrist)
            for (int f = 0; f < (int)FingerType.MAX_FINGER_COUNT; f++) {

                for (int j = 0; j < (int)JointType.MAX_JOINT_COUNT; j++) {

                    if((FingerType)f == FingerType.Thumb && (JointType)j == JointType.Intermediate) {
                        // skip impossible joints
                        continue;
                    }

                    data.SetJoint((FingerType)f, (JointType)j, new Pose(BytesToVec(GetSegment(ref bytes, 3)) + position , Quaternion.identity));
                }
            }

            // Wrist
            Wrist = new Pose(position, rotation);

            // Fingers
            SyncFingers(data);
        }


        #endregion Networking

        #region Conversions

        /// <summary>
        /// returns a byte array with the length of 4 with a compressed value of the given quaternion.
        /// 
        /// This encoding ONLY works with normalized Quaternions, taking advantage of the fact that their
        /// components sum to 1 to only encode three of Quaternion components. As a result,
        /// this method encodes a Quaternion as a single unsigned integer (4 bytes).
        /// 
        /// Sources:
        /// https://bitbucket.org/Unity-Technologies/networking/pull-requests/9/quaternion-compression-for-sending/diff
        /// and
        /// http://stackoverflow.com/questions/3393717/c-sharp-converting-uint-to-byte
        /// </summary>
        private byte[] QuatToBytes(Quaternion quat) {
            int largest = 0;
            float a, b, c;

            float abs_w = Mathf.Abs(quat.w);
            float abs_x = Mathf.Abs(quat.x);
            float abs_y = Mathf.Abs(quat.y);
            float abs_z = Mathf.Abs(quat.z);

            float largest_value = abs_x;

            if (abs_y > largest_value) {
                largest = 1;
                largest_value = abs_y;
            }
            if (abs_z > largest_value) {
                largest = 2;
                largest_value = abs_z;
            }
            if (abs_w > largest_value) {
                largest = 3;
                largest_value = abs_w;
            }
            if (quat[largest] >= 0f) {
                a = quat[(largest + 1) % 4];
                b = quat[(largest + 2) % 4];
                c = quat[(largest + 3) % 4];
            } else {
                a = -quat[(largest + 1) % 4];
                b = -quat[(largest + 2) % 4];
                c = -quat[(largest + 3) % 4];
            }

            // serialize
            const float minimum = -1.0f / 1.414214f;        // note: 1.0f / sqrt(2)
            const float maximum = +1.0f / 1.414214f;
            const float delta = maximum - minimum;
            const uint maxIntegerValue = (1 << 10) - 1; // 10 bits
            const float maxIntegerValueF = (float)maxIntegerValue;
            float normalizedValue;
            uint integerValue;

            uint sentData = ((uint)largest) << 30;
            // a
            normalizedValue = Mathf.Clamp01((a - minimum) / delta);
            integerValue = (uint)Mathf.Floor(normalizedValue * maxIntegerValueF + 0.5f);
            sentData = sentData | ((integerValue & maxIntegerValue) << 20);
            // b
            normalizedValue = Mathf.Clamp01((b - minimum) / delta);
            integerValue = (uint)Mathf.Floor(normalizedValue * maxIntegerValueF + 0.5f);
            sentData = sentData | ((integerValue & maxIntegerValue) << 10);
            // c
            normalizedValue = Mathf.Clamp01((c - minimum) / delta);
            integerValue = (uint)Mathf.Floor(normalizedValue * maxIntegerValueF + 0.5f);
            sentData = sentData | (integerValue & maxIntegerValue);

            return BitConverter.GetBytes(sentData);
        }

        /// <summary>
        /// returns a Quaternion back of the given bytes.
        /// 
        /// Sources:
        /// https://bitbucket.org/Unity-Technologies/networking/pull-requests/9/quaternion-compression-for-sending/diff
        /// and
        /// http://stackoverflow.com/questions/3393717/c-sharp-converting-uint-to-byte
        /// </summary>
        public Quaternion BytesToQuat(byte[] bytes) {
            uint readData = BitConverter.ToUInt32(bytes);

            int largest = (int)(readData >> 30);
            float a, b, c;

            const float minimum = -1.0f / 1.414214f;        // note: 1.0f / sqrt(2)
            const float maximum = +1.0f / 1.414214f;
            const float delta = maximum - minimum;
            const uint maxIntegerValue = (1 << 10) - 1; // 10 bits
            const float maxIntegerValueF = (float)maxIntegerValue;
            uint integerValue;
            float normalizedValue;
            // a
            integerValue = (readData >> 20) & maxIntegerValue;
            normalizedValue = (float)integerValue / maxIntegerValueF;
            a = (normalizedValue * delta) + minimum;
            // b
            integerValue = (readData >> 10) & maxIntegerValue;
            normalizedValue = (float)integerValue / maxIntegerValueF;
            b = (normalizedValue * delta) + minimum;
            // c
            integerValue = readData & maxIntegerValue;
            normalizedValue = (float)integerValue / maxIntegerValueF;
            c = (normalizedValue * delta) + minimum;

            Quaternion value = Quaternion.identity;
            float d = Mathf.Sqrt(1f - a * a - b * b - c * c);
            value[largest] = d;
            value[(largest + 1) % 4] = a;
            value[(largest + 2) % 4] = b;
            value[(largest + 3) % 4] = c;

            return value;
        }




        /// <summary>
        /// Converts a Vector3 into a byte-array. With the value "compressed" Compresses a float into a byte based on the desired movement range.
        /// </summary>
        public byte[] VecToBytes(Vector3 inVector, bool compressed = false) {

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < 3; i++) {

                if (compressed) {

                    float compressRange = 0.3f;

                    float clamped = Mathf.Clamp(inVector[i], -compressRange / 2f, compressRange / 2f);
                    clamped += compressRange / 2f;
                    clamped /= compressRange;
                    clamped *= 255f;
                    clamped = Mathf.Floor(clamped);

                    bytes.Add((byte)clamped);

                } else {
                    bytes.AddRange(BitConverter.GetBytes(inVector[i]));
                }
            }

            return bytes.ToArray();
        }


        /// <summary>
        /// Converts a byte array back into a Vector3 position. Depending on the lenght of the array we have a compressed or uncompressed vector.
        /// </summary>
        public Vector3 BytesToVec(byte[] inBytes) {

            Vector3 vector = new Vector3();

            for (int i = 0; i < 3; i++) {

                if (inBytes.Length == 3) {
                    // Compressed

                    float compressRange = 0.3f;

                    float clamped = inBytes[i];
                    clamped /= 255f;
                    clamped *= compressRange;
                    clamped -= compressRange / 2f;

                    vector[i] = clamped;
                } else {
                    // Uncompressed

                    vector[i] = BitConverter.ToSingle(inBytes, i * 4);
                }
            }

            return vector;
        }

        /// <summary>
        /// Returns a byte array of the given list and removes the bytes from the list, so we can smoother through work the received data stream
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] GetSegment(ref List<byte> input, int length = 1) {

            if (length < 1) {
                length = 1;
            }

            // get needed bytes
            byte[] bytes = input.GetRange(0, length).ToArray();

            // remove bytes from list
            input.RemoveRange(0, length);

            return bytes;
        }

        #endregion Conversions
    }
}
