using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ExplosiveBolt : MonoBehaviour
{
    [SerializeField] float damage = 15f;
    [SerializeField] float radius = 1.5f;
    [SerializeField] float lifeSeconds = 10f;

    Rigidbody rb; Collider col;

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
    }

    public void SetDamage(float d) { damage = d; }
    public void SetRadius(float r) { radius = Mathf.Max(0f, r); }
    public void SetLifetime(float seconds) { lifeSeconds = Mathf.Max(0.01f, seconds); }

    public void Launch(Vector3 direction, float speed, Collider[] ignoreColliders = null)
    {
        if (ignoreColliders != null && col != null)
            foreach (var ig in ignoreColliders) if (ig && ig != col) Physics.IgnoreCollision(col, ig, true);

        rb.linearVelocity = direction.normalized * speed;
        Debug.Log($"[ExplosiveBolt] Launch dir={direction.normalized} speed={speed}");
        Invoke(nameof(Despawn), lifeSeconds);
    }

    void OnCollisionEnter(Collision c)
    {
        Debug.Log($"[ExplosiveBolt] Hit {c.collider.name}");
        Despawn();
    }

    void Despawn() { Destroy(gameObject); }
}
