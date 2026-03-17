using System.Collections;
using TMPro;
using UnityEngine;

public class UI_LevelIntroText : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI levelText;   // 关卡文本（使用它原本的 text）

    [Header("Timing")]
    [SerializeField] private float textSpeed = 0.05f;     // 每个字符间隔
    [SerializeField] private float visibleDuration = 5f;  // 打完之后停留的时间
    [SerializeField] private float fadeOutDuration = 1f;  // 渐隐时长

    [Header("SFX")]
    [SerializeField] private AudioSource typeSfxSource;
    [SerializeField] private AudioClip typeSfx;

    private string originalText;      // 一开始 TMP 上的文本
    private string fullTextToShow;    // 实际要打字的文本
    private Coroutine typeTextCo;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (levelText != null)
        {
            // 先缓存原始文本（Inspector 里写的内容）
            originalText = levelText.text;

            // 确保有 CanvasGroup，用来控制透明度
            canvasGroup = levelText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = levelText.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            levelText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // 进入关卡时自动播放
        PlayLevelText();
    }

    /// <summary>
    /// 对外接口：开始播放关卡文字
    /// </summary>
    public void PlayLevelText()
    {
        if (levelText == null)
            return;

        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            typeTextCo = null;
        }

        levelText.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;   // 开始时完全可见

        // 使用 Text 本身的内容（初始时缓存的原始文本）
        fullTextToShow = originalText;

        typeTextCo = StartCoroutine(TypeTextCo());
    }

    private IEnumerator TypeTextCo()
    {
        levelText.text = "";

        foreach (char letter in fullTextToShow)
        {
            levelText.text += letter;

            // 播放打字音效（不对空白字符播放）
            if (typeSfxSource != null && typeSfx != null && !char.IsWhiteSpace(letter))
            {
                typeSfxSource.PlayOneShot(typeSfx);
            }

            yield return new WaitForSeconds(textSpeed);
        }

        // 打完后停留一段时间
        yield return new WaitForSeconds(visibleDuration);

        // 再做淡出
        yield return StartCoroutine(FadeOutCo());

        levelText.gameObject.SetActive(false);
        typeTextCo = null;
    }

    private IEnumerator FadeOutCo()
    {
        if (canvasGroup == null)
            yield break;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            typeTextCo = null;
        }
    }
}
