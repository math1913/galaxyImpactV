using UnityEngine;
using TMPro;

/// <summary>
/// Controla la interfaz de usuario (HUD) del juego:
///  - Muestra la vida del jugador.
///  - Muestra la munición actual.
///  - Muestra la oleada actual.
/// </summary>
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

    [Header("Elementos de UI")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text waveText;

    private void Start()
    {
        // Suscribirse a eventos del WaveManager
        if (waveManager)
        {
            waveManager.OnWaveStarted.AddListener(UpdateWaveText);
        }

        // Actualizar valores iniciales
        RefreshHealth();
        RefreshAmmo();
    }

    private void Update()
    {
        // Actualiza continuamente vida y munición
        RefreshHealth();
        RefreshAmmo();
    }

    /// <summary>
    /// Actualiza la vida en pantalla.
    /// </summary>
    private void RefreshHealth()
    {
        if (!playerHealth || !healthText) return;
        healthText.text = $"HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
    }

    /// <summary>
    /// Actualiza la munición en pantalla.
    /// </summary>
    private void RefreshAmmo()
    {
        if (!playerWeapon || !ammoText) return;

        if (playerWeapon.IsReloading)
            ammoText.text = "Reloading...";
        else
            ammoText.text = $"Ammo: {playerWeapon.CurrentAmmo}";
    }

    /// <summary>
    /// Se ejecuta al comenzar una nueva oleada.
    /// </summary>
    private void UpdateWaveText(int wave)
    {
        if (!waveText) return;
        waveText.text = $"Wave {wave}";
    }
}
