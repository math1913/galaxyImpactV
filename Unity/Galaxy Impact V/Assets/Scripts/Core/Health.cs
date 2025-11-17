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

    private Shield shield; // referencia si el jugador tiene escudo

    private void Awake()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        shield = GetComponent<Shield>();
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
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || amount <= 0) return;

        int remainingDamage = amount;

        //Primero recibe el daño el escudo si existe
        if (shield != null && shield.CurrentShield > 0)
        {
            int absorbed = shield.AbsorbDamage(remainingDamage);
            remainingDamage -= absorbed; // lo que queda pasa a la vida

            if (remainingDamage <= 0)
            {
                OnDamage?.Invoke(amount); // todo el daño fue absorbido
                return; // no pasamos a la vida
            }
        }
        
        //Si todavia queda daño por recibir lo recibe la vida
        CurrentHealth = Mathf.Max(0, CurrentHealth - remainingDamage);

        OnDamage?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

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
        yield return null;
        Destroy(gameObject);
    }

    public void ResetHealth()
    {
        _isDead = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
