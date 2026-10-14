using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    private static ItemDropManager _instance;
    public static ItemDropManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemDropManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ItemDropManager");
                    _instance = go.AddComponent<ItemDropManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Item Prefabs")]
    public GameObject healthItemPrefab;
    public GameObject shieldItemPrefab;
    public GameObject weaponUpgradePrefab;
    public GameObject magnetItemPrefab;
    public GameObject starPrefab;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Tự động load từ Resources nếu chưa kéo thả trên Inspector
        if (healthItemPrefab == null) healthItemPrefab = Resources.Load<GameObject>("HealthItem");
        if (shieldItemPrefab == null) shieldItemPrefab = Resources.Load<GameObject>("ShieldItem");
        if (weaponUpgradePrefab == null) weaponUpgradePrefab = Resources.Load<GameObject>("WeaponUpgradeItem");
        if (magnetItemPrefab == null) magnetItemPrefab = Resources.Load<GameObject>("MagnetItem");
        if (starPrefab == null) starPrefab = Resources.Load<GameObject>("Star");
    }

    // Cơ chế phân tích tình trạng người chơi để thả vật phẩm thông minh
    public GameObject GetSmartDrop()
    {
        int lives = (GameManager.Instance != null) ? GameManager.Instance.GetCurrentLives() : 3;
        int weaponLvl = (PlayerController.Instance != null) ? PlayerController.Instance.weaponLevel : 1;

        float roll = Random.value;

        // 1. CẤP CỨU: Còn 1 máu -> 50% rơi Tim hồi máu cứu nguy
        if (lives <= 1 && healthItemPrefab != null && roll < 0.50f)
        {
            return healthItemPrefab;
        }

        // 2. MÁU YẾU: Còn 2 máu -> 25% rơi Tim hồi máu
        if (lives == 2 && healthItemPrefab != null && roll < 0.25f)
        {
            return healthItemPrefab;
        }

        // 3. SÚNG CÒN YẾU (Cấp 1): 40% rơi Nâng Cấp Súng để nhanh chóng mạnh lên
        if (weaponLvl == 1 && weaponUpgradePrefab != null && roll < 0.40f)
        {
            return weaponUpgradePrefab;
        }
        else if (weaponLvl == 2 && weaponUpgradePrefab != null && roll < 0.25f)
        {
            return weaponUpgradePrefab;
        }

        // 4. SÚNG ĐÃ ĐẠT TỐI ĐA (Cấp 3): Không bao giờ rơi Nâng Cấp Súng nữa
        // Thay bằng Khiên bảo vệ (40%), Nam châm hút sao (35%), hoặc Sao điểm (25%)
        if (weaponLvl >= 3)
        {
            float maxRoll = Random.value;
            if (maxRoll < 0.40f && shieldItemPrefab != null) return shieldItemPrefab;
            if (maxRoll < 0.75f && magnetItemPrefab != null) return magnetItemPrefab;
            return starPrefab != null ? starPrefab : shieldItemPrefab;
        }

        // 5. MẶC ĐỊNH BÌNH THƯỜNG:
        float normalRoll = Random.value;
        if (normalRoll < 0.30f && shieldItemPrefab != null) return shieldItemPrefab;
        if (normalRoll < 0.55f && magnetItemPrefab != null) return magnetItemPrefab;
        if (normalRoll < 0.80f && weaponUpgradePrefab != null) return weaponUpgradePrefab;
        return (healthItemPrefab != null) ? healthItemPrefab : shieldItemPrefab;
    }
}
