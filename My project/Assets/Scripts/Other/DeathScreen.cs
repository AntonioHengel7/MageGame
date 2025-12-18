using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class DeathScreen : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] CanvasGroup fadeCanvasGroup;
    [SerializeField] float fadeInDuration = 0.75f;
    [SerializeField] float fadeOutDuration = 0.75f;

    [Header("Retry Text")]
    [SerializeField] TMP_Text retryText;
    [SerializeField] float pulseSpeed = 4f;
    [SerializeField] float pulseIntensity = 0.2f;

    Color baseTextColor;
    bool canInput;
    bool isReloading;

    void Awake()
    {
        if (fadeCanvasGroup)
            fadeCanvasGroup.alpha = 1f;

        if (retryText)
            baseTextColor = retryText.color;
    }

    void Start()
    {
        if (fadeCanvasGroup && fadeInDuration > 0f)
            StartCoroutine(FadeIn());
        else
            canInput = true;
    }

    void Update()
    {
        if (retryText)
            UpdateBreathingText();

        if (!canInput || isReloading)
            return;

        if (WasRetryPressed())
            StartReload();
    }

    void UpdateBreathingText()
    {
        float pulse = Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f;
        float brightness = 1f + (pulse - 0.5f) * 2f * pulseIntensity;
        retryText.color = baseTextColor * brightness;
    }

    bool WasRetryPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            return true;
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            return true;
#endif
        return false;
    }

    IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeInDuration);
            if (fadeCanvasGroup) fadeCanvasGroup.alpha = 1f - a;
            yield return null;
        }

        if (fadeCanvasGroup) fadeCanvasGroup.alpha = 0f;
        canInput = true;
    }

    void StartReload()
    {
        if (isReloading) return;
        isReloading = true;

        if (fadeCanvasGroup && fadeOutDuration > 0f)
            StartCoroutine(FadeOutAndReload());
        else
            ReloadScene();
    }

    IEnumerator FadeOutAndReload()
    {
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeOutDuration);
            if (fadeCanvasGroup) fadeCanvasGroup.alpha = a;
            yield return null;
        }

        if (fadeCanvasGroup) fadeCanvasGroup.alpha = 1f;
        ReloadScene();
    }

    void ReloadScene()
    {
        string target = Health.LastSceneName;
        if (string.IsNullOrEmpty(target))
            target = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(target);
    }
}
