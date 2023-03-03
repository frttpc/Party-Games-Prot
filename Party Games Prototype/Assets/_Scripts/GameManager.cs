using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    private bool firstTime = true;
    private bool isStarted = false;
    private WaitForSecondsRealtime oneSecondRealTime = new(1f);

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
        if (firstTime && playerInputManager.playerCount == playerInputManager.maxPlayerCount)
        {
            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        firstTime = false;
        yield return oneSecondRealTime;
        OnGameStart?.Invoke();
    }

    public void StopTimeScale() => Time.timeScale = 0;

    public void StartTimeScale() => Time.timeScale = 1;

    public void CountdownFinished() => OnGameStart?.Invoke();
}
