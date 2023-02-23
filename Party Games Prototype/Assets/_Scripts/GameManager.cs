using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerInputManager;

    private bool isStarting = true;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        ChangeTimeScale(0);
    }

    private void Update()
    {
        //if (isStarting && PlayerManager.Instance.GetPlayerCount() == playerInputManager.maxPlayerCount)
            StartCoroutine(GameStart());
    }

    private IEnumerator GameStart()
    {
        yield return new WaitForSecondsRealtime(3f);
        ChangeTimeScale(1);
        isStarting = false;
        StopCoroutine(GameStart());
    }

    public void ChangeTimeScale(int i)
    {
        Time.timeScale = i;
    }
}
