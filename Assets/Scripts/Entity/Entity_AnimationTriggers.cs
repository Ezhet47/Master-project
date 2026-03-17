using UnityEngine;

public class Entity_AnimationTriggers : MonoBehaviour
{
    private Entity entity;
    private Entity_Combat entityCombat;
    private Entity_SFX entitySfx; 

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();
        entityCombat = GetComponentInParent<Entity_Combat>();
        entitySfx = GetComponentInParent<Entity_SFX>();
    }

    private void CurrentStateTrigger()
    {
        entity.CurrentStateAnimationTrigger();
    }

    private void AttackTrigger()
    {
        entityCombat.PerformAttack();
    }
    
    public void FootstepTrigger()
    {
        if (entitySfx != null)
            entitySfx.PlayFootstepOnce();
    }
    
    public void DashTrigger()
    {
        if (entitySfx != null)
            entitySfx.PlayDashOnce();
    }
    
    public void JumpTrigger()
    {
        if (entitySfx != null)
            entitySfx.PlayJumpOnce();
    }
}
