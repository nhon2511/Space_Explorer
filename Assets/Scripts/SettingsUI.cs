using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI Controls")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Panel References")]
    public GameObject settingsPanel;
    [Tooltip("Menu trước đó cần ẩn khi mở Settings (Ví dụ: PauseMenuPanel hoặc MainButtons)")]
    public GameObject previousMenuPanel;

    void Start()
    {
        // 1. Lấy giá trị âm lượng hiện tại từ AudioManager hoặc PlayerPrefs
        float bgmVal = (AudioManager.Instance != null) ? AudioManager.Instance.GetBGMVolume() : PlayerPrefs.GetFloat("BGM_Volume", 0.5f);
        float sfxVal = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : PlayerPrefs.GetFloat("SFX_Volume", 0.8f);

        // 2. Gán vào Sliders
        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVal;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVal;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }
    }

    public void OnBGMChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(val);
        }
    }

    public void OnSFXChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(val);
        }
    }

    // Các hàm mở/đóng bảng Settings
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        else
            gameObject.SetActive(true);

        // Tạm ẩn menu trước đó để không bị đè chữ
        if (previousMenuPanel != null)
        {
            previousMenuPanel.SetActive(false);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        else
            gameObject.SetActive(false);

        // Hiện lại menu trước đó
        if (previousMenuPanel != null)
        {
            previousMenuPanel.SetActive(true);
        }
    }
}
