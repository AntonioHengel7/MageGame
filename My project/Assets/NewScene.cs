using UnityEngine;
using UnityEngine.SceneManagement;
public class NewScene : MonoBehaviour
{

    private int Location;
    private string NextLevel = "Level_1";
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
 
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Level_1");
        }
    }
}
