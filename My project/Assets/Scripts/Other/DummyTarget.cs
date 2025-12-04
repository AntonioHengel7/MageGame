using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DummyTarget : MonoBehaviour
{
    [Header("Health (optional)")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool useInternalHealth = true; // if false, script will use an existing Health component
    [SerializeField] Renderer[] renderers;
    [SerializeField] Color flashColor = Color.yellow;
    [SerializeField] float flashTime = 0.07f;

    float current;
    Color[] baseColors;
    float flashTimer;

    void Awake()
    {
        if (useInternalHealth)
            current = maxHealth;

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] && renderers[i].material)
                baseColors[i] = renderers[i].material.color;
        }

        Debug.Log($"[DummyTarget] Awake on {gameObject.name}. MaxHealth = {maxHealth}");
    }

    public void ApplyDamage(float amount)
    {
        Debug.Log($"[DummyTarget] {gameObject.name} took {amount} damage.");
        if (!useInternalHealth) return;
        current -= amount;
        OnDamaged();
        Debug.Log($"[DummyTarget] {gameObject.name} health now {current}");
        if (current <= 0f)
        {
            Debug.Log($"[DummyTarget] {gameObject.name} destroyed.");
            Destroy(gameObject);
        }
    }

    public void OnDamaged()
    {
        Debug.Log($"[DummyTarget] Flash triggered on {gameObject.name}");
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] && renderers[i].material)
                renderers[i].material.color = flashColor;
        }
        flashTimer = flashTime;
    }

    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] && renderers[i].material)
                        renderers[i].material.color = baseColors[i];
                }
                Debug.Log($"[DummyTarget] Flash reset on {gameObject.name}");
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(0.5f, 2f, 0.5f));
    }
#endif
}
