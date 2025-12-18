using UnityEngine;
using UnityEngine.SceneManagement;

public class NextFloorLoader : MonoBehaviour
{
    [Header("Use EITHER nextByIndex or sceneName")]
    public bool useNextBuildIndex = true;
    public string sceneName = "Goblin Boss";   

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (useNextBuildIndex)
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentIndex + 1);
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("No sceneName set on NextFloorLoader!");
            }
        }
    }
}
