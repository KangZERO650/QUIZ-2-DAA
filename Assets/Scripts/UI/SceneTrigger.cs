using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Tuliskan nama scene persis seperti yang ada di Build Settings")]
    [SerializeField] private string targetSceneName;

    public void ChangeScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Target Scene Name belum diisi di Inspector!");
            return;
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Keluar dari Game...");
        Application.Quit();
    }
}