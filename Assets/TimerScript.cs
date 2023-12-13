using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    public TextAlignment timerText;
    public float totalTime = 300f;
    private float currentTime;

    void Start()
    {
        currentTime = totalTime;
    }


    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
        }

        else
        {
            Debug.Log("Time's up!");
        }
    }

    private void UpdateTimerDisplay()
    {
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.ToString = string.Format("{0:00}:{5:00}", minutes, seconds);
    }
}
