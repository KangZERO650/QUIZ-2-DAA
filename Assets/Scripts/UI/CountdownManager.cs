using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText;
    
    [Header("Settings")]
    [SerializeField] private int countdownTime = 3;

    public bool IsGameStarted { get; private set; } = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        int tempTime = countdownTime;

        while (tempTime > 0)
        {
            countdownText.text = tempTime.ToString();
            
            countdownText.transform.localScale = Vector3.one * 1.5f;
            
            float timer = 0;
            while(timer < 1f)
            {
                timer += Time.deltaTime;
                countdownText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, timer);
                yield return null;
            }

            tempTime--;
        }

        countdownText.text = "GO!";
        IsGameStarted = true;

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }
}