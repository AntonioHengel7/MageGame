using System;

public static class GameEvents
{
    public static event Action<int> EnemyKilled; // payload = ult charge
    public static void RaiseEnemyKilled(int amount) => EnemyKilled?.Invoke(amount);
}
