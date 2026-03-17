using System;
using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamageable
{
    public event Action OnHealthUpdate;

    private Slider healthBar;
    private Entity_VFX entityVfx;
    private Entity entity;

    [Header("Health")]
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float currentHealth;
    [SerializeField] public bool isDead;
    [SerializeField] protected bool canTakeDamage = true;

    [Header("On Damage Knockback")]
    [SerializeField] private Vector2 knockbackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 heavyKnockbackPower = new Vector2(7f, 7f);
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float heavyKnockbackDuration = 0.5f;

    [Header("On Heavy Damage")]
    [SerializeField] private float heavyDamageThreshold = 0.3f; // 伤害占最大生命的比例，超过则视为 heavy

    protected virtual void Awake()
    {
        entityVfx = GetComponent<Entity_VFX>();
        entity = GetComponent<Entity>();
        healthBar = GetComponentInChildren<Slider>();
        
    }

    protected virtual void Start()
    {
        SetupHealth();
    }

    protected virtual void SetupHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        OnHealthUpdate?.Invoke();
    }
    
    public virtual bool TakeDamage(float damage, Transform damageDealer)
    {
        if (isDead || !canTakeDamage)
            return false;

        // 击退
        Vector2 knockback = CalculateKnockback(damage, damageDealer);
        float duration = CalculateDuration(damage);
        entity?.ReciveKnockback(knockback, duration);

        // 受伤特效
        entityVfx?.PlayOnDamageVfx();

        // 扣血 & 检查死亡
        ReduceHealth(damage);

        return true;
    }

    public void SetCanTakeDamage(bool value) => canTakeDamage = value;

    public void IncreaseHealth(float amount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
        OnHealthUpdate?.Invoke();
    }

    protected void ReduceHealth(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();
        OnHealthUpdate?.Invoke();

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        entity?.EntityDeath();
        // 原版这里还有 dropManager 掉落，你现在没有就不写
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.value = currentHealth / maxHealth;
    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;

        Vector2 knockback = IsHeavyDamage(damage) ? heavyKnockbackPower : knockbackPower;
        knockback.x *= direction;

        return knockback;
    }

    private float CalculateDuration(float damage)
        => IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;

    private bool IsHeavyDamage(float damage)
        => damage / maxHealth > heavyDamageThreshold;
}
