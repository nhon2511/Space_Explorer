using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    private static UpgradeManager _instance;
    public static UpgradeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<UpgradeManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("UpgradeManager");
                    _instance = go.AddComponent<UpgradeManager>();
                }
            }
            return _instance;
        }
    }

    [Header("EXP & Level Settings")]
    public int currentExp = 0;
    public int currentLevel = 1;
    public int expToNextLevel = 50;

    [Header("EXP UI (Thanh kinh nghiệm)")]
    public Slider expSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

    [Header("Upgrade Modal UI (Bảng chọn 1 trong 3 thẻ)")]
    public GameObject upgradePanel;
    public UpgradeCardUI[] cardSlots; // 3 thẻ hiển thị trên bảng

    [Header("Prefabs for Upgrades")]
    public GameObject orbitalDronePrefab;
    public GameObject homingMissilePrefab;

    [Header("Perk Icons (Tùy chọn gán từ Inspector)")]
    public Sprite droneIcon;
    public Sprite missileIcon;
    public Sprite piercingIcon;
    public Sprite armorIcon;
    public Sprite energyIcon;
    public Sprite magnetIcon;

    private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    private bool isSelectionOpen = false;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadIcons();
        InitializeUpgrades();
    }

    void Start()
    {
        UpdateExpUI();

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        // Tự động nạp Prefabs nếu chưa gán
        if (orbitalDronePrefab == null)
            orbitalDronePrefab = Resources.Load<GameObject>("OrbitalDrone");
        if (homingMissilePrefab == null)
            homingMissilePrefab = Resources.Load<GameObject>("HomingMissile");
    }

    void Update()
    {
        // Phím tắt U: Thử nghiệm tăng cấp tức thì khi làm bài / test game
        if (Input.GetKeyDown(KeyCode.U) && !isSelectionOpen)
        {
            AddExp(expToNextLevel - currentExp);
        }
    }

    private void LoadIcons()
    {
        if (droneIcon == null) droneIcon = Resources.Load<Sprite>("Icon_Drone");
        if (missileIcon == null) missileIcon = Resources.Load<Sprite>("Icon_Missile");
        if (piercingIcon == null) piercingIcon = Resources.Load<Sprite>("Icon_Laser");
        if (armorIcon == null) armorIcon = Resources.Load<Sprite>("Icon_Armor");
        if (energyIcon == null) energyIcon = Resources.Load<Sprite>("Icon_Energy");
        if (magnetIcon == null) magnetIcon = Resources.Load<Sprite>("Icon_Magnet");
    }

    private void InitializeUpgrades()
    {
        allUpgrades.Clear();

        // 1. VỆ TINH HỘ VỆ (Orbital Drone)
        allUpgrades.Add(new UpgradeData(
            UpgradeType.OrbitalDrone,
            "Vệ Tinh Hộ Vệ",
            "+1 Vệ tinh xoay quanh tàu (Tối đa 3 Drone). Càng lên cấp càng có nhiều vệ tinh nghiền nát kẻ địch.",
            droneIcon,
            3
        ));

        // 2. TÊN LỬA TẦM NHIỆT (Homing Missile)
        allUpgrades.Add(new UpgradeData(
            UpgradeType.HomingMissile,
            "Tên Lửa Tầm Nhiệt",
            "+1 Tên lửa mỗi loạt bắn (Cấp 1: 1 quả, Cấp 2: 2 quả, Cấp 3: 3 quả). Tự khóa và diệt mục tiêu.",
            missileIcon,
            3
        ));

        // 3. LASER XUYÊN THẤU (Piercing Laser)
        allUpgrades.Add(new UpgradeData(
            UpgradeType.PiercingLaser,
            "Laser Xuyên Thấu",
            "+1 Lần xuyên mục tiêu. Cấp 3 biến thành Laser Siêu Thanh bắn xuyên vô hạn không biến mất!",
            piercingIcon,
            3
        ));

        // 4. GIÁP TITAN (Titanium Hull)
        allUpgrades.Add(new UpgradeData(
            UpgradeType.TitaniumHull,
            "Giáp Titan Siêu Bền",
            "+1 Trái Tim Mạng tối đa trên màn hình, hồi ngay 1 Máu và tăng thời gian bất tử khi trúng đòn.",
            armorIcon,
            3
        ));

        // 5. KÍCH XUNG NĂNG LƯỢNG (Overcharge)
        allUpgrades.Add(new UpgradeData(
            UpgradeType.Overcharge,
            "Kích Xung Năng Lượng",
            "+30% Tốc độ bắn liên thanh (Fire Rate) và tăng tốc độ bay cơ động của tàu phi thuyền.",
            energyIcon,
            3
        ));

        // 6. LÕI HẤP DẪN (Graviton Core)
        allUpgrades.Add(new UpgradeData(
            UpgradeType.GravitonCore,
            "Lõi Hấp Dẫn Vũ Trụ",
            "Mở rộng từ trường cực mạnh, vĩnh viễn hút tất cả Sao và Vật phẩm rơi lại gần bạn.",
            magnetIcon,
            3
        ));
    }

    // --- CỘNG KINH NGHIỆM & THĂNG CẤP ---
    public void AddExp(int amount)
    {
        currentExp += amount;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.4f);

            TriggerLevelUp();
        }

        UpdateExpUI();
    }

    private void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }

        if (levelText != null)
        {
            levelText.text = $"CẤP {currentLevel}";
        }

        if (expText != null)
        {
            expText.text = $"{currentExp}/{expToNextLevel} EXP";
        }
    }

    private void TriggerLevelUp()
    {
        isSelectionOpen = true;

        if (GameManager.Instance != null && PlayerController.Instance != null)
        {
            GameManager.Instance.SpawnFloatingText($"[LEVEL UP! CẤP {currentLevel}]", PlayerController.Instance.transform.position, Color.cyan, 6.5f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShieldPickup();
        }

        // Tạm dừng game khi chọn thẻ
        Time.timeScale = 0f;

        ShowUpgradeSelection();
    }

    private void ShowUpgradeSelection()
    {
        if (upgradePanel == null || cardSlots == null || cardSlots.Length == 0)
        {
            Time.timeScale = 1f;
            isSelectionOpen = false;
            return;
        }

        // Lọc ra các nâng cấp chưa đạt cấp tối đa
        List<UpgradeData> available = new List<UpgradeData>();
        foreach (var up in allUpgrades)
        {
            if (!up.IsMaxed) available.Add(up);
        }

        // Nếu tất cả đã max cấp thì tiếp tục game
        if (available.Count == 0)
        {
            Time.timeScale = 1f;
            isSelectionOpen = false;
            return;
        }

        // Trộn ngẫu nhiên (Shuffle)
        for (int i = 0; i < available.Count; i++)
        {
            UpgradeData temp = available[i];
            int rand = Random.Range(i, available.Count);
            available[i] = available[rand];
            available[rand] = temp;
        }

        // Hiển thị tối đa 3 thẻ trên giao diện
        int countToDisplay = Mathf.Min(cardSlots.Length, available.Count);
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < countToDisplay)
            {
                cardSlots[i].gameObject.SetActive(true);
                UpgradeData data = available[i];
                cardSlots[i].SetupCard(data, () => SelectUpgrade(data));
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }

        upgradePanel.SetActive(true);
    }

    public void SelectUpgrade(UpgradeData chosen)
    {
        chosen.currentLevel++;

        ApplyUpgradeEffect(chosen);

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        isSelectionOpen = false;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShieldPickup();
        }

        if (PlayerController.Instance != null && GameManager.Instance != null)
        {
            GameManager.Instance.SpawnFloatingText($"ĐÃ CHỌN: {chosen.title}!", PlayerController.Instance.transform.position, Color.yellow, 5.5f);
        }
    }

    private void ApplyUpgradeEffect(UpgradeData data)
    {
        PlayerController pc = PlayerController.Instance;
        if (pc == null) return;

        switch (data.type)
        {
            case UpgradeType.OrbitalDrone:
                if (orbitalDronePrefab == null)
                    orbitalDronePrefab = Resources.Load<GameObject>("OrbitalDrone");
                pc.ApplyDroneUpgrade(data.currentLevel, orbitalDronePrefab);
                break;

            case UpgradeType.HomingMissile:
                if (homingMissilePrefab == null)
                    homingMissilePrefab = Resources.Load<GameObject>("HomingMissile");
                pc.ApplyMissileUpgrade(data.currentLevel, homingMissilePrefab);
                break;

            case UpgradeType.PiercingLaser:
                pc.laserPierces = data.currentLevel;
                if (GameManager.Instance != null)
                {
                    string pierceText = (data.currentLevel >= 3) ? "[LASER: XUYÊN VÔ HẠN!]" : $"[LASER: XUYÊN {data.currentLevel} MỤC TIÊU]";
                    GameManager.Instance.SpawnFloatingText(pierceText, pc.transform.position, Color.cyan, 6f);
                }
                break;

            case UpgradeType.TitaniumHull:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.IncreaseMaxLives(1);
                }
                PlayerCollision col = pc.GetComponent<PlayerCollision>();
                if (col != null) col.invulnerableDuration += 0.5f;
                break;

            case UpgradeType.Overcharge:
                pc.fireRate = Mathf.Max(0.06f, pc.fireRate * 0.72f); // Bắn nhanh hơn 28%
                pc.moveSpeed += 1.2f;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SpawnFloatingText("[KÍCH XUNG: +30% TỐC ĐỘ]", pc.transform.position, Color.yellow, 6f);
                }
                break;

            case UpgradeType.GravitonCore:
                pc.hasPermanentMagnet = true;
                pc.magnetRadius = 5.0f + (data.currentLevel * 3.5f);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SpawnFloatingText($"[LÕI HẤP DẪN: PHẠM VI {pc.magnetRadius:0}M]", pc.transform.position, Color.magenta, 6f);
                }
                break;
        }
    }
}
