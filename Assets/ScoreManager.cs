using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;
    public static int scoreCount = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score:" + Mathf.Round(scoreCount); 
    }
}
