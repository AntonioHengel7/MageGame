using UnityEngine;
using UnityEngine.UI;

public class UIResourcesHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Health health;
    [SerializeField] Mana mana;
    [SerializeField] SuperMeter super;

    [Header("Bars (fill Images)")]
    [SerializeField] Image healthFill;
    [SerializeField] Image manaFill;
    [SerializeField] Image superFill;

    [Header("Base colors (full value)")]
    [SerializeField] Color healthColorFull = new Color(0.90f, 0.25f, 0.25f);
    [SerializeField] Color manaColorFull   = new Color(0.25f, 0.45f, 0.95f);
    [SerializeField] Color superColorFull  = new Color(1.0f, 0.85f, 0.20f);

    [Header("Colors at minimum value")]
    [SerializeField] Color healthColorLow = new Color(0.40f, 0.05f, 0.05f);
    [SerializeField] Color manaColorLow   = new Color(0.05f, 0.10f, 0.30f);
    [SerializeField] Color superColorLow  = new Color(0.40f, 0.25f, 0.05f);

    [Header("Fill animation")]
    [SerializeField] float fillLerpSpeed = 8f;

    [Header("Pulse settings")]
    [SerializeField] float pulseSpeed = 4f;
    [SerializeField] float lowThreshold = 0.25f;
    [SerializeField] float lowPulseIntensity = 0.15f;
    [SerializeField] float superFullPulseIntensity = 0.35f;
    [SerializeField] float superSaturationBoost = 0.15f;

    void Awake()
    {
        // initialize displayed colors
        if (healthFill) healthFill.color = healthColorFull;
        if (manaFill)   manaFill.color   = manaColorFull;
        if (superFill)  superFill.color  = superColorFull;

     
        Transform root = health ? health.transform.root : null;
        if (!mana && root) mana = root.GetComponentInChildren<Mana>();
        if (!super && root) super = root.GetComponentInChildren<SuperMeter>();
        if (!health && (mana || super))
        {
            Transform r = mana ? mana.transform.root : super.transform.root;
            health = r.GetComponentInChildren<Health>();
        }
    }

    void Update()
    {
        float t, pulse, brightness;

        // ------------------------------
        // Health Bar
        // ------------------------------
        if (health && healthFill && health.Max > 0f)
        {
            t = Mathf.Clamp01(health.Current / health.Max);

            // smooth fill update
            healthFill.fillAmount = Mathf.Lerp(
                healthFill.fillAmount,
                t,
                Time.deltaTime * fillLerpSpeed
            );

            // base color interpolation
            Color baseCol = Color.Lerp(healthColorLow, healthColorFull, healthFill.fillAmount);

            // low health pulse effect
            if (t < lowThreshold)
            {
                pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
                brightness = 1f + pulse * lowPulseIntensity;
                healthFill.color = baseCol * brightness;
            }
            else
            {
                healthFill.color = baseCol;
            }
        }

        // ------------------------------
        // Mana Bar 
        // ------------------------------
        if (mana && manaFill && mana.Max > 0f)
        {
            t = Mathf.Clamp01(mana.Current / mana.Max);

            manaFill.fillAmount = Mathf.Lerp(
                manaFill.fillAmount,
                t,
                Time.deltaTime * fillLerpSpeed
            );

            Color baseCol = Color.Lerp(manaColorLow, manaColorFull, manaFill.fillAmount);

            // low mana pulse
            if (t < lowThreshold)
            {
                pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
                brightness = 1f + pulse * lowPulseIntensity;
                manaFill.color = baseCol * brightness;
            }
            else
            {
                manaFill.color = baseCol;
            }
        }

        // ------------------------------
        // Super Meter 
        // ------------------------------
        if (super && superFill && super.Max > 0f)
        {
            t = Mathf.Clamp01(super.Current / super.Max);

            superFill.fillAmount = Mathf.Lerp(
                superFill.fillAmount,
                t,
                Time.deltaTime * fillLerpSpeed
            );

            Color baseCol = Color.Lerp(superColorLow, superColorFull, superFill.fillAmount);

            // full super pulse
            if (t >= 1f)
            {
                pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;

                brightness = 1f + pulse * superFullPulseIntensity;

                // slight saturation push on pulse
                Color boosted = Color.Lerp(
                    baseCol,
                    new Color(baseCol.r * 1.2f, baseCol.g * 1.2f, baseCol.b * 0.8f),
                    superSaturationBoost * pulse
                );

                superFill.color = boosted * brightness;
            }
            else
            {
                superFill.color = baseCol;
            }
        }
    }
}
