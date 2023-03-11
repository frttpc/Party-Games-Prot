using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    private TextMeshProUGUI timerText;
    [SerializeField] private float countdownTime = 3f;

    private void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        timerText.enabled = true;
        countdownTime += 0.99f;
    }

    private void Update()
    {
        if (countdownTime > 1)
        {
            timerText.text = ((int)countdownTime).ToString();
            countdownTime -= Time.unscaledDeltaTime;
        }
        else
        {
            UIManager.Instance.CountdownFinished();
            gameObject.SetActive(false);
        }
    }

    public void SetCountdownTime(float time) => countdownTime = time + 0.99f;
}
