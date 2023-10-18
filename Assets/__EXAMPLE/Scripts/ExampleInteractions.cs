using HCIG.UI;

using Photon.Pun;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace HCIG.Course.AVRP {

    [RequireComponent(typeof(PhotonView))]
    public class ExampleInteractions : MonoBehaviour {

        private PhotonView _photonView = null;

        private TMP_InputField _inputField = null;

        [Header("Object Created")]
        [SerializeField]
        private GameObject _prefab = null;

        [Header("Interactables")]
        [SerializeField]
        private Button _button = null;

        /// <summary>
        /// Will be called ONCE when script gets activated for the first time [BEFORE -> Start()]
        /// </summary>
        private void Awake() {

            _inputField = FindObjectOfType<TMP_InputField>();

            // Inputfield - Code
            _inputField.onSelect.AddListener((_) => OpenKeyboard()); 

            // Button - Code
            if(_button != null) {
                _button.onClick.AddListener(() => Button2DPressed(_button));
            }

            // Networking
            _photonView = GetComponent<PhotonView>();
        }

        /// <summary>
        /// Will be called ONCE when script gets activated for the first time [AFTER -> Awake()]
        /// </summary>
        private void Start() {

        }

        /// <summary>
        /// Will be called every frame when the script is currently activated [time steps of call before and next one can vary]
        /// </summary>
        private void Update() {

        }

        /// <summary>
        /// Will be called in a fixed time step [set in "Project Settings" -> "Time" -> "Fixed Timestep"] 
        /// </summary>
        private void FixedUpdate() {

        }

        #region Button 2D

        /// <summary>
        /// This function was added to a button by code
        /// 
        /// PRO: you see the connection/ callers of this function
        /// 
        /// CON: you have to get button in your script [access interactable via inspector OR by "FindObjectOf..."-functionalities]
        /// </summary>
        private void Button2DPressed(Button button) {
            button.targetGraphic.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        #endregion Button 2D

        #region Button 3D

        /// <summary>
        /// This function was added to a button via inspector
        /// 
        /// PRO: easy drag and drop selection of functin in inspector 
        /// 
        /// CON: hard to keep track of the calling parties & if reference once accidentally deleted in the inspector... 
        ///      very hard to set up the sequence/calls again, because absolute loss of the needed function is almost inevitable
        /// </summary>
        public void Button3DPressed() {
            Instantiate(_prefab, new Vector3(0, 2, 1), Quaternion.identity);
        }

        #endregion Button 3D

        #region Inputfield

        /// <summary>
        /// 1) Opens the available VR keyboard, so we can type in some text
        /// 
        /// 2) When finished (Keyboard gets closed by us...), the typed in text gets synced to all networkers
        /// </summary>
        public void OpenKeyboard() {

            // 1)
            KeyboardManager.Instance.Open((text) => {

                // 2)
                _photonView.RPC(nameof(UpdateInputField), RpcTarget.All, text);
            });
        }

        /// <summary>
        /// Types of RpcTarget...
        ///  - All               (every one in the session gets the function triggered)
        ///  - MasterClient      (just the creator of the session gets called)
        ///  - Other             (everyone except of us gets triggered)
        ///  
        /// OR 
        /// 
        /// we name an explicit player
        ///  - just on this selected player the function gets called 
        /// </summary>
        [PunRPC]
        private void UpdateInputField(string text) {
            _inputField.text = text;
        }

        #endregion Inputfield
    }
}