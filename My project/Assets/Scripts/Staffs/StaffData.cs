using UnityEngine;

[CreateAssetMenu(menuName = "FPS Mage/Staff Data")]
public class StaffData : ScriptableObject
{
    [Header("View (first-person)")]
    public GameObject viewModelPrefab;   

    [Header("World Pickup")]
    public GameObject worldPickupPrefab; 

    [Header("Hold Offsets (relative to WeaponSocket)")]
    public Vector3 localPosition;
    public Vector3 localEulerAngles;

    [Header("Optional")]
    public string displayName;
}
