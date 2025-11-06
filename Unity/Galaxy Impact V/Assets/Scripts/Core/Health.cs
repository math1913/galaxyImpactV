using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// Componente genérico de vida, con eventos para UI o efectos.
public class Health : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath = false;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // (current, max)
    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamage; // daño recibido

    private bool _isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
    public void SetMaxHealth(int newMaxHealth, bool resetCurrent = true)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        if (resetCurrent)
            CurrentHealth = maxHealth;

        _isDead = false;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth); //para que no tenga vida negativa ni mas del max
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        //Debug.Log($"Player HP: {CurrentHealth}/{maxHealth}");
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || amount <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnDamage?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        //Debug.Log($"Player HP: {CurrentHealth}/{maxHealth}");
        if (CurrentHealth == 0)
            Die();
    }

private void Die()
{
    if (_isDead) return;

    _isDead = true;
    OnDeath?.Invoke();

    if (destroyOnDeath)
        StartCoroutine(DestroyAfterFrame());
}

private IEnumerator DestroyAfterFrame()
{
    yield return null; // Espera 1 frame (permite que el HUD procese el evento)
    Destroy(gameObject);
}

    public void ResetHealth()
    {
        _isDead = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
