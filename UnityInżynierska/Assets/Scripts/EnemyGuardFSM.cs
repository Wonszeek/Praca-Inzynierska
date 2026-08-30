using UnityEngine;

public class EnemyGuardFSM : MonoBehaviour
{
    private enum State
    {
        Guard,
        Alert,
        Attack
    }

    [Header("Guard")]
    public float turnInterval = 2f;
    public int startFacingDirection = 1;

    private float turnTimer;
    private int facingDirection;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.4f;

    [Header("Attack")]
    public int attackDamage = 1;
    public float attackCooldown = 1.8f;
    private float attackTimer = 0f;

    [Header("Attack Hitbox")]
    public Transform attackPoint;
    public float attackHitRange = 0.9f;

    [Header("Debug")]
    public bool showGizmos = true;

    private State currentState = State.Guard;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Start()
    {
        facingDirection = startFacingDirection >= 0 ? 1 : -1;

        turnTimer = turnInterval;
        attackTimer = 0f;

        SetAnimSpeed(0f);
        Flip(facingDirection);
    }

    private void Update()
    {
        if (player == null) return;

        if (enemyHealth != null && enemyHealth.GetCurrentHealth() <= 0)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Guard:
                UpdateGuard();
                break;

            case State.Alert:
                UpdateAlert();
                break;

            case State.Attack:
                UpdateAttack();
                break;
        }
    }

    private void UpdateGuard()
    {
        StopMoving();

        turnTimer -= Time.deltaTime;

        if (turnTimer <= 0f)
        {
            facingDirection *= -1;
            Flip(facingDirection);
            turnTimer = turnInterval;
        }

        if (CanSeePlayer())
        {
            ChangeState(State.Alert);
        }
    }

    private void UpdateAlert()
    {
        StopMoving();
        FacePlayer();

        float dist = DistanceToPlayer();

        if (dist > detectionRange)
        {
            ChangeState(State.Guard);
            return;
        }

        if (dist <= attackRange)
        {
            ChangeState(State.Attack);
            return;
        }
    }

    private void UpdateAttack()
    {
        StopMoving();
        FacePlayer();

        float dist = DistanceToPlayer();

        if (dist > detectionRange)
        {
            ChangeState(State.Guard);
            return;
        }

        if (dist > attackRange)
        {
            ChangeState(State.Alert);
            return;
        }

        if (attackTimer <= 0f && CanStartAttackAnimation())
        {
            StartAttack();
        }
    }

    private bool CanSeePlayer()
    {
        float dist = DistanceToPlayer();

        if (dist > detectionRange)
            return false;

        Vector2 directionToPlayer = player.position - transform.position;

        bool playerInFront =
            (facingDirection == 1 && directionToPlayer.x > 0) ||
            (facingDirection == -1 && directionToPlayer.x < 0);

        return playerInFront;
    }

    private float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    private void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            facingDirection = 1;
        else
            facingDirection = -1;

        Flip(facingDirection);
    }

    private void StopMoving()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        SetAnimSpeed(0f);
    }

    private void StartAttack()
    {
        if (anim == null) return;

        anim.ResetTrigger("EnemyAttackTrigger");
        anim.SetTrigger("EnemyAttackTrigger");

        attackTimer = attackCooldown;

        Debug.Log("[EnemyGuardFSM] StartAttack");
    }

    private bool CanStartAttackAnimation()
    {
        if (anim == null) return true;

        if (anim.IsInTransition(0))
            return false;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("EnemyAttack"))
            return false;

        return true;
    }

    private void ForceIdleAnimation()
    {
        if (anim == null) return;

        anim.ResetTrigger("EnemyAttackTrigger");

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("EnemyAttack"))
        {
            anim.Play("EnemyIdle", 0, 0f);
        }
    }

    private void SetAnimSpeed(float speed)
    {
        if (anim != null)
            anim.SetFloat("EnemySpeed", speed);
    }

    private void Flip(int direction)
    {
        transform.localScale = new Vector3(direction, 1f, 1f);
    }

    public void HitPlayer()
    {
        Debug.Log("[EnemyGuardFSM] HitPlayer z Animation Event!");

        if (enemyHealth != null && enemyHealth.GetCurrentHealth() <= 0)
            return;

        if (player == null)
            return;

        if (Vector2.Distance(transform.position, player.position) > attackRange * 1.5f)
        {
            Debug.Log("[EnemyGuardFSM] Gracz poza zasięgiem ataku — brak obrażeń.");
            return;
        }

        if (attackPoint == null)
        {
            Debug.LogWarning("[EnemyGuardFSM] Brakuje AttackPoint!");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackHitRange
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();

            if (health == null)
                health = hit.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(attackDamage, transform);
                Debug.Log("[EnemyGuardFSM] Trafiono gracza!");
                return;
            }
        }

        Debug.Log("[EnemyGuardFSM] Atak nikogo nie trafił.");
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;

        Debug.Log($"[EnemyGuardFSM] {currentState} → {newState}");

        if (currentState == State.Attack && newState != State.Attack)
        {
            ForceIdleAnimation();
        }

        currentState = newState;

        if (currentState == State.Attack)
        {
            StartAttack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRange);
        }
    }
}