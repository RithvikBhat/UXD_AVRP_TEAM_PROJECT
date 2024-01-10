using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class FoodScoreManager : MonoBehaviour
{

    public Text scoreText;

    [SerializeField]
    private ExampleTrigger _countScript;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score:" + Mathf.Round(_countScript.ScoreCount);
    }
}
