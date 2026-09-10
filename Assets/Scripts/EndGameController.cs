using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGameController : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI finalScoreText;

    void Start()
    {
        // Lấy điểm số cuối cùng được lưu từ scene GamePlay qua PlayerPrefs
        int score = PlayerPrefs.GetInt("FinalScore", 0);

        if (finalScoreText != null)
        {
            finalScoreText.text = "YOUR SCORE: " + score;
        }
    }

    // Nút quay lại màn hình Menu chính
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Nút thoát game
    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}