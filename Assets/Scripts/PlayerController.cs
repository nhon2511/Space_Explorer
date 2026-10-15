using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;

    [Header("Screen Boundary Settings (Giới hạn Map)")]
    [Tooltip("Tự động đo kích thước màn hình theo Camera chính")]
    public bool autoDetectBounds = true;
    [Tooltip("Khoảng cách đệm an toàn tính từ mép màn hình vào trong")]
    public float screenPadding = 0.3f;
    // Tọa độ giới hạn thủ công (nếu tắt autoDetectBounds)
    public float minX = -8.2f;
    public float maxX = 8.2f;
    public float minY = -5.5f;
    public float maxY = 5.5f;

    [Header("Weapon Settings")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserSpeed = 12f;
    [Tooltip("Thời gian chờ giữa 2 lần bắn (0.2s = 5 viên/giây)")]
    public float fireRate = 0.2f;
    [Range(1, 3)]
    [Tooltip("Cấp độ súng: 1 = Đạn đơn, 2 = Đạn đôi, 3 = Đạn chùm 3 tia")]
    public int weaponLevel = 1;

    [Header("VFX Settings")]
    public Animator thrusterAnimator;

    [Header("Phase 4: Roguelite Upgrades")]
    public int laserPierces = 0;
    public bool hasPermanentMagnet = false;
    public float magnetRadius = 5.0f;
    private System.Collections.Generic.List<GameObject> activeDrones = new System.Collections.Generic.List<GameObject>();
    private MissileLauncher missileLauncher;

    public static PlayerController Instance;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextFireTime = 0f;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tự động tính toán chuẩn xác cả 4 mép màn hình theo Camera thực tế (kể cả khi Camera bị lệch tâm Y)
        if (autoDetectBounds && Camera.main != null)
        {
            Camera cam = Camera.main;
            Vector3 lowerLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 10f));
            Vector3 upperRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 10f));

            minBounds = new Vector2(lowerLeft.x + screenPadding, lowerLeft.y + screenPadding);
            maxBounds = new Vector2(upperRight.x - screenPadding, upperRight.y - screenPadding);
        }

        // Tự động nạp Phi Thuyền được chọn từ Cửa Hàng (Shop)
        LoadSelectedShip();
    }

    private void LoadSelectedShip()
    {
        if (ShopManager.Instance == null) return;

        ShipData ship = ShopManager.Instance.GetSelectedShipData();
        if (ship != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && ship.shipSprite != null)
            {
                sr.sprite = ship.shipSprite;
            }

            moveSpeed = ship.moveSpeed;
            fireRate = ship.fireRate;
            weaponLevel = ship.startingWeaponLevel;

            // Kích hoạt đặc quyền riêng nếu có
            if (ship.startsWithDrone)
            {
                ApplyDroneUpgrade(1, null);
            }
        }
    }

    void Update()
    {
        // 1. Nhận phím di chuyển (mũi tên hoặc WASD)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. Cập nhật hoạt ảnh lửa đuôi tàu
        if (thrusterAnimator != null)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            thrusterAnimator.SetBool("isMoving", isMoving);
        }

        // 3. Bắn tia Laser (Hỗ trợ nhấp hoặc giữ phím Space có Cooldown)
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        // 4. Lõi hấp dẫn vĩnh viễn (Graviton Core)
        if (hasPermanentMagnet)
        {
            Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, magnetRadius);
            foreach (var col in items)
            {
                if (col.CompareTag("Star") || col.CompareTag("Health") || col.CompareTag("Shield") || col.CompareTag("Magnet") || col.CompareTag("WeaponUpgrade"))
                {
                    col.transform.position = Vector3.MoveTowards(col.transform.position, transform.position, 9.5f * Time.deltaTime);
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Di chuyển bằng vật lý (Unity 6)
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    void LateUpdate()
    {
        // Kẹp vị trí tàu luôn nằm trong màn hình sau khi vật lý di chuyển
        ClampPosition();
    }

    // --- LOGIC GIỚI HẠN VÙNG DI CHUYỂN ---
    void ClampPosition()
    {
        Vector3 currentPos = transform.position;

        if (autoDetectBounds)
        {
            currentPos.x = Mathf.Clamp(currentPos.x, minBounds.x, maxBounds.x);
            currentPos.y = Mathf.Clamp(currentPos.y, minBounds.y, maxBounds.y);
        }
        else
        {
            currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
            currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);
        }

        transform.position = currentPos;
    }

    // --- LOGIC BẮN ĐẠN THEO CẤP ĐỘ ---
    void Shoot()
    {
        if (laserPrefab == null || firePoint == null) return;

        switch (weaponLevel)
        {
            case 1:
                // Cấp 1: 1 viên bắn thẳng
                SpawnLaser(firePoint.position, Vector2.up);
                break;

            case 2:
                // Cấp 2: 2 viên song song 2 bên cánh
                SpawnLaser(firePoint.position + new Vector3(-0.28f, 0, 0), Vector2.up);
                SpawnLaser(firePoint.position + new Vector3(0.28f, 0, 0), Vector2.up);
                break;

            case 3:
                // Cấp 3: 3 viên tỏa góc hình rẻ quạt (1 thẳng, 2 lệch 15 độ)
                SpawnLaser(firePoint.position, Vector2.up);
                SpawnLaser(firePoint.position, Quaternion.Euler(0, 0, 15) * Vector2.up);
                SpawnLaser(firePoint.position, Quaternion.Euler(0, 0, -15) * Vector2.up);
                break;
        }

        // Phát âm thanh tiếng súng Laser
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaser();
        }
    }

    void SpawnLaser(Vector3 spawnPos, Vector2 direction)
    {
        // Tính góc xoay của sprite Laser theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

        // Thiết lập khả năng đạn xuyên thấu (Piercing Laser)
        PlayerLaser pl = laser.GetComponent<PlayerLaser>();
        if (pl == null) pl = laser.AddComponent<PlayerLaser>();
        int pierces = (laserPierces >= 3) ? 999 : laserPierces;
        pl.SetPiercing(pierces);

        // Hiệu ứng đạn to và sáng hơn khi đạt cấp xuyên tối đa
        if (laserPierces >= 3)
        {
            laser.transform.localScale *= 1.35f;
            SpriteRenderer sr = laser.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.2f, 1f, 1f, 1f);
        }

        Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
        if (laserRb == null) laserRb = laser.AddComponent<Rigidbody2D>();
        laserRb.gravityScale = 0;
        laserRb.linearVelocity = direction.normalized * laserSpeed;

        Destroy(laser, 2.5f);
    }

    // --- LOGIC VŨ KHÍ ROGUELITE ---
    public void ApplyDroneUpgrade(int droneCount, GameObject dronePrefab)
    {
        if (dronePrefab == null)
            dronePrefab = Resources.Load<GameObject>("OrbitalDrone");

        if (dronePrefab == null) return;

        // Xóa drone cũ trước khi tạo lại đội hình phân bố đều góc
        foreach (var d in activeDrones)
        {
            if (d != null) Destroy(d);
        }
        activeDrones.Clear();

        float angleStep = 360f / Mathf.Max(1, droneCount);
        for (int i = 0; i < droneCount; i++)
        {
            GameObject droneObj = Instantiate(dronePrefab, transform.position, Quaternion.identity);
            droneObj.name = $"OrbitalDrone_{i + 1}";
            OrbitalDrone drone = droneObj.GetComponent<OrbitalDrone>();
            if (drone != null)
            {
                float speed = 150f + (droneCount * 25f);
                float radius = 1.75f + (droneCount * 0.1f);
                drone.contactDamage = 2 + (droneCount - 1); // Cấp 1: 2 dmg, Cấp 2: 3 dmg, Cấp 3: 4 dmg
                drone.SetOrbitParams(transform, radius, speed, i * angleStep);
            }
            activeDrones.Add(droneObj);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnFloatingText($"[VỆ TINH: {droneCount} DRONE]", transform.position, Color.cyan, 6f);
        }
    }

    public void ApplyMissileUpgrade(int level, GameObject missilePrefab)
    {
        if (missileLauncher == null)
        {
            missileLauncher = GetComponent<MissileLauncher>();
            if (missileLauncher == null)
            {
                missileLauncher = gameObject.AddComponent<MissileLauncher>();
            }
        }

        if (missilePrefab == null)
        {
            missilePrefab = Resources.Load<GameObject>("HomingMissile");
        }

        if (missilePrefab != null)
        {
            missileLauncher.missilePrefab = missilePrefab;
        }

        missileLauncher.UpgradeLauncher(level);
    }

    // --- CÁC HÀM NHẬN HIỆU ỨNG TỪ VẬT PHẨM ---

    // Nâng cấp súng khi nhặt được Item Nâng Cấp
    public void UpgradeWeapon()
    {
        weaponLevel = Mathf.Min(3, weaponLevel + 1);
    }

    // Kích hoạt hút sao khi nhặt được Item Nam Châm
    public void ActivateMagnet(float duration = 7f)
    {
        StartCoroutine(MagnetRoutine(duration));
    }

    private IEnumerator MagnetRoutine(float duration)
    {
        float timer = 0f;
        float pullRadius = 6.0f; // Bán kính hút
        float pullSpeed = 8.5f;  // Tốc độ hút sao bay về tàu

        while (timer < duration)
        {
            Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, pullRadius);
            foreach (var col in objectsInRange)
            {
                if (col.CompareTag("Star"))
                {
                    col.transform.position = Vector3.MoveTowards(col.transform.position, transform.position, pullSpeed * Time.deltaTime);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
}