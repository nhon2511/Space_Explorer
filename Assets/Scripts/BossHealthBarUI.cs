using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance;

    [Header("UI References")]
    public GameObject barContainer;
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI healthPercentText;

    private BossEnemy currentBoss;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Mặc định ẩn thanh máu khi chưa có Boss
        if (barContainer != null) barContainer.SetActive(false);
        else gameObject.SetActive(false);
    }

    public void ActivateBossBar(BossEnemy boss, string bossName, int maxHP)
    {
        currentBoss = boss;

        if (barContainer != null) barContainer.SetActive(true);
        else gameObject.SetActive(true);

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = maxHP;
        }

        if (healthPercentText != null)
        {
            healthPercentText.text = "100%";
        }
    }

    public void UpdateHealth(int currentHP, int maxHP)
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Max(0, currentHP);
        }

        if (healthPercentText != null)
        {
            float pct = Mathf.Clamp01((float)currentHP / maxHP) * 100f;
            healthPercentText.text = $"{Mathf.CeilToInt(pct)}%";
        }
    }

    public void HideBossBar()
    {
        if (barContainer != null) barContainer.SetActive(false);
        else gameObject.SetActive(false);
    }
}
