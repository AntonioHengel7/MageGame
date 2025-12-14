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

    [Header("Optional colors")]
    [SerializeField] Color healthColor = new Color(0.90f, 0.25f, 0.25f);
    [SerializeField] Color manaColor = new Color(0.25f, 0.45f, 0.95f);
    [SerializeField] Color superColor = new Color(0.95f, 0.75f, 0.25f);

    void Awake()
    {
        if (healthFill) healthFill.color = healthColor;
        if (manaFill) manaFill.color = manaColor;
        if (superFill) superFill.color = superColor;

        var root = health ? health.transform.root : null;
        if (!mana && root) mana = root.GetComponentInChildren<Mana>();
        if (!super && root) super = root.GetComponentInChildren<SuperMeter>();
        if (!health && (mana || super))
        {
            var r = (mana ? mana.transform.root : super.transform.root);
            health = r.GetComponentInChildren<Health>();
        }
    }

    void Update()
    {
        if (health && healthFill && health.Max > 0f)
            healthFill.fillAmount = Mathf.Clamp01(health.Current / health.Max);

        if (mana && manaFill && mana.Max > 0f)
            manaFill.fillAmount = Mathf.Clamp01(mana.Current / mana.Max);

        if (super && superFill && super.Max > 0f)
            superFill.fillAmount = Mathf.Clamp01(super.Current / super.Max);
    }
}
