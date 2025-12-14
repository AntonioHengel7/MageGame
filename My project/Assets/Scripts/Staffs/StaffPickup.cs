using UnityEngine;

public class StaffPickup : MonoBehaviour
{
    public StaffData staffData;

    public void OnInteract(WeaponManager wm)
    {
        if (wm == null || staffData == null) return;

        wm.SwapTo(staffData);
        Destroy(gameObject);
    }
}
