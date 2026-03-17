using UnityEngine;

public class Enemy_Slime : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_SlimeDeadState slimeDeadState { get; set; }

    [Header("Slime specifics")]
    [SerializeField] private GameObject slimeToCreatePrefab;
    [SerializeField] private int amountOfSlimesToCreate = 2;

    [SerializeField] private bool hasRecoveryAnimation = true;

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        battleState = new Enemy_BattleState(this, stateMachine, "battle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        slimeDeadState = new Enemy_SlimeDeadState(this, stateMachine, "idle");

        anim.SetBool("hasStunRecovery", hasRecoveryAnimation);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    public override void EntityDeath()
    {
        stateMachine.ChangeState(slimeDeadState);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public void CreateSlimeOnDeath()
    {
        if (slimeToCreatePrefab == null)
            return;

        // 水平和垂直偏移（可以根据效果再微调）
        float separationX = 1.2f;
        float separationY = 0.25f;

        for (int i = 0; i < amountOfSlimesToCreate; i++)
        {
            // 让几个史莱姆以当前敌人的位置为中心，左右展开
            float offsetX = (i - (amountOfSlimesToCreate - 1) / 2f) * separationX;

            // 简单的奇偶交替高度差
            float offsetY = (i % 2 == 0) ? 0f : separationY;

            Vector3 spawnPos = transform.position + new Vector3(offsetX, offsetY, 0f);

            GameObject newSlime = Instantiate(slimeToCreatePrefab, spawnPos, Quaternion.identity);
            Enemy_Slime slimeScript = newSlime.GetComponent<Enemy_Slime>();

            // 你项目里没有 Entity_Stats，就保持注释
            // slimeScript.stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);

            slimeScript.ApplyRespawnVelocity();
            slimeScript.StartBattleStateCheck(player);
        }
    }


    public void ApplyRespawnVelocity()
    {
        Vector2 velocity = new Vector2(stunnedVelocity.x * Random.Range(-1f, 1f), stunnedVelocity.y * Random.Range(1f, 2f));
        SetVelocity(velocity.x, velocity.y);
    }

    public void StartBattleStateCheck(Transform player)
    {
        TryEnterBattleState(player);
        InvokeRepeating(nameof(ReEnterBattleState), 0, .3f);
    }

    private void ReEnterBattleState()
    {
        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
        {
            CancelInvoke(nameof(ReEnterBattleState));
            return;
        }

        stateMachine.ChangeState(battleState);
    }
}
