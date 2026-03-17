using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : MonoBehaviour
{
    private UI ui;
    
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI[] dialogueChoicesText;
    
    [SerializeField] private float textSpeed = 0.1f;
    private string fullTextToShow;
    private Coroutine typeTextCo;
    
    private DialogueLineSO currentLine;
    private DialogueLineSO[] currentChoices;
    private DialogueLineSO selectedChoice;
    private int selectedChoiceIndex;
    
    private bool waitingToConfirm;
    private bool canInteract;
    
    [SerializeField] private AudioSource typeSfxSource;
    [SerializeField] private AudioClip typeSfx;
    
    private void Awake()
    {
        ui = GetComponentInParent<UI>();
    }
    
    public void PlayDialogueLine(DialogueLineSO line)
    {
        currentLine = line;
        currentChoices = line.choiceLines;
        canInteract = false;
        selectedChoice = null;
        selectedChoiceIndex = 0;

        HideAllChoices();
        
        speakerName.text = line.speaker.speakerName;

        fullTextToShow = line.actionType == DialogueActionType.None || line.actionType == DialogueActionType.PlayerMakeChoice ?
            line.GetRandomLine() : line.actionLine;

        typeTextCo = StartCoroutine(TypeTextCo(fullTextToShow));
        StartCoroutine(EnableInteractionCo());
    }
    
    private void HandleNextAction()
    {
        switch (currentLine.actionType)
        {
            case DialogueActionType.PlayerMakeChoice:
                if (selectedChoice == null)
                {
                    ShowChoices();
                }
                else
                {
                    DialogueLineSO selectedChoice = currentChoices[selectedChoiceIndex];
                    PlayDialogueLine(selectedChoice);
                }
                break;
            case DialogueActionType.CloseDialogue:
                ui.SwitchToInGameUI();
                break;
        }
    }
    
    public void DialogueInteraction()
    {
        if (canInteract == false)
            return;

        if (typeTextCo != null)
        {
            CompleteTyping();

            if (currentLine.actionType != DialogueActionType.PlayerMakeChoice)
                waitingToConfirm = true;
            else
                HandleNextAction();

            return;
        }

        if (waitingToConfirm || selectedChoice != null)
        {
            waitingToConfirm = false;
            HandleNextAction();
        }
    }
    
    private void CompleteTyping()
    {
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            dialogueText.text = fullTextToShow;
            typeTextCo = null;
        }
    }
    
    private void ShowChoices()
    {
        for (int i = 0; i < dialogueChoicesText.Length; i++)
        {
            if (i < currentChoices.Length)
            {
                DialogueLineSO choice = currentChoices[i];
                string choiceText = choice.playerChoiceAnswer;

                dialogueChoicesText[i].gameObject.SetActive(true);
                dialogueChoicesText[i].text = selectedChoiceIndex == i ?
                    $"<color=yellow> > {choiceText}" :
                    $"> {choiceText}";
            }
            else
            {
                dialogueChoicesText[i].gameObject.SetActive(false);
            }
        }
        selectedChoice = currentChoices[selectedChoiceIndex];
    }
    
    private void HideAllChoices()
    {
        foreach (var obj in dialogueChoicesText)
            obj.gameObject.SetActive(false);
    }
    
    public void NavigateChoice(int direction)
    {
        if (currentChoices == null || currentChoices.Length <= 1)
            return;

        selectedChoiceIndex = selectedChoiceIndex + direction;
        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, currentChoices.Length - 1);
        ShowChoices();
    }
    
    private IEnumerator TypeTextCo(string text)
    {
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            
            if (typeSfx != null && typeSfxSource != null && !char.IsWhiteSpace(letter))
                typeSfxSource.PlayOneShot(typeSfx);
            
            yield return new WaitForSeconds(textSpeed);
        }

        if (currentLine.actionType != DialogueActionType.PlayerMakeChoice)
        {
            waitingToConfirm = true;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            selectedChoice = null;
            HandleNextAction();
        }

        typeTextCo = null;
    }
    
    private IEnumerator EnableInteractionCo()
    {
        yield return null;
        canInteract = true;
    }
}
