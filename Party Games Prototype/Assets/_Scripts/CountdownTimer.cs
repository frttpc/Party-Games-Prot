using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private float countdownTime = 1;
    private bool timerIsStarted = false;


    public CountdownTimer(int time)
    {
        countdownTime = time;
    }

    private void Update()
    {
        if (timerIsStarted)
        {
            if (countdownTime < 0)
            {
                countdownTime = 0;
                timerIsStarted = false;
            }
            else
            {
                countdownTime -= Time.deltaTime;
            }
        }
    }

    public void StartTimer() => timerIsStarted = true;

    public bool GetTimerStatus() => timerIsStarted;

    public float GetTime() => countdownTime;

    public void SetCountdownTime(float time) => countdownTime = time;
}
