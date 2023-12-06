using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerObjects
{
    Stuhl,
    Schrank,
    Klopfen,
    Tafel,
    Fenster
}

public class TriggerDetector : MonoBehaviour
{
    private Material _material;

    [SerializeField]
    private TriggerObjects _type;

    private List<Collider> _objects = new List<Collider>();

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;

        // default - red
        _material.color = new Color(1, 0, 0, 0.2f);
    }
    void AddScore()
    {
        if (_objects.Count > 0)
        {
            ScoreManager.scoreCount++;
        }
    }
    void Update()
    {
        if ((_objects.Count > 0) && (_objects.Count < 6))
        {
            // green
            _material.color = new Color(0, 1, 0, 0.2f);
            //ScoreManager.scoreCount += 1;
            
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
        if (!other.TryGetComponent<TriggerObject>(out TriggerObject triggerObject)){
            return;
        }

        if(triggerObject.Type == _type)
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
        if (!other.TryGetComponent<TriggerObject>(out TriggerObject triggerObject))
        {
            return;
        }

        if (triggerObject.Type == _type)
        {
            _objects.Remove(other);
        }
    }
}
