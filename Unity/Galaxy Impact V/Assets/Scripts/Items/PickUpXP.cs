using UnityEngine;

public class PickupXP : MonoBehaviour
{
    public int xpAmount = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.AddXP(xpAmount);
        }

        // Aquí puedes reproducir sonido, partículas, etc
        Destroy(gameObject);
    }
}
