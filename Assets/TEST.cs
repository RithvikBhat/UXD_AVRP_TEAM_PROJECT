using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TEST : MonoBehaviour
{
    [SerializeField]
    private TMP_Text duosMessage;

    // Start is called before the first frame update
    void Start()
    {
        duosMessage.text = "HELLO";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
