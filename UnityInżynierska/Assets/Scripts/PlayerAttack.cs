using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1f;
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    private float nextAttackTime = 0f;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;
        TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        if (anim != null)
            anim.SetTrigger("AttackTrigger");
        nextAttackTime = Time.time + attackCooldown;
    }

    public void Attack()
    {
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.5f;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage, transform);
        }

        Debug.Log("Player attacked!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackOrigin = (Vector2)transform.position + attackDirection * 0.5f;
        Gizmos.DrawWireSphere(attackOrigin, attackRange);
    }
}