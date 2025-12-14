using UnityEngine;
using UnityEngine.UI;

public class PlayerUltimate : MonoBehaviour
{
    public int currentUlt;
    public int maxUlt = 100;
    public Slider ultSlider;

    void OnEnable() => GameEvents.EnemyKilled += OnEnemyKilled;
    void OnDisable() => GameEvents.EnemyKilled -= OnEnemyKilled;

    void OnEnemyKilled(int amount)
    {
        currentUlt = Mathf.Clamp(currentUlt + amount, 0, maxUlt);
        if (ultSlider) ultSlider.value = (float)currentUlt / maxUlt;
    }

    public bool CanCastUltimate() => currentUlt >= maxUlt;
    public void SpendUltimate() => currentUlt = 0;
}
