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
    [SerializeField] float deathReloadDelay = 1.0f;
    [SerializeField] CanvasGroup fadeCanvasGroup;
    [SerializeField] float fadeDuration = 1.0f;
    [SerializeField] string deathSceneName = "DeathScene";

    public static string LastSceneName { get; private set; }

    bool isDying;

    void Awake()
    {
        Current = max;

        if (fadeCanvasGroup)
            fadeCanvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (!IsAlive || max <= 0f) return;

        Current = Mathf.Min(max, Current + regenPerSec * Time.deltaTime);
    }

    public void ApplyDamage(float amount)
    {
        if (!IsAlive) return;

        Current = Mathf.Max(0f, Current - Mathf.Max(0f, amount));
    }

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

        float remaining = Mathf.Max(0f, deathReloadDelay - timer);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        LastSceneName = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(deathSceneName))
            SceneManager.LoadScene(deathSceneName);
        else
            SceneManager.LoadScene(LastSceneName);
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
