using System;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    // 原 OnDoingPhysicalDamage -> 通用伤害事件
    public event Action<float> OnDoingDamage;

    private Entity_SFX sfx;

    [Header("Damage")]
    public float damage = 10f;

    [Header("Target detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius = 1f;
    [SerializeField] private LayerMask whatIsTarget;

    private void Awake()
    {
        sfx = GetComponent<Entity_SFX>();
    }
    
    public void PerformAttack()
    {
        bool anyTargetReallyHit = false;

        foreach (var target in GetDetectedColliders())
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            // 由 IDamageable 告诉我们这一下是否真正造成伤害（没被格挡/无敌）
            bool thisTargetHit = damageable.TakeDamage(damage, transform);

            if (!thisTargetHit)
                continue;

            anyTargetReallyHit = true;

            // 原 OnDoingPhysicalDamage，只在真正命中时触发
            OnDoingDamage?.Invoke(damage);

            // 命中音效（原本就在 “if (targetGotHit)” 里）
            sfx?.PlayAttackHit();
        }

        // 一次攻击下来，没有任何目标“真正受伤” -> 播 Miss
        if (!anyTargetReallyHit)
            sfx?.PlayAttackMiss();
    }
    
    public void PerformAttackOnTarget(Transform target)
    {
        // 目标本身不存在 -> 直接视为 Miss
        if (target == null)
        {
            sfx?.PlayAttackMiss();
            return;
        }

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            sfx?.PlayAttackMiss();
            return;
        }

        bool targetGotHit = damageable.TakeDamage(damage, transform);

        if (targetGotHit)
        {
            OnDoingDamage?.Invoke(damage);
            sfx?.PlayAttackHit();
        }
        else
        {
            // 有碰撞，但被格挡/无敌 -> 视为 Miss
            sfx?.PlayAttackMiss();
        }
    }

    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(
            targetCheck.position,
            targetCheckRadius,
            whatIsTarget
        );
    }

    private void OnDrawGizmos()
    {
        if (targetCheck == null) return;
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
