using UnityEngine;
using UnityEngine.UI;           // Remove this if you use TextMeshPro instead

public class Doorinteraction : MonoBehaviour
{
    [Header("Door State")]
    public bool isLocked = true;
    private bool isOpen = false;

    [Header("References")]
    public Text messageText;    // Drag your UI Text here in the Inspector

    [Header("Messages")]
    [TextArea]
    public string lockedMessage = "You need to find the lever.";
    [TextArea]
    public string openPrompt = "Press E to open the door.";

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isOpen) return;

        if (isLocked)
        {
            ShowMessage(lockedMessage);
        }
        else
        {
            ShowMessage(openPrompt);

            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenDoor();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ClearMessage();
    }

    public void UnlockDoor()
    {
        isLocked = false;
    }

    private void OpenDoor()
    {
        isOpen = true;
        ClearMessage();



            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
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

    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
}