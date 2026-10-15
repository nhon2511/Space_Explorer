using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score Settings")]
    public TextMeshProUGUI scoreText;
    private int score = 0;

    [Header("Combo & Multiplier Settings")]
    public TextMeshProUGUI comboText;
    public float comboDuration = 2.5f; // Thời gian giữ Combo
    private int comboCount = 0;
    private float comboMultiplier = 1.0f;
    private float comboTimer = 0f;

    [Header("Lives Settings")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Lives UI (Hình Trái Tim)")]
    public Image[] heartImages; // Mảng 3 hình trái tim ban đầu
    private System.Collections.Generic.List<Image> activeHeartList = new System.Collections.Generic.List<Image>();

    [Header("Floating Text VFX")]
    public GameObject floatingTextPrefab;

    [Header("Pause Menu Settings")]
    [Tooltip("Kéo PausePanel từ Canvas vào đây")]
    public GameObject pauseMenuPanel;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Tự động nạp FloatingText nếu chưa kéo thả trên Inspector
        if (floatingTextPrefab == null)
        {
            floatingTextPrefab = Resources.Load<GameObject>("FloatingText");
        }
    }

    void Start()
    {
        // Luôn đảm bảo thời gian chạy bình thường khi bắt đầu ván
        Time.timeScale = 1f;

        // Nạp danh sách trái tim ban đầu
        activeHeartList.Clear();
        if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null) activeHeartList.Add(heartImages[i]);
            }
        }

        currentLives = maxLives;
        UpdateScoreUI();
        UpdateLivesUI();
        UpdateComboUI();

        // Mặc định ẩn bảng Pause khi vừa vào game
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Đếm ngược Combo
        if (comboCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        // 2. Nhấn phím Escape hoặc phím P để Bật/Tắt Pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- LOGIC PAUSE & RESUME ---
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Đóng băng toàn bộ game

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Khôi phục game chạy lại bình thường

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    // Gắn vào nút Restart (Chơi lại ngay ván mới)
    public void RestartGame()
    {
        Time.timeScale = 1f; // Phải khôi phục thời gian trước khi reload scene!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Gắn vào nút Main Menu (Quay về màn hình chính)
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    public int GetCurrentLives() => currentLives;
    public int GetMaxLives() => maxLives;

    // --- CỘNG / TRỪ ĐIỂM SỐ & COMBO ---
    public void AddScore(int amount, Vector3? worldPosition = null)
    {
        // 1. Tăng Combo khi ăn điểm liên tiếp
        comboCount++;
        comboTimer = comboDuration;

        // Tính toán Multiplier theo bậc combo
        if (comboCount >= 10) comboMultiplier = 3.0f;
        else if (comboCount >= 6) comboMultiplier = 2.0f;
        else if (comboCount >= 3) comboMultiplier = 1.5f;
        else comboMultiplier = 1.0f;

        int finalPoints = Mathf.RoundToInt(amount * comboMultiplier);
        score += finalPoints;

        UpdateScoreUI();
        UpdateComboUI();

        // 2. Tạo hiệu ứng chữ điểm nổi (Floating Text)
        if (worldPosition.HasValue)
        {
            string displayText = comboMultiplier > 1f ? $"+{finalPoints} (x{comboMultiplier})" : $"+{finalPoints}";
            Color textColor = (comboMultiplier > 1f) ? Color.yellow : Color.green;
            SpawnFloatingText(displayText, worldPosition.Value, textColor);
        }
    }

    public void DeductScore(int amount, Vector3? worldPosition = null)
    {
        score = Mathf.Max(0, score - amount);
        UpdateScoreUI();

        // Va chạm bị mất đòn sẽ gãy chuỗi Combo
        ResetCombo();

        if (worldPosition.HasValue)
        {
            SpawnFloatingText($"-{amount}", worldPosition.Value, Color.red);
        }
    }

    private void ResetCombo()
    {
        comboCount = 0;
        comboMultiplier = 1.0f;
        comboTimer = 0f;
        UpdateComboUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void UpdateComboUI()
    {
        if (comboText == null) return;

        if (comboCount >= 3)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"COMBO x{comboMultiplier:0.0}!";
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    // Sinh chữ bay trên màn hình
    public void SpawnFloatingText(string text, Vector3 position, Color color, float fontSize = 5f)
    {
        if (floatingTextPrefab == null) return;

        GameObject ftObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        FloatingText ft = ftObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, color, fontSize);
        }
    }

    // --- QUẢN LÝ MÁU / MẠNG SỐNG ---
    public bool TakeDamage(int damage)
    {
        currentLives = Mathf.Max(0, currentLives - damage);
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            GameOver();
            return true;
        }

        return false;
    }

    public void Heal(int amount, Vector3? worldPosition = null)
    {
        currentLives = Mathf.Min(maxLives, currentLives + amount);
        UpdateLivesUI();

        if (worldPosition.HasValue)
        {
            SpawnFloatingText("+1 HP", worldPosition.Value, Color.cyan);
        }
    }

    public void IncreaseMaxLives(int amount)
    {
        maxLives += amount;

        // Sinh thêm hình trái tim mới vào hàng HeartsGroup
        if (activeHeartList != null && activeHeartList.Count > 0)
        {
            Image template = activeHeartList[0];
            if (template != null && template.transform.parent != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject newHeartObj = Instantiate(template.gameObject, template.transform.parent);
                    newHeartObj.name = $"Heart_{activeHeartList.Count + 1}";
                    Image newHeartImg = newHeartObj.GetComponent<Image>();
                    activeHeartList.Add(newHeartImg);
                }
            }
        }

        Heal(amount);

        if (PlayerController.Instance != null)
        {
            SpawnFloatingText("[+1 MAX HP!]", PlayerController.Instance.transform.position, Color.green, 6.5f);
        }
    }

    void UpdateLivesUI()
    {
        if (activeHeartList != null && activeHeartList.Count > 0)
        {
            for (int i = 0; i < activeHeartList.Count; i++)
            {
                if (activeHeartList[i] != null)
                {
                    activeHeartList[i].gameObject.SetActive(i < currentLives);
                }
            }
        }
        else if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].gameObject.SetActive(i < currentLives);
                }
            }
        }
    }

    // --- KẾT THÚC VÁN CHƠI ---
    public void GameOver()
    {
        Time.timeScale = 1f; // Khôi phục thời gian

        PlayerPrefs.SetInt("FinalScore", score);

        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetInt("IsNewRecord", 1);
        }
        else
        {
            PlayerPrefs.SetInt("IsNewRecord", 0);
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("GameEnd");
    }
}