using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UIManager : MonoBehaviour
{
    public List<Image> dashBars;

    [SerializeField] private List<TextMeshProUGUI> joinTexts;
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private PlayerInputManager playerInputManager;

    private bool countdownFinished = false;

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += PlayerJoinText;
        playerInputManager.onPlayerJoined += PlayerDashBar;
    }

    private void OnDisable() 
    {
        playerInputManager.onPlayerJoined -= PlayerJoinText;
        playerInputManager.onPlayerJoined -= PlayerDashBar;
    }

    private void PlayerJoinText(PlayerInput player) => joinTexts[playerInputManager.playerCount - 1].gameObject.SetActive(false);

    private void PlayerDashBar(PlayerInput player) => dashBars[playerInputManager.playerCount - 1].transform.parent.gameObject.SetActive(true);

    public void UpdateDashBar(Player player, float amount)
    {
        Image dashBar = dashBars[((int)player) - 1];
        dashBar.fillAmount = amount;
    }

    public void StartCountdown() => countdownTimer.enabled = true;

    public void CountdownFinished() => countdownFinished = true;

    public bool IsCountdownFinished() => countdownFinished;
}
