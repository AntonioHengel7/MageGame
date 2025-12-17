using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class LobbedExplosive : MonoBehaviour
{
    [SerializeField] float damage = 100f;
    [SerializeField] float radius = 8f;
    [SerializeField] float fuseSeconds = 1.2f;
    [SerializeField] LayerMask damageMask = ~0;

    Rigidbody rb;
    Collider col;
    bool armed;


    Collider[] ignoredForDamage;

    public void SetDamage(float d) { damage = d; }
    public void SetRadius(float r) { radius = Mathf.Max(0f, r); }
    public void SetFuse(float f) { fuseSeconds = Mathf.Max(0.05f, f); }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(Vector3 initialVelocity, Collider[] ignoreColliders = null)
    {

        ignoredForDamage = ignoreColliders;

        rb.linearVelocity = initialVelocity;

        if (!armed)
        {
            armed = true;
            Invoke(nameof(Explode), fuseSeconds);
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (!armed) return;


        Explode();
        Destroy(gameObject);
    }

    void Explode()
    {
  
        var hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Ignore);

        foreach (var h in hits)
        {
            if (IsIgnored(h)) 
                continue;

            if (h.TryGetComponent<IDamageable>(out var dmg))
            {
                Vector3 point = h.ClosestPoint(transform.position);
                dmg.TakeDamage(damage, point);
            }
        }
    }

    bool IsIgnored(Collider c)
    {
        if (ignoredForDamage == null) return false;

        foreach (var ig in ignoredForDamage)
        {
            if (!ig) continue;
            if (ig == c) return true;
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
