using System;
using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    private void Start()
    {
        transform.root.GetComponentInChildren<UI_FadeScreen>().DoFadeIn();
    }

    public void PlayButton()
    {
        AudioManager.instance.PlayGlobalSFX("button_click");
        GameManager.instance.ContinuePlay();
    }
    
    public void QuitButton()
    {
        AudioManager.instance.PlayGlobalSFX("button_click");
        Application.Quit();
    }
}
