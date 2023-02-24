using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;

    [SerializeField] private List<TextMeshProUGUI> joinTexts;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CountdownTimer countdownTimer = new(3);
    private bool startCountdown = false;

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (startCountdown)
        {
            countdownTimer.StartTimer();
            timerText.gameObject.SetActive(true);
            startCountdown = false;
        }

        if (!countdownTimer.GetTimerStatus())
            timerText.text = ((int)countdownTimer.GetTime()).ToString();
    }

    private void OnEnable() => playerInputManager.onPlayerJoined += PlayerJoinText;

    private void OnDisable() => playerInputManager.onPlayerJoined -= PlayerJoinText;

    private void PlayerJoinText(PlayerInput player) => joinTexts[playerInputManager.playerCount - 1].gameObject.SetActive(false);

    public bool CountdownIsStarted() => startCountdown;

    public void StartCountdown() => startCountdown = true;
}
