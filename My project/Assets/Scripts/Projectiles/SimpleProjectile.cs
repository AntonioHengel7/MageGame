using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SimpleProjectile : MonoBehaviour
{
    [SerializeField] float damage = 20f;
    [SerializeField] bool useGravity = false;
    [SerializeField] float drag = 0f;

    Rigidbody rb;
    Collider col;
    float lifeSeconds = 6f;

    public void SetDamage(float d) { damage = d; }
    public void SetLifetime(float seconds) { lifeSeconds = Mathf.Max(0.01f, seconds); }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.useGravity = useGravity;
        rb.linearDamping = drag;
        rb.angularDamping = 0f;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(Vector3 direction, float speed, Collider[] ignoreColliders = null)
    {
        if (ignoreColliders != null && col != null)
        {
            foreach (var ig in ignoreColliders)
                if (ig && ig != col) Physics.IgnoreCollision(col, ig, true);
        }
        rb.linearVelocity = direction.normalized * speed;
        if (lifeSeconds > 0f) Invoke(nameof(Despawn), lifeSeconds);
    }

    void OnCollisionEnter(Collision c)
    {
        ApplyDamageIfPossible(c.collider);
        Despawn();
    }

    void OnTriggerEnter(Collider other)
    {
        ApplyDamageIfPossible(other);
        Despawn();
    }

    void ApplyDamageIfPossible(Collider target)
{
    if (target.TryGetComponent<IDamageable>(out var dmg))
    {
        Vector3 hitPoint = target.ClosestPoint(transform.position);
        dmg.TakeDamage(damage, hitPoint);
    }
}


    void Despawn() => Destroy(gameObject);
}
