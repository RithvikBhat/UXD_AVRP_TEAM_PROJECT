using Unity.XR.CoreUtils;

using UnityEngine;

namespace HCIG {

    /// <summary>
    /// All relevant informations of the XR Rig that could be interesting for others
    /// </summary>
    public class BaseXR : Singleton<BaseXR> {

        protected XROrigin Origin {
            get {
                if (_origin == null) {
                    _origin = GetComponentInChildren<XROrigin>(true);
                }
                return _origin;
            }
        }
        private XROrigin _origin = null;

        public Camera Camera {
            get {
                return Origin.Camera;
            }
        }

        public Transform Offset {
            get {

                return Origin.CameraFloorOffsetObject.transform;
            }
        }
    }
}
