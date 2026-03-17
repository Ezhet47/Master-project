using UnityEngine;

public class Object_Geir : Object_NPC, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Update()
    {
        base.Update();
    }
    
    public override void Interact()
    {
        base.Interact();
        
        ui.OpenDialogueUI(firstDialogueLine);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
    }
}
