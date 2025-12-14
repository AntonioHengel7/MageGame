using UnityEngine;
using UnityEngine.InputSystem;

public class InteractRaycaster : MonoBehaviour
{
    [Header("Input (new Input System)")]
    public InputActionReference interactAction; 

    [Header("Settings")]
    public float interactRange = 3f;

    private WeaponManager wm;
    private Camera cam;

    void Awake()
    {
        wm = GetComponent<WeaponManager>();
        cam = Camera.main;
    }

    void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryInteract();
    }

    private void TryInteract()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            var pickup = hit.collider.GetComponentInParent<StaffPickup>();
            if (pickup != null)
            {
                pickup.OnInteract(wm);
            }
        }

        // for debugging
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green, 0.25f);
    }
}
