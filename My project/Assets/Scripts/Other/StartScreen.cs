using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class StartScreen : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] string sceneToLoad;

    [Header("Timing")]
    [SerializeField] float minimumScreenTime = 0.5f;

    [Header("Fade")]
    [SerializeField] CanvasGroup fadeGroup;
    [SerializeField] float fadeDuration = 0.75f;

    [Header("Audio - Background Music")]
    [SerializeField] AudioSource musicSource;
    [Range(0f, 1f)]
    [SerializeField] float musicVolume = 0.7f;

    [Header("Audio - Start Sound")]
    [SerializeField] AudioSource startSoundSource;
    [Range(0f, 1f)]
    [SerializeField] float startSoundVolume = 1f;

    bool canStart;
    bool hasStarted;

    void Awake()
    {
        if (fadeGroup)
            fadeGroup.alpha = 0f;
    }

    void Start()
    {
        if (musicSource)
        {
            musicSource.volume = musicVolume;
            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        if (startSoundSource)
            startSoundSource.volume = startSoundVolume;

        if (minimumScreenTime <= 0f)
            canStart = true;
        else
            Invoke(nameof(EnableStart), minimumScreenTime);
    }

    void EnableStart()
    {
        canStart = true;
    }

    void Update()
    {
        if (!canStart || hasStarted)
            return;

        if (WasAnyButtonPressedThisFrame())
            StartGame();
    }

    void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        if (startSoundSource)
            startSoundSource.Play();

        if (fadeGroup && fadeDuration > 0f)
            StartCoroutine(FadeToBlackAndLoad());
        else
            LoadNextScene();
    }

    IEnumerator FadeToBlackAndLoad()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);

            if (fadeGroup) fadeGroup.alpha = a;
            if (musicSource) musicSource.volume = musicVolume * (1f - a);

            yield return null;
        }

        if (fadeGroup) fadeGroup.alpha = 1f;
        if (musicSource) musicSource.volume = 0f;

        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[StartScreen] sceneToLoad is not set.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    bool WasAnyButtonPressedThisFrame()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.anyKeyDown)
            return true;
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
            return true;

        foreach (var g in Gamepad.all)
        {
            if (g == null) continue;

            if (g.buttonSouth.wasPressedThisFrame ||
                g.buttonNorth.wasPressedThisFrame ||
                g.buttonEast.wasPressedThisFrame ||
                g.buttonWest.wasPressedThisFrame ||
                g.startButton.wasPressedThisFrame ||
                g.selectButton.wasPressedThisFrame ||
                g.leftShoulder.wasPressedThisFrame ||
                g.rightShoulder.wasPressedThisFrame ||
                g.leftTrigger.wasPressedThisFrame ||
                g.rightTrigger.wasPressedThisFrame)
                return true;
        }
#endif
        return false;
    }
}
