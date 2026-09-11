using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        // 1. Lấy điểm số từ PlayerPrefs
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // 2. Gán lên giao diện
        if (finalScoreText != null)
        {
            finalScoreText.text = "YOUR SCORE: " + finalScore;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore;
        }
    }

    // Gắn vào nút MainMenuButton
    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Gắn vào nút QuitButton
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}