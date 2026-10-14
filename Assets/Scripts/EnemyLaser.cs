using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 4f;

    private Vector2 direction = Vector2.down;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Tự động gán Tag EnemyLaser nếu chưa gán
        gameObject.tag = "EnemyLaser";

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        // Xoay sprite đạn theo hướng bắn
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    void Update()
    {
        // Dự phòng nếu Rigidbody2D là Kinematic
        if (rb == null || rb.bodyType == RigidbodyType2D.Kinematic)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra va chạm với Tàu người chơi (hỗ trợ cả Tag lẫn component)
        PlayerCollision playerCol = other.GetComponent<PlayerCollision>();
        if (playerCol != null)
        {
            playerCol.TakeHitFromEnemy(damage);
            Destroy(gameObject);
        }
    }
}
