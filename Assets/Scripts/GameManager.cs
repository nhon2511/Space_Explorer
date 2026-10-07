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

    [Header("Lives Settings")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Lives UI (Hình Trái Tim)")]
    public Image[] heartImages; // Mảng 3 hình trái tim

    [Header("Pause Menu Settings")]
    [Tooltip("Kéo PausePanel từ Canvas vào đây")]
    public GameObject pauseMenuPanel;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Luôn đảm bảo thời gian chạy bình thường khi bắt đầu ván
        Time.timeScale = 1f;

        currentLives = maxLives;
        UpdateScoreUI();
        UpdateLivesUI();

        // Mặc định ẩn bảng Pause khi vừa vào game
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Nhấn phím Escape hoặc phím P để Bật/Tắt Pause
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

    // --- CỘNG / TRỪ ĐIỂM SỐ ---
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void DeductScore(int amount)
    {
        score = Mathf.Max(0, score - amount);
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
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

    public void Heal(int amount)
    {
        currentLives = Mathf.Min(maxLives, currentLives + amount);
        UpdateLivesUI();
    }

    void UpdateLivesUI()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].gameObject.SetActive(i < currentLives);
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