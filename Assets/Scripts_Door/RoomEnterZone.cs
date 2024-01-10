using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnterZone : MonoBehaviour
{
    [SerializeField]
    private TimerScript _timerScript;

    // Start is called before the first frame update
    public GameObject door1;

    void Start()
    {
        door1.SetActive(true);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "MovementTrigger")
        {
            if (door1 != null)
            {
                door1.SetActive(false);
            }

            if(_timerScript != null)
            {
            _timerScript.StartTimer();
            }
        }
    }
}
