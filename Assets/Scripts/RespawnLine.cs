using UnityEngine;

public class RespawnLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            GameManager.instance.RestartScene();
    }
}
