using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class UniversalStaff : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera cam;
    [SerializeField] Transform muzzle;
    [SerializeField] Transform ignoreRoot;
    [SerializeField] GameObject projectilePrefab;

    [Header("Fire")]
    [SerializeField] float fireRate = 8f;
    [SerializeField] float projectileSpeed = 100f;
    [SerializeField] float projectileLife = 6f;
    [SerializeField] Vector3 projectileScale = Vector3.one;

    [Header("Aim")]
    [SerializeField] float maxAimDistance = 1000f;
    [SerializeField] LayerMask aimMask = ~0;

    [Header("Anti-clipping")]
    [SerializeField] float spawnForwardOffset = 0.25f;
    [SerializeField] float recheckFromMuzzleSlack = 0.02f;

    [Header("Resources")]
    [SerializeField] Mana mana;
    [SerializeField] float manaPerShot = 5f;
    [SerializeField] SuperMeter super;

    [Header("Alt Barrage")]
    [SerializeField] float barrageManaCost = 100f;
    [SerializeField] float barrageSuperCost = 100f;
    [SerializeField] float barrageDuration = 4f;
    [SerializeField] float barrageDPS = 25f;
    [SerializeField] float barrageConeAngleDeg = 35f;
    [SerializeField] float barrageRange = 25f;
    [SerializeField] float barrageProjectilesPerSecond = 12f;
    [SerializeField] float barrageProjectileSpeed = 90f;
    [SerializeField] float barrageProjectileLife = 2.5f;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    [Header("Input (New Input System)")]
    [SerializeField] InputActionReference primaryFire;
    [SerializeField] InputActionReference altFire;
#endif

    [Header("Audio")]
    [SerializeField] AudioSource primaryFireAudio;
    [Range(0f, 1f)]
    [SerializeField] float primaryFireVolume = 1f;
    [SerializeField] AudioSource altFireAudio;
    [Range(0f, 1f)]
    [SerializeField] float altFireVolume = 1f;

    [Header("Recoil")]
    [SerializeField] Transform recoilTarget;
    [SerializeField] float recoilDistance = 0.05f;
    [SerializeField] float recoilRecoverySpeed = 10f;

    float nextFireTime;
    Collider[] cachedIgnore;
    bool barrageActive;
    float barrageTimer;
    float barrageTick;
    Vector3 recoilOffset;
    Vector3 recoilBaseLocalPos;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!mana)
        {
            var root = transform.root;
            if (root) mana = root.GetComponentInChildren<Mana>();
        }
        if (!super)
        {
            var root = transform.root;
            if (root) super = root.GetComponentInChildren<SuperMeter>();
        }
        if (!ignoreRoot) ignoreRoot = transform.root;
        if (ignoreRoot) cachedIgnore = ignoreRoot.GetComponentsInChildren<Collider>(true);

        if (!recoilTarget) recoilTarget = transform;
        recoilBaseLocalPos = recoilTarget.localPosition;

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
        bool wantsFire = false;
        bool wantsAltPress = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (primaryFire) wantsFire = primaryFire.action.IsPressed();
        if (altFire) wantsAltPress = altFire.action.WasPressedThisFrame();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!wantsFire) wantsFire = Input.GetButton("Fire1");
        if (!wantsAltPress) wantsAltPress = Input.GetButtonDown("Fire2");
#endif

        if (!barrageActive && wantsFire && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / Mathf.Max(1f, fireRate);
            TryFireOnce();
        }

        if (wantsAltPress && !barrageActive) TryStartBarrage();

        if (barrageActive)
        {
            barrageTimer -= Time.deltaTime;
            barrageTick -= Time.deltaTime;
            while (barrageTick <= 0f)
            {
                barrageTick += 1f / Mathf.Max(1f, barrageProjectilesPerSecond);
                SpawnBarrageProjectile();
            }
            if (barrageTimer <= 0f) barrageActive = false;
        }

        UpdateRecoil();
    }

    void TryFireOnce()
    {
        if (!cam || !projectilePrefab) return;
        if (mana != null)
        {
            if (!mana.TrySpend(manaPerShot))
            {
                Debug.Log($"[UniversalStaff] Not enough mana (need {manaPerShot}). Current: {mana.Current:0.0}/{mana.Max:0.0}");
                return;
            }
            Debug.Log($"[UniversalStaff] Spent {manaPerShot} mana. Remaining: {mana.Current:0.0}/{mana.Max:0.0}");
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

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        go.transform.localScale = projectileScale;

        var proj = go.GetComponent<SimpleProjectile>();
        if (!proj) proj = go.GetComponentInChildren<SimpleProjectile>();
        if (!proj)
        {
            Debug.LogError("[UniversalStaff] Projectile prefab missing SimpleProjectile");
            return;
        }

        proj.SetLifetime(projectileLife);
        proj.Launch(dir, projectileSpeed, cachedIgnore);

        PlayPrimaryFireAudio();
        ApplyPrimaryRecoil();
    }

    void TryStartBarrage()
    {
        if (!cam) return;
        if (!mana || !super)
        {
            Debug.Log("[UniversalStaff] Missing Mana or SuperMeter.");
            return;
        }
        bool superFull = super.Current >= super.Max && super.Max > 0f;
        if (!superFull)
        {
            Debug.Log($"[UniversalStaff] Super not full. Current: {super.Current:0.0}/{super.Max:0.0}");
            return;
        }
        if (mana.Current < barrageManaCost)
        {
            Debug.Log($"[UniversalStaff] Not enough mana for barrage (need {barrageManaCost}). Current: {mana.Current:0.0}/{mana.Max:0.0}");
            return;
        }
        if (!super.TrySpend(barrageSuperCost))
        {
            Debug.Log($"[UniversalStaff] Could not spend super {barrageSuperCost}. Current: {super.Current:0.0}/{super.Max:0.0}");
            return;
        }
        if (!mana.TrySpend(barrageManaCost))
        {
            Debug.Log($"[UniversalStaff] Could not spend mana {barrageManaCost}). Current: {mana.Current:0.0}/{mana.Max:0.0}");
            super.Add(barrageSuperCost);
            return;
        }
        barrageActive = true;
        barrageTimer = barrageDuration;
        barrageTick = 0f;

        PlayAltFireAudio();

        Debug.Log($"[UniversalStaff] Barrage started. Spent {barrageManaCost} mana (left {mana.Current:0.0}/{mana.Max:0.0}) and {barrageSuperCost} super (left {super.Current:0.0}/{super.Max:0.0}).");
    }

    void SpawnBarrageProjectile()
    {
        if (!projectilePrefab || !cam) return;

        Vector3 origin = muzzle ? muzzle.position : cam.transform.position;
        Vector3 forward = cam.transform.forward;

        float half = barrageConeAngleDeg * 0.5f;
        float yaw = Random.Range(-half, half);
        float pitch = Random.Range(-half, half);
        Quaternion spread = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 dir = (spread * forward).normalized;

        Vector3 spawnPos = origin + dir * spawnForwardOffset;

        if (Physics.Raycast(spawnPos, dir, out var block, barrageRange, aimMask, QueryTriggerInteraction.Ignore))
            dir = (block.point - spawnPos).normalized;

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        go.transform.localScale = projectileScale;

        var proj = go.GetComponent<SimpleProjectile>();
        if (!proj) proj = go.GetComponentInChildren<SimpleProjectile>();
        if (!proj)
        {
            Debug.LogError("[UniversalStaff] Projectile prefab missing SimpleProjectile");
            return;
        }

        float perProjectileDamage = barrageDPS / Mathf.Max(1f, barrageProjectilesPerSecond);
        var setDamage = proj.GetType().GetMethod("SetDamage");
        if (setDamage != null) setDamage.Invoke(proj, new object[] { perProjectileDamage });

        proj.SetLifetime(barrageProjectileLife);
        proj.Launch(dir, barrageProjectileSpeed, cachedIgnore);
    }

    void PlayPrimaryFireAudio()
    {
        if (!primaryFireAudio) return;
        primaryFireAudio.volume = primaryFireVolume;
        primaryFireAudio.Play();
    }

    void PlayAltFireAudio()
    {
        if (!altFireAudio) return;
        altFireAudio.volume = altFireVolume;
        altFireAudio.Play();
    }

    void ApplyPrimaryRecoil()
    {
        if (!recoilTarget) return;
        recoilOffset += new Vector3(0f, 0f, -recoilDistance);
    }

    void UpdateRecoil()
    {
        if (!recoilTarget) return;
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
        recoilTarget.localPosition = recoilBaseLocalPos + recoilOffset;
    }
}
