using UnityEngine;
using TMPro;

public class TMPBreath : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] float speed = 2f;
    [SerializeField] float minScale = 0.95f;
    [SerializeField] float maxScale = 1.05f;
    [SerializeField] float minAlpha = 0.5f;
    [SerializeField] float maxAlpha = 1f;

    Vector3 baseScale;
    Color baseColor;

    void Awake()
    {
        if (!tmp) tmp = GetComponent<TextMeshProUGUI>();
        baseScale = transform.localScale;
        if (tmp) baseColor = tmp.color;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;

        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * s;

        if (!tmp) return;
        Color c = baseColor;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        tmp.color = c;
    }
}
