using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Splitting Settings")]
    public bool canSplit = true;             // Tích chọn nếu là thiên thạch to (có thể vỡ đôi)
    public GameObject smallerAsteroidPrefab; // Kéo Prefab Asteroid_Small vào đây
    public int scoreValue = 20;              // Điểm nhận được khi bắn nổ

    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 4.5f;
    private Rigidbody2D rb;
    private float speedMultiplier = 1f; // Hệ số nhân tốc độ từ Spawner
    private bool isLaunched = false;     // Đánh dấu nếu là mảnh vỡ vừa được bắn ra

    [Header("Visual Effects (VFX)")]
    public GameObject explosionPrefab; // Kéo Prefab vụ nổ vào đây

    [Header("Shield Drop Settings")]
    public GameObject shieldDropPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.35f; // Tỉ lệ rơi khiên (35%)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Chỉ áp dụng chuyển động rơi tự nhiên nếu không phải là mảnh vỡ vừa văng ra
        if (!isLaunched)
        {
            ApplyMovement();
        }

        // Tự hủy sau 8 giây khi trôi khỏi màn hình
        Destroy(gameObject, 8f);
    }

    // Hàm nhận hệ số tăng tốc từ Spawner
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;

        if (rb != null && !isLaunched)
        {
            ApplyMovement();
        }
    }

    // Áp dụng hướng di chuyển rơi thông thường
    private void ApplyMovement()
    {
        float speed = Random.Range(minSpeed, maxSpeed) * speedMultiplier;
        Vector2 moveDirection = new Vector2(Random.Range(-0.25f, 0.25f), -1f).normalized;
        rb.linearVelocity = moveDirection * speed;
        rb.angularVelocity = Random.Range(-50f, 50f);
    }

    // Hàm phóng mảnh vỡ văng chéo theo hướng và tốc độ chỉ định
    public void LaunchSplit(Vector2 direction, float speed)
    {
        isLaunched = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        rb.linearVelocity = direction.normalized * speed;
        rb.angularVelocity = Random.Range(-120f, 120f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Khi bị đạn Laser bắn trúng
        if (other.CompareTag("Laser"))
        {
            // 1. Cộng điểm
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // 2. Phát âm thanh nổ thiên thạch
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            // 3. Rung nhẹ màn hình
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.12f, 0.15f);
            }

            Vector3 currentPos = transform.position;

            // 4. Kích hoạt hiệu ứng hoạt ảnh nổ
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, currentPos, Quaternion.identity);
            }

            // 5. Cơ chế tách mảnh: Nếu là thiên thạch to -> vỡ thành 2 mảnh con
            if (canSplit && smallerAsteroidPrefab != null)
            {
                // Mảnh 1 văng chéo sang trái
                GameObject piece1 = Instantiate(smallerAsteroidPrefab, currentPos + new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
                Asteroid ast1 = piece1.GetComponent<Asteroid>();
                if (ast1 != null)
                {
                    ast1.LaunchSplit(new Vector2(-0.75f, -0.65f), Random.Range(4f, 5.5f));
                }

                // Mảnh 2 văng chéo sang phải
                GameObject piece2 = Instantiate(smallerAsteroidPrefab, currentPos + new Vector3(0.25f, 0f, 0f), Quaternion.identity);
                Asteroid ast2 = piece2.GetComponent<Asteroid>();
                if (ast2 != null)
                {
                    ast2.LaunchSplit(new Vector2(0.75f, -0.65f), Random.Range(4f, 5.5f));
                }
            }
            else
            {
                // Khi mảnh nhỏ nổ tung mới xét rơi khiên
                if (shieldDropPrefab != null && Random.value <= dropChance)
                {
                    Instantiate(shieldDropPrefab, currentPos, Quaternion.identity);
                }
            }

            // 6. Hủy viên đạn Laser và thiên thạch hiện tại
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}