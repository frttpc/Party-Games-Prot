using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    private CountdownTimer oneSecondCountdownTimer = new(1);
    private bool isStarting = true;

    public event Action OnGameStart;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        StopTimeScale();
    }

    private void OnEnable()
    {
        OnGameStart += StartTimeScale;
    }

    private void OnDisable()
    {
        OnGameStart -= StartTimeScale;
    }

    private void Update()
    {
        if (isStarting && playerInputManager.playerCount == playerInputManager.maxPlayerCount)
        {
            if (!oneSecondCountdownTimer.GetTimerStatus())
            {
                oneSecondCountdownTimer.StartTimer();
            }
            else
            {
                if (!UIManager.Instance.CountdownIsStarted())
                {
                    UIManager.Instance.StartCountdown();
                }
                else
                {
                    OnGameStart?.Invoke();
                    isStarting = false;
                }
            }
        }
    }

    public void StopTimeScale() => Time.timeScale = 0;

    public void StartTimeScale() => Time.timeScale = 1;
}
