using UnityEngine;

/// <summary>
/// Prosty NPC z Finite State Machine (FSM).
/// Stany: Patrol → Chase → Attack.
/// 
/// Wymagania na obiekcie Enemy:
/// - Rigidbody2D
/// - Collider2D
/// - Animator z parametrami:
///     EnemySpeed - Float
///     EnemyAttackTrigger - Trigger
/// - Child object GroundCheck
/// - Child object AttackPoint
/// - Player z Tag: Player i Layer: Player
/// - Ground z Layer: Ground
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFSM : MonoBehaviour
{
    // ── Stany FSM ────────────────────────────────────────────────────────────
    private enum State { Patrol, Chase, Attack }
    private State currentState;

    // ── Referencje ───────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private PlayerHealth playerHealth;

    // ── Parametry patrolu ────────────────────────────────────────────────────
    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;
    private Vector2 startPosition;
    private int patrolDirection = 1; // 1 = prawo, -1 = lewo

    [Header("Wykrywanie krawędzi")]
    public Transform groundCheck;
    public float groundCheckDistance = 1.5f;
    public LayerMask groundLayer;

    // ── Parametry detekcji gracza ────────────────────────────────────────────
    [Header("Detekcja gracza")]
    public float detectionRange = 5f;
    public float loseRange = 8f;

    // ── Parametry gonienia ───────────────────────────────────────────────────
    [Header("Chase")]
    public float chaseSpeed = 3.5f;

    // ── Parametry ataku ──────────────────────────────────────────────────────
    [Header("Atak")]
    public float attackRange = 1.2f; // dystans wejścia w stan Attack
    public int attackDamage = 1;
    public float attackCooldown = 1.2f;
    private float attackTimer = 0f;

    [Header("Hitbox ataku")]
    public Transform attackPoint;
    public float attackHitRange = 0.6f;
    public LayerMask playerLayer;

    // ── Hit stun ─────────────────────────────────────────────────────────────
    [Header("Hit / Stun")]
    public float hitStunDuration = 0.2f;
    private float hitStunTimer = 0f;

    // ── Debug ────────────────────────────────────────────────────────────────
    [Header("Debug")]
    public bool showGizmos = true;

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        startPosition = transform.position;
        currentState = State.Patrol;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                Debug.LogWarning("[EnemyFSM] Nie znaleziono PlayerHealth na graczu!");
        }
        else
        {
            Debug.LogWarning("[EnemyFSM] Nie znaleziono obiektu z tagiem 'Player'!");
        }
    }

    private void Update()
    {
        if (player == null) return;

        attackTimer -= Time.deltaTime;

        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
            SetAnimSpeed(0f);
            return;
        }

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;
        }
    }

    // ── PATROL ───────────────────────────────────────────────────────────────
    private void UpdatePatrol()
    {
        if (groundCheck != null)
        {
            bool groundAhead = Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            if (!groundAhead)
            {
                patrolDirection *= -1;
            }
        }

        rb.linearVelocity = new Vector2(patrolSpeed * patrolDirection, rb.linearVelocity.y);
        Flip(patrolDirection);
        SetAnimSpeed(patrolSpeed);

        float distFromStart = transform.position.x - startPosition.x;

        if (distFromStart > patrolDistance)
            patrolDirection = -1;

        if (distFromStart < -patrolDistance)
            patrolDirection = 1;

        if (DistanceToPlayer() <= detectionRange)
        {
            Debug.Log("[EnemyFSM] Wykryto gracza → Chase");
            ChangeState(State.Chase);
        }
    }

    // ── CHASE ────────────────────────────────────────────────────────────────
    private void UpdateChase()
    {
        float dist = DistanceToPlayer();

        if (dist > loseRange)
        {
            Debug.Log("[EnemyFSM] Zgubiono gracza → Patrol");
            ChangeState(State.Patrol);
            return;
        }

        if (dist <= attackRange)
        {
            Debug.Log("[EnemyFSM] W zasięgu ataku → Attack");
            ChangeState(State.Attack);
            return;
        }

        int dir = player.position.x > transform.position.x ? 1 : -1;

        rb.linearVelocity = new Vector2(chaseSpeed * dir, rb.linearVelocity.y);
        Flip(dir);
        SetAnimSpeed(chaseSpeed);
    }

    // ── ATTACK ───────────────────────────────────────────────────────────────
    private void UpdateAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        SetAnimSpeed(0f);

        int dir = player.position.x > transform.position.x ? 1 : -1;
        Flip(dir);

        float dist = DistanceToPlayer();

        if (dist > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    // ── Logika ataku ─────────────────────────────────────────────────────────
    private void PerformAttack()
    {
        TriggerAttackAnim();
        
    }

    /// <summary>
    /// Docelowo tę metodę wywoła Animation Event w klipie EnemyAttack.
    /// </summary>
    public void HitPlayer()
    {
        Debug.Log("[EnemyFSM] HitPlayer z Animation Event!");

        if (player == null)
        {
            Debug.LogWarning("[EnemyFSM] Brakuje referencji do gracza!");
            return;
        }

        if (attackPoint == null)
        {
            Debug.LogWarning("[EnemyFSM] Brakuje AttackPoint!");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange * 1.3f)
        {
            Debug.Log("[EnemyFSM] Gracz poza zasięgiem ataku — brak obrażeń.");
            return;
        }

        Collider2D hitPlayer = Physics2D.OverlapCircle(
            attackPoint.position,
            attackHitRange,
            playerLayer
        );

        if (hitPlayer == null)
        {
            Debug.Log("[EnemyFSM] Hitbox ataku nikogo nie trafił.");
            return;
        }

        PlayerHealth health = hitPlayer.GetComponent<PlayerHealth>();

        if (health == null)
            health = hitPlayer.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(attackDamage, transform);
            Debug.Log($"[EnemyFSM] Trafiono gracza! Obrażenia: {attackDamage}");
        }
        else
        {
            Debug.LogWarning("[EnemyFSM] Trafiony obiekt nie ma PlayerHealth!");
        }
    }

    // ── Pomocnicze ───────────────────────────────────────────────────────────
    private void ChangeState(State newState)
    {
        currentState = newState;
    }

    public void ApplyHitStun()
    {
        hitStunTimer = hitStunDuration;
    }

    private float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    private void Flip(int direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void SetAnimSpeed(float speed)
    {
        if (anim != null)
            anim.SetFloat("EnemySpeed", Mathf.Abs(speed));
    }

    private void TriggerAttackAnim()
    {
        if (anim != null)
            anim.SetTrigger("EnemyAttackTrigger");
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Vector3 start = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Gizmos.DrawLine(start + Vector3.left * patrolDistance, start + Vector3.right * patrolDistance);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * groundCheckDistance
            );
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRange);
        }
    }
}