using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controla el sistema de oleadas de enemigos:
///  - Spawnea enemigos en posiciones aleatorias alrededor del jugador.
///  - Espera a que todos mueran antes de iniciar la siguiente oleada.
///  - Incrementa dificultad progresivamente.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Configuración general")]
    [Tooltip("Prefab del enemigo que se instanciará en cada oleada.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Número de oleadas totales que se generarán.")]
    [SerializeField] private int totalWaves = 5;

    [Tooltip("Tiempo entre oleadas (segundos).")]
    [SerializeField] private float timeBetweenWaves = 5f;

    [Tooltip("Distancia mínima desde el jugador al generar enemigos.")]
    [SerializeField] private float spawnRadius = 8f;

    [Tooltip("Distancia máxima desde el jugador al generar enemigos.")]
    [SerializeField] private float spawnRadiusMax = 14f;

    [Header("Escalado de dificultad")]
    [Tooltip("Número inicial de enemigos en la primera oleada.")]
    [SerializeField] private int enemiesFirstWave = 3;

    [Tooltip("Incremento de enemigos por oleada.")]
    [SerializeField] private int enemyIncreasePerWave = 2;

    [Tooltip("Multiplicador de velocidad por oleada (1 = sin cambio).")]
    [SerializeField] private float speedMultiplierPerWave = 1.1f;

    [Header("Referencias")]
    [Tooltip("Referencia al jugador, para saber desde dónde calcular el spawn.")]
    [SerializeField] private Transform player;

    // Eventos opcionales (para actualizar el HUD, sonidos, etc.)
    [Header("Eventos")]
    public UnityEvent<int> OnWaveStarted;      // Pasa el número de oleada
    public UnityEvent<int> OnWaveCompleted;    // Pasa el número de oleada

    // Estado interno
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool spawning = false;

    // Control de la corrutina principal
    private Coroutine waveRoutine;

    private void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Inicia la rutina de oleadas
        waveRoutine = StartCoroutine(WaveLoop());
        
    }

    /// <summary>
    /// Bucle principal de oleadas.
    /// </summary>
    private IEnumerator WaveLoop()
    {
        // Recorre todas las oleadas hasta alcanzar el total configurado
        for (int i = 1; i <= totalWaves; i++)
        {
            currentWave = i;

            // Notifica inicio de oleada (HUD puede mostrar “Wave X”)
            OnWaveStarted?.Invoke(currentWave);

            // Calcula cuántos enemigos debe tener esta oleada
            int enemiesThisWave = enemiesFirstWave + (i - 1) * enemyIncreasePerWave;

            // Spawnea enemigos
            yield return StartCoroutine(SpawnWave(enemiesThisWave));

            // Espera hasta que todos los enemigos mueran
            yield return new WaitUntil(() => enemiesAlive <= 0);

            // Notifica fin de oleada
            OnWaveCompleted?.Invoke(currentWave);

            // Espera unos segundos antes de la siguiente oleada
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        Debug.Log("✅ Todas las oleadas completadas.");
    }

    /// <summary>
    /// Genera una oleada de 'count' enemigos.
    /// </summary>
    private IEnumerator SpawnWave(int count)
    {
        spawning = true;
        enemiesAlive = count;

        Debug.Log($"--- Oleada {currentWave} iniciada ({count} enemigos) ---");

        for (int i = 0; i < count; i++)
        {
            // Calcula posición aleatoria en un anillo alrededor del jugador
            Vector2 spawnDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(spawnRadius, spawnRadiusMax);
            Vector3 spawnPos = player.position + new Vector3(spawnDir.x, spawnDir.y, 0) * distance;

            // Instancia el enemigo
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Escala su dificultad si tiene EnemyController
            if (enemy.TryGetComponent<EnemyController>(out var ec))
            {
                ec.SetDifficultyMultiplier(Mathf.Pow(speedMultiplierPerWave, currentWave - 1));
                ec.OnDeath.AddListener(() => enemiesAlive--);
            }

            // Espera pequeña pausa entre spawns (mejor ritmo)
            yield return new WaitForSeconds(0.3f);
        }

        spawning = false;
    }

    /// <summary>
    /// Para corrutina si el juego se reinicia o pausa.
    /// </summary>
    public void StopWaves()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }
}
