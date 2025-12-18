using UnityEngine;
using UnityEngine.Events;

public class Health2 : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [Min(1)] public float maxHealth = 100f;
    public float Current { get; private set; }
    public bool IsAlive => Current > 0f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onDamaged; 

    [Header("Super Reward")]
    [SerializeField] SuperMeter super;     
    [SerializeField] float superOnDeath = 10f; 

    void Awake()
    {
        Current = maxHealth;
        if (!super)
        {
            super = FindObjectOfType<SuperMeter>();
        }
    }

    public void Initialize(float newMax)
    {
        maxHealth = Mathf.Max(1f, newMax);
        Current = maxHealth;
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (!IsAlive) return;

        amount = Mathf.Max(0f, amount);

        float old = Current;
        Current = Mathf.Max(0f, Current - amount);
        float delta = old - Current; 
        onDamaged?.Invoke(delta);

        if (!IsAlive)
        {
            onDeath?.Invoke();

            if (super != null && superOnDeath > 0f)
            {
                super.Add(superOnDeath);
            }
        }
    }
}
