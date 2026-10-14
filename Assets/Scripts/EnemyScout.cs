using UnityEngine;

public class EnemyScout : EnemyBase
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float waveFrequency = 3f;  // Tần số uốn lượn Sin
    public float waveAmplitude = 1.2f; // Biên độ uốn lượn

    [Header("Weapon")]
    public GameObject enemyLaserPrefab;
    public Transform firePoint;
    public float fireRate = 2.2f;

    private float nextFireTime;
    private float startX;
    private float timeAlive;

    protected override void Start()
    {
        base.Start();
        startX = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.8f, fireRate);
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        // 1. Di chuyển lượn sóng Sin từ trên xuống
        float newX = startX + Mathf.Sin(timeAlive * waveFrequency) * waveAmplitude;
        float newY = transform.position.y - (moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, newY, transform.position.z);

        // 2. Bắn đạn thẳng xuống
        if (enemyLaserPrefab != null && Time.time >= nextFireTime && transform.position.y > -2.5f)
        {
            nextFireTime = Time.time + fireRate;
            ShootLaser();
        }
    }

    void ShootLaser()
    {
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + Vector3.down * 0.4f;
        GameObject laser = Instantiate(enemyLaserPrefab, spawnPos, Quaternion.identity);
        EnemyLaser laserScript = laser.GetComponent<EnemyLaser>();
        if (laserScript != null)
        {
            laserScript.SetDirection(Vector2.down);
        }
    }
}
