using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject instructionsPanel;
    public GameObject settingsPanel;

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

    // Mở bảng cài đặt
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Đóng bảng cài đặt
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}