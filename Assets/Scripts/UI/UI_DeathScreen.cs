using UnityEngine;

public class UI_DeathScreen : MonoBehaviour
{
    public void Respawn()
    {
        AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.RestartScene();
    }
    
    public void GoToMainMenu()
    {
        AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.GoToMainMenu();
    }
}