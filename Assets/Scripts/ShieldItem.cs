using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    [Header("Falling Settings")]
    public float fallSpeed = 3f; // Tốc độ trôi xuống

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Nếu có Rigidbody2D -> gán vận tốc thẳng đứng xuống dưới
        if (rb != null)
        {
            rb.gravityScale = 0f; // Tắt trọng lực tự do
            rb.linearVelocity = Vector2.down * fallSpeed;
        }

        // Tự hủy sau 7 giây nếu trôi khỏi màn hình mà người chơi không nhặt
        Destroy(gameObject, 7f);
    }

    void Update()
    {
        // Dự phòng nếu không dùng Rigidbody2D hoặc Rigidbody2D bị tắt, 
        // nó vẫn sẽ bị ép rơi xuống đều đặn qua Transform
        if (rb == null || rb.isKinematic)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        }
    }
}