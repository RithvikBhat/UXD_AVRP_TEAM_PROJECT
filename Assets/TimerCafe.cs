using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CafeTimer: MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private float totalTime = 60f; // Set the total time in seconds

    private float currentTime;

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (IsTimerRunning())
        {
            UpdateTimer();
        }
        else
        {
            HandleTimerCompletion();
        }
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        // Display the timer in minutes and seconds format
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private bool IsTimerRunning()
    {
        return currentTime > 0;
    }

    private void HandleTimerCompletion()
    {
        Debug.Log("Time's up!");
        // Add any additional actions you want to perform when the timer reaches zero
    }

    public void ResetTimer()
    {
        currentTime = totalTime;
        UpdateTimerDisplay();
    }

    // Make sure this method is public
    public void StartTimerWithButton()
    {
        ResetTimer();
    }
}
