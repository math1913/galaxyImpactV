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

    [Tooltip("Máximo número de instancias de este pickup al inicio/fin de ronda.")]
    public int maxPerWave = 2;
}

public class WaveManager : MonoBehaviour
{
    [Header("Configuración general")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int totalWaves = 5;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Radio de spawn para enemigos (respecto a 0,0)")]
    [SerializeField] private float enemySpawnRadiusMin = 8f;
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

    [Header("Pickups - Propiedades generales")]
    [SerializeField] private LayerMask pickupObstructMask;
    [SerializeField, Min(0f)] private float spawnPadding = 0.05f;

    [Tooltip("Lista de pickups con sus probabilidades.")]
    [SerializeField] private List<PickupEntry> pickups = new List<PickupEntry>();

    [Header("Pickups por oleada")]
    [SerializeField] private bool spawnPickupsOnWaveStart = false;
    [SerializeField] private bool spawnPickupsOnWaveEnd = true;

    [Tooltip("Radio mínimo de spawn alrededor del 0,0 (solo para oleadas).")]
    [SerializeField] private float pickupSpawnRadiusMin = 4f;

    [Tooltip("Radio máximo de spawn alrededor del 0,0 (solo para oleadas).")]
    [SerializeField] private float pickupSpawnRadiusMax = 10f;

    [Header("Drops por muertes de enemigos")]
    [Tooltip("Mínimo de muertes antes de generar un pickup.")]
    [SerializeField] private int minKillsForPickup = 3;

    [Tooltip("Máximo de muertes antes de generar un pickup.")]
    [SerializeField] private int maxKillsForPickup = 10;

    // Estado interno
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private Coroutine waveRoutine;

    private int killsSinceLastPickup = 0;
    private int nextKillThreshold = 0;

    private void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (maxKillsForPickup < minKillsForPickup)
            maxKillsForPickup = minKillsForPickup;

        ResetPickupKillThreshold();
        waveRoutine = StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        for (int i = 1; i <= totalWaves; i++)
        {
            currentWave = i;
            OnWaveStarted?.Invoke(currentWave);

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
    }

    private IEnumerator SpawnWave(int count)
    {
        enemiesAlive = count;

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(enemySpawnRadiusMin, enemySpawnRadiusMax);
            Vector3 pos = new Vector3(dir.x, dir.y, 0) * dist;

            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

            if (enemy.TryGetComponent<AIDestinationSetter>(out var setter))
                setter.target = player;

            if (enemy.TryGetComponent<EnemyController>(out var ec))
            {
                ec.SetDifficultyMultiplier(Mathf.Pow(speedMultiplierPerWave, currentWave - 1));
                Transform t = enemy.transform;

                ec.OnDeath.AddListener(() =>
                {
                    HandleEnemyDeath(t.position);
                });
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    // ---------------------------
    //  SISTEMA 2: Pickups por MUERTES
    // ---------------------------
    private void HandleEnemyDeath(Vector3 pos)
    {
        enemiesAlive--;
        killsSinceLastPickup++;

        if (killsSinceLastPickup >= nextKillThreshold)
        {
            killsSinceLastPickup = 0;
            ResetPickupKillThreshold();
            SpawnPickupAtPosition(pos);
        }
    }

    private void ResetPickupKillThreshold()
    {
        nextKillThreshold = Random.Range(minKillsForPickup, maxKillsForPickup + 1);
    }

    private void SpawnPickupAtPosition(Vector3 pos)
    {
        PickupEntry entry = ChoosePickupWeighted();
        if (entry == null) return;

        float radius = 0.4f + spawnPadding;
        Vector3 safePos = PickupBase.GetValidSpawn(pos, radius, pickupObstructMask);

        Instantiate(entry.prefab, safePos, Quaternion.identity);
    }

    // Selección por probabilidad (spawnChance)
    private PickupEntry ChoosePickupWeighted()
    {
        float total = 0f;
        foreach (var e in pickups)
            total += e.spawnChance;

        float roll = Random.value * total;
        float accum = 0f;

        foreach (var e in pickups)
        {
            accum += e.spawnChance;
            if (roll <= accum)
                return e;
        }
        return null;
    }

    // ---------------------------
    //  SISTEMA 1: Pickups por OLEADA
    // ---------------------------
    private void SpawnPickupsForWave()
    {
        foreach (var entry in pickups)
        {
            if (entry.prefab == null) continue;

            if (Random.value > entry.spawnChance) continue;
            if (entry.maxPerWave <= 0) continue;

            int count = Random.Range(1, entry.maxPerWave + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float dist = Random.Range(pickupSpawnRadiusMin, pickupSpawnRadiusMax);
                Vector3 spawnPos = new Vector3(dir.x, dir.y, 0) * dist;

                Vector3 goodPos = PickupBase.GetValidSpawn(spawnPos, 0.4f, pickupObstructMask);
                Instantiate(entry.prefab, goodPos, Quaternion.identity);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Vector3.zero;

        Gizmos.color = new Color(1,0.2f,0.2f,0.4f);
        Gizmos.DrawWireSphere(center, enemySpawnRadiusMin);
        Gizmos.DrawWireSphere(center, enemySpawnRadiusMax);

        Gizmos.color = new Color(0.2f,1,0.2f,0.4f);
        Gizmos.DrawWireSphere(center, pickupSpawnRadiusMin);
        Gizmos.DrawWireSphere(center, pickupSpawnRadiusMax);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(center, 0.2f);
    }
#endif
}
