using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI scoreText;
    private int score = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateScoreUI();
    }

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

    public void GameOver()
    {
        // 1. Lưu điểm vòng chơi hiện tại
        PlayerPrefs.SetInt("FinalScore", score);

        // 2. Đọc kỷ lục cũ (mặc định là 0 nếu chơi lần đầu)
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        // 3. Nếu điểm mới cao hơn, cập nhật kỷ lục mới
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetInt("IsNewRecord", 1); // Cờ đánh dấu vừa phá kỷ lục
        }
        else
        {
            PlayerPrefs.SetInt("IsNewRecord", 0);
        }

        PlayerPrefs.Save();

        // 4. Chuyển sang màn hình kết thúc
        SceneManager.LoadScene("GameEnd");
    }
}