using UnityEngine;

public class Scroller : MonoBehaviour
{
    [SerializeField] private Transform targetGroup;
    [SerializeField] private float followAmount;
    [SerializeField] private Vector2 offset;

    private void Update()
    {
        transform.position = targetGroup.position / followAmount + new Vector3(offset.x, offset.y, 0);
    }
}
