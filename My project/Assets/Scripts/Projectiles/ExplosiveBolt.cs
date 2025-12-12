using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ExplosiveBolt : MonoBehaviour
{
    [SerializeField] float damage = 15f;
    [SerializeField] float radius = 1.5f;
    [SerializeField] float lifeSeconds = 10f;
    [SerializeField] LayerMask damageMask = ~0;

    Rigidbody rb;
    Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Debug.Log($"[ExplosiveBolt] Awake {name}");
        if (lifeSeconds > 0f) Invoke(nameof(Despawn), lifeSeconds);
    }

    public void SetDamage(float d) { damage = d; }
    public void SetRadius(float r) { radius = Mathf.Max(0f, r); }
    public void SetLifetime(float seconds) { lifeSeconds = Mathf.Max(0.01f, seconds); }

    public void Launch(Vector3 direction, float speed, Collider[] ignoreColliders = null)
    {
        if (ignoreColliders != null && col != null)
            foreach (var ig in ignoreColliders)
                if (ig && ig != col) Physics.IgnoreCollision(col, ig, true);

        rb.linearVelocity = direction.normalized * speed;
        Debug.Log($"[ExplosiveBolt] Launch dir={direction.normalized} speed={speed}");
    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log($"[ExplosiveBolt] Hit {c.collider.name}");
        Explode();
        Despawn();
    }

    void Explode()
    {
        // Find everything in the explosion radius
        var hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.TryGetComponent<IDamageable>(out var dmg))
            {
                Vector3 hitPoint = h.ClosestPoint(transform.position);
                dmg.TakeDamage(damage, hitPoint);
            }
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
