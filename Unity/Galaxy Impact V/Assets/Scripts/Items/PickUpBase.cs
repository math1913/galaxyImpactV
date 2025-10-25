using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] protected float floatAmplitude = 0.2f;
    [SerializeField] protected float floatSpeed = 2f;
    [SerializeField] protected AudioClip pickupSound;

    private Vector3 startPos;

    protected virtual void Start() => startPos = transform.position;

    protected virtual void Update()
    {
        // animación flotante
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.Rotate(Vector3.forward * 45f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OnPickup(other);
        if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        Destroy(gameObject);
    }

    protected abstract void OnPickup(Collider2D player);
}
