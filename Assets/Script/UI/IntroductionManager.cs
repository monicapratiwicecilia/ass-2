using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroductionManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject splashScreenPanel;
    public GameObject mainMenuPanel;

    [Header("Modals (Matching mobile.html)")]
    public GameObject caseListModal;
    public GameObject probeGuideModal;
    public GameObject manualBookModal;
    public GameObject aboutDeveloperModal;
    public GameObject quitConfirmModal;

    [Header("Top Status Bar")]
    public TextMeshProUGUI clockText;

    [Header("Active Case Display")]
    public TextMeshProUGUI activeCaseLabel;
    public int activeCaseIndex = 1; // 1 to 7

    [Header("Splash Screen Elements")]
    public Image splashLogoImage;
    public TextMeshProUGUI splashSubtitleText;
    public Slider splashProgressBar;
    public TextMeshProUGUI splashProgressText;
    public Sprite publisherLogo;        // Telkom University
    public Sprite developerLogo;        // Tim A.U.T.O.P.S.Y.
    public Sprite clientPartnerLogo;     // Fakultas Kedokteran Telkom University
    public float splashDurationPerLogo = 2.0f;
    public Button skipSplashButton;

    [Header("Audio")]
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;

    private Coroutine splashCoroutine;
    private bool splashSkipped = false;

    private readonly string[] caseTitles = new string[]
    {
        "[CS-01] BATHROOM AC // 20°C",
        "[CS-02] WAREHOUSE HEAT // 33°C",
        "[CS-03] OPEN ORCHARD // 24°C",
        "[CS-04] FRESHWATER SUBMERSION // 22°C",
        "[CS-05] SALTWATER SUBMERSION // 26°C",
        "[CS-06] MECHANICAL ASPHYXIA // 18°C",
        "[CS-07] METHANOL TOXICITY // 24°C"
    };

    private void Start()
    {
        // Load active case
        activeCaseIndex = PlayerPrefs.GetInt("PMI_ACTIVE_CASE", 1);
        UpdateActiveCaseDisplay();

        // Audio setup
        float bgm = PlayerPrefs.GetFloat("PMI_BGM_VOL", 0.75f);
        if (bgmAudioSource != null)
        {
            if (bgmAudioSource.clip == null)
            {
                bgmAudioSource.clip = UISoundGenerator.CreateAmbientHumClip();
            }
            bgmAudioSource.volume = bgm;
            bgmAudioSource.loop = true;
            if (!bgmAudioSource.isPlaying) bgmAudioSource.Play();
        }

        if (sfxAudioSource != null && sfxAudioSource.clip == null)
        {
            sfxAudioSource.clip = UISoundGenerator.CreateClickClip();
        }

        CloseAllModals();

        // Start clock
        InvokeRepeating(nameof(UpdateClock), 0f, 1f);

        // Splash screen check
        if (splashScreenPanel != null)
        {
            splashScreenPanel.SetActive(true);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            splashCoroutine = StartCoroutine(PlaySplashScreenSequence());
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void UpdateClock()
    {
        if (clockText != null)
        {
            clockText.text = System.DateTime.Now.ToString("HH:mm");
        }
    }

    private IEnumerator PlaySplashScreenSequence()
    {
        var logos = new (Sprite sprite, string title)[]
        {
            (publisherLogo, "PUBLISHER\nTELKOM UNIVERSITY"),
            (developerLogo, "DEVELOPER\nTIM A.U.T.O.P.S.Y."),
            (clientPartnerLogo, "CLIENT & PARTNER\nFAKULTAS KEDOKTERAN TELKOM UNIVERSITY")
        };

        float totalDuration = logos.Length * splashDurationPerLogo;
        float elapsed = 0f;

        for (int i = 0; i < logos.Length; i++)
        {
            if (splashSkipped) break;

            if (splashLogoImage != null && logos[i].sprite != null)
            {
                splashLogoImage.sprite = logos[i].sprite;
            }
            if (splashSubtitleText != null)
            {
                splashSubtitleText.text = logos[i].title;
            }

            float logoElapsed = 0f;
            while (logoElapsed < splashDurationPerLogo && !splashSkipped)
            {
                logoElapsed += Time.deltaTime;
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / totalDuration);
                if (splashProgressBar != null) splashProgressBar.value = progress;
                if (splashProgressText != null) splashProgressText.text = $"MEMUAT PROTOKOL FORENSIK... {Mathf.RoundToInt(progress * 100)}%";

                yield return null;
            }
        }

        ShowMainMenu();
    }

    public void SkipSplashScreen()
    {
        splashSkipped = true;
        if (splashCoroutine != null) StopCoroutine(splashCoroutine);
        PlayClickSound();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (splashScreenPanel != null) splashScreenPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        CloseAllModals();
    }

    public void CloseAllModals()
    {
        if (caseListModal != null) caseListModal.SetActive(false);
        if (probeGuideModal != null) probeGuideModal.SetActive(false);
        if (manualBookModal != null) manualBookModal.SetActive(false);
        if (aboutDeveloperModal != null) aboutDeveloperModal.SetActive(false);
        if (quitConfirmModal != null) quitConfirmModal.SetActive(false);
    }

    // --- Action Card Selection ---

    public void SelectCase(int caseIndex)
    {
        activeCaseIndex = Mathf.Clamp(caseIndex, 1, 7);
        PlayerPrefs.SetInt("PMI_ACTIVE_CASE", activeCaseIndex);
        PlayerPrefs.Save();
        UpdateActiveCaseDisplay();
        PlayClickSound();
        CloseAllModals();
        Debug.Log($"[PMI Mobile] Kasus aktif diubah menjadi: {activeCaseIndex}");
    }

    public void UpdateActiveCaseDisplay()
    {
        if (activeCaseLabel != null)
        {
            int idx = activeCaseIndex - 1;
            if (idx >= 0 && idx < caseTitles.Length)
            {
                activeCaseLabel.text = caseTitles[idx];
            }
        }
    }

    // --- Modal Openers (Matching mobile.html) ---

    public void OnClickOpenARProbe()
    {
        PlayClickSound();
        SceneManager.LoadScene("ASS 2");
    }

    public void OnClickOpenCaseList()
    {
        PlayClickSound();
        CloseAllModals();
        if (caseListModal != null) caseListModal.SetActive(true);
    }

    public void OnClickOpenProbeGuide()
    {
        PlayClickSound();
        CloseAllModals();
        if (probeGuideModal != null) probeGuideModal.SetActive(true);
    }

    public void OnClickOpenManualBook()
    {
        PlayClickSound();
        CloseAllModals();
        if (manualBookModal != null) manualBookModal.SetActive(true);
    }

    public void OnClickOpenAboutDeveloper()
    {
        PlayClickSound();
        CloseAllModals();
        if (aboutDeveloperModal != null) aboutDeveloperModal.SetActive(true);
    }

    public void OnClickOpenQuitConfirm()
    {
        PlayClickSound();
        CloseAllModals();
        if (quitConfirmModal != null) quitConfirmModal.SetActive(true);
    }

    public void OnClickCloseModal()
    {
        PlayClickSound();
        CloseAllModals();
    }

    public void OnClickConfirmQuit()
    {
        PlayClickSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PlayClickSound()
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.Play();
        }
    }
}
