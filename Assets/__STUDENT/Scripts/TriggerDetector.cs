using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private bool _placed = false;

    //private List<Collider> _objects = new List<Collider>();

    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;

        // default - red
        _material.color = new Color(1, 0, 0, 0.2f);
    }
    void AddScore()
    {

    }

    void Update()
    {
        
        if (_placed)
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
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<TriggerObject>(out TriggerObject triggerObject)){
            return;
        }

        if(triggerObject.Type == _type)
        {
            ScoreManager.scoreCount++;
            _placed = true;
        }
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
            ScoreManager.scoreCount--;
         _placed = false;
        }
    }
}
