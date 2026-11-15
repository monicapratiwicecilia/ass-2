using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TacticalAppController : MonoBehaviour
{
    public static TacticalAppController Instance { get; private set; }

    [Header("Main Screen Views")]
    public GameObject splashScreenView;
    public GameObject mainMenuView;
    public GameObject arProbeView;

    [Header("Modals (WBS A-2 + mobile.html)")]
    public GameObject modalKasus;
    public GameObject modalPanduanProbe;
    public GameObject modalBukuPanduan;
    public GameObject modalPengaturan;   // Option/Setting (+1 WBS)
    public GameObject modalTentang;      // About Developer (+1 WBS)
    public GameObject modalKeluar;       // Quit Application (+1 WBS)
    public GameObject modalKoneksi;

    [Header("Status Bar & Identity")]
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI menuActiveCaseLabel;
    public TextMeshProUGUI probeActiveCaseLabel;
    public TextMeshProUGUI probeRoomLabel;

    [Header("Active Case Index (1 - 7)")]
    public int activeCaseIndex = 1;

    [Header("Splash Screen (Logos ONLY, NO text below)")]
    public Slider splashProgressBar;
    public TextMeshProUGUI splashProgressText;
    public float splashDuration = 2.5f;

    [Header("Settings Controls (Option/Setting)")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI bgmValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("AR Reticle & Slot Scanner")]
    public RectTransform laserScanLine;
    public TextMeshProUGUI reticleModeLabel;
    public TextMeshProUGUI activeSlotIndicator;
    public GameObject blueprintSlotGuide;

    [Header("Probe Sub-Views (Viewports)")]
    public GameObject viewPelvis;
    public GameObject viewDada;
    public GameObject viewKepala;
    public GameObject viewAbdomen;

    [Header("Pelvis / Thermal Probe Elements")]
    public TextMeshProUGUI pelvisLcdTemp;
    public TextMeshProUGUI pelvisSensorStatus;
    public TextMeshProUGUI pelvisDepthLabel;
    public Slider pelvisProgressBar;
    public TextMeshProUGUI btnProbePelvisText;

    [Header("Dada / Blanch Test Elements")]
    public Image livorPatchImage;
    public TextMeshProUGUI livorCenterText;
    public TextMeshProUGUI btnBlanchText;
    public Image blanchProgressRing;

    [Header("Cranium & Abdomen Elements")]
    public TextMeshProUGUI craniumObservationText;
    public TextMeshProUGUI abdomenObservationText;

    [Header("4-Slot Probe Buttons")]
    public Button btnSlotKepala;
    public Button btnSlotDada;
    public Button btnSlotAbdomen;
    public Button btnSlotPelvis;
    public Image imgSlotKepala;
    public Image imgSlotDada;
    public Image imgSlotAbdomen;
    public Image imgSlotPelvis;

    [Header("Audio")]
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;

    private Coroutine splashCoroutine;
    private bool splashDone = false;
    private string currentActiveSlot = "PELVIS";
    private bool isBlanching = false;
    private bool isPelvisProbed = false;

    // Clinical specs for the 7 cases (matches mobile.html)
    public struct CaseClinicalSpec
    {
        public string codeName;
        public float temp;
        public bool canBlanch;
        public Color livorColor;
        public string craniumNotes;
        public string abdomenNotes;
    }

    private readonly CaseClinicalSpec[] clinicalSpecs = new CaseClinicalSpec[]
    {
        // 1. Kamar Mandi
        new CaseClinicalSpec {
            codeName = "[CS-01] BATHROOM AC // 20 C",
            temp = 31.5f,
            canBlanch = true,
            livorColor = new Color(0.49f, 0.13f, 0.81f),
            craniumNotes = "Wajah & kornea normal jernih. Bibir pucat wajar. Tidak ada trauma cranium atau buih oronasal.",
            abdomenNotes = "Dinding abdomen rata, caecum tenang, belum ada tanda pembusukan awal."
        },
        // 2. Gudang Pabrik
        new CaseClinicalSpec {
            codeName = "[CS-02] WAREHOUSE HEAT // 33 C",
            temp = 34.5f,
            canBlanch = false,
            livorColor = new Color(0.35f, 0.11f, 0.53f),
            craniumNotes = "Kongesti vena wajah (merah gelap kusam & sedikit edema) akibat posisi telungkup menahan gravitasi.",
            abdomenNotes = "Distensi gas awal pada caecum, lebam perut depan menetap (> 8-10 jam)."
        },
        // 3. Kebun Terbuka
        new CaseClinicalSpec {
            codeName = "[CS-03] OPEN ORCHARD // 24 C",
            temp = 25.5f,
            canBlanch = false,
            livorColor = new Color(0.30f, 0.11f, 0.58f),
            craniumNotes = "Kornea mulai keruh (cloudy). Perubahan warna pucat kehijauan pada batas leher.",
            abdomenNotes = "Greenish discoloration caecum jelas di perut kanan bawah. Marbling vena hijau kehitaman melintasi abdomen."
        },
        // 4. Tenggelam Air Tawar
        new CaseClinicalSpec {
            codeName = "[CS-04] FRESHWATER SUBMERSION // 22 C",
            temp = 26.0f,
            canBlanch = true,
            livorColor = new Color(0.75f, 0.09f, 0.36f),
            craniumNotes = "Busa putih halus di lubang hidung & mulut (champignon de mousse). Washerwoman skin pada telapak.",
            abdomenNotes = "Distensi cairan encer di lambung, hemodilusi akut."
        },
        // 5. Tenggelam Air Laut
        new CaseClinicalSpec {
            codeName = "[CS-05] SALTWATER SUBMERSION // 26 C",
            temp = 28.5f,
            canBlanch = true,
            livorColor = new Color(0.62f, 0.09f, 0.30f),
            craniumNotes = "Busa kemerahan keluar dari saluran nafas akibat edema paru masif salinitas tinggi. Cutis anserina.",
            abdomenNotes = "Salinitas lambung tinggi, darah pekat hemokonsentrasi."
        },
        // 6. Gantung Diri
        new CaseClinicalSpec {
            codeName = "[CS-06] MECHANICAL ASPHYXIA // 18 C",
            temp = 29.5f,
            canBlanch = false,
            livorColor = new Color(0.58f, 0.64f, 0.72f),
            craniumNotes = "Jejas jerat berbentuk 'V' tidak terputus di atas os tiroid. Petechiae konjungtiva mata.",
            abdomenNotes = "Organ dalam mengalami kongesti pasif hebat. Lebam berkumpul di tungkai bawah."
        },
        // 7. Keracunan Metanol
        new CaseClinicalSpec {
            codeName = "[CS-07] METHANOL TOXICITY // 24 C",
            temp = 33.2f,
            canBlanch = true,
            livorColor = new Color(0.42f, 0.13f, 0.66f),
            craniumNotes = "Sianosis kebiruan gelap nyata pada bibir dan cuping hidung. Pupil mata midriasis maksimal.",
            abdomenNotes = "Mukosa gaster hiperemis akibat iritasi alkohol oplosan."
        }
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        activeCaseIndex = PlayerPrefs.GetInt("PMI_ACTIVE_CASE", 1);
        UpdateCaseLabels();

        // Audio initialization
        float bgm = PlayerPrefs.GetFloat("PMI_BGM_VOL", 0.75f);
        float sfx = PlayerPrefs.GetFloat("PMI_SFX_VOL", 0.90f);

        if (bgmAudioSource != null)
        {
            if (bgmAudioSource.clip == null) bgmAudioSource.clip = UISoundGenerator.CreateAmbientHumClip();
            bgmAudioSource.volume = bgm;
            bgmAudioSource.loop = true;
            if (!bgmAudioSource.isPlaying) bgmAudioSource.Play();
        }
        if (sfxAudioSource != null)
        {
            if (sfxAudioSource.clip == null) sfxAudioSource.clip = UISoundGenerator.CreateClickClip();
            sfxAudioSource.volume = sfx;
        }

        // Initialize Settings UI if bound
        if (bgmSlider != null)
        {
            bgmSlider.value = bgm;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfx;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        UpdateSettingsLabels(bgm, sfx);

        CloseAllModals();

        // Start clock ticker
        InvokeRepeating(nameof(UpdateClock), 0f, 1f);

        // Synchronize with CaseColorManager
        SyncWithCaseColorManager();

        // Splash Screen sequence
        if (splashScreenView != null)
        {
            splashScreenView.SetActive(true);
            if (mainMenuView != null) mainMenuView.SetActive(false);
            if (arProbeView != null) arProbeView.SetActive(false);
            splashCoroutine = StartCoroutine(PlaySplashScreenSequence());
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void Update()
    {
        // Animate laser scanline in AR Probe mode
        if (laserScanLine != null && arProbeView != null && arProbeView.activeInHierarchy)
        {
            float y = Mathf.PingPong(Time.time * 260f, 500f) - 250f;
            laserScanLine.anchoredPosition = new Vector2(laserScanLine.anchoredPosition.x, y);
        }
    }

    private void UpdateClock()
    {
        if (clockText != null)
        {
            clockText.text = System.DateTime.Now.ToString("HH:mm");
        }
    }

    // --- Splash Screen (Logos ONLY, NO text below) ---

    private IEnumerator PlaySplashScreenSequence()
    {
        float elapsed = 0f;
        while (elapsed < splashDuration && !splashDone)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / splashDuration);
            if (splashProgressBar != null) splashProgressBar.value = progress;
            if (splashProgressText != null) splashProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            yield return null;
        }

        ShowMainMenu();
    }

    public void SkipSplashScreen()
    {
        splashDone = true;
        if (splashCoroutine != null) StopCoroutine(splashCoroutine);
        PlayClickSound();
        ShowMainMenu();
    }

    // --- Navigation ---

    public void ShowMainMenu()
    {
        if (splashScreenView != null) splashScreenView.SetActive(false);
        if (mainMenuView != null) mainMenuView.SetActive(true);
        if (arProbeView != null) arProbeView.SetActive(false);
        CloseAllModals();
    }

    public void ShowARProbeView()
    {
        PlayClickSound();

        // Check if we are in MainMenu standalone scene and need to load ASS 2
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu" && arProbeView == null)
        {
            SceneManager.LoadScene("ASS 2");
            return;
        }

        if (splashScreenView != null) splashScreenView.SetActive(false);
        if (mainMenuView != null) mainMenuView.SetActive(false);
        if (arProbeView != null) arProbeView.SetActive(true);
        CloseAllModals();

        // Default to Pelvis Thermal Probe
        SelectProbeSlot("PELVIS");
    }

    public void CloseAllModals()
    {
        if (modalKasus != null) modalKasus.SetActive(false);
        if (modalPanduanProbe != null) modalPanduanProbe.SetActive(false);
        if (modalBukuPanduan != null) modalBukuPanduan.SetActive(false);
        if (modalPengaturan != null) modalPengaturan.SetActive(false);
        if (modalTentang != null) modalTentang.SetActive(false);
        if (modalKoneksi != null) modalKoneksi.SetActive(false);
        if (modalKeluar != null) modalKeluar.SetActive(false);
    }

    // --- Case Management ---

    public void SelectCase(int caseNum)
    {
        activeCaseIndex = Mathf.Clamp(caseNum, 1, 7);
        PlayerPrefs.SetInt("PMI_ACTIVE_CASE", activeCaseIndex);
        PlayerPrefs.Save();

        UpdateCaseLabels();
        SyncWithCaseColorManager();
        RefreshProbeViewport();

        PlayClickSound();
        CloseAllModals();
    }

    private void UpdateCaseLabels()
    {
        int idx = activeCaseIndex - 1;
        if (idx >= 0 && idx < clinicalSpecs.Length)
        {
            string code = clinicalSpecs[idx].codeName;
            if (menuActiveCaseLabel != null) menuActiveCaseLabel.text = code;
            if (probeActiveCaseLabel != null) probeActiveCaseLabel.text = code;
        }
    }

    private void SyncWithCaseColorManager()
    {
        var mgr = Object.FindAnyObjectByType<CaseColorManager>();
        if (mgr != null)
        {
            mgr.SetActiveCase(activeCaseIndex - 1);
        }
    }

    // --- Probe Slot Selector (Cranium, Blanch, Caecum, Thermal) ---

    public void SelectProbeSlot(string slot)
    {
        currentActiveSlot = slot.ToUpper();
        PlayClickSound();

        // Hide all views
        if (viewPelvis != null) viewPelvis.SetActive(false);
        if (viewDada != null) viewDada.SetActive(false);
        if (viewKepala != null) viewKepala.SetActive(false);
        if (viewAbdomen != null) viewAbdomen.SetActive(false);

        Color normalCol = new Color(13f / 255f, 19f / 255f, 31f / 255f, 1f);
        Color activeCol = new Color(59f / 255f, 130f / 255f, 246f / 255f, 0.35f);

        if (imgSlotKepala != null) imgSlotKepala.color = (currentActiveSlot == "KEPALA") ? activeCol : normalCol;
        if (imgSlotDada != null) imgSlotDada.color = (currentActiveSlot == "DADA") ? activeCol : normalCol;
        if (imgSlotAbdomen != null) imgSlotAbdomen.color = (currentActiveSlot == "ABDOMEN") ? activeCol : normalCol;
        if (imgSlotPelvis != null) imgSlotPelvis.color = (currentActiveSlot == "PELVIS") ? activeCol : normalCol;

        var mgr = Object.FindAnyObjectByType<CaseColorManager>();

        switch (currentActiveSlot)
        {
            case "KEPALA":
                if (viewKepala != null) viewKepala.SetActive(true);
                if (reticleModeLabel != null) reticleModeLabel.text = "[PROBE: CRANIUM & CERVICAL // CRN-01]";
                if (activeSlotIndicator != null) activeSlotIndicator.text = "[CRN-01] ENGAGED";
                if (mgr != null) mgr.RevealRegion(BodyRegion.Kepala);
                break;

            case "DADA":
                if (viewDada != null) viewDada.SetActive(true);
                if (reticleModeLabel != null) reticleModeLabel.text = "[PROBE: THORACIC LIVOR MORTIS // THX-02]";
                if (activeSlotIndicator != null) activeSlotIndicator.text = "[THX-02] ENGAGED";
                if (mgr != null) mgr.RevealRegion(BodyRegion.Dada);
                break;

            case "ABDOMEN":
                if (viewAbdomen != null) viewAbdomen.SetActive(true);
                if (reticleModeLabel != null) reticleModeLabel.text = "[PROBE: ABDOMINAL CAECUM // ABD-03]";
                if (activeSlotIndicator != null) activeSlotIndicator.text = "[ABD-03] ENGAGED";
                if (mgr != null) mgr.RevealRegion(BodyRegion.Perut);
                break;

            case "PELVIS":
            default:
                if (viewPelvis != null) viewPelvis.SetActive(true);
                if (reticleModeLabel != null) reticleModeLabel.text = "[PROBE: RECTAL CORE THERMAL // PLV-04]";
                if (activeSlotIndicator != null) activeSlotIndicator.text = "[PLV-04] ENGAGED";
                if (mgr != null) mgr.RevealRegion(BodyRegion.Pelvis);
                break;
        }

        RefreshProbeViewport();
    }

    private void RefreshProbeViewport()
    {
        int idx = activeCaseIndex - 1;
        if (idx < 0 || idx >= clinicalSpecs.Length) return;
        var spec = clinicalSpecs[idx];

        // 1. Pelvis
        if (!isPelvisProbed)
        {
            if (pelvisLcdTemp != null) pelvisLcdTemp.text = "--.- C";
            if (pelvisSensorStatus != null) pelvisSensorStatus.text = "READY";
            if (pelvisDepthLabel != null) pelvisDepthLabel.text = "0.0 CM";
            if (pelvisProgressBar != null) pelvisProgressBar.value = 0f;
            if (btnProbePelvisText != null) btnProbePelvisText.text = "PROBE TISSUE (CLICK)";
        }
        else
        {
            if (pelvisLcdTemp != null) pelvisLcdTemp.text = $"{spec.temp:F1} C";
            if (pelvisSensorStatus != null) pelvisSensorStatus.text = "VERIFIED";
            if (pelvisDepthLabel != null) pelvisDepthLabel.text = "12.0 CM";
            if (pelvisProgressBar != null) pelvisProgressBar.value = 1f;
            if (btnProbePelvisText != null) btnProbePelvisText.text = "PROBE TISSUE (CLICK ULANG)";
        }

        // 2. Dada
        if (livorPatchImage != null)
        {
            livorPatchImage.color = spec.livorColor;
        }
        if (livorCenterText != null)
        {
            livorCenterText.text = "TOUCH TARGET";
        }

        // 3. Cranium
        if (craniumObservationText != null)
        {
            craniumObservationText.text = spec.craniumNotes;
        }

        // 4. Abdomen
        if (abdomenObservationText != null)
        {
            abdomenObservationText.text = spec.abdomenNotes;
        }
    }

    // --- Interactive Physical Tests (Pelvis Probe & Blanching Test) ---

    public void OnClickTriggerPelvisProbe()
    {
        PlayClickSound();
        StartCoroutine(ExecutePelvisProbeRoutine());
    }

    private IEnumerator ExecutePelvisProbeRoutine()
    {
        if (btnProbePelvisText != null) btnProbePelvisText.text = "PENETRASI SENSOR...";
        float dur = 1.5f;
        float el = 0f;
        while (el < dur)
        {
            el += Time.deltaTime;
            float p = el / dur;
            if (pelvisProgressBar != null) pelvisProgressBar.value = p;
            if (pelvisDepthLabel != null) pelvisDepthLabel.text = $"{p * 12f:F1} CM";
            yield return null;
        }

        isPelvisProbed = true;
        int idx = activeCaseIndex - 1;
        float t = clinicalSpecs[idx].temp;
        if (pelvisLcdTemp != null) pelvisLcdTemp.text = $"{t:F1} C";
        if (pelvisSensorStatus != null) pelvisSensorStatus.text = "VERIFIED";
        if (btnProbePelvisText != null) btnProbePelvisText.text = $"[OK] VERIFIED ({t:F1} C)";
    }

    public void OnClickTriggerBlanchTest()
    {
        if (isBlanching) return;
        PlayClickSound();
        StartCoroutine(ExecuteBlanchTestRoutine());
    }

    private IEnumerator ExecuteBlanchTestRoutine()
    {
        isBlanching = true;
        int idx = activeCaseIndex - 1;
        var spec = clinicalSpecs[idx];

        if (btnBlanchText != null) btnBlanchText.text = "MENEKAN JARINGAN (3.0s)...";
        if (livorCenterText != null) livorCenterText.text = "BLANCHING...";

        // If it can blanch (lebam memudar), fade color to pale
        if (spec.canBlanch && livorPatchImage != null)
        {
            livorPatchImage.color = new Color(0.9f, 0.8f, 0.8f);
        }

        yield return new WaitForSeconds(3.0f);

        if (spec.canBlanch)
        {
            if (livorCenterText != null) livorCenterText.text = "MEMUDAR (< 8 JAM)";
            if (btnBlanchText != null) btnBlanchText.text = "LEBAM MEMUDAR (BELUM MENETAP)";
            // Slowly recover color
            yield return new WaitForSeconds(1.5f);
            if (livorPatchImage != null) livorPatchImage.color = spec.livorColor;
        }
        else
        {
            if (livorCenterText != null) livorCenterText.text = "MENETAP (> 8 JAM)";
            if (btnBlanchText != null) btnBlanchText.text = "LEBAM MENETAP (TIDAK MEMUDAR)";
        }

        isBlanching = false;
    }

    // --- Audio & Settings Handlers (Option/Setting WBS +1) ---

    public void SetBGMVolume(float vol)
    {
        vol = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat("PMI_BGM_VOL", vol);
        PlayerPrefs.Save();
        if (bgmAudioSource != null) bgmAudioSource.volume = vol;
        UpdateSettingsLabels(vol, sfxSlider != null ? sfxSlider.value : 0.9f);
    }

    public void SetSFXVolume(float vol)
    {
        vol = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat("PMI_SFX_VOL", vol);
        PlayerPrefs.Save();
        if (sfxAudioSource != null) sfxAudioSource.volume = vol;
        UpdateSettingsLabels(bgmSlider != null ? bgmSlider.value : 0.75f, vol);
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteKey("PMI_ACTIVE_CASE");
        PlayerPrefs.DeleteKey("PMI_BGM_VOL");
        PlayerPrefs.DeleteKey("PMI_SFX_VOL");
        PlayerPrefs.Save();
        activeCaseIndex = 1;
        UpdateCaseLabels();
        SyncWithCaseColorManager();
        if (bgmSlider != null) bgmSlider.value = 0.75f;
        if (sfxSlider != null) sfxSlider.value = 0.90f;
        PlayClickSound();
    }

    private void UpdateSettingsLabels(float bgm, float sfx)
    {
        if (bgmValueText != null) bgmValueText.text = $"{Mathf.RoundToInt(bgm * 100)}%";
        if (sfxValueText != null) sfxValueText.text = $"{Mathf.RoundToInt(sfx * 100)}%";
    }

    // --- Modals Handlers ---

    public void OpenModalKasus() { PlayClickSound(); CloseAllModals(); if (modalKasus) modalKasus.SetActive(true); }
    public void OpenModalPanduanProbe() { PlayClickSound(); CloseAllModals(); if (modalPanduanProbe) modalPanduanProbe.SetActive(true); }
    public void OpenModalBukuPanduan() { PlayClickSound(); CloseAllModals(); if (modalBukuPanduan) modalBukuPanduan.SetActive(true); }
    public void OpenModalPengaturan() { PlayClickSound(); CloseAllModals(); if (modalPengaturan) modalPengaturan.SetActive(true); }
    public void OpenModalTentang() { PlayClickSound(); CloseAllModals(); if (modalTentang) modalTentang.SetActive(true); }
    public void OpenModalKoneksi() { PlayClickSound(); CloseAllModals(); if (modalKoneksi) modalKoneksi.SetActive(true); }
    public void OpenModalKeluar() { PlayClickSound(); CloseAllModals(); if (modalKeluar) modalKeluar.SetActive(true); }

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
        if (sfxAudioSource != null) sfxAudioSource.Play();
    }
}
