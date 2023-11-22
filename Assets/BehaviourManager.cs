using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(InteractionBehaviour interactionbehaviour in FindObjectsOfType(typeof(InteractionBehaviour), true)){
            interactionbehaviour.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
