using System.Collections;
using UnityEngine;

public enum GamePhase
{
    WarmUp,         // 0s - 15s: Làm quen, né thiên thạch chậm, ăn sao
    ScoutRaids,     // 15s - 45s: Tàu Scout xuất hiện uốn lượn, bắn đạn đơn
    Crossfire,      // 45s - 80s: Tàu Sniper xuất hiện ở trên cao, áp lực tăng dần
    HeavyAssault,   // 80s - 130s: Bão thiên thạch + Đội tàu hỗn hợp
    EndlessStorm    // 130s+: Tốc độ tối đa, quái ra dồn dập
}

public class Spawner : MonoBehaviour
{
    [Header("Core Prefabs")]
    public GameObject asteroidPrefab;
    public GameObject starPrefab;

    [Header("Enemy Prefabs")]
    public GameObject enemyScoutPrefab;
    public GameObject enemySniperPrefab;

    [Header("Spawn Position Boundaries")]
    public float minX = -7.5f;
    public float maxX = 7.5f;
    public float spawnY = 6f;

    [Header("Progress Settings")]
    public float currentSpeedMultiplier = 1f;
    public float maxSpeedMultiplier = 2.1f;

    private float elapsedTime = 0f;
    private GamePhase currentPhase = GamePhase.WarmUp;
    private float nextSupplyDropTime = 30f;
    private float nextMeteorShowerTime = 90f;

    void Awake()
    {
        // Tự động nạp Prefabs dự phòng nếu chưa kéo thả
        if (asteroidPrefab == null) asteroidPrefab = Resources.Load<GameObject>("Asteroid");
        if (starPrefab == null) starPrefab = Resources.Load<GameObject>("Star");
        if (enemyScoutPrefab == null) enemyScoutPrefab = Resources.Load<GameObject>("Enemy_Scout");
        if (enemySniperPrefab == null) enemySniperPrefab = Resources.Load<GameObject>("Enemy_Sniper");
    }

    void Start()
    {
        StartCoroutine(SpawnDirectorRoutine());
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Cập nhật hệ số tốc độ tăng dần theo thời gian (tăng 0.8% mỗi giây sống sót)
        currentSpeedMultiplier = Mathf.Min(1f + (elapsedTime * 0.008f), maxSpeedMultiplier);

        // Kiểm tra chuyển Giai đoạn (Phase Transition)
        UpdatePhase();

        // Chu kỳ thả Hòm Tiếp Tế Hỗ Trợ (Supply Drop) mỗi 35 giây
        if (elapsedTime >= nextSupplyDropTime)
        {
            nextSupplyDropTime = elapsedTime + 35f;
            SpawnSupplyDrop();
        }
    }

    void UpdatePhase()
    {
        if (elapsedTime < 15f)
        {
            if (currentPhase != GamePhase.WarmUp)
            {
                currentPhase = GamePhase.WarmUp;
            }
        }
        else if (elapsedTime < 45f)
        {
            if (currentPhase != GamePhase.ScoutRaids)
            {
                currentPhase = GamePhase.ScoutRaids;
                AnnounceWarning("PHASE 2: ENEMY SCOUTS DETECTED!", Color.yellow);
            }
        }
        else if (elapsedTime < 80f)
        {
            if (currentPhase != GamePhase.Crossfire)
            {
                currentPhase = GamePhase.Crossfire;
                AnnounceWarning("PHASE 3: SNIPER SHIPS INCOMING!", new Color(1f, 0.4f, 0f));
            }
        }
        else if (elapsedTime < 130f)
        {
            if (currentPhase != GamePhase.HeavyAssault)
            {
                currentPhase = GamePhase.HeavyAssault;
                AnnounceWarning("PHASE 4: METEOR SHOWER & HEAVY FLEET!", Color.red);
            }
        }
        else
        {
            if (currentPhase != GamePhase.EndlessStorm)
            {
                currentPhase = GamePhase.EndlessStorm;
                AnnounceWarning("SURVIVAL MODE: ENDLESS STORM!", Color.magenta);
            }
        }
    }

