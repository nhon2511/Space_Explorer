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
    private float speedMultiplier = 1f;
    private bool isLaunched = false;

    [Header("Visual Effects (VFX)")]
    public GameObject explosionPrefab;

    [Header("Drop Settings")]
    [Tooltip("Danh sách các vật phẩm có thể rơi (Khiên, Nâng cấp đạn, Hồi máu, Nam châm)")]
    public GameObject[] dropPrefabs;
    [Tooltip("Fallback nếu không dùng mảng dropPrefabs")]
    public GameObject shieldDropPrefab;
    [Range(0f, 1f)]
    public float dropChance = 0.35f; // Tỉ lệ rơi vật phẩm (35%)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (!isLaunched)
        {
            ApplyMovement();
        }

        // Tự hủy sau 8 giây khi trôi khỏi màn hình
        Destroy(gameObject, 8f);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;

        if (rb != null && !isLaunched)
        {
            ApplyMovement();
        }
    }

    private void ApplyMovement()
    {
        float speed = Random.Range(minSpeed, maxSpeed) * speedMultiplier;
        Vector2 moveDirection = new Vector2(Random.Range(-0.25f, 0.25f), -1f).normalized;
        rb.linearVelocity = moveDirection * speed;
        rb.angularVelocity = Random.Range(-50f, 50f);
    }

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
            PlayerLaser pl = other.GetComponent<PlayerLaser>();
            if (pl != null)
            {
                if (!pl.CanHit(gameObject)) return;
                pl.RegisterHit(gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }

            // 1. Cộng điểm & EXP
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue, transform.position);
            }

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.AddExp(canSplit ? 10 : 5);
            }

            // 2. Âm thanh nổ
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            // 3. Rung màn hình
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.12f, 0.15f);
            }

            Vector3 currentPos = transform.position;

            // 4. Sinh hiệu ứng vụ nổ
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, currentPos, Quaternion.identity);
            }

            // 5. Tách đôi nếu là thiên thạch to
            if (canSplit && smallerAsteroidPrefab != null)
            {
                // Mảnh 1 sang trái
                GameObject piece1 = Instantiate(smallerAsteroidPrefab, currentPos + new Vector3(-0.25f, 0f, 0f), Quaternion.identity);
                Asteroid ast1 = piece1.GetComponent<Asteroid>();
                if (ast1 != null)
                {
                    ast1.LaunchSplit(new Vector2(-0.75f, -0.65f), Random.Range(4f, 5.5f));
                }

                // Mảnh 2 sang phải
                GameObject piece2 = Instantiate(smallerAsteroidPrefab, currentPos + new Vector3(0.25f, 0f, 0f), Quaternion.identity);
                Asteroid ast2 = piece2.GetComponent<Asteroid>();
                if (ast2 != null)
                {
                    ast2.LaunchSplit(new Vector2(0.75f, -0.65f), Random.Range(4f, 5.5f));
                }
            }
            else
            {
                // Thiên thạch nhỏ nổ -> Xét rơi vật phẩm
                SpawnRandomDrop(currentPos);
            }

            // 6. Hủy thiên thạch
            Destroy(gameObject);
        }
    }

    void SpawnRandomDrop(Vector3 spawnPosition)
    {
        if (Random.value > dropChance) return;

        // Ưu tiên rơi đồ thích ứng theo trạng thái máu và súng của người chơi
        if (ItemDropManager.Instance != null)
        {
            GameObject smartItem = ItemDropManager.Instance.GetSmartDrop();
            if (smartItem != null)
            {
                Instantiate(smartItem, spawnPosition, Quaternion.identity);
                return;
            }
        }

        // Nếu có danh sách nhiều vật phẩm
        if (dropPrefabs != null && dropPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, dropPrefabs.Length);
            if (dropPrefabs[randomIndex] != null)
            {
                Instantiate(dropPrefabs[randomIndex], spawnPosition, Quaternion.identity);
                return;
            }
        }

        // Fallback: nếu chưa gán mảng dropPrefabs thì rơi khiên như cũ
        if (shieldDropPrefab != null)
        {
            Instantiate(shieldDropPrefab, spawnPosition, Quaternion.identity);
        }
    }
}