using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerInput> players = new();
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private List<Transform> startingPoints = new();
    [SerializeField] private List<Color> playerColors = new();

    private PlayerInputManager playerInputManager;
    WaitForSeconds threeSeconds = new WaitForSeconds(3f);

    public event Action OnPlayerDeath;

    public static PlayerManager Instance;

    private void Awake()
    {
        Instance = this;
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable() => playerInputManager.onPlayerJoined += AddPlayer;
    
    private void OnDisable() => playerInputManager.onPlayerJoined -= AddPlayer;

    private void AddPlayer(PlayerInput player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);

            Transform playerTransform = player.transform;
            playerTransform.position = startingPoints[players.Count - 1].position;

            player.GetComponent<PlayerController>().SetColor(playerColors[players.Count - 1]);

            targetGroup.AddMember(playerTransform, 1f, 1f);
        }
    }

    public void PlayerDied(GameObject respawnedObject)
    {
        StartCoroutine(RespawnPlayer(respawnedObject));
    }

    public IEnumerator RespawnPlayer(GameObject respawnedObject)
    {
        DisablePlayer(respawnedObject);
        respawnedObject.transform.position = respawnPoint.position;
        yield return threeSeconds;
        EnablePlayer(respawnedObject);
    }

    private void DisablePlayer(GameObject respawnedObject)
    {
        respawnedObject.GetComponent<SpriteRenderer>().enabled = false;
        respawnedObject.GetComponent<TrailRenderer>().enabled = false;

        Rigidbody2D RB = respawnedObject.GetComponent<Rigidbody2D>();
        RB.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX;

        PlayerController player = respawnedObject.GetComponent<PlayerController>();
        player.GetColorRenderer().enabled = false;
        player.enabled = false;
    }

    private void EnablePlayer(GameObject respawnedObject)
    {
        respawnedObject.GetComponent<SpriteRenderer>().enabled = true;
        respawnedObject.GetComponent<TrailRenderer>().enabled = true;

        Rigidbody2D RB = respawnedObject.GetComponent<Rigidbody2D>();
        RB.constraints = RigidbodyConstraints2D.FreezeRotation;
        RB.velocity = Vector2.zero;

        PlayerController player = respawnedObject.GetComponent<PlayerController>();
        player.GetColorRenderer().enabled = true;
        player.enabled = true;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }
}
