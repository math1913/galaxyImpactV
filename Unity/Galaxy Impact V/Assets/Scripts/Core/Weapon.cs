using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// Control básico de disparo con recarga y dispersión opcional.
public class Weapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;         // Punto de salida
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ObjectPool bulletPool;

    [Header("Stats")]
    [SerializeField] private float fireRate = 10f;     // disparos/seg
    [SerializeField] private int magazineSize = 20;
    [SerializeField] private float reloadTime = 1.2f;
    [SerializeField, Range(0f, 8f)] private float spreadDeg = 2f;

    [Header("Ammo (Total)")]
    [Tooltip("Balas en reserva")]
    [SerializeField] private int totalAmmo = 90;
    public int TotalAmmo { get; private set; }   // reserva actual

    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }
    public UnityEvent<int> OnAmmoChanged; // evento (valor actual del cargador)
    public UnityEvent<int> OnTotalAmmoChanged; // evento (valor actual de la reserva)
    float cooldown;

    private void Awake()
    {
        CurrentAmmo = magazineSize;
        TotalAmmo = totalAmmo; // inicializa la reserva con el valor del inspector

        // Actualizar UI si hay listeners
        OnAmmoChanged?.Invoke(CurrentAmmo);
        OnTotalAmmoChanged?.Invoke(TotalAmmo);
    }

    private void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (Input.GetButton("Fire1")) TryFire();
        if (Input.GetKeyDown(KeyCode.R)) Reload();
    }

    public void TryFire()
    {
        if (IsReloading || cooldown > 0f) return;

        if (CurrentAmmo <= 0)
        {
            // SFX de vacío si quieres
            return;
        }

        Fire();
    }

    public void Reload()
    {
        // Si ya está recargando, o cargador lleno, o no hay reserva, no hace nada.
        if (IsReloading || CurrentAmmo == magazineSize || TotalAmmo <= 0) return;
        IsReloading = true;
        OnAmmoChanged?.Invoke(CurrentAmmo); // Actualizar UI del cargador (opcional)
        OnTotalAmmoChanged?.Invoke(TotalAmmo); // Actualizar UI de la reserva (opcional)
        StartCoroutine(ReloadRoutine());
    }
    public void AddAmmo(int amount)
    {
        TotalAmmo += amount;
        OnTotalAmmoChanged?.Invoke(TotalAmmo);
    }

    void Fire()
    {
        cooldown = 1f / Mathf.Max(0.01f, fireRate);
        CurrentAmmo--;

        // Instanciar / Pool
        GameObject go = bulletPool ? bulletPool.Get() : Instantiate(bulletPrefab);
        go.transform.position = muzzle.position;
        go.transform.rotation = muzzle.rotation * Quaternion.Euler(0, 0, Random.Range(-spreadDeg, spreadDeg));
        go.SetActive(true);

        if (go.TryGetComponent<Bullet>(out var b) && bulletPool)
            b.Init(bulletPool);

        // aualizar UI
        OnAmmoChanged?.Invoke(CurrentAmmo);
        // Nota: disparar no afecta la reserva, así que no tocamos TotalAmmo aquí.
    }

    IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(reloadTime);

        // Cuántas balas necesita el cargador
        int needed = magazineSize - CurrentAmmo;
        // Cuántas balas podemos tomar de la reserva
        int takeFromReserve = Mathf.Min(needed, TotalAmmo);

        // Rellenar cargador con lo tomado
        CurrentAmmo += takeFromReserve;
        TotalAmmo -= takeFromReserve;

        IsReloading = false;

        // Actualizar UI
        OnAmmoChanged?.Invoke(CurrentAmmo);
        OnTotalAmmoChanged?.Invoke(TotalAmmo);
    }
}
