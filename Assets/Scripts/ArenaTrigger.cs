using UnityEngine;

public class ArenaTrigger : MonoBehaviour
{
    [SerializeField] private UI_InGame uiInGame;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player entered boss arena");
        if (other.GetComponent<Player>() != null)
        {
            uiInGame.SetBossHealthVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            uiInGame.SetBossHealthVisible(false);
        }
    }
}