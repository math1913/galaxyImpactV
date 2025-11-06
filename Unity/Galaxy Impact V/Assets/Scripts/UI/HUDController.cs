using UnityEngine;
using TMPro;
using System.Diagnostics;

/// Controla la interfaz de usuario (HUD) del juego:
///  - Muestra la vida del jugador.
///  - Muestra la munición actual.
///  - Muestra la oleada actual.
public class HUDController : MonoBehaviour
{
    [Header("Referencias del jugador")]
    [Tooltip("Componente Health del jugador.")]
    [SerializeField] private Health playerHealth;

    [Tooltip("Componente Weapon del jugador.")]
    [SerializeField] private Weapon playerWeapon;

    [Header("Referencias del Wave Manager")]
    [Tooltip("Script WaveManager para mostrar la oleada actual.")]
    [SerializeField] private WaveManager waveManager;

    [SerializeField] private UnityEngine.UI.Slider healthBar;

    [Header("Elementos de UI")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text waveText;

    private void Start()
    {
        // Suscribirse a eventos del WaveManager
        if (waveManager)
            waveManager.OnWaveStarted.AddListener(UpdateWaveText);

        if (playerHealth)
            playerHealth.OnHealthChanged.AddListener(OnHealthChanged);

        if (playerWeapon)
        {
            playerWeapon.OnAmmoChanged.AddListener(UpdateAmmo);
            playerWeapon.OnTotalAmmoChanged.AddListener(UpdateAmmo);
            //Debug.Log("HUD → Suscrito al evento de arma correctamente");
            UpdateAmmo(playerWeapon.CurrentAmmo);
        }
        // Actualizar valores iniciales
        RefreshHealth();
    }

    /// Actualiza la vida en pantalla.
    private void RefreshHealth()
    {
        if (!playerHealth || !healthText) return;

        float ratio = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        healthText.text = $"HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";

        if (healthBar)
            healthBar.value = ratio;
    }

    /// Se ejecuta al comenzar una nueva oleada.
    private void UpdateWaveText(int wave)
    {
        if (!waveText) return;
        waveText.text = $"ROUND {wave}";
    }

    /// Se llama cuando el evento de Weapon cambia la munición
    private void UpdateAmmo(int _)
    {
        if (!ammoText) return;

        int current = playerWeapon.CurrentAmmo;
        int total = playerWeapon.TotalAmmo;

        ammoText.text = playerWeapon.IsReloading
            ? "Reloading..."
            : $"Ammo: {current} / {total}";
    }

    private void OnHealthChanged(int current, int max)
    {
        float ratio = (float)current / max;
        if (healthText)
            healthText.text = $"HP: {current}";
        if (healthBar)
            healthBar.value = ratio;
    }
}
