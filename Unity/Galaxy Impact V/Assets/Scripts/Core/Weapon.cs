using UnityEngine;
using System.Collections;

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

    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }

    float cooldown;

    private void Awake() => CurrentAmmo = magazineSize;

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
        if (IsReloading || CurrentAmmo == magazineSize) return;
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        IsReloading = true;
        yield return new WaitForSeconds(reloadTime);
        CurrentAmmo = magazineSize;
        IsReloading = false;
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

        if (go.TryGetComponent<Bullet>(out var b) && bulletPool) b.Init(bulletPool);
    }
}
