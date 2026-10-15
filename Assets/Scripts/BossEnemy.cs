using System.Collections;
using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("Boss Specific Stats")]
    public float enterSpeed = 2.0f;
    public float targetY = 2.8f;
    public float patrolSpeed = 2.2f;
    public float patrolRange = 4.8f;

    [Header("Boss Weapons")]
    public GameObject bossLaserPrefab;
    public GameObject scoutEscortPrefab;
    public Transform[] firePoints; // 3 hoặc nhiều nòng súng trên thân Boss
    public float spreadFireRate = 2.4f;
    public float summonEscortRate = 9f;

    [Header("Enrage Settings")]
    public float enrageThreshold = 0.5f; // Máu dưới 50% bật Cuồng Nộ
    private bool isEnraged = false;

    private bool hasEntered = false;
    private int patrolDirection = 1;
    private float nextSpreadFireTime;
    private float nextSummonTime;
    private float nextBarrageTime;
    [Header("Visual Settings")]
    public bool autoFaceDown = true;

    protected override void Awake()
    {
        base.Awake();
        if (autoFaceDown)
        {
            // Tự động xoay 180 độ để mũi tàu chúc thẳng xuống phía dưới hướng về Player
            transform.rotation = Quaternion.Euler(0, 0, 180f);
        }
    }

    protected override void Start()
    {
        // 1. Chỉ số máu khủng và điểm lớn
        maxHealth = 40;
        currentHealth = maxHealth;
        scoreValue = 500;
        contactDamage = 2;

        base.Start();

        // Xóa lệnh tự hủy 15s của quái thường (Boss ở lại cho đến khi bị đánh bại)
        CancelInvoke();

        patrolDirection = (Random.value > 0.5f) ? 1 : -1;
        nextSpreadFireTime = Time.time + 2.5f;
        nextSummonTime = Time.time + 6.0f;
        nextBarrageTime = Time.time + 4.0f;

        // Cập nhật thanh máu Boss trên UI
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.ActivateBossBar(this, "DREADNOUGHT MOTHERSHIP", maxHealth);
        }

        // Tự động nạp Prefab đạn dự phòng nếu chưa kéo
        if (bossLaserPrefab == null)
        {
            bossLaserPrefab = Resources.Load<GameObject>("EnemyLaser");
        }
        if (scoutEscortPrefab == null)
        {
            scoutEscortPrefab = Resources.Load<GameObject>("Enemy_Scout");
        }
    }

    void Update()
    {
        // 1. Tiến vào màn hình đến vị trí chiến đấu
        if (!hasEntered)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetY, 0), enterSpeed * Time.deltaTime);
            if (Mathf.Abs(transform.position.y - targetY) < 0.05f)
            {
                hasEntered = true;
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(0.4f, 0.35f);
                }
            }
            return;
        }

        // 2. Tuần tra qua lại theo chiều ngang
        float currentSpeed = isEnraged ? patrolSpeed * 1.5f : patrolSpeed;
        transform.Translate(Vector3.right * patrolDirection * currentSpeed * Time.deltaTime, Space.World);

        if (transform.position.x >= patrolRange) patrolDirection = -1;
        else if (transform.position.x <= -patrolRange) patrolDirection = 1;

        // 3. Cơ chế tấn công đạn chùm (Spread Fire)
        if (Time.time >= nextSpreadFireTime)
        {
            nextSpreadFireTime = Time.time + (isEnraged ? spreadFireRate * 0.7f : spreadFireRate);
            FireSpreadLasers();
        }

        // 4. Cơ chế triệu hồi tàu hộ tống (Summon Scout)
        if (Time.time >= nextSummonTime)
        {
            nextSummonTime = Time.time + summonEscortRate;
            SummonEscort();
        }

        // 5. Chiêu đặc biệt khi Cuồng Nộ: Bắn xoay vòng 8 hướng (Spiral Barrage)
        if (isEnraged && Time.time >= nextBarrageTime)
        {
            nextBarrageTime = Time.time + 3.8f;
            StartCoroutine(SpiralBarrageRoutine());
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Cập nhật thanh máu UI
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        // Kiểm tra kích hoạt chế độ Cuồng Nộ (Enrage) khi máu <= 50%
        if (!isEnraged && ((float)currentHealth / maxHealth) <= enrageThreshold)
        {
            TriggerEnrageMode();
        }
    }

    void TriggerEnrageMode()
    {
        isEnraged = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnFloatingText("[WARNING] BOSS ENRAGED!", new Vector3(0, 1.8f, 0), Color.red, 7.5f);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.5f, 0.4f);
        }

        // Chuyển màu Boss sang đỏ rực
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            originalColor = spriteRenderer.color;
        }
    }

    // --- CÁC ĐÒN TẤN CÔNG CỦA BOSS ---

    // Đòn 1: Bắn chùm 5 tia rẻ quạt
    void FireSpreadLasers()
    {
        if (bossLaserPrefab == null) return;

        Vector3 centerPos = transform.position + Vector3.down * 0.7f;
        float[] angles = isEnraged ? new float[] { -30f, -15f, 0f, 15f, 30f } : new float[] { -20f, 0f, 20f };

        foreach (float angle in angles)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.down;
            GameObject laser = Instantiate(bossLaserPrefab, centerPos, Quaternion.identity);
            EnemyLaser laserScript = laser.GetComponent<EnemyLaser>();
            if (laserScript != null)
            {
                laserScript.speed = isEnraged ? 8.5f : 7f;
                laserScript.SetDirection(dir);
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaser();
        }
    }

    // Đòn 2: Triệu hồi 1-2 tàu Scout hai bên sườn
    void SummonEscort()
    {
        if (scoutEscortPrefab == null) return;

        Vector3 leftWing = transform.position + new Vector3(-2.2f, -0.4f, 0);
        Vector3 rightWing = transform.position + new Vector3(2.2f, -0.4f, 0);

        Instantiate(scoutEscortPrefab, leftWing, Quaternion.identity);
        if (isEnraged)
        {
            Instantiate(scoutEscortPrefab, rightWing, Quaternion.identity);
        }
    }

    // Đòn 3 (Enrage): Bắn đạn xoay vòng 8 hướng
    IEnumerator SpiralBarrageRoutine()
    {
        if (bossLaserPrefab == null) yield break;

        Vector3 centerPos = transform.position;
        int bulletCount = 8;
        float angleStep = 360f / bulletCount;

        for (int round = 0; round < 2; round++)
        {
            float offsetAngle = round * 22.5f;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = (i * angleStep) + offsetAngle;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.down;

                GameObject laser = Instantiate(bossLaserPrefab, centerPos, Quaternion.identity);
                EnemyLaser laserScript = laser.GetComponent<EnemyLaser>();
                if (laserScript != null)
                {
                    laserScript.speed = 6.5f;
                    laserScript.SetDirection(dir);
                }
            }
            yield return new WaitForSeconds(0.35f);
        }
    }

    // --- KHI BOSS BỊ TIÊU DIỆT ---
    protected override void Die()
    {
        StartCoroutine(BossDefeatSequence());
    }

    IEnumerator BossDefeatSequence()
    {
        // Ẩn thanh máu Boss
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.HideBossBar();
        }

        // Tạm dừng di chuyển và tấn công
        enabled = false;

        // Làm chậm thời gian (Slow-motion chiến thắng)
        Time.timeScale = 0.35f;

        // Chuỗi nổ liên hoàn 6 vụ nổ quanh thân tàu
        for (int i = 0; i < 6; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1.8f, 1.8f), Random.Range(-1.2f, 1.2f), 0);
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position + randomOffset, Quaternion.identity);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.2f, 0.3f);
            }

            yield return new WaitForSecondsRealtime(0.18f);
        }

        // Khôi phục thời gian bình thường
        Time.timeScale = 1f;

        // Cộng điểm lớn
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue, transform.position);
            GameManager.Instance.SpawnFloatingText("[VICTORY] BOSS DEFEATED! +500", new Vector3(0, 1.5f, 0), Color.green, 8f);
        }

        // Rơi ra nhiều phần thưởng đặc biệt
        SpawnBossLoot();

        // Báo cho Spawner tiếp tục nhịp game
        Spawner spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null)
        {
            spawner.OnBossDefeated();
        }

        Destroy(gameObject);
    }

    void SpawnBossLoot()
    {
        if (ItemDropManager.Instance == null) return;

        // Rơi Tim, Khiên và Nâng cấp vũ khí tỏa ra 3 hướng
        if (ItemDropManager.Instance.healthItemPrefab != null)
            Instantiate(ItemDropManager.Instance.healthItemPrefab, transform.position + Vector3.left * 1.5f, Quaternion.identity);

        if (ItemDropManager.Instance.shieldItemPrefab != null)
            Instantiate(ItemDropManager.Instance.shieldItemPrefab, transform.position, Quaternion.identity);

        if (ItemDropManager.Instance.weaponUpgradePrefab != null)
            Instantiate(ItemDropManager.Instance.weaponUpgradePrefab, transform.position + Vector3.right * 1.5f, Quaternion.identity);
    }
}
