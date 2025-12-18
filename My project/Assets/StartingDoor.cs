using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingDoor : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;

    [Header("Scene To Load")]
    public string nextSceneName = "Level_1";   // <-- set this in the Inspector

    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("nextSceneName is not set on LoadNextSceneOnE!");
        }
    }
}