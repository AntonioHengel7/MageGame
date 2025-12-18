using UnityEngine;
using UnityEngine.UI;       // Remove if using TextMeshPro only

public class Lever_Mec : MonoBehaviour
{
    public bool pulled = false; 
    public Doorinteraction door;   // Drag the door with DoorInteraction here
    public Text messageText;       // Can be the same UI text as the door

   void start(){}
    public string interactPrompt = "Press E to pull the lever.";

    private bool isActivated = false;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isActivated)
        {
            ShowMessage(interactPrompt);

            if (Input.GetKeyDown(KeyCode.E))
            {
                ActivateLever();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ClearMessage();
    }

    private void ActivateLever()
    {
        isActivated = true;
        ClearMessage();

        if (door != null)
        {
            door.UnlockDoor();
        }
    }

    private void ShowMessage(string msg)
    {
        
        if (messageText != null)
        {
            messageText.text = msg;
        }
        else
        {
            Debug.Log(msg);
        }
    }
    void Update(){}
    // Update is called once per frame
    private void ClearMessage()
    {
        
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}