using UnityEngine;

/// Movimiento top-down + rotación hacia el mouse.
/// Requiere Rigidbody2D.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;

    [Header("Referencias")]
    [SerializeField] private Camera cam;

    [Header("Límites del mapa")]
    [SerializeField] private SpriteRenderer mapBounds; // el sprite del fondo
    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float halfWidth;
    private float halfHeight;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Vector2 _currentVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (!cam) cam = Camera.main;
        if (mapBounds)
        {
            minBounds = mapBounds.bounds.min;
            maxBounds = mapBounds.bounds.max;

            // tamaño aproximado del jugador (para no salirse por la mitad)
            halfWidth = transform.localScale.x / 2f;
            halfHeight = transform.localScale.y / 2f;
        }
    }

    private void Update()
    {
        // Movimiento (WASD)
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Rotación hacia el cursor
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rb.SetRotation(angle);
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = _moveInput * moveSpeed;

        // Interpolación entre aceleración y frenado
        float rate = (targetVelocity.magnitude > _rb.linearVelocity.magnitude)? acceleration : deceleration;

        _currentVelocity = Vector2.MoveTowards(_rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
        _rb.linearVelocity = _currentVelocity;
        //para mantener al jugador dentro de los límites del mapa
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth); //ajuste para dejar espacio para la UI
        pos.y = Mathf.Clamp(pos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight); //ajuste para dejar espacio para la UI
        transform.position = pos;

    }

    private void OnCollisionStay2D(Collision2D collision){ //funcion para que el jugador no empuje a los enemigos, ni viceversa
        if (!collision.collider.CompareTag("Enemy")) // si no hay contacto sale
            return;

        // Vector desde el jugador hacia el enemigo
        Vector2 toEnemy = (collision.collider.transform.position - transform.position).normalized;

        // Proyección de la velocidad en la dirección del enemigo
        float towardEnemy = Vector2.Dot(_rb.linearVelocity, toEnemy);
        // Si la velocidad va hacia el enemigo, cancela solo esa componente
        if (towardEnemy > 0f)
            _rb.linearVelocity -= toEnemy * towardEnemy;
    }
}