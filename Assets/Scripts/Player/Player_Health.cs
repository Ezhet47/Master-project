using UnityEngine;

public class Player_Health : Entity_Health
{
    private Player player;
    [SerializeField] private float counterHealAmount = 5f;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }
    
    public void HealOnCounter()
    {
        if (isDead)
            return;

        if (counterHealAmount <= 0f)
            return;

        IncreaseHealth(counterHealAmount);
    }

    protected override void Die()
    {
        base.Die();
        
        player.ui.OpenDeathScreenUI();
    }
}
