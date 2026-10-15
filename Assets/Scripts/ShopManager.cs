using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private static ShopManager _instance;
    public static ShopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ShopManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ShopManager");
                    _instance = go.AddComponent<ShopManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Ship Catalog")]
    public List<ShipData> allShips = new List<ShipData>();

    private const string PREF_CREDITS = "TotalCredits";
    private const string PREF_SELECTED_SHIP = "SelectedShipId";
    private const string PREF_SHIP_UNLOCKED = "ShipUnlocked_";

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            if (!IsCatalogValid())
            {
                InitializeCatalog();
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private bool IsCatalogValid()
    {
        if (allShips == null || allShips.Count < 4) return false;
        foreach (var ship in allShips)
        {
            if (ship == null || string.IsNullOrEmpty(ship.shipName) || ship.shipSprite == null)
                return false;
        }
        return true;
    }

#if UNITY_EDITOR
    void Reset()
    {
        InitializeCatalog();
    }

    void OnValidate()
    {
        if (!IsCatalogValid())
        {
            InitializeCatalog();
        }
    }
#endif

    [ContextMenu("Reinitialize Catalog")]
    public void ReinitializeCatalog()
    {
        InitializeCatalog();
    }

    void Update()
    {
        // Phím tắt C: Tặng 500 điểm để tiện test mua tàu trong Unity Editor
        if (Input.GetKeyDown(KeyCode.C))
        {
            AddCredits(500);
            Debug.Log($"[SHOP] Đã cộng 500 điểm test! Tổng điểm hiện có: {GetTotalCredits()} PTS");
        }
    }

    private Sprite LoadShipSprite(string name)
    {
        Sprite s = Resources.Load<Sprite>(name);
#if UNITY_EDITOR
        if (s == null)
        {
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{name}.png");
        }
        if (s == null)
        {
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{name}.png");
        }
#endif
        return s;
    }

    private void InitializeCatalog()
    {
        allShips.Clear();

        // 1. VOID STRIKER (Tàu mặc định)
        Sprite s0 = LoadShipSprite("Ship_Default");
        allShips.Add(new ShipData(
            0,
            "Void Striker",
            "Phi thuyền tiêu chuẩn của Liên minh Vũ Trụ. Cân bằng hoàn hảo giữa tốc độ, độ bền và hỏa lực.",
            0, // Miễn phí
            s0,
            lives: 3,
            speed: 6.0f,
            fireRate: 0.20f,
            startWep: 1,
            drone: false
        ));

        // 2. AERO INTERCEPTOR (Tốc độ & Liên thanh)
        Sprite s1 = LoadShipSprite("Ship_Blue_Interceptor");
        allShips.Add(new ShipData(
            1,
            "Aero Interceptor",
            "Chiến cơ xanh dương siêu nhẹ. Tốc độ bay cực nhanh (+30%) và tốc độ nhả đạn liên thanh xé gió.",
            500, // 500 PTS
            s1,
            lives: 2,
            speed: 7.8f,
            fireRate: 0.14f,
            startWep: 1,
            drone: false
        ));

        // 3. CRIMSON DREAD (Chiến hạm hỏa lực hạng nặng)
        Sprite s2 = LoadShipSprite("Ship_Red_Dread");
        allShips.Add(new ShipData(
            2,
            "Crimson Dread",
            "Chiến hạm bọc thép đỏ rực. Bắt đầu trận chiến với sẵn Súng Cấp 2 (Bắn đạn đôi) và Giáp 4 Máu.",
            1000, // 1,000 PTS
            s2,
            lives: 4,
            speed: 5.2f,
            fireRate: 0.22f,
            startWep: 2, // Đạn đôi
            drone: false
        ));

        // 4. PHANTOM STEALTH (Công nghệ vệ tinh & Đột kích)
        Sprite s3 = LoadShipSprite("Ship_Green_Phantom");
        allShips.Add(new ShipData(
            3,
            "Phantom Stealth",
            "Chiến hạm tàng hình ngọc lục bảo. Tự động xuất kích cùng 1 Vệ Tinh Hộ Vệ ngay từ đầu trận.",
            1600, // 1,600 PTS
            s3,
            lives: 3,
            speed: 6.8f,
            fireRate: 0.16f,
            startWep: 1,
            drone: true // Có sẵn drone!
        ));

        // Mặc định luôn mở khóa tàu 0
        PlayerPrefs.SetInt(PREF_SHIP_UNLOCKED + "0", 1);
    }

    // --- QUẢN LÝ TIỀN / ĐIỂM TÍCH LŨY ---
    public int GetTotalCredits()
    {
        return PlayerPrefs.GetInt(PREF_CREDITS, 0);
    }

    public void AddCredits(int amount)
    {
        int total = GetTotalCredits() + amount;
        PlayerPrefs.SetInt(PREF_CREDITS, total);
        PlayerPrefs.Save();
    }

    public bool SpendCredits(int amount)
    {
        int total = GetTotalCredits();
        if (total >= amount)
        {
            PlayerPrefs.SetInt(PREF_CREDITS, total - amount);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    // --- QUẢN LÝ SỞ HỮU & TRANG BỊ TÀU ---
    public bool IsShipUnlocked(int id)
    {
        if (id == 0) return true;
        return PlayerPrefs.GetInt(PREF_SHIP_UNLOCKED + id, 0) == 1;
    }

    public bool BuyShip(int id)
    {
        ShipData ship = GetShipData(id);
        if (ship == null || IsShipUnlocked(id)) return false;

        if (SpendCredits(ship.price))
        {
            PlayerPrefs.SetInt(PREF_SHIP_UNLOCKED + id, 1);
            SelectShip(id);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }

    public int GetSelectedShipId()
    {
        return PlayerPrefs.GetInt(PREF_SELECTED_SHIP, 0);
    }

    public void SelectShip(int id)
    {
        if (IsShipUnlocked(id))
        {
            PlayerPrefs.SetInt(PREF_SELECTED_SHIP, id);
            PlayerPrefs.Save();
        }
    }

    public ShipData GetShipData(int id)
    {
        if (allShips == null || allShips.Count == 0) InitializeCatalog();
        return allShips.Find(s => s.id == id);
    }

    public ShipData GetSelectedShipData()
    {
        int selectedId = GetSelectedShipId();
        ShipData ship = GetShipData(selectedId);
        if (ship == null && allShips.Count > 0) ship = allShips[0];
        return ship;
    }
}
