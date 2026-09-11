using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject asteroidPrefab;
    public GameObject starPrefab;

    [Header("Dynamic Spawn Settings")]
    public float initialSpawnInterval = 1.0f; // Thời gian chờ lúc mới vào game (1s)
    public float minSpawnInterval = 0.35f;    // Ngưỡng sinh nhanh nhất (không rơi nhanh hơn 0.35s)
    public float intervalDecreaseRate = 0.02f; // Mỗi lần sinh giảm bớt 0.02s

    [Header("Dynamic Speed Settings")]
    public float speedIncreasePerSecond = 0.015f; // Tốc độ tăng 1.5% mỗi giây sống sót
    public float maxSpeedMultiplier = 2.2f;       // Tối đa bay nhanh gấp 2.2 lần ban đầu

    [Header("Spawn Position Boundaries")]
    public float minX = -7.5f;
    public float maxX = 7.5f;
    public float spawnY = 6f;

    private float currentSpawnInterval;
    private float elapsedTime = 0f;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        // Đếm thời gian sống sót của ván chơi
        elapsedTime += Time.deltaTime;
    }

    IEnumerator SpawnRoutine()
    {
        // Chờ 0.5s lúc mới load màn
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            SpawnObject();

            // Giảm dần thời gian chờ (sinh càng lúc càng dồn dập)
            if (currentSpawnInterval > minSpawnInterval)
            {
                currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - intervalDecreaseRate);
            }

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnObject()
    {
        Vector2 spawnPos = new Vector2(Random.Range(minX, maxX), spawnY);

        // 75% sinh Thiên thạch, 25% sinh Ngôi sao
        if (Random.value < 0.75f)
        {
            if (asteroidPrefab != null)
            {
                GameObject newAsteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);

                // Tính hệ số tăng tốc theo thời gian
                float currentMultiplier = Mathf.Min(1f + (elapsedTime * speedIncreasePerSecond), maxSpeedMultiplier);

                Asteroid asteroidScript = newAsteroid.GetComponent<Asteroid>();
                if (asteroidScript != null)
                {
                    asteroidScript.SetSpeedMultiplier(currentMultiplier);
                }
            }
        }
        else
        {
            Vector2 starPos = new Vector2(Random.Range(minX, maxX), Random.Range(-3.5f, 4f));
            if (starPrefab != null)
            {
                Instantiate(starPrefab, starPos, Quaternion.identity);
            }
        }
    }
}