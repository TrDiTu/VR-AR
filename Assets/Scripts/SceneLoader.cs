using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void Sceneloader(int sceneIndex)
    {
    SceneManager.LoadScene(sceneIndex);
    }
    
}
