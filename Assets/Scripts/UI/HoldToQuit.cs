using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HoldToQuit : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float holdDuration = 2f;
    
    [Header("UI Feedback (Optional)")]
    [SerializeField] private Image fillImage;

    private float timer = 0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            timer += Time.deltaTime;

            if (fillImage != null)
            {
                fillImage.fillAmount = timer / holdDuration;
            }

            if (timer >= holdDuration)
            {
                timer = 0f;
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
        else
        {
            timer = 0f;
            if (fillImage != null) fillImage.fillAmount = 0f;
        }
    }
}