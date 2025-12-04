using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    [Header("Referencias")]
    public AuthService authService;   // Asignar en el inspector

    [Header("Stats de esta partida")]
    public int killsThisRun = 0;
    public int xpThisRun = 0;

    [Header("Config XP por ronda")]
    [Tooltip("XP base que se da al completar cada ronda.")]
    public int xpPerWave = 20;
    [Tooltip("XP extra cada X rondas (por ejemplo cada 5 oleadas).")]
    public int xpBonusEveryXWaves = 5;
    public int xpBonusAmount = 50;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persiste entre escenas
    }

    // ======= KILLS & XP =======
    public void RegisterKill(int xpGained)
    {
        killsThisRun++;
        xpThisRun += xpGained;
        // Debug.Log($"Kill registrada. XP +{xpGained}. Totales: Kills={killsThisRun}, XP={xpThisRun}");
    }

    public void AddXP(int amount)
    {
        xpThisRun += amount;
        // Debug.Log($"XP +{amount}. XP total run = {xpThisRun}");
    }

    // ======= XP por ronda =======
    public void OnWaveCompleted(int waveNumber)
    {
        // XP fija por oleada
        xpThisRun += xpPerWave;

        // Bonus cada X rondas
        if (xpBonusEveryXWaves > 0 && waveNumber % xpBonusEveryXWaves == 0)
        {
            xpThisRun += xpBonusAmount;
        }

        // Debug.Log($"Wave {waveNumber} completada. XP total run = {xpThisRun}");
    }

    // ======= Al morir el player / terminar partida =======
    public async void EndRunAndSendToApi()
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogWarning("No hay userId en PlayerPrefs. No se puede enviar stats.");
            SceneManager.LoadScene("Login");
            return;
        }

        Debug.Log($"Enviando stats a API: kills={killsThisRun}, xpThisRun={xpThisRun}");

        var updatedUser = await authService.UpdateStats(userId, killsThisRun, xpThisRun);

        if (updatedUser == null)
        {
            Debug.LogError("Error al actualizar stats en la API");
        }

        // Reseteamos stats de la partida actual
        killsThisRun = 0;
        xpThisRun = 0;
    }
}
