using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScoreText;

    void Start()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text =  CollectibleManager.FinalScore.ToString();
        }
    }
}