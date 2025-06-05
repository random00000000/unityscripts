using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load when calling LoadScene")] 
    public string sceneName;

    /// <summary>
    /// Load the configured scene.
    /// </summary>
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneLoader: sceneName is empty.");
        }
    }

    /// <summary>
    /// Quit the game. Works in builds.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
