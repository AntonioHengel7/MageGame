using UnityEngine;

public class TEMPPLAYERMOVEMENT : MonoBehaviour
{
    public float moveSpeed = 5.0f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get input for horizontal and vertical movement
            float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow keys
            float verticalInput = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow keys

            // Calculate movement direction
            Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);

            // Normalize the movement vector to prevent faster diagonal movement
            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }

            // Apply movement to the character's position
            transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
