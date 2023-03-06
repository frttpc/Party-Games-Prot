using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private List<Transform> respawnPoints = new();
    [SerializeField] private List<Transform> startingPoints = new();

    [ColorUsage(true, true)]
    public List<Color> playerColors = new();
    public List<LayerMask> playerLayers = new();
    private bool respawningPlayer = false;

    private PlayerInputManager playerInputManager;
    private readonly WaitForSeconds threeSeconds = new(3f);

    public event Action OnPlayerDeath;

    public static PlayerManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable() => playerInputManager.onPlayerJoined += AddPlayer;
    
    private void OnDisable() => playerInputManager.onPlayerJoined -= AddPlayer;

    private void AddPlayer(PlayerInput player)
    {
        player.transform.position = startingPoints[playerInputManager.playerCount - 1].position;

        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.SetColor(playerColors[playerInputManager.playerCount - 1]);
        playerController.player = (Player)playerInputManager.playerCount;
        playerController.enemyLayer = playerLayers[playerInputManager.playerCount - 1];

        targetGroup.AddMember(player.transform, 1f, 1f);

        player.name = "Player " + playerInputManager.playerCount;

        if (playerInputManager.playerCount == playerInputManager.maxPlayerCount)
            playerController.GetComponent<SpriteRenderer>().flipX = true;
    }

    public void PlayerDied(PlayerController diedPlayer)
    {
        StartCoroutine(RespawnPlayer(diedPlayer));
    }

    public IEnumerator RespawnPlayer(PlayerController respawnedPlayer)
    {
        DisablePlayer(respawnedPlayer);

        int i = respawningPlayer ? 0 : 1;
        respawnedPlayer.transform.position = respawnPoints[i].position;
        respawningPlayer = true;

        yield return threeSeconds;
        EnablePlayer(respawnedPlayer);

        respawningPlayer = false;
    }

    private void DisablePlayer(PlayerController respawnedPlayer)
    {
        respawnedPlayer.spriteRenderer.enabled = false;
        respawnedPlayer.trailRenderer.enabled = false;

        Rigidbody2D RB = respawnedPlayer.playerRB;
        RB.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        respawnedPlayer.GetColorRenderer().enabled = false;
        respawnedPlayer.enabled = false;
    }

    private void EnablePlayer(PlayerController respawnedPlayer)
    {
        respawnedPlayer.spriteRenderer.enabled = true;
        respawnedPlayer.trailRenderer.enabled = true;

        Rigidbody2D RB = respawnedPlayer.playerRB;
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;
        RB.velocity = Vector2.zero;

        respawnedPlayer.GetColorRenderer().enabled = true;
        respawnedPlayer.enabled = true;
    }

    public int GetPlayerCount()
    {
        return playerInputManager.playerCount;
    }
}

public enum Player
{
    None,
    P1,
    P2
}
