using UnityEngine;

public class SuperMeter : MonoBehaviour
{
  [SerializeField] float max = 100f;
  public float Current { get; private set; }
  public float Max => max;
  public float Normalized => Mathf.Approximately(max, 0) ? 0 : Current / max;

  void Awake() { Current = max; } // full for  testing

  public void Add(float amount) { Current = Mathf.Clamp(Current + amount, 0f, max); }
  public bool TrySpend(float amount)
  {
    if (Current < amount) return false;
    Current -= amount;
    return true;
  }
}