    // Hiển thị chữ thông báo đợt trên màn hình
    void AnnounceWarning(string message, Color color)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnFloatingText(message, new Vector3(0, 2f, 0), color, 6.5f);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.2f);
        }
    }

    // Luồng chính điều phối nhịp điệu Spawn
    IEnumerator SpawnDirectorRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        while (true)
        {
            float delay = 1.2f;

            switch (currentPhase)
            {
                case GamePhase.WarmUp:
                    // Giai đoạn 1: 70% Thiên thạch chậm, 30% Sao
                    delay = Random.Range(1.0f, 1.3f);
                    if (Random.value < 0.70f) SpawnSingleAsteroid();
                    else SpawnSingleStar();
                    break;

                case GamePhase.ScoutRaids:
                    // Giai đoạn 2: 60% Thiên thạch, 25% Scout, 15% Sao
                    delay = Random.Range(0.85f, 1.15f);
                    float roll2 = Random.value;
                    if (roll2 < 0.25f && enemyScoutPrefab != null)
                    {
                        // Thỉnh thoảng xuất hiện cặp đôi Scout 2 cánh
                        if (Random.value < 0.35f) SpawnScoutPair();
                        else SpawnSingleScout();
                    }
                    else if (roll2 < 0.85f)
                    {
                        SpawnSingleAsteroid();
                    }
                    else
                    {
                        SpawnSingleStar();
                    }
                    break;

                case GamePhase.Crossfire:
                    // Giai đoạn 3: Sniper chiếm lĩnh trên cao, Scout quấy rối
                    delay = Random.Range(0.65f, 0.95f);
                    float roll3 = Random.value;
                    if (roll3 < 0.20f && enemySniperPrefab != null)
                    {
                        SpawnSingleSniper();
                    }
                    else if (roll3 < 0.45f && enemyScoutPrefab != null)
                    {
                        SpawnSingleScout();
                    }
                    else if (roll3 < 0.90f)
                    {
                        SpawnSingleAsteroid();
                    }
                    else
                    {
                        SpawnSingleStar();
                    }
                    break;

                case GamePhase.HeavyAssault:
                    // Giai đoạn 4: Áp lực dày đặc + Kích hoạt Bão thiên thạch định kỳ
                    delay = Random.Range(0.5f, 0.75f);
                    
                    if (elapsedTime >= nextMeteorShowerTime)
                    {
                        nextMeteorShowerTime = elapsedTime + 40f;
                        yield return StartCoroutine(MeteorShowerRoutine());
                    }
                    else
                    {
                        float roll4 = Random.value;
                        if (roll4 < 0.22f && enemySniperPrefab != null) SpawnSingleSniper();
                        else if (roll4 < 0.48f && enemyScoutPrefab != null) SpawnSingleScout();
                        else SpawnSingleAsteroid();
                    }
                    break;

                case GamePhase.EndlessStorm:
                    // Giai đoạn 5: Nhịp độ nhanh nhất, quái và đá rơi dồn dập
                    delay = Random.Range(0.38f, 0.58f);
                    float roll5 = Random.value;
                    if (roll5 < 0.25f && enemySniperPrefab != null) SpawnSingleSniper();
                    else if (roll5 < 0.55f && enemyScoutPrefab != null) SpawnSingleScout();
                    else SpawnSingleAsteroid();
                    break;
            }

            yield return new WaitForSeconds(delay);
        }
    }

    // --- CÁC HÀM SINH ĐỐI TƯỢNG ĐƠN LẺ ---

    void SpawnSingleAsteroid()
    {
        if (asteroidPrefab == null) return;
        Vector2 pos = new Vector2(Random.Range(minX, maxX), spawnY);
        GameObject ast = Instantiate(asteroidPrefab, pos, Quaternion.identity);

        Asteroid aScript = ast.GetComponent<Asteroid>();
        if (aScript != null)
        {
            aScript.SetSpeedMultiplier(currentSpeedMultiplier);
        }
    }

    void SpawnSingleStar()
    {
        if (starPrefab == null) return;
        Vector2 pos = new Vector2(Random.Range(minX, maxX), Random.Range(-3f, 4f));
        Instantiate(starPrefab, pos, Quaternion.identity);
    }

    void SpawnSingleScout()
    {
        if (enemyScoutPrefab == null) return;
        Vector2 pos = new Vector2(Random.Range(minX * 0.75f, maxX * 0.75f), spawnY);
        Instantiate(enemyScoutPrefab, pos, Quaternion.identity);
    }

    void SpawnSingleSniper()
    {
        if (enemySniperPrefab == null) return;
        // Chỉ sinh nếu trên màn hình chưa có quá 2 con Sniper để tránh nghẽn
        EnemySniper[] snipers = FindObjectsByType<EnemySniper>(FindObjectsSortMode.None);
        if (snipers.Length >= 2) return;

        Vector2 pos = new Vector2(Random.Range(minX * 0.7f, maxX * 0.7f), spawnY);
        Instantiate(enemySniperPrefab, pos, Quaternion.identity);
    }

    // --- CÁC ĐỘI HÌNH ĐẶC BIỆT (FORMATIONS) ---

    // Cặp đôi Scout xuất hiện từ 2 bên mép cánh
    void SpawnScoutPair()
    {
        if (enemyScoutPrefab == null) return;
        Vector2 leftPos = new Vector2(minX * 0.8f, spawnY);
        Vector2 rightPos = new Vector2(maxX * 0.8f, spawnY);

        Instantiate(enemyScoutPrefab, leftPos, Quaternion.identity);
        Instantiate(enemyScoutPrefab, rightPos, Quaternion.identity);
    }

    // Bão thiên thạch rơi liên tiếp theo đợt
    IEnumerator MeteorShowerRoutine()
    {
        AnnounceWarning("METEOR SHOWER INCOMING!", Color.red);
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < 5; i++)
        {
            SpawnSingleAsteroid();
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Thùng tiếp tế không gian trôi từ từ giữa màn hình
    void SpawnSupplyDrop()
    {
        if (ItemDropManager.Instance == null) return;

        GameObject dropPrefab = ItemDropManager.Instance.GetSmartDrop();
        if (dropPrefab != null)
        {
            Vector2 supplyPos = new Vector2(Random.Range(-3f, 3f), spawnY);
            Instantiate(dropPrefab, supplyPos, Quaternion.identity);
            AnnounceWarning("SUPPLY DROP INBOUND!", Color.green);
        }
    }
}