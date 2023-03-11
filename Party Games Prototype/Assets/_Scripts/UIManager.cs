using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using RengeGames.HealthBars;

public class UIManager : MonoBehaviour
{
    [SerializeField] List<GameObject> HUDs;
    public List<RadialSegmentedHealthBar> powerBars;
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
        playerInputManager.onPlayerJoined += PlayerHUD;
    }

    private void OnDisable() 
    {
        playerInputManager.onPlayerJoined -= PlayerHUD;
    }

    private void Start()
    {
        foreach (RadialSegmentedHealthBar bar in powerBars)
        {
            bar.InnerColor.Value = PlayerManager.Instance.playerColors[powerBars.IndexOf(bar)];
        }

    }

    private void PlayerHUD(PlayerInput player)
    {
        joinTexts[playerInputManager.playerCount - 1].gameObject.SetActive(false);
        HUDs[playerInputManager.playerCount - 1].SetActive(true);
    }

    public void UpdateDashBar(Player player, float amount)
    {
        Image dashBar = dashBars[((int)player) - 1];
        dashBar.fillAmount = amount;
    }

    public void UpdatePowerBar(Player player, float amount)
    {
        RadialSegmentedHealthBar powerBar = powerBars[((int)player) - 1];
        powerBar.SetPercent(amount);
    }

    public void StartCountdown() => countdownTimer.enabled = true;

    public void CountdownFinished() => countdownFinished = true;

    public bool IsCountdownFinished() => countdownFinished;
}
