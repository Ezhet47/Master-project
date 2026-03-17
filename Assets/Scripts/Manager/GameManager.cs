using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Scene Names")]
    [SerializeField] private string startGameSceneName = "Level_0";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // 如果别的脚本想拿到名字，可以通过这两个属性
    public string StartGameSceneName => startGameSceneName;
    public string MainMenuSceneName => mainMenuSceneName;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 从主菜单开始游戏：使用 Inspector 里配置的起始场景
    public void ContinuePlay()
    {
        ChangeScene(startGameSceneName);
    }

    // 回到主菜单：使用 Inspector 里配置的主菜单场景
    public void GoToMainMenu()
    {
        ChangeScene(mainMenuSceneName);
    }

    // 重启当前场景
    public void RestartScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName);
    }

    // 外部统一调用这个接口切场景
    public void ChangeScene(string sceneName)
    {
        Time.timeScale = 1f;
        StartCoroutine(ChangeSceneCo(sceneName));
    }

    private IEnumerator ChangeSceneCo(string sceneName)
    {
        // 1. 淡出
        UI_FadeScreen fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeOut();             // 透明 -> 黑
            yield return fadeScreen.fadeEffectCo;
        }

        // 2. 加载新场景
        SceneManager.LoadScene(sceneName);

        // 等一帧，让新场景的 Awake/Start 先跑完
        yield return null;

        // 3. 淡入
        fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeIn();              // 黑 -> 透明
        }

        // 4. 把玩家挪到新场景的 waypoint 出生点（如果有）
        Player player = Player.instance;
        if (player == null)
            yield break;

        // 场景中最多只会有一个 Object_Waypoint
        Object_Waypoint waypoint = FindFirstObjectByType<Object_Waypoint>();
        if (waypoint != null)
        {
            Vector3 spawnPos = waypoint.GetRespawnPosition();
            player.TeleportPlayer(spawnPos);
        }
        // 如果没有 waypoint，就不改玩家位置
    }

    private UI_FadeScreen FindFadeScreenUI()
    {
        if (UI.instance != null)
            return UI.instance.fadeScreenUI;

        return FindFirstObjectByType<UI_FadeScreen>();
    }
}
