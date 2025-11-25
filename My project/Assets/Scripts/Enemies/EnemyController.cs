using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public EnemyDefinition def;
    public string playerTag = "Player";

    NavMeshAgent agent;
    Transform player;
    Health health;
    float nextAttackTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        // Apply stats from definition
        health.Initialize(def.maxHealth);
        agent.speed = def.moveSpeed;
        agent.angularSpeed = def.angularSpeed;
        agent.stoppingDistance = def.stoppingDistance;

        // On death -> grant ult and remove
        health.onDeath.AddListener(() =>
        {
            GameEvents.RaiseEnemyKilled(def.ultimateValueOnKill);
            Destroy(gameObject, 0.05f);
        });
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
    }

    void Update()
    {
        if (player == null || !health.IsAlive) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Chase
        if (dist > def.attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else // Attack
        {
            agent.isStopped = true;
            Face(player.position);
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + def.attackCooldown;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    System.Collections.IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(def.attackWindup);

        if (def.attackKind == AttackKind.Melee)
        {
            if (player && Vector3.Distance(transform.position, player.position) <= def.attackRange)
            {
                if (player.TryGetComponent<IDamageable>(out var dmg))
                    dmg.TakeDamage(def.attackDamage, player.position);
            }
        }
        else // Ranged
        {
            if (!def.projectilePrefab || !player) yield break;

            Vector3 dir = (player.position + Vector3.up * 0.8f - transform.position).normalized;
            var go = Instantiate(def.projectilePrefab,
                                 transform.position + dir * 0.8f + Vector3.up * 0.7f,
                                 Quaternion.LookRotation(dir));

            var proj = go.GetComponent<SpellProjectile>();
            if (proj)
            {
                proj.speed = def.projectileSpeed;
                proj.damage = def.attackDamage;
                proj.life = def.projectileLifetime;
                proj.owner = transform;
                proj.hitMask = LayerMask.GetMask("Player"); // ensure Player layer exists
            }
        }
    }

    void Face(Vector3 pos)
    {
        Vector3 d = pos - transform.position; d.y = 0f;
        if (d.sqrMagnitude > 0.001f)
        {
            var r = Quaternion.LookRotation(d);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, r, agent.angularSpeed * Time.deltaTime);
        }
    }
}
