using UnityEngine;

public class Enemy_ReaperSpellCastState : EnemyState
{

    private Enemy_Reaper enemyReaper;
    public Enemy_ReaperSpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyReaper = enemy as Enemy_Reaper;
    }

    public override void Enter()
    {
        base.Enter();

        enemyReaper.SetVelocity(0, 0);
        enemyReaper.SetSpellCastPreformed(false);
        enemyReaper.SetSpellCastOnCooldown();
    }

    public override void Update()
    {
        base.Update();
        
        // 开始施法：不可被攻击
        enemyReaper.MakeUntargetable(false);

        if (enemyReaper.spellCastPreformed)
            anim.SetBool("spellCast_Performed", true);

        if (triggerCalled)
        {
            if (enemyReaper.ShouldTeleport())
                stateMachine.ChangeState(enemyReaper.reaperTeleportState);
            else
                stateMachine.ChangeState(enemyReaper.reaperBattleState);
        }

    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("spellCast_Performed", false);
        
        // 结束施法：恢复可被攻击
        enemyReaper.MakeUntargetable(true);
    }
}
