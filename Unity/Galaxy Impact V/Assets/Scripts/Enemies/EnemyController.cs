using UnityEngine;

/// Enemigo que sigue al jugador y daña por contacto.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Ataque por contacto")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float touchCooldown = 0.5f;

    [Header("Referencias")]
    [SerializeField] private Health health;
    private Transform player;
    private Rigidbody2D rb;
    private float touchTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!health)
            health = GetComponent<Health>();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Cuando muere, lo destruimos
        if (health != null)
            health.OnDeath.AddListener(() => Destroy(gameObject));
    }

    private void FixedUpdate(){
        if (!player) return;

        Vector2 currentPos = rb.position;
        Vector2 dir = ((Vector2)player.position - currentPos).normalized;
        Vector2 nextPos = currentPos + dir * (moveSpeed * Time.fixedDeltaTime);

        // Solo mover si no hay contacto directo con el jugador
        bool touchingPlayer = Physics2D.OverlapCircle(currentPos, 0.4f, LayerMask.GetMask("Player"));
        if (!touchingPlayer)
        {
            rb.MovePosition(nextPos);
        }

        if (touchTimer > 0f)
            touchTimer -= Time.fixedDeltaTime;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (touchTimer > 0f) return;
        if (collision.collider.CompareTag("Player"))
        {
            if (collision.collider.TryGetComponent<Health>(out var hp))
                hp.TakeDamage(contactDamage);

            touchTimer = touchCooldown;
        }
    }
}
