using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public float speed = 16f;
    public float damage = 10f;
    public float life = 4f;
    public LayerMask hitMask;    // set in Inspector (Player layer)
    public Transform owner;      // who fired (ignored on hit)
    float t;

    void Update()
    {
        t += Time.deltaTime;
        if (t >= life) { Destroy(gameObject); return; }
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner && other.transform == owner) return;
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        if (other.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(damage, transform.position);

        Destroy(gameObject);
    }
}
