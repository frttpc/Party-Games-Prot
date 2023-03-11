using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;
    private bool firstTime = true;
    public bool isStarted { get; private set; } = false;
    private WaitForSecondsRealtime oneSecondRealTime = new(2.6f);

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
            isStarted = true;
            UIManager.Instance.StartCountdown();
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

}
