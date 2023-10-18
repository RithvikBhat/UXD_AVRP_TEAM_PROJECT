using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HCIG {

    public class SceneStartMarker : MonoBehaviour {

        // Just in Editor for us to see where we would start in the scene, deactivates itself during runtime
        void Awake() {
            gameObject.SetActive(false);
        }
    }
}
