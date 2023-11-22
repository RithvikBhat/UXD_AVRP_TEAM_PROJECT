using HCIG;
using Leap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{

    private GameObject cursor;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(BaseManager.Instance.Camera.transform.position.x, 0, BaseManager.Instance.Camera.transform.position.z);
    }
}
