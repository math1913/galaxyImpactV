using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerDeathHandler : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.OnDeath.AddListener(OnPlayerDeath);
    }

    private void OnPlayerDeath()
    {
        
        SceneManager.LoadScene("GameOver");
        Debug.Log("El jugador ha muerto. Enviando stats a la API...");

        if (GameStatsManager.Instance != null)
            GameStatsManager.Instance.EndRunAndSendToApi();
    }
}
