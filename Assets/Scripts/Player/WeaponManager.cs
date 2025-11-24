using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform weaponSocket;  
    public float dropForwardForce = 3f;
    public float dropUpForce = 1f;

    [Header("Runtime State (read-only)")]
    public StaffData currentStaffData;
    public GameObject currentViewModelInstance;

    Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    public void Equip(StaffData newStaff)
    {

        if (currentViewModelInstance != null)
            Destroy(currentViewModelInstance);

        currentStaffData = newStaff;

        if (currentStaffData != null && currentStaffData.viewModelPrefab != null)
        {
            currentViewModelInstance = Instantiate(
                currentStaffData.viewModelPrefab, weaponSocket);


            currentViewModelInstance.transform.localPosition = currentStaffData.localPosition;
            currentViewModelInstance.transform.localRotation = Quaternion.Euler(currentStaffData.localEulerAngles);

 
            foreach (var rb in currentViewModelInstance.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);
            foreach (var col in currentViewModelInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }
    }

    public void DropCurrentAsPickup()
    {
        if (currentStaffData == null || currentStaffData.worldPickupPrefab == null) return;

        Vector3 spawnPos = _cam.transform.position + _cam.transform.forward * 0.7f + Vector3.up * 0.2f;
        Quaternion spawnRot = Quaternion.LookRotation(_cam.transform.forward, Vector3.up);

        GameObject pickup = Instantiate(currentStaffData.worldPickupPrefab, spawnPos, spawnRot);

  
        var rb = pickup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(_cam.transform.forward * dropForwardForce + Vector3.up * dropUpForce, ForceMode.VelocityChange);
        }

        if (currentViewModelInstance != null)
        {
            Destroy(currentViewModelInstance);
            currentViewModelInstance = null;
        }

        currentStaffData = null;
    }

    /// <summary>

    /// </summary>
    public void SwapTo(StaffData newStaff)
    {
        if (newStaff == currentStaffData) return;

        if (currentStaffData != null)
            DropCurrentAsPickup();

        Equip(newStaff);
    }
}
