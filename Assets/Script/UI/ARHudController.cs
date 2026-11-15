using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARHudController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject quickManualModal;
    public GameObject reticleHUD;

    [Header("Buttons")]
    public Button backToMenuButton;
    public Button openManualButton;
    public Button closeManualButton;

    private void Start()
    {
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
        }
        if (openManualButton != null)
        {
            openManualButton.onClick.RemoveAllListeners();
            openManualButton.onClick.AddListener(OnClickOpenManual);
        }
        if (closeManualButton != null)
        {
            closeManualButton.onClick.RemoveAllListeners();
            closeManualButton.onClick.AddListener(OnClickCloseManual);
        }

        if (quickManualModal != null)
        {
            quickManualModal.SetActive(false);
        }
        if (reticleHUD != null)
        {
            reticleHUD.SetActive(true);
        }
    }

    public void OnClickBackToMenu()
    {
        MainMenu.skipSplashOnNextLoad = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickOpenManual()
    {
        if (quickManualModal != null)
        {
            quickManualModal.SetActive(true);
        }
    }

    public void OnClickCloseManual()
    {
        if (quickManualModal != null)
        {
            quickManualModal.SetActive(false);
        }
    }
}
