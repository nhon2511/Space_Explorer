using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 4.5f;
    private Rigidbody2D rb;

    [Header("Shield Drop Settings")]
    public GameObject shieldDropPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.35f; // Tỉ lệ rơi khiên (35%)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tốc độ ngẫu nhiên
        float speed = Random.Range(minSpeed, maxSpeed);

        // Rơi từ trên xuống dưới (hơi lệch góc nhẹ ngẫu nhiên)
        Vector2 moveDirection = new Vector2(Random.Range(-0.25f, 0.25f), -1f).normalized;
        rb.linearVelocity = moveDirection * speed;

        // Xoay nhẹ viên đá
        rb.angularVelocity = Random.Range(-50f, 50f);

        // Tự hủy sau 8 giây khi trôi khỏi màn hình
        Destroy(gameObject, 8f);
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

            // 2. Lưu chính xác vị trí vụ nổ trong không gian (World Position)
            Vector3 dropPosition = transform.position;

            // 3. Kiểm tra tỉ lệ rơi khiên
            if (shieldDropPrefab != null && Random.value <= dropChance)
            {
                // Sinh khiên ngay tại đúng tâm vị trí thiên thạch vừa bị nổ
                Instantiate(shieldDropPrefab, dropPosition, Quaternion.identity);
            }

            // 4. Hủy viên đạn Laser và thiên thạch
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}