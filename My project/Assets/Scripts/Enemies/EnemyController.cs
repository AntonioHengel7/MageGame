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

    [Header("Audio")]
    public AudioSource audioSource;         // assign or auto-grab from this object
    public AudioClip[] attackClips;        // random attack grunts / shots
    public AudioClip[] deathClips;         // random death sounds
    [Range(0f, 1f)]
    public float attackVoiceChance = 0.1f; // 10% default

    NavMeshAgent agent;
    Transform player;
    Health2 health;
    float nextAttackTime;
    bool isDead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health2>();

        // Try to auto-grab an AudioSource if not set in inspector
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

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

            // Optional: also face the player while moving
            Face(player.position);

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

        // 10% chance (or whatever attackVoiceChance is) to play an attack sound
        TryPlayRandomClip(attackClips, attackVoiceChance, spatial: false);

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

            // Choose where the projectile starts
            Vector3 spawnPos;
            if (projectileSpawnPoint != null)
            {
                // e.g. wizard hand / staff tip
                spawnPos = projectileSpawnPoint.position;
            }
            else
            {
                // fallback: enemy chest height
                spawnPos = transform.position + Vector3.up * 1.2f;
            }

            // Aim roughly at the player's chest
            Vector3 targetPos = player.position + Vector3.up * 0.5f;

            // Direction from spawn to target
            Vector3 dir = (targetPos - spawnPos).normalized;

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

        // Always play a death sound if we have one
        TryPlayRandomClip(deathClips, 1f, spatial: true);

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

    // --- AUDIO HELPERS ---

    void TryPlayRandomClip(AudioClip[] clips, float chance, bool spatial)
    {
        if (clips == null || clips.Length == 0) return;
        if (chance <= 0f) return;
        if (Random.value > chance) return;

        var clip = clips[Random.Range(0, clips.Length)];
        if (!clip) return;

        if (spatial)
        {
            // Plays at world position, survives even if this enemy is destroyed
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        else
        {
            if (audioSource)
                audioSource.PlayOneShot(clip);
        }
    }
}
