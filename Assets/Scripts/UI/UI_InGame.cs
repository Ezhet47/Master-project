using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private Entity_Health playerHealth;

    [Header("Player Health")]
    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;

    [Header("Boss Health")]
    [SerializeField] private GameObject bossHealthRoot;  // Boss 血条整块 Panel
    [SerializeField] private Slider bossHealthSlider;    // Boss 血条 Slider

    private Enemy_Reaper boss;
    private Entity_Health bossHealth;

    // 一旦被触发显示过，就锁定为“只在 Boss 死亡时才隐藏”
    private bool bossHealthLockedVisible = false;

    private void Start()
    {
        // 玩家血量
        player = FindFirstObjectByType<Player>();
        playerHealth = player.GetComponent<Entity_Health>();
        playerHealth.OnHealthUpdate += UpdatePlayerHealthBar;
        UpdatePlayerHealthBar();

        // Boss / Reaper 血量
        boss = FindFirstObjectByType<Enemy_Reaper>();
        if (boss != null)
        {
            bossHealth = boss.GetComponent<Entity_Health>(); // Enemy_Health 继承自 Entity_Health
            if (bossHealth != null)
            {
                bossHealth.OnHealthUpdate += UpdateBossHealthBar;
                UpdateBossHealthBar();
            }
        }

        // 默认关掉 Boss 血条
        if (bossHealthRoot != null)
            bossHealthRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthUpdate -= UpdatePlayerHealthBar;

        if (bossHealth != null)
            bossHealth.OnHealthUpdate -= UpdateBossHealthBar;
    }

    // ===== 玩家血条 =====
    private void UpdatePlayerHealthBar()
    {
        float currentHealth = Mathf.RoundToInt(playerHealth.currentHealth);
        float maxHealth = playerHealth.maxHealth;

        healthSlider.value = currentHealth / maxHealth;
    }

    // ===== Boss 血条数值 =====
    private void UpdateBossHealthBar()
    {
        if (bossHealth == null || bossHealthSlider == null)
            return;

        float currentHealth = Mathf.RoundToInt(bossHealth.currentHealth);
        float maxHealth = bossHealth.maxHealth;

        bossHealthSlider.value = currentHealth / maxHealth;

        // 如果 Boss 已经死亡，这里强制关闭血条
        if (bossHealth.isDead && bossHealthRoot != null)
        {
            bossHealthRoot.SetActive(false);
        }
    }

    // ===== 提供给 Trigger 调用的显示/隐藏开关 =====
    // 需求：一旦触发显示后，就只会在 Boss 死亡后才隐藏
    public void SetBossHealthVisible(bool visible)
    {
        if (bossHealthRoot == null)
            return;

        // Boss 死了就强制隐藏
        if (bossHealth == null || bossHealth.isDead)
        {
            bossHealthRoot.SetActive(false);
            return;
        }

        if (visible)
        {
            // 第一次（或之后）要求显示：直接显示并锁定
            bossHealthLockedVisible = true;
            bossHealthRoot.SetActive(true);
        }
        else
        {
            // 只有在“还没被锁定”的阶段，才允许通过 false 隐藏
            // 一旦被锁定（说明战斗已经开始），就忽略隐藏请求
            if (!bossHealthLockedVisible)
            {
                bossHealthRoot.SetActive(false);
            }
            // 如果已经锁定，什么都不做（保持显示，直到 Boss 死亡）
        }
    }
}
