using System.Collections.Generic;
using UnityEngine;


namespace HCIG.Input.Data {

    public class InputData : MonoBehaviour {

        [Header("Data")]
        protected Hand _left = new VirtualHand();
        protected Hand _right = new VirtualHand();

        protected virtual void Awake() {

            foreach (Hand hand in GetComponentsInChildren<Hand>(true)){
                if(hand.Chirality == Chirality.Left) {
                    _left = hand;
                } else {
                    _right = hand;
                }
            }
        }

        public virtual Hand GetHand(Chirality chirality) {
            if(chirality == Chirality.Left) {
                if (_left.IsValid) {
                    return _left;
                }
            } else {
                if (_right.IsValid) {
                    return _right;
                }
            }

            return new VirtualHand();
        }

        public virtual int HandCount {
            get {
                return (_left.IsValid ? 1 : 0) + (_right.IsValid ? 1 : 0);
            }
        }
    }
}
