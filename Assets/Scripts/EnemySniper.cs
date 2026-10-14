using UnityEngine;

public class EnemySniper : EnemyBase
{
    [Header("Sniper Movement")]
    public float enterSpeed = 2.5f;
    public float targetY = 3.2f;
    public float patrolSpeed = 1.8f;
    public float patrolRange = 6f;

    [Header("Sniper Weapon")]
    public GameObject enemyLaserPrefab;
    public Transform firePoint;
    public float fireRate = 2.5f;
    public float bulletSpeed = 9.5f;

    private bool hasEntered = false;
    private int patrolDirection = 1;
    private float nextFireTime;

    protected override void Start()
    {
        base.Start();
        maxHealth = 3;
        currentHealth = maxHealth;
        scoreValue = 70;

        patrolDirection = (Random.value > 0.5f) ? 1 : -1;
        // Bắn phát đầu tiên sau 1.2 giây
        nextFireTime = Time.time + 1.2f;

        // Tự động tìm Player
        FindPlayerTarget();
    }

    void Update()
    {
        // 1. Tiến vào màn hình đến độ cao targetY
        if (!hasEntered)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetY, 0), enterSpeed * Time.deltaTime);
            if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
            {
                hasEntered = true;
            }
        }
        else
        {
            // 2. Tuần tra qua lại theo chiều ngang
            transform.Translate(Vector3.right * patrolDirection * patrolSpeed * Time.deltaTime, Space.World);

            if (transform.position.x >= patrolRange) patrolDirection = -1;
            else if (transform.position.x <= -patrolRange) patrolDirection = 1;
        }

        // 3. Ngắm bắn định kỳ
        if (enemyLaserPrefab != null && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            AimAndShoot();
        }
    }

    private void FindPlayerTarget()
    {
        if (playerTransform != null) return;

        // Tìm theo component trước (không phụ thuộc vào Tag)
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            playerTransform = pc.transform;
            return;
        }

        // Dự phòng tìm theo Tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerTransform = p.transform;
        }
    }

    void AimAndShoot()
    {
        FindPlayerTarget();

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + Vector3.down * 0.5f;
        
        // Hướng bắn: nhắm vào Player nếu tìm thấy, hoặc bắn thẳng xuống dưới nếu không tìm thấy
        Vector2 aimDirection = Vector2.down;
        if (playerTransform != null)
        {
            aimDirection = (playerTransform.position - spawnPos).normalized;
        }

        GameObject laser = Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);
        EnemyLaser laserScript = laser.GetComponent<EnemyLaser>();
        if (laserScript != null)
        {
            laserScript.speed = bulletSpeed;
            laserScript.SetDirection(aimDirection);
        }
    }
}
