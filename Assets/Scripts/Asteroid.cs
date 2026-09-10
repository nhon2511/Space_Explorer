using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement")]
    public float minSpeed = 2f;
    public float maxSpeed = 4.5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tốc độ rơi ngẫu nhiên
        float speed = Random.Range(minSpeed, maxSpeed);

        // Hướng bay: chủ yếu hướng xuống dưới (Vector2.down), hơi lệch nhẹ sang trái/phải ngẫu nhiên
        Vector2 moveDirection = new Vector2(Random.Range(-0.3f, 0.3f), -1f).normalized;
        rb.linearVelocity = moveDirection * speed;

        // Xoay nhẹ viên đá cho chân thật
        rb.angularVelocity = Random.Range(-50f, 50f);

        // Tự hủy sau 8 giây khi đã rơi qua đáy màn hình
        Destroy(gameObject, 8f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Bị đạn Laser bắn trúng
        if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject); // Xóa viên đạn
            Destroy(gameObject);       // Xóa thiên thạch
        }
    }
}