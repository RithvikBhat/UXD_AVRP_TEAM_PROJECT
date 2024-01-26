using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Text timerText;
    [SerializeField] private float totalTime = 60f;

    private float currentTime;
    private bool _isTimerRunning = false;

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (_isTimerRunning)
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

        if (currentTime <= 0)
        {
            _isTimerRunning = false;
            currentTime = 0; // Ensure the timer doesn't go negative
            HandleTimerCompletion();
        }
    }

    private void UpdateTimerDisplay()
    {
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void HandleTimerCompletion()
    {
        Debug.Log("Time's up!");
        // Additional actions you want to perform when the timer reaches zero

        ShowGameOverText();
    }

    private void ShowGameOverText()
    {
        // Display "Game Over!" in the timer text
        timerText.text = "Game Over!";
        // Add any additional actions related to game over here

        background.color = Color.red;
    }

    public void ResetTimer()
    {
        currentTime = totalTime;
        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        _isTimerRunning = true;
    }
}
