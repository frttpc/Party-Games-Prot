using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
       playerManager.PlayerDied(collision.gameObject.GetComponent<PlayerController>());
    }
}
