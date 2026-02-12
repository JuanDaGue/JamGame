using UnityEngine;

public class SceneController : MonoBehaviour
{
    public void CargarSceneIndex(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    public void CargarSceneNombre(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
