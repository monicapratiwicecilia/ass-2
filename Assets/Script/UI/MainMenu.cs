using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Splash Screen")]
    public GameObject splashScreenPanel;

    [Header("Panel UI")]
    public GameObject panelMenu;
    public GameObject panelManual;
    public GameObject panelAbout;

    [Header("Konfigurasi Scene")]
    public string gameScene = "ASS 2";

    // Static flag: jika bernilai true (misal saat kembali dari scene AR ASS 2), splash screen akan dilewati dan langsung ke Menu Utama
    public static bool skipSplashOnNextLoad = false;

    private void Start()
    {
        EnsureCenterContainerActive();

        if (skipSplashOnNextLoad)
        {
            skipSplashOnNextLoad = false;
            if (splashScreenPanel != null) splashScreenPanel.SetActive(false);
            ShowMenu();
        }
        else if (splashScreenPanel != null)
        {
            splashScreenPanel.SetActive(true);
            SetActiveSafe(panelMenu, false);
            SetActiveSafe(panelManual, false);
            SetActiveSafe(panelAbout, false);
        }
        else
        {
            ShowMenu();
        }
    }

    private void EnsureCenterContainerActive()
    {
        if (panelMenu != null)
        {
            var cc = panelMenu.transform.Find("CenterContainer");
            if (cc != null && !cc.gameObject.activeSelf)
            {
                cc.gameObject.SetActive(true);
            }
        }
    }

    public void ShowMenu()
    {
        if (splashScreenPanel != null) splashScreenPanel.SetActive(false);
        EnsureCenterContainerActive();
        SetActiveSafe(panelMenu, true);
        SetActiveSafe(panelManual, false);
        SetActiveSafe(panelAbout, false);
    }

    public void ShowManual()
    {
        if (splashScreenPanel != null) splashScreenPanel.SetActive(false);
        SetActiveSafe(panelMenu, false);
        SetActiveSafe(panelManual, true);
        SetActiveSafe(panelAbout, false);

        if (panelManual != null)
        {
            var mb = panelManual.GetComponentInChildren<ManualBook>(true);
            if (mb != null)
            {
                mb.ShowPage(0);
            }
        }
    }

    public void ShowAbout()
    {
        if (splashScreenPanel != null) splashScreenPanel.SetActive(false);
        SetActiveSafe(panelMenu, false);
        SetActiveSafe(panelManual, false);
        SetActiveSafe(panelAbout, true);
    }

    public void Play()
    {
        if (!string.IsNullOrEmpty(gameScene))
        {
            SceneManager.LoadScene(gameScene);
        }
        else
        {
            Debug.LogWarning("[MainMenu] Nama gameScene kosong! Pastikan diisi 'ASS 2'.");
        }
    }

    public void Quit()
    {
        Debug.Log("[MainMenu] Menutup aplikasi...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }
}
