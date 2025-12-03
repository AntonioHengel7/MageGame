using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class VolcanoStaff : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera cam;
    [SerializeField] Transform muzzle;
    [SerializeField] Transform ignoreRoot;
    [SerializeField] GameObject primaryProjectilePrefab;
    [SerializeField] GameObject altProjectilePrefab;

    [Header("Primary")]
    [SerializeField] float fireRate = 6f;
    [SerializeField] float primarySpeed = 60f;
    [SerializeField] float primaryLife = 6f;
    [SerializeField] float primaryDamage = 15f;
    [SerializeField] float primaryRadius = 1.5f;
    [SerializeField] float primaryManaCost = 5f;
    [SerializeField] Vector3 primaryScale = Vector3.one;

    [Header("Alt")]
    [SerializeField] float altManaCost = 100f;
    [SerializeField] float altSuperCost = 100f;
    [SerializeField] float altFuse = 1.2f;
    [SerializeField] float altDamage = 120f;
    [SerializeField] float altRadius = 8f;
    [SerializeField] float altForwardSpeed = 22f;
    [SerializeField] float altUpwardBoost = 8f;

    [Header("Aim")]
    [SerializeField] float maxAimDistance = 1000f;
    [SerializeField] LayerMask aimMask = ~0;

    [Header("Anti-clipping")]
    [SerializeField] float spawnForwardOffset = 0.4f;
    [SerializeField] float recheckFromMuzzleSlack = 0.02f;

    [Header("Resources")]
    [SerializeField] Mana mana;
    [SerializeField] SuperMeter super;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    [Header("Input")]
    [SerializeField] InputActionReference primaryFire;
    [SerializeField] InputActionReference altFire;
#endif

    [Header("Debug")]
    [SerializeField] bool debugPrimary = true;

    float nextFireTime;
    Collider[] cachedIgnore;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!mana) { var r0 = transform.root; if (r0) mana = r0.GetComponentInChildren<Mana>(); }
        if (!super) { var r1 = transform.root; if (r1) super = r1.GetComponentInChildren<SuperMeter>(); }
        if (!ignoreRoot) ignoreRoot = transform.root;
        if (ignoreRoot) cachedIgnore = ignoreRoot.GetComponentsInChildren<Collider>(true);
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        primaryFire?.action.Enable();
        altFire?.action.Enable();
#endif
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        primaryFire?.action.Disable();
        altFire?.action.Disable();
#endif
    }

    void Update()
    {
        bool p = false, aDown = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (primaryFire) p = primaryFire.action.IsPressed();
        if (altFire) aDown = altFire.action.WasPressedThisFrame();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!p) p = Input.GetButton("Fire1");
        if (!aDown) aDown = Input.GetButtonDown("Fire2");
#endif
        if (p && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / Mathf.Max(1f, fireRate);
            FirePrimary();
        }
        if (aDown) FireAlt();
    }

    void FirePrimary()
    {
        if (!cam || !primaryProjectilePrefab) return;
        if (mana != null)
        {
            if (!mana.TrySpend(primaryManaCost))
            {
                Debug.Log($"[VolcanoStaff] Not enough mana (need {primaryManaCost}). Current: {mana.Current:0.0}/{mana.Max:0.0}");
                return;
            }
            Debug.Log($"[VolcanoStaff] Spent {primaryManaCost} mana. Remaining: {mana.Current:0.0}/{mana.Max:0.0}");
        }

        Vector3 aimPoint = cam.transform.position + cam.transform.forward * maxAimDistance;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var camHit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
            aimPoint = camHit.point;

        Vector3 origin = muzzle ? muzzle.position : cam.transform.position;
        Vector3 dir = (aimPoint - origin).normalized;
        if (dir.sqrMagnitude < 1e-6f) dir = cam.transform.forward;
        Vector3 spawnPos = origin + dir * spawnForwardOffset;

        float toAimDist = Vector3.Distance(spawnPos, aimPoint);
        if (toAimDist > recheckFromMuzzleSlack &&
            Physics.Raycast(spawnPos, dir, out var muzzleHit, toAimDist, aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = muzzleHit.point;
            dir = (aimPoint - spawnPos).normalized;
        }

        var go = Instantiate(primaryProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        go.transform.localScale = primaryScale;

        var bolt = go.GetComponent<ExplosiveBolt>();
        if (!bolt) bolt = go.GetComponentInChildren<ExplosiveBolt>();
        if (!bolt) { Debug.LogError("[VolcanoStaff] Primary prefab missing ExplosiveBolt"); return; }

        bolt.SetLifetime(primaryLife);
        bolt.SetDamage(primaryDamage);
        bolt.SetRadius(primaryRadius);
        bolt.Launch(dir, primarySpeed, cachedIgnore);

        if (debugPrimary) Debug.DrawRay(spawnPos, dir * 3f, Color.red, 1.5f);
    }

    void FireAlt()
    {
        if (!cam || !altProjectilePrefab) return;
        if (!mana || !super) { Debug.Log("[VolcanoStaff] Missing Mana or SuperMeter."); return; }
        bool superFull = super.Current >= super.Max && super.Max > 0f;
        if (!superFull) { Debug.Log($"[VolcanoStaff] Super not full. Current: {super.Current:0.0}/{super.Max:0.0}"); return; }
        if (mana.Current < altManaCost) { Debug.Log($"[VolcanoStaff] Not enough mana for alt (need {altManaCost}). Current: {mana.Current:0.0}/{mana.Max:0.0}"); return; }
        if (!super.TrySpend(altSuperCost)) { Debug.Log($"[VolcanoStaff] Could not spend super {altSuperCost}. Current: {super.Current:0.0}/{super.Max:0.0}"); return; }
        if (!mana.TrySpend(altManaCost)) { Debug.Log($"[VolcanoStaff] Could not spend mana {altManaCost}. Current: {mana.Current:0.0}/{mana.Max:0.0}"); super.Add(altSuperCost); return; }
        Debug.Log($"[VolcanoStaff] Alt fired. Spent {altManaCost} mana (left {mana.Current:0.0}/{mana.Max:0.0}) and {altSuperCost} super (left {super.Current:0.0}/{super.Max:0.0}).");

        Vector3 origin = muzzle ? muzzle.position : cam.transform.position;
        Vector3 forward = cam.transform.forward;
        Vector3 v = forward.normalized * altForwardSpeed + Vector3.up * altUpwardBoost;
        Vector3 spawnPos = origin + forward.normalized * spawnForwardOffset;

        var go = Instantiate(altProjectilePrefab, spawnPos, Quaternion.LookRotation(forward));
        var proj = go.GetComponent<LobbedExplosive>();
        if (!proj) proj = go.GetComponentInChildren<LobbedExplosive>();
        if (!proj) { Debug.LogError("[VolcanoStaff] Alt prefab missing LobbedExplosive"); return; }

        proj.SetDamage(altDamage);
        proj.SetRadius(altRadius);
        proj.SetFuse(altFuse);
        proj.Launch(v, cachedIgnore);
    }
}
