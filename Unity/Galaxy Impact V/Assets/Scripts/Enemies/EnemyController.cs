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

    // 🔁 NUEVO: rotación hacia el jugador
    [Header("Rotación hacia el jugador")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform visualToRotate; // el sprite / hijo que rota (puede ser el propio transform)

    private AIDestinationSetter destSetter;
    private Transform target;

    private void Awake()
    {
        if (!health) health = GetComponent<Health>();

        if (health)
            health.OnDeath.AddListener(() => OnDeath.Invoke());

        // 👇 NUEVO
        destSetter = GetComponent<AIDestinationSetter>();

        // Si no asignas nada en el inspector, rotaremos este mismo objeto
        if (visualToRotate == null)
            visualToRotate = transform;
    }

    private void Update()
    {
        if (touchTimer > 0f)
            touchTimer -= Time.deltaTime;

        // 👇 NUEVO: coger el target aunque se asigne más tarde (EnemySetup lo pone en Start)
        if (target == null && destSetter != null)
        {
            target = destSetter.target;
        }

        RotateTowardsTarget(); // 👈 NUEVO
    }

    // 👇 NUEVO MÉTODO
    private void RotateTowardsTarget()
    {
        if (target == null || visualToRotate == null) return;

        Vector3 dir = target.position - visualToRotate.position;
        dir.z = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // IMPORTANTE:
        // - si tu sprite "mira" hacia la derecha, deja esto tal cual
        // - si tu sprite "mira" hacia arriba, pon: angle -= 90f;

        // angle -= 90f; // <- descomenta esta línea si el sprite mira hacia arriba en el editor

        Quaternion desiredRot = Quaternion.AngleAxis(angle, Vector3.forward);
        visualToRotate.rotation = Quaternion.Lerp(
            visualToRotate.rotation,
            desiredRot,
            rotationSpeed * Time.deltaTime
        );
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
