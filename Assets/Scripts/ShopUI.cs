using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Shop Header")]
    public TextMeshProUGUI creditsText;

    [Header("Ship Display")]
    public Image shipPreviewImage;
    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipDescriptionText;
    public TextMeshProUGUI statsText;

    [Header("Buttons")]
    public Button prevButton;
    public Button nextButton;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;
    public Button closeButton;

    [Header("Optional Links")]
    public GameObject previousMenuPanel; // Bảng menu trước đó (để bật lại khi bấm Đóng)

    private int currentIndex = 0;

    void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevClicked);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (actionButton != null) actionButton.onClick.AddListener(OnActionClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
    }

    void OnEnable()
    {
        if (ShopManager.Instance != null)
        {
            currentIndex = ShopManager.Instance.GetSelectedShipId();
        }
        UpdateUI();
    }

    public void OpenShop()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void OpenShop(GameObject prevMenu)
    {
        previousMenuPanel = prevMenu;
        if (previousMenuPanel != null)
        {
            previousMenuPanel.SetActive(false);
        }

        gameObject.SetActive(true);
        UpdateUI();
    }

    private void UpdateUI()
    {
        ShopManager sm = ShopManager.Instance;
        if (sm == null || sm.allShips == null || sm.allShips.Count == 0) return;

        // 1. Cập nhật số tiền hiện có
        if (creditsText != null)
        {
            creditsText.text = $"QUỸ ĐIỂM: {sm.GetTotalCredits():N0} PTS";
        }

        // Kẹp chỉ số trong danh sách
        currentIndex = Mathf.Clamp(currentIndex, 0, sm.allShips.Count - 1);
        ShipData currentShip = sm.allShips[currentIndex];

        // 2. Hình ảnh phi thuyền
        if (shipPreviewImage != null)
        {
            if (currentShip.shipSprite != null)
            {
                shipPreviewImage.sprite = currentShip.shipSprite;
                shipPreviewImage.enabled = true;
            }
        }

        // 3. Tên & Mô tả
        if (shipNameText != null) shipNameText.text = currentShip.shipName;
        if (shipDescriptionText != null) shipDescriptionText.text = currentShip.description;

        // 4. Chỉ số so sánh
        if (statsText != null)
        {
            statsText.text = $"MÁU (HP): {currentShip.baseLives} Mạng\n" +
                             $"TỐC ĐỘ BAY: {currentShip.moveSpeed:0.0} m/s\n" +
                             $"TỐC ĐỘ BẮN: {1f / currentShip.fireRate:0.0} viên/giây\n" +
                             $"HỎA LỰC ĐẦU: Cấp {currentShip.startingWeaponLevel}" +
                             (currentShip.startsWithDrone ? " + 1 VỆ TINH" : "");
        }

        // 5. Nút Thao tác (MUA / TRANG BỊ / ĐANG CHỌN)
        bool isUnlocked = sm.IsShipUnlocked(currentShip.id);
        bool isSelected = (sm.GetSelectedShipId() == currentShip.id);
        int totalCredits = sm.GetTotalCredits();

        if (actionButton != null && actionButtonText != null)
        {
            Image btnImg = actionButton.GetComponent<Image>();

            if (isSelected)
            {
                actionButtonText.text = "[ĐANG TRANG BỊ]";
                actionButton.interactable = false;
                if (btnImg != null) btnImg.color = new Color(0.2f, 0.75f, 0.35f, 1f); // Xanh lá
            }
            else if (isUnlocked)
            {
                actionButtonText.text = "TRANG BỊ";
                actionButton.interactable = true;
                if (btnImg != null) btnImg.color = new Color(0.15f, 0.65f, 0.95f, 1f); // Xanh dương
            }
            else
            {
                actionButtonText.text = $"MUA ({currentShip.price:N0} PTS)";
                bool canAfford = totalCredits >= currentShip.price;
                actionButton.interactable = canAfford;
                if (btnImg != null)
                {
                    btnImg.color = canAfford ? new Color(1f, 0.75f, 0.1f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f); // Vàng cam hoặc Xám
                }
            }
        }
    }

    private void OnPrevClicked()
    {
        ShopManager sm = ShopManager.Instance;
        if (sm == null || sm.allShips.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = sm.allShips.Count - 1;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayLaser();
        UpdateUI();
    }

    private void OnNextClicked()
    {
        ShopManager sm = ShopManager.Instance;
        if (sm == null || sm.allShips.Count == 0) return;

        currentIndex++;
        if (currentIndex >= sm.allShips.Count) currentIndex = 0;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayLaser();
        UpdateUI();
    }

    private void OnActionClicked()
    {
        ShopManager sm = ShopManager.Instance;
        if (sm == null) return;

        ShipData currentShip = sm.allShips[currentIndex];
        bool isUnlocked = sm.IsShipUnlocked(currentShip.id);

        if (!isUnlocked)
        {
            // Mua tàu
            if (sm.BuyShip(currentShip.id))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayShieldPickup();
            }
        }
        else
        {
            // Trang bị tàu
            sm.SelectShip(currentShip.id);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLaser();
        }

        UpdateUI();
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
        if (previousMenuPanel != null)
        {
            previousMenuPanel.SetActive(true);
        }
    }
}
