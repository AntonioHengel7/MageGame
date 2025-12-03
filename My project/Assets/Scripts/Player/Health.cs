using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float max = 100f;
    [SerializeField] float regenPerSec = 5f;

    public float Current { get; private set; }
    public float Max => max;
    public float Normalized => max <= 0f ? 0f : Current / max;

    void Awake() { Current = max; }

    void Update()
    {
        if (Current <= 0f || max <= 0f) return;
        Current = Mathf.Min(max, Current + regenPerSec * Time.deltaTime);
    }

    public void ApplyDamage(float amount)
    {
        Current = Mathf.Max(0f, Current - Mathf.Max(0f, amount));
    }

    public void SetMax(float newMax, bool refill = true)
    {
        max = Mathf.Max(0f, newMax);
        if (refill) Current = max;
        else Current = Mathf.Min(Current, max);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || max <= 0f) return;
        Current = Mathf.Min(max, Current + amount);
    }
}
