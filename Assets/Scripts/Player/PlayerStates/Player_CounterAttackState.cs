public class Player_CounterAttackState : PlayerState
{
    private Player_Combat combat;
    private Player_Health playerHealth;
    private bool counteredSombody;

    public Player_CounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
        playerHealth = player.GetComponent<Player_Health>();
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = combat.GetCounterRecoveryDuration();
        counteredSombody = combat.CounterAttackPerformed();
        
        if (counteredSombody && playerHealth != null)
            playerHealth.HealOnCounter();

        anim.SetBool("counterAttackPerformed", counteredSombody);
    }

    public override void Update()
    {
        base.Update();
        player.SetVelocity(0, rb.linearVelocity.y);


        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);

        if (stateTimer < 0 && counteredSombody == false)
            stateMachine.ChangeState(player.idleState);
    }
}
