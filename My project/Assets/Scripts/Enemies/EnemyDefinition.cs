using UnityEngine;

public enum AttackKind { Melee, Ranged }

[CreateAssetMenu(menuName = "MageDoom/Enemy Definition", fileName = "Enemy_")]
public class EnemyDefinition : ScriptableObject
{
    [Header("Identity & Stats")]
    public string enemyName = "Goblin";
    public float maxHealth = 20f;
    public float moveSpeed = 2.5f;
    public float angularSpeed = 360f;
    public float stoppingDistance = 1.25f;

    [Header("Combat")]
    public AttackKind attackKind = AttackKind.Melee;
    public float attackRange = 1.5f;
    public float attackDamage = 8f;
    public float attackCooldown = 1.2f;
    public float attackWindup = 0.2f;

    [Header("Ranged")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 16f;
    public float projectileLifetime = 4f;

    [Header("Rewards")]
    public int ultimateValueOnKill = 5;

    [Header("Presentation")]
    public GameObject visualPrefab;
    public AudioClip[] attackSfx;
    public AudioClip[] hurtSfx;
    public AudioClip[] deathSfx;
}
