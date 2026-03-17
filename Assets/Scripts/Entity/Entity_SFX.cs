using UnityEngine;

public class Entity_SFX : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("SFX Names")]
    [SerializeField] private string attackHit;
    [SerializeField] private string attackMiss;
    [SerializeField] private string footstep; 
    [SerializeField] private string dash;
    [SerializeField] private string jump;
    [Space]
    [SerializeField] private float soundDistance = 15f;
    [SerializeField] private bool showGizmo;


    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayAttackHit()
    {
        AudioManager.instance.PlaySFX(attackHit, audioSource, soundDistance);
    }

    public void PlayAttackMiss()
    {
        AudioManager.instance.PlaySFX(attackMiss, audioSource, soundDistance);
    }
    
    public void PlayFootstepOnce()
    {
        if (string.IsNullOrEmpty(footstep)) return;
        AudioManager.instance.PlaySFX(footstep, audioSource, soundDistance);
    }
    
    public void PlayDashOnce()
    {
        if (string.IsNullOrEmpty(footstep)) return;
        AudioManager.instance.PlaySFX(dash, audioSource, soundDistance);
    }
    
    public void PlayJumpOnce()
    {
        if (string.IsNullOrEmpty(footstep)) return;
        AudioManager.instance.PlaySFX(jump, audioSource, soundDistance);
    }

    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, soundDistance);
        }
    }
}
