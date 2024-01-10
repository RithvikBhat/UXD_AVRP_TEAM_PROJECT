using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleTrigger : MonoBehaviour
{
    public int ScoreCount
    {
        get
        {
            return _objects.Count;
        }
    }


    private Material _material;

    private List<Collider> _objects = new List<Collider>();

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;

        // default - red
        _material.color = new Color(1, 0, 0, 0.2f);
    }

    void Update()
    {
        if (_objects.Count > 5)
        {
            // green
            _material.color = new Color(0, 1, 0, 0.2f);
        }
        else
        {
            // red
            _material.color = new Color(1, 0, 0, 0.2f);
        }
    }

    /// <summary>
    /// Gets called once when an object "collider" enters the trigger area
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Trigger")
        {
            _objects.Add(other);
        }
    }

    /// <summary>
    /// Gets called every frame and for every "collider" that touches the trigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {

    }

    /// <summary>
    /// Gets called once when an object "collider" leaves the trigger area again
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Trigger")
        {
            _objects.Remove(other);
        }
    }
}
