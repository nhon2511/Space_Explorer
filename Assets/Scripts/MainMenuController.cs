using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject instructionsPanel;

    // Chuyển sang scene GamePlay
    public void PlayGame()
    {
        SceneManager.LoadScene("GamePlay");
    }

    // Mở bảng hướng dẫn
    public void OpenInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }

    // Đóng bảng hướng dẫn
    public void CloseInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }
}