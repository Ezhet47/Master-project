using UnityEngine;

public class Enemy_Health : Entity_Health
{
    private Enemy enemy;

    protected override void Start()
    {
        base.Start();

        enemy = GetComponent<Enemy>();
    }

    public override bool TakeDamage(float damage, Transform damageDealer)
    {
        Debug.Log($"[BOSS HP] Try TakeDamage {damage}, isDead={isDead}, canTakeDamage={canTakeDamage}");

        if (isDead || !canTakeDamage)
        {
            Debug.Log("[BOSS HP] 返回 false，不结算这次伤害");
            return false;
        }
        
        if(canTakeDamage == false) 
            return false;

        bool wasHit = base.TakeDamage(damage, damageDealer);

        if (wasHit == false)
            return false;

        if(damageDealer.GetComponent<Player>() != null)
            enemy.TryEnterBattleState(damageDealer);

        return true;    
    }
}