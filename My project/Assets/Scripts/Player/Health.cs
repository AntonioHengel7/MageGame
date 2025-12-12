using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float max = 100f;
    [SerializeField] float regenPerSec = 5f;

    public float Current { get; private set; }
    public float Max => max;
    public float Normalized => max <= 0f ? 0f : Current / max;

    public bool IsAlive => Current > 0f;

    [Header("Death / Respawn")]
    [Tooltip("Total delay before reloading the scene (seconds).")]
    [SerializeField] float deathReloadDelay = 1.0f;

    [Tooltip("CanvasGroup used to fade the screen to black.")]
    [SerializeField] CanvasGroup fadeCanvasGroup;

    [Tooltip("How long the fade takes (seconds).")]
    [SerializeField] float fadeDuration = 1.0f;

    bool isDying;

    void Awake()
    {
        Current = max;

        // Ensure fade starts transparent
        if (fadeCanvasGroup)
            fadeCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (!IsAlive || max <= 0f) return;

        Current = Mathf.Min(max, Current + regenPerSec * Time.deltaTime);
    }

    // internal damage logic
    public void ApplyDamage(float amount)
    {
        if (!IsAlive) return;

        Current = Mathf.Max(0f, Current - Mathf.Max(0f, amount));
    }

    // IDamageable implementation
    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (!IsAlive || isDying) return;

        ApplyDamage(amount);

        if (!IsAlive && !isDying)
        {
            isDying = true;
            StartCoroutine(DeathRoutine());
        }
    }

    IEnumerator DeathRoutine()
    {
        float timer = 0f;

        // Fade out
        if (fadeCanvasGroup && fadeDuration > 0f)
        {
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeDuration);
                fadeCanvasGroup.alpha = t;
                yield return null;
            }
        }

        // Ensure total time until reload is at least deathReloadDelay
        float remaining = Mathf.Max(0f, deathReloadDelay - timer);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetMax(float newMax, bool refill = true)
    {
        max = Mathf.Max(0f, newMax);

        if (refill) Current = max;
        else Current = Mathf.Min(Current, max);
    }

    public void Heal(float amount)
    {
        if (!IsAlive || amount <= 0f || max <= 0f) return;

        Current = Mathf.Min(max, Current + amount);
    }
}
