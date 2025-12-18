using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health2))] // change to Health if your script is named that
public class EnemyController : MonoBehaviour
{
    [Header("Definition")]
    public EnemyDefinition def;
    public string playerTag = "Player";

    [Header("Animation")]
    public Animator animator;               // assign the goblin_anim Animator here
    public string moveBool = "IsMoving";    // bool parameter for idle/walk blend
    public string attackTrigger = "Attack"; // trigger for attack animation
    public string dieTrigger = "Die";       // trigger for death animation
    public float deathDestroyDelay = 2f;    // time to let death anim play

    [Header("Ranged Settings (optional)")]
    public Transform projectileSpawnPoint;  // assign hand/mouth/etc; optional

    NavMeshAgent agent;
    Transform player;
    Health2 health;
    float nextAttackTime;
    bool isDead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health2>();

        

        // Apply stats from definition
        health.Initialize(def.maxHealth);
        agent.speed = def.moveSpeed;
        agent.angularSpeed = def.angularSpeed;
        agent.stoppingDistance = def.stoppingDistance;

        // On death -> play animation, grant ult and remove after delay
        health.onDeath.AddListener(OnDeath);
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null || !health.IsAlive)
        {
            SetMoving(false);
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        // Chase
        if (dist > def.attackRange)
        {
            //agent.isStopped = false;
            //agent.SetDestination(player.position);

            // movement animation based on velocity
            bool moving = agent.velocity.sqrMagnitude > 0.01f;
            SetMoving(moving);
        }
        else // Attack
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            SetMoving(false);

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
        // trigger attack animation
        if (animator && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        // wait for windup so the hit happens in sync with the anim
        yield return new WaitForSeconds(def.attackWindup);

        if (!health.IsAlive || isDead) yield break;

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

            Vector3 targetPos = player.position + Vector3.up * 0.8f;
            Vector3 dir = (targetPos - transform.position).normalized;

            Vector3 spawnPos;
            if (projectileSpawnPoint)
                spawnPos = projectileSpawnPoint.position;
            else
                spawnPos = transform.position + dir * 0.8f + Vector3.up * 0.7f;

            var go = Instantiate(def.projectilePrefab,
                                 spawnPos,
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
        Vector3 d = pos - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude > 0.001f)
        {
            var r = Quaternion.LookRotation(d);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                r,
                agent.angularSpeed * Time.deltaTime
            );
        }
    }

    void OnDeath()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        agent.ResetPath();
        SetMoving(false);

        // death animation
        if (animator && !string.IsNullOrEmpty(dieTrigger))
            animator.SetTrigger(dieTrigger);

        // grant ult
        GameEvents.RaiseEnemyKilled(def.ultimateValueOnKill);

        // destroy after the death animation has time to play
        Destroy(gameObject, deathDestroyDelay);
    }

    void SetMoving(bool moving)
    {
        if (!animator || string.IsNullOrEmpty(moveBool)) return;
        animator.SetBool(moveBool, moving);
    }
}
