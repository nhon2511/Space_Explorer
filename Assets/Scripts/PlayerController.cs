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
    public float screenPadding = 0.6f;
    // Tọa độ giới hạn thủ công (nếu tắt autoDetectBounds)
    public float minX = -8.2f;
    public float maxX = 8.2f;
    public float minY = -4.5f;
    public float maxY = 3.5f;

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
    public Animator thrusterAnimator; // Kéo đối tượng con ThrusterVisual vào đây

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextFireTime = 0f;
    private Vector2 calculatedBounds;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tự động tính toán biên màn hình theo Camera
        if (autoDetectBounds && Camera.main != null)
        {
            Camera cam = Camera.main;
            Vector3 upperRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
            calculatedBounds = new Vector2(upperRight.x - screenPadding, upperRight.y - screenPadding);
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
            currentPos.x = Mathf.Clamp(currentPos.x, -calculatedBounds.x, calculatedBounds.x);
            // Giới hạn trục Y không vượt quá 70% chiều cao màn hình để tránh chỗ thiên thạch spawn
            currentPos.y = Mathf.Clamp(currentPos.y, -calculatedBounds.y, calculatedBounds.y * 0.7f);
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

        Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
        if (laserRb == null) laserRb = laser.AddComponent<Rigidbody2D>();
        laserRb.gravityScale = 0;
        laserRb.linearVelocity = direction.normalized * laserSpeed;

        Destroy(laser, 2.5f);
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