using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("Missile Specs")]
    public float speed = 5f;
    public float maxSpeed = 12f;
    public float acceleration = 6f;
    public float rotateSpeed = 260f;
    public int damage = 4;
    public float lifetime = 5f;

    [Header("Visual & SFX")]
    public GameObject explosionPrefab;

    private Transform target;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        FindClosestTarget();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Tăng tốc dần theo thời gian
        if (speed < maxSpeed)
        {
            speed += acceleration * Time.deltaTime;
        }

        // Nếu mục tiêu cũ bị hủy, tìm mục tiêu mới
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindClosestTarget();
        }

        Vector2 direction;
        if (target != null)
        {
            direction = ((Vector2)target.position - rb.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
            rb.rotation = nextAngle;
        }

        // Bay về hướng mũi tên lửa (hướng 'up' của Transform)
        rb.linearVelocity = transform.up * speed;
    }

    private void FindClosestTarget()
    {
        target = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        // 1. Tìm kẻ địch (EnemyBase / Boss)
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                target = enemy.transform;
            }
        }

        // 2. Nếu không có kẻ địch, tìm thiên thạch
        if (target == null)
        {
            Asteroid[] asteroids = FindObjectsByType<Asteroid>(FindObjectsSortMode.None);
            foreach (var ast in asteroids)
            {
                if (ast == null || !ast.gameObject.activeInHierarchy) continue;
                float dist = Vector3.Distance(currentPos, ast.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    target = ast.transform;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Bỏ qua va chạm với Tàu người chơi, Đạn của người chơi và Đạn tên lửa khác
        if (other.CompareTag("Player") || other.CompareTag("Laser") || other.GetComponent<HomingMissile>() != null)
            return;

        bool detonated = false;

        // Trúng tàu địch / Boss
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            detonated = true;
        }
        else
        {
            // Trúng thiên thạch
            Asteroid asteroid = other.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.AddScore(asteroid.scoreValue, asteroid.transform.position);

                if (asteroid.explosionPrefab != null)
                    Instantiate(asteroid.explosionPrefab, asteroid.transform.position, Quaternion.identity);

                Destroy(asteroid.gameObject);
                detonated = true;
            }
        }

        if (detonated)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.25f, 0.25f);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
