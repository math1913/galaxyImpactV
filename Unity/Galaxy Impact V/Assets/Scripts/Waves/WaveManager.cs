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
    [Tooltip("Probabilidad relativa de que este pickup sea elegido.")]
    public float spawnChance = 0.3f;

    [Tooltip("Número máximo de instancias de este pickup por oleada (solo para spawn al inicio/fin de ronda).")]
    public int maxPerWave = 2;

    [Tooltip("Ronda mínima a partir de la cual este pickup puede aparecer.")]
    public int minWave = 1;
}

public class WaveManager : MonoBehaviour
{
    [Header("Configuración general")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Radio de spawn para enemigos (respecto a 0,0)")]
    [Tooltip("Distancia mínima desde el punto central (0,0) al generar enemigos.")]
    [SerializeField] private float enemySpawnRadiusMin = 8f;

    [Tooltip("Distancia máxima desde el punto central (0,0) al generar enemigos.")]
    [SerializeField] private float enemySpawnRadiusMax = 14f;

    [Header("Escalado de dificultad")]
    [SerializeField] private int enemiesFirstWave = 3;
    [SerializeField] private int enemyIncreasePerWave = 2;
    [SerializeField] private float speedMultiplierPerWave = 1.1f;

    [Header("Referencias")]
    [SerializeField] private Transform player;

    [Header("Eventos")]
    public UnityEvent<int> OnWaveStarted;
    public UnityEvent<int> OnWaveCompleted;

    [Header("Pickups - Configuración general")]
    [SerializeField] private LayerMask pickupObstructMask;
    [SerializeField] private bool includeTriggerObstacles = true;   // ahora mismo no se usa, pero lo dejamos por si amplías
    [SerializeField, Min(0f)] private float spawnPadding = 0.05f;
    [Tooltip("Lista de pickups con probabilidad, máximo por oleada y ronda mínima.")]
    [SerializeField] private List<PickupEntry> pickups = new List<PickupEntry>();

    [Header("Pickups por oleada (inicio/fin)")]
    [Tooltip("Si true, spawnea pickups al INICIAR cada oleada.")]
    [SerializeField] private bool spawnPickupsOnWaveStart = false;

    [Tooltip("Si true, spawnea pickups al TERMINAR cada oleada.")]
    [SerializeField] private bool spawnPickupsOnWaveEnd = true;

    [Tooltip("Distancia mínima desde el punto central (0,0) al generar pickups de oleada.")]
    [SerializeField] private float pickupSpawnRadiusMin = 4f;

    [Tooltip("Distancia máxima desde el punto central (0,0) al generar pickups de oleada.")]
    [SerializeField] private float pickupSpawnRadiusMax = 10f;

    [Header("Drops por muertes de enemigos")]
    [Tooltip("Mínimo de muertes antes de que pueda aparecer un pickup.")]
    [SerializeField, Min(1)] private int minKillsForPickup = 3;

    [Tooltip("Máximo de muertes antes de que pueda aparecer un pickup.")]
    [SerializeField, Min(1)] private int maxKillsForPickup = 10;

    // Estado interno
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool spawning = false;
    private Coroutine waveRoutine;

    // Contadores de sistema por muertes
    private int killsSinceLastPickup = 0;
    private int nextKillThreshold = 0;

    private void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (minKillsForPickup < 1) minKillsForPickup = 1;
        if (maxKillsForPickup < minKillsForPickup) maxKillsForPickup = minKillsForPickup;

        ResetPickupKillThreshold();

        waveRoutine = StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        for (int i = 1; i <= totalWaves; i++)
        {
            currentWave = i;
            OnWaveStarted?.Invoke(currentWave);

            // Reset de sistema por muertes por ronda
            killsSinceLastPickup = 0;
            ResetPickupKillThreshold();

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
            float distance = Random.Range(enemySpawnRadiusMin, enemySpawnRadiusMax);
            Vector3 spawnPos = Vector3.zero + new Vector3(spawnDir.x, spawnDir.y, 0) * distance;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            var setter = enemy.GetComponent<AIDestinationSetter>();
            if (setter != null)
                setter.target = player; // los enemigos siguen al jugador

            if (enemy.TryGetComponent<EnemyController>(out var ec))
            {
                ec.SetDifficultyMultiplier(Mathf.Pow(speedMultiplierPerWave, currentWave - 1));

                Transform enemyTransform = enemy.transform;

                // Cuando el enemigo muere:
                ec.OnDeath.AddListener(() =>
                {
                    HandleEnemyDeath(enemyTransform.position);
                });
            }

            yield return new WaitForSeconds(0.3f);
        }

        spawning = false;
    }

