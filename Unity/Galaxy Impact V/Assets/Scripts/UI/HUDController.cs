using UnityEngine;
using TMPro;
using System.Diagnostics;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Referencias del jugador")]
    [Tooltip("Componente Health del jugador.")]
    [SerializeField] private Health playerHealth;

    [Tooltip("Componente Weapon del jugador.")]
    [SerializeField] private Weapon playerWeapon;

    [Tooltip("Componente Shield del jugador.")]
    [SerializeField] private Shield playerShield;

    [Header("Referencias del Wave Manager")]
    [Tooltip("Script WaveManager para mostrar la oleada actual.")]
    [SerializeField] private WaveManager waveManager;

    [Header("UI de Vida")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    [Header("UI de Escudo")]
    [SerializeField] private UnityEngine.UI.Slider shieldBar;
    [SerializeField] private TMP_Text shieldText;

    [Header("Elementos de UI")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text waveText;

    [Header("Damage Flash")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color healthFlashColor = Color.red;
    [SerializeField] private Color shieldFlashColor = Color.cyan;

    private Color healthOriginalColor;
    private Color shieldOriginalColor;
    private int lastShieldValue;

    private void Start()
    {
        if (waveManager)
            waveManager.OnWaveStarted.AddListener(UpdateWaveText);

        if (playerHealth)
        {
            playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (playerShield)
        {
            playerShield.OnShieldChanged.AddListener(OnShieldChanged);
            OnShieldChanged(playerShield.CurrentShield, playerShield.MaxShield);
            lastShieldValue = playerShield.CurrentShield;

        }

        if (playerWeapon)
        {
            playerWeapon.OnAmmoChanged.AddListener(UpdateAmmo);
            playerWeapon.OnTotalAmmoChanged.AddListener(UpdateAmmo);
            UpdateAmmo(playerWeapon.CurrentAmmo);
        }
        if (healthBar)
            healthOriginalColor = healthBar.fillRect.GetComponent<UnityEngine.UI.Image>().color;

        if (shieldBar)
            shieldOriginalColor = shieldBar.fillRect.GetComponent<UnityEngine.UI.Image>().color;

    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthText)
            healthText.text = $"HP: {current}/{max}";

        if (healthBar)
            healthBar.value = (float)current / max;

        // Flash cuando baja la salud
        if (current < max)
            StartCoroutine(FlashBar(healthBar, healthFlashColor, healthOriginalColor));
    }
    private void OnShieldChanged(int current, int max)
    {
        if (shieldText)
            shieldText.text = $"SHD: {current}/{max}";

        if (shieldBar)
            shieldBar.value = (float)current / max;

        // solo si el escudo bajo
        if (current < lastShieldValue)
            StartCoroutine(FlashBar(shieldBar, shieldFlashColor, shieldOriginalColor));

        // actualizar el valor anterior
        lastShieldValue = current;
    }

    private void UpdateAmmo(int _)
    {
        if (!ammoText) return;

        int current = playerWeapon.CurrentAmmo;
        int total = playerWeapon.TotalAmmo;

        ammoText.text = playerWeapon.IsReloading
            ? "Reloading..."
            : $"Ammo: {current} / {total}";
    }


    private void UpdateWaveText(int wave)
    {
        if (!waveText) return;
        waveText.text = $"ROUND {wave}";
    }

    private IEnumerator FlashBar(UnityEngine.UI.Slider bar, Color flashColor, Color originalColor)
    {
        var image = bar.fillRect.GetComponent<UnityEngine.UI.Image>();
        image.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        image.color = originalColor;
    }

}
