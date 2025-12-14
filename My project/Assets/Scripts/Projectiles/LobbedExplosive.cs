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

    public void SetDamage(float d) { damage = d; }
    public void SetRadius(float r) { radius = Mathf.Max(0f, r); }
    public void SetFuse(float f) { fuseSeconds = Mathf.Max(0.05f, f); }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(Vector3 initialVelocity, Collider[] ignoreColliders = null)
    {
        if (ignoreColliders != null && col != null)
        {
            foreach (var ig in ignoreColliders)
                if (ig && ig != col) Physics.IgnoreCollision(col, ig, true);
        }
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
    }

    void Explode()
    {
        var hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.TryGetComponent<Health>(out var hp))
            {
                hp.ApplyDamage(damage);
                if (hp.TryGetComponent<DummyTarget>(out var dummy)) dummy.OnDamaged();
            }
            else if (h.TryGetComponent<DummyTarget>(out var d))
            {
                d.ApplyDamage(damage);
            }
        }
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