    // ============================
    //   SISTEMA 2: PICKUPS POR MUERTES
    // ============================
    private void HandleEnemyDeath(Vector3 deathPosition)
    {
        enemiesAlive--;

        if (pickups == null || pickups.Count == 0)
            return;

        killsSinceLastPickup++;

        if (killsSinceLastPickup >= nextKillThreshold)
        {
            killsSinceLastPickup = 0;
            ResetPickupKillThreshold();
            SpawnPickupAtPosition(deathPosition);
        }
    }

    private void ResetPickupKillThreshold()
    {
        if (minKillsForPickup < 1)
            minKillsForPickup = 1;
        if (maxKillsForPickup < minKillsForPickup)
            maxKillsForPickup = minKillsForPickup;

        // int: Random.Range(min, maxExclusive)
        nextKillThreshold = Random.Range(minKillsForPickup, maxKillsForPickup + 1);
        // Debug.Log($"Siguiente pickup tras {nextKillThreshold} muertes.");
    }

    private void SpawnPickupAtPosition(Vector3 deathPosition)
    {
        var chosen = ChoosePickupWeighted();
        if (chosen == null || chosen.prefab == null)
            return;

        float radius = 0.4f + spawnPadding;
        Vector3 goodPos = GetValidSpawn2D(deathPosition, radius);

        Instantiate(chosen.prefab, goodPos, Quaternion.identity);
    }

    /// <summary>
    /// Elige un pickup usando spawnChance como peso, filtrando por minWave y prefab válido.
    /// </summary>
    private PickupEntry ChoosePickupWeighted()
    {
        float totalChance = 0f;

        foreach (var e in pickups)
        {
            if (e == null || e.prefab == null) continue;
            if (e.minWave > currentWave) continue;
            if (e.spawnChance <= 0f) continue;

            totalChance += e.spawnChance;
        }

        if (totalChance <= 0f)
            return null;

        float roll = Random.value * totalChance;
        float accum = 0f;

        foreach (var e in pickups)
        {
            if (e == null || e.prefab == null) continue;
            if (e.minWave > currentWave) continue;
            if (e.spawnChance <= 0f) continue;

            accum += e.spawnChance;
            if (roll <= accum)
                return e;
        }

        return null;
    }

    // ============================
    //   SISTEMA 1: PICKUPS POR OLEADA
    // ============================
    private void SpawnPickupsForWave()
    {
        foreach (var entry in pickups)
        {
            if (entry?.prefab == null) continue;
            if (entry.minWave > currentWave) continue;         // 🔥 solo si la ronda >= minWave
            if (Random.value > entry.spawnChance) continue;
            if (entry.maxPerWave <= 0) continue;

            int count = Random.Range(1, entry.maxPerWave + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float dist = Random.Range(pickupSpawnRadiusMin, pickupSpawnRadiusMax);
                Vector3 pos = Vector3.zero + new Vector3(dir.x, dir.y, 0) * dist;

                Vector3 goodPos = GetValidSpawn2D(pos, 0.4f);
                Instantiate(entry.prefab, goodPos, Quaternion.identity);
            }
        }
    }

    // ============================
    //   UTILIDADES / GESTIÓN
    // ============================
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

    public void StopWaves()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }
    }

#if UNITY_EDITOR
    // Gizmos de depuración visual en el Editor
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Vector3.zero;

        // Enemigos (rojo)
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(center, enemySpawnRadiusMin);
        Gizmos.DrawWireSphere(center, enemySpawnRadiusMax);

        // Pickups (verde)
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(center, pickupSpawnRadiusMin);
        Gizmos.DrawWireSphere(center, pickupSpawnRadiusMax);

        // Centro (blanco)
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(center, 0.2f);
    }
#endif
}
