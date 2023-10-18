using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

namespace HCIG.UI {

    public class RoomListItem : MonoBehaviour {

        [Header("Components")]
        [SerializeField]
        private TMP_Text _name;

        [SerializeField]
        private TMP_Text _number;

        [SerializeField]
        private Button _enter;

        /// <summary>
        /// Initialization of the room list item with a "room" string and the feedback function that is called when we click the button
        /// </summary>
        /// <param name="room"></param>
        /// <param name="curPlayers"></param>
        /// <param name="maxPlayers"></param>
        /// <param name="selectionFeedback"></param>
        public void Initialize(string room, int curPlayers, int maxPlayers, Action<string> selectionFeedback) {
            
            // list 
            _name.text = room;
            _number.text = curPlayers + " / " + maxPlayers;

            // feedback 
            _enter.onClick.AddListener(() => selectionFeedback.Invoke(room));        
        }

        /// <summary>
        /// Initialization of the room list item with a specified scene and the feedback function that is called when we click the button
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="curPlayers"></param>
        /// <param name="maxPlayers"></param>
        /// <param name="selectionFeedback"></param>
        public void Initialize(Scene environment, int curPlayers, int maxPlayers, Action<Scene> selectionFeedback) {

            // list 
            _name.text = environment.ToString();
            _number.text = curPlayers + " / " + maxPlayers;

            // feedback 
            _enter.onClick.AddListener(() => selectionFeedback.Invoke(environment));
        }
    }
}