using HCIG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(BaseManager.Instance.Camera.transform.position.x, 2, BaseManager.Instance.Camera.transform.position.z);
    }
}
