using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI instance { get; private set; }
    [SerializeField] private GameObject[] uiElements;
    
    private PlayerInputSet input;
    public UI_InGame inGameUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    public UI_DeathScreen deathScreenUI { get; private set; }
    public UI_FadeScreen fadeScreenUI { get; private set; }
    public UI_Dialogue dialogueUI { get; private set; }
    
    private void Awake()
    {
        instance = this;
        
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        deathScreenUI = GetComponentInChildren<UI_DeathScreen>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
    }

    public void SetupControlsUI(PlayerInputSet inputSet)
    {
        input = inputSet;

        input.UI.OptionsUI.performed += ctx =>
        { 
            foreach (var element in uiElements)
            {
                if (element.activeSelf)
                {
                    Time.timeScale = 1;
                    SwitchToInGameUI();
                    return;
                }
            }

            Time.timeScale = 0;
            OpenOptionsUI();
        };
        
        input.UI.DialogueInteraction.performed += ctx =>
        {
            if (dialogueUI.gameObject.activeInHierarchy)
                dialogueUI.DialogueInteraction();
        };

        input.UI.DialogueNavigation.performed += ctx =>
        {
            int direction = Mathf.RoundToInt(ctx.ReadValue<float>());

            if (dialogueUI.gameObject.activeInHierarchy)
                dialogueUI.NavigateChoice(direction);
        };
    }

    private void SwitchTo(GameObject objectToSwitchOn)
    {
        foreach (var element in uiElements)
            element.gameObject.SetActive(false);

        objectToSwitchOn.SetActive(true);
    }
    
    private void StopPlayerControls(bool stopControls)
    {
        if (stopControls)
            input.Player.Disable();
        else
            input.Player.Enable();
    }
    
    private void StopPlayerControls()
    {
        foreach (var element in uiElements)
        {
            if (element.activeSelf)
            {
                StopPlayerControls(true);
                return;
            }
        }

        StopPlayerControls(false);
    }
    
    public void OpenDialogueUI(DialogueLineSO firstLine)
    {
        StopPlayerControls(true);
        
        dialogueUI.gameObject.SetActive(true);
        dialogueUI.PlayDialogueLine(firstLine);
        
    }

    public void OpenDeathScreenUI()
    {
        SwitchTo(deathScreenUI.gameObject);
        input.Disable();
    }

    public void OpenOptionsUI()
    {
        StopPlayerControls(true);
        SwitchTo(optionsUI.gameObject);
    }
    
    public void SwitchToInGameUI()
    {
        StopPlayerControls(false);
        SwitchTo(inGameUI.gameObject);
    }
}