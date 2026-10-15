using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("Card UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public Button selectButton;

    private System.Action onCardSelected;

    void Awake()
    {
        if (selectButton == null)
            selectButton = GetComponent<Button>();

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }
    }

    public void SetupCard(UpgradeData data, System.Action onClickCallback)
    {
        onCardSelected = onClickCallback;

        if (titleText != null)
            titleText.text = data.title;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }

        if (levelText != null)
        {
            int nextLv = data.currentLevel + 1;
            levelText.text = $"[CẤP {nextLv}/{data.maxLevel}]";
        }
    }

    private void OnSelectClicked()
    {
        onCardSelected?.Invoke();
    }
}
