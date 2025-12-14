using UnityEngine;
using UnityEngine.Events;

public class Health2 : MonoBehaviour, IDamageable
{
    [Min(1)] public float maxHealth = 100f;
    public float Current { get; private set; }
    public UnityEvent onDeath;
    public UnityEvent<float> onDamaged; // amount delta

    public bool IsAlive => Current > 0f;

    void Awake() => Current = maxHealth;

    public void Initialize(float newMax)
    {
        maxHealth = Mathf.Max(1f, newMax);
        Current = maxHealth;
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (!IsAlive) return;
        Current = Mathf.Max(0f, Current - Mathf.Max(0f, amount));
        onDamaged?.Invoke(amount);
        if (Current <= 0f) onDeath?.Invoke();
    }
}
