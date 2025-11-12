using UnityEngine;
using Pathfinding;
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Daño por contacto")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float touchCooldown = 0.5f;
    [SerializeField] private float moveSpeed = 3.5f;
    private float touchTimer;

    [Header("Referencias")]
    [SerializeField] private Health health; // tu script de vida del enemigo
    public UnityEngine.Events.UnityEvent OnDeath = new UnityEngine.Events.UnityEvent();

    private void Awake()
    {
        if (!health) health = GetComponent<Health>();

        if (health)
            health.OnDeath.AddListener(() => OnDeath.Invoke());
    }

    private void Update()
    {
        if (touchTimer > 0f)
            touchTimer -= Time.deltaTime;
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
    public void SetDifficultyMultiplier(float multiplier)
    {
        moveSpeed *= multiplier;

        // Ahora sincronizamos con el componente de pathfinding
        var ai = GetComponent<IAstarAI>(); // funciona con AIPath o AILerp
        if (ai != null)
        {
            ai.maxSpeed = moveSpeed;
        }
    }
}
