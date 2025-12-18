using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorNextSceneWhenClear : MonoBehaviour
{
    public string nextSceneName = "";
    public KeyCode interactKey = KeyCode.E;

    bool playerInRange;

    void Update()
    {
        if (!playerInRange) return;
        if (!Input.GetKeyDown(interactKey)) return;
        if (!AllEnemiesDead()) return;

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    bool AllEnemiesDead()
    {
        var enemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < enemies.Length; i++)
        {
            var h = enemies[i].GetComponent<Health2>();
            if (h != null && h.IsAlive) return false;
        }
        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}
