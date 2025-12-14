using UnityEngine;

public class Mana : MonoBehaviour {
  [SerializeField] float max = 100f;
  [SerializeField] float regenPerSec = 10f;
  public float Current { get; private set; }
  public float Max => max;
  public float Normalized => Current / max;

  void Awake(){ Current = max; }
  void Update(){ Current = Mathf.Min(max, Current + regenPerSec * Time.deltaTime); }
  public bool TrySpend(float cost){ if(Current < cost) return false; Current -= cost; return true; }
}
