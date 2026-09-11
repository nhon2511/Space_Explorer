using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 4.5f;
    private Rigidbody2D rb;
    private float speedMultiplier = 1f; // Hệ số nhân tốc độ từ Spawner

    [Header("Visual Effects (VFX)")]
    public GameObject explosionPrefab; // Kéo Prefab vụ nổ vào đây

    [Header("Shield Drop Settings")]
    public GameObject shieldDropPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.35f; // Tỉ lệ rơi khiên (35%)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyMovement();

        // Tự hủy sau 8 giây khi trôi khỏi màn hình
        Destroy(gameObject, 8f);
    }

    // Hàm nhận hệ số tăng tốc từ Spawner
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;

        // Cập nhật ngay vận tốc nếu Rigidbody2D đã sẵn sàng
        if (rb != null)
        {
            ApplyMovement();
        }
    }

    // Áp dụng hướng di chuyển và vận tốc thực tế
    private void ApplyMovement()
    {
        // Tốc độ ngẫu nhiên nhân với hệ số độ khó tăng dần
        float speed = Random.Range(minSpeed, maxSpeed) * speedMultiplier;

        // Rơi từ trên xuống dưới (hơi lệch góc nhẹ ngẫu nhiên)
        Vector2 moveDirection = new Vector2(Random.Range(-0.25f, 0.25f), -1f).normalized;
        rb.linearVelocity = moveDirection * speed;

        // Xoay nhẹ viên đá
        rb.angularVelocity = Random.Range(-50f, 50f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Khi bị đạn Laser bắn trúng
        if (other.CompareTag("Laser"))
        {
            // 1. Phát âm thanh nổ thiên thạch
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            // 2. Rung nhẹ màn hình tạo cảm giác lực nổ (rung trong 0.12s, biên độ 0.15)
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.12f, 0.15f);
            }

            // 3. Lưu vị trí nổ
            Vector3 dropPosition = transform.position;

            // 4. Kích hoạt hiệu ứng hoạt ảnh nổ
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, dropPosition, Quaternion.identity);
            }

            // 5. Kiểm tra tỉ lệ rơi khiên
            if (shieldDropPrefab != null && Random.value <= dropChance)
            {
                Instantiate(shieldDropPrefab, dropPosition, Quaternion.identity);
            }

            // 6. Hủy viên đạn Laser và thiên thạch
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}