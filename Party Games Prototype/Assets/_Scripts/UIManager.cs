using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;

    [SerializeField] private List<TextMeshProUGUI> joinTexts;
    [SerializeField] private CountdownTimer countdownTimer;
    private bool countdownFinished = false;

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnEnable() => playerInputManager.onPlayerJoined += PlayerJoinText;

    private void OnDisable() => playerInputManager.onPlayerJoined -= PlayerJoinText;

    private void PlayerJoinText(PlayerInput player) => joinTexts[playerInputManager.playerCount - 1].gameObject.SetActive(false);

    public void StartCountdown() => countdownTimer.enabled = true;

    public void CountdownFinished() => countdownFinished = true;

    public bool IsCountdownFinished() => countdownFinished;
}
