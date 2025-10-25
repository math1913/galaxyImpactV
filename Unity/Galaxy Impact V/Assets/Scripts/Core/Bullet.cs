using UnityEngine;

/// Proyectil simple que avanza en +X local, daña y tiene TTL.
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask hitMask; // Enemigos / Obstáculos

    private float t;
    private ObjectPool pool;

    public void Init(ObjectPool p) => pool = p;

    private void OnEnable() { t = 0f; }

    private void Update()
    {
        transform.position += transform.right * (speed * Time.deltaTime);
        t += Time.deltaTime;
        if (t >= lifeTime) Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Filtra por máscara
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        if (other.TryGetComponent<Health>(out var hp))
            hp.TakeDamage(damage);

        Despawn();
    }

    private void Despawn()
    {
        if (pool) pool.Return(gameObject);
        else gameObject.SetActive(false);
    }
}
