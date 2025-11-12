using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pathfinding;
[System.Serializable]
public class PickupEntry
{
    public GameObject prefab;
    [Range(0f, 1f)]
    [Tooltip("Probabilidad de que este pickup aparezca en una oleada.")]
    public float spawnChance = 0.3f;
    [Tooltip("Número máximo de instancias de este pickup por oleada.")]
    public int maxPerWave = 2;
}

public class WaveManager : MonoBehaviour
{
    [Header("Configuración general")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Tooltip("Distancia mínima desde el jugador al generar enemigos/pickups.")]
    [SerializeField] private float spawnRadius = 8f;

    [Tooltip("Distancia máxima desde el jugador al generar enemigos/pickups.")]
    [SerializeField] private float spawnRadiusMax = 14f;

    [Header("Escalado de dificultad")]
    [SerializeField] private int enemiesFirstWave = 3;
    [SerializeField] private int enemyIncreasePerWave = 2;
    [SerializeField] private float speedMultiplierPerWave = 1.1f;

    [Header("Referencias")]
    [SerializeField] private Transform player;

    [Header("Eventos")]
    public UnityEvent<int> OnWaveStarted;
    public UnityEvent<int> OnWaveCompleted;

    [Header("Pickups")]
    [SerializeField] private LayerMask pickupObstructMask;         // layers que bloquean
    [SerializeField] private bool includeTriggerObstacles = true;  // contar triggers como obstáculo
    [SerializeField, Min(0f)] private float spawnPadding = 0.05f;  // margen extra alrededor del collider
    [Tooltip("Lista de tipos de pickup con su probabilidad y máximo por oleada.")]
    [SerializeField] private List<PickupEntry> pickups = new List<PickupEntry>();

    [Tooltip("Si true, spawnea pickups al INICIAR cada oleada.")]
    [SerializeField] private bool spawnPickupsOnWaveStart = false;

    [Tooltip("Si true, spawnea pickups al TERMINAR cada oleada.")]
    [SerializeField] private bool spawnPickupsOnWaveEnd = true;

    // Estado interno
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool spawning = false;
    private Coroutine waveRoutine;

    private void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        waveRoutine = StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        for (int i = 1; i <= totalWaves; i++)
        {
            currentWave = i;
            OnWaveStarted?.Invoke(currentWave);

            if (spawnPickupsOnWaveStart)
                SpawnPickupsForWave();

            int enemiesThisWave = enemiesFirstWave + (i - 1) * enemyIncreasePerWave;

            yield return StartCoroutine(SpawnWave(enemiesThisWave));
            yield return new WaitUntil(() => enemiesAlive <= 0);

            OnWaveCompleted?.Invoke(currentWave);

            if (spawnPickupsOnWaveEnd)
                SpawnPickupsForWave();

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        Debug.Log("Todas las oleadas completadas.");
    }

    private IEnumerator SpawnWave(int count)
    {
        spawning = true;
        enemiesAlive = count;

        Debug.Log($"--- Oleada {currentWave} iniciada ({count} enemigos) ---");

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(spawnRadius, spawnRadiusMax);
            Vector3 spawnPos = player.position + new Vector3(spawnDir.x, spawnDir.y, 0) * distance;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            var setter = enemy.GetComponent<AIDestinationSetter>();
            setter.target = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (enemy.TryGetComponent<EnemyController>(out var ec))
            {
                ec.SetDifficultyMultiplier(Mathf.Pow(speedMultiplierPerWave, currentWave - 1)); // no existe mas el metodo porque por ahora no quiero que aumente la velocidad de los enemigos cada ronda
                ec.OnDeath.AddListener(() => enemiesAlive--);
            }

            yield return new WaitForSeconds(0.3f);
        }

        spawning = false;
    }

    //lógica de spawn de pickups
    private void SpawnPickupsForWave()
    {
        if (player == null)
        {
            Debug.LogWarning("WaveManager: player no asignado; no se pueden spawnear pickups.");
            return;
        }

        foreach (var entry in pickups)
        {
            if (entry?.prefab == null) continue;

            // Probabilidad por tipo
            if (Random.value > entry.spawnChance) continue;

            // Entre 1 y maxPerWave (si maxPerWave <= 0, se salta)
            if (entry.maxPerWave <= 0) continue;
            int count = Random.Range(1, entry.maxPerWave + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float dist = Random.Range(spawnRadius, spawnRadiusMax);
                Vector3 pos = player.position + new Vector3(dir.x, dir.y, 0) * dist;

                var goodPos = GetValidSpawn2D(pos, 0.4f);
                Instantiate(entry.prefab, goodPos, Quaternion.identity);

            }
        }
    }
    private Vector3 GetValidSpawn2D(Vector3 candidate, float radius, int tries = 10000)
    {
        while (tries-- > 0)
        {
            if (!Physics2D.OverlapCircle(candidate, radius, pickupObstructMask))
                return candidate;

            // si está bloqueado → muevo un poco al costado
            candidate += (Vector3)(Random.insideUnitCircle * radius);
        }

        return candidate; // fallback
    }
    // buffer reusado para evitar GC
    public void StopWaves()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }
}
