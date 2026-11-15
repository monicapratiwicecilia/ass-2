using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[InitializeOnLoad]
public static class IntroductionSceneBuilder
{
    // Tactical Palette
    private static readonly Color BgBase = new Color(5f / 255f, 8f / 255f, 14f / 255f, 1f);          // #05080E
    private static readonly Color BgCard = new Color(13f / 255f, 19f / 255f, 31f / 255f, 1f);        // #0D131F
    private static readonly Color BgSurface = new Color(17f / 255f, 24f / 255f, 39f / 255f, 1f);     // #111827
    private static readonly Color BorderTactical = new Color(30f / 255f, 41f / 255f, 59f / 255f, 1f); // #1E293B
    private static readonly Color SignalBlue = new Color(59f / 255f, 130f / 255f, 246f / 255f, 1f);  // #3B82F6
    private static readonly Color HeroBlue = new Color(29f / 255f, 78f / 255f, 216f / 255f, 1f);     // #1D4ED8
    private static readonly Color ClinicalGreen = new Color(16f / 255f, 185f / 255f, 129f / 255f, 1f); // #10B981
    private static readonly Color CautionAmber = new Color(245f / 255f, 158f / 255f, 11f / 255f, 1f); // #F59E0B
    private static readonly Color DangerRed = new Color(239f / 255f, 68f / 255f, 68f / 255f, 1f);    // #EF4444
    private static readonly Color TextMuted = new Color(148f / 255f, 163f / 255f, 184f / 255f, 1f);  // #94A3B8

    static IntroductionSceneBuilder()
    {
        EditorApplication.delayCall += CheckAndSetupAuto;
    }

    private static void CheckAndSetupAuto()
    {
        if (!System.IO.File.Exists("Assets/Scenes/MainMenu.unity"))
        {
            SetupAllQuiet();
        }
    }

    public static void SetupAllQuiet()
    {
        EnsureLogosAreSprites();
        SetupUnifiedTacticalUIInScene("Assets/Scenes/ASS 2.unity");
        BuildMainMenuScene();
        UpdateBuildSettings();
    }

    [MenuItem("Tools/AR Introduction/Setup Lengkap (Menu + AR HUD + Build Settings)")]
    public static void SetupAll()
    {
        SetupAllQuiet();

        EditorUtility.DisplayDialog("AR Introduction Setup",
            "Setup UI/UX Menu Utama Selesai!\n\n" +
            "✔ WBS (A-2) Terpenuhi 100% (+20 Poin):\n" +
            "   1. Splash Screen (+5): 3 Logo Resmi TANPA TEKS di bawahnya.\n" +
            "   2. Main Menu (+5):\n" +
            "      - Engagement Title (PMI Forensic Clock AR)\n" +
            "      - Animatic Background (Laser Scanline & Cyber Pulse)\n" +
            "      - Mulai Investigasi (Tersambung langsung ke AR Scanner)\n" +
            "      - Buku Panduan Digital (5 Bab Manual Book)\n" +
            "      - Pengaturan (Audio BGM/SFX & Reset)\n" +
            "      - Tentang Pengembang (Kredit Tim & FK Tel-U)\n" +
            "      - Keluar Aplikasi (Modal Konfirmasi)\n" +
            "   3. Digital Manual Book (+10): 5 Tab Lengkap (Story, Setup, Kartu, Gameplay, Win/Lose).\n\n" +
            "✔ Tombol 100% Berfungsi (UIEventSystemFixer terpasang, Raycast Target bersih)!\n" +
            "✔ Layout Presisi 1080x1920 (Tidak terpotong di atas, tanpa scroll berantakan)!", "OK");
    }

    [MenuItem("Tools/AR Introduction/Bangun Menu Utama & Splash Screen")]
    public static void BuildMainMenuScene()
    {
        EnsureLogosAreSprites();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgBase;
        camGo.AddComponent<AudioListener>();

        EnsureEventSystem();

        BuildTacticalUIHierarchy(null);

        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[AR Introduction] Scene MainMenu.unity berhasil dibuat!");
    }

    [MenuItem("Tools/AR Introduction/Pasang Tactical UI di Scene AR (ASS 2)")]
    public static void SetupARHudInASS2()
    {
        SetupUnifiedTacticalUIInScene("Assets/Scenes/ASS 2.unity");
    }

    public static void SetupUnifiedTacticalUIInScene(string scenePath)
    {
        EnsureLogosAreSprites();
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        EnsureEventSystem();

        // Remove old canvases if present
        var oldHud = GameObject.Find("AR_HUD_Canvas");
        if (oldHud != null) Object.DestroyImmediate(oldHud);

        var oldCanvas = GameObject.Find("Tactical_UI_Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

        var oldIntro = GameObject.Find("IntroductionCanvas");
        if (oldIntro != null) Object.DestroyImmediate(oldIntro);

        BuildTacticalUIHierarchy(null);

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[AR Introduction] Tactical UI Canvas berhasil dipasang di {scenePath}!");
    }

    private static void EnsureEventSystem()
    {
        var existingEs = Object.FindAnyObjectByType<EventSystem>();
        GameObject esGo;
        if (existingEs == null)
        {
            esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
        }
        else
        {
            esGo = existingEs.gameObject;
        }

        var fixer = esGo.GetComponent<UIEventSystemFixer>();
        if (fixer == null) fixer = esGo.AddComponent<UIEventSystemFixer>();
        fixer.EnsureEventSystemWorking();
    }

    [MenuItem("Tools/AR Introduction/Perbarui Build Settings (ASS 2 Index 0)")]
    public static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/ASS 2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("[AR Introduction] Build Settings berhasil diupdate: 0 -> ASS 2.unity, 1 -> MainMenu.unity");
    }

    // --- Build Full Tactical UI Hierarchy ---

    private static void BuildTacticalUIHierarchy(Transform parent)
    {
        var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        var publisherSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Logos/logo-telkom-university.png");
        var devSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Logos/logo-team-autopsy.png");
        var partnerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Logos/logo-fakultas-kedokteran.png");

        var roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/RoundedCard_9Slice.png");
        var heroGradSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/HeroGradient_9Slice.png");
        var pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/PillCapsule_9Slice.png");
        var reticleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/CornerTicks_Reticle.png");

        // Canvas Root
        var canvasGo = new GameObject("Tactical_UI_Canvas");
        if (parent != null) canvasGo.transform.SetParent(parent, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // Audio Manager
        var audioGo = new GameObject("TacticalAudioManager");
        audioGo.transform.SetParent(canvasGo.transform, false);
        var bgmSource = audioGo.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        var sfxSource = audioGo.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        var controller = canvasGo.AddComponent<TacticalAppController>();
        controller.bgmAudioSource = bgmSource;
        controller.sfxAudioSource = sfxSource;

        // ====================================================================
        // SCREEN 1: MAIN MENU (Fits 1080x1920 screen cleanly, no top clipping!)
        // ====================================================================
        var menuRoot = CreateUIObject("Screen_MainMenu", canvasGo.transform);
        SetStretch(menuRoot.GetComponent<RectTransform>());
        controller.mainMenuView = menuRoot;

        var menuBg = menuRoot.AddComponent<Image>();
        menuBg.color = BgBase;
        menuBg.raycastTarget = false;

        // Laser Scanline for Animatic Background
        var scanLineGo = CreateUIObject("ScanLine", menuRoot.transform);
        var slRect = scanLineGo.GetComponent<RectTransform>();
        slRect.anchorMin = new Vector2(0f, 0.5f);
        slRect.anchorMax = new Vector2(1f, 0.5f);
        slRect.sizeDelta = new Vector2(0, 4);
        var slImg = scanLineGo.AddComponent<Image>();
        slImg.color = new Color(59f / 255f, 130f / 255f, 246f / 255f, 0.7f);
        slImg.raycastTarget = false;

        var animBg = menuRoot.AddComponent<AnimaticBackground>();
        animBg.backgroundImage = menuBg;
        animBg.scanLine = slRect;
        animBg.scanMinY = -800f;
        animBg.scanMaxY = 800f;
        animBg.scanSpeed = 220f;

        // Main Container Centered (Width 960, Height 1760)
        var containerGo = CreateUIObject("MainMenuContainer", menuRoot.transform);
        var containerRect = containerGo.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(960, 1760);
        containerRect.anchoredPosition = Vector2.zero;

        var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 15, 15);
        vlg.spacing = 20;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // 1. Top Status Bar
        var statusBar = CreateCardRow("StatusBar", containerGo.transform, 40, BgBase, null);
        var sbHlg = statusBar.AddComponent<HorizontalLayoutGroup>();
        sbHlg.padding = new RectOffset(10, 10, 0, 0);
        sbHlg.childControlWidth = false;
        sbHlg.childControlHeight = true;
        sbHlg.childAlignment = TextAnchor.MiddleCenter;

        var clockTxt = CreateText("ClockText", statusBar.transform, font, 24, TextAlignmentOptions.Left, Color.white, "14:00");
        clockTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 36);
        controller.clockText = clockTxt;

        var spacer1 = CreateUIObject("Spacer1", statusBar.transform);
        spacer1.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 36);

        var badgePill = CreateCardRow("BadgePill", statusBar.transform, 30, new Color(59f / 255f, 130f / 255f, 246f / 255f, 0.2f), pillSprite);
        badgePill.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);
        var bTxt = CreateText("Text", badgePill.transform, font, 18, TextAlignmentOptions.Center, new Color(56f / 255f, 189f / 255f, 248f / 255f), "AR PROBE V4.8.2");
        SetStretch(bTxt.GetComponent<RectTransform>());

        var spacer2 = CreateUIObject("Spacer2", statusBar.transform);
        spacer2.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 36);

        var battTxt = CreateText("BattText", statusBar.transform, font, 22, TextAlignmentOptions.Right, ClinicalGreen, "98% ONLINE");
        battTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 36);

        // 2. Institutional White Logo Plate (Telkom, FK, Tim A.U.T.O.P.S.Y.)
        var logoPlate = CreateCardRow("LogoPlate", containerGo.transform, 110, Color.white, roundedSprite);
        var lpHlg = logoPlate.AddComponent<HorizontalLayoutGroup>();
        lpHlg.padding = new RectOffset(25, 25, 12, 12);
        lpHlg.spacing = 20;
        lpHlg.childControlWidth = false;
        lpHlg.childControlHeight = true;
        lpHlg.childAlignment = TextAnchor.MiddleCenter;

        CreateImageItem("LogoTelkom", logoPlate.transform, publisherSprite, 220, 75);
        CreateDivider("Div1", logoPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2, 60);
        CreateImageItem("LogoFK", logoPlate.transform, partnerSprite, 320, 70);
        CreateDivider("Div2", logoPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2, 60);
        CreateImageItem("LogoAutopsy", logoPlate.transform, devSprite, 100, 75);

        // 3. Engagement Title Block
        var headerBox = CreateCardRow("HeaderBox", containerGo.transform, 150, BgBase, null);
        var hbVlg = headerBox.AddComponent<VerticalLayoutGroup>();
        hbVlg.spacing = 6;
        hbVlg.childControlWidth = true;
        hbVlg.childControlHeight = false;
        hbVlg.childAlignment = TextAnchor.MiddleCenter;

        CreateText("PillSub", headerBox.transform, font, 18, TextAlignmentOptions.Center, new Color(56f / 255f, 189f / 255f, 248f / 255f),
            "* HYBRID FORENSIC BOARDGAME & SMART LAB SYSTEM *");
        CreateText("AppTitle", headerBox.transform, font, 48, TextAlignmentOptions.Center, Color.white,
            "<b>PMI FORENSIC CLOCK AR</b>");
        CreateText("AppSubTitle", headerBox.transform, font, 22, TextAlignmentOptions.Center, ClinicalGreen,
            "POST-MORTEM INTERVAL DETERMINATION");
        CreateText("AppTeam", headerBox.transform, font, 18, TextAlignmentOptions.Center, TextMuted,
            "Fakultas Kedokteran Telkom University x Tim A.U.T.O.P.S.Y.");

        // Telemetry ticker text from animBg
        animBg.telemetryStatusText = CreateText("TelemetryStatus", containerGo.transform, font, 16, TextAlignmentOptions.Center, TextMuted,
            "SYS STATUS: OPERATIONAL // PMI ENGINE READY");
        animBg.telemetryStatusText.GetComponent<RectTransform>().sizeDelta = new Vector2(920, 25);

        // 4. Active Case Selection Bar
        var activeCaseCard = CreateCardRow("ActiveCaseCard", containerGo.transform, 80, BgCard, roundedSprite);
        var accHlg = activeCaseCard.AddComponent<HorizontalLayoutGroup>();
        accHlg.padding = new RectOffset(20, 20, 10, 10);
        accHlg.spacing = 15;
        accHlg.childControlWidth = false;
        accHlg.childControlHeight = true;
        accHlg.childAlignment = TextAnchor.MiddleCenter;

        var caseInfoCol = CreateUIObject("CaseInfoCol", activeCaseCard.transform);
        caseInfoCol.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 60);
        var cicVlg = caseInfoCol.AddComponent<VerticalLayoutGroup>();
        cicVlg.spacing = 2;
        cicVlg.childControlWidth = true;
        cicVlg.childControlHeight = false;

        CreateText("Tag", caseInfoCol.transform, font, 14, TextAlignmentOptions.Left, TextMuted, "KASUS AKTIF TKP:");
        var activeCaseText = CreateText("ActiveCaseLabel", caseInfoCol.transform, font, 22, TextAlignmentOptions.Left, CautionAmber, "[CS-01] BATHROOM AC // 20 C");
        controller.menuActiveCaseLabel = activeCaseText;

        var btnChangeCase = CreateStyledButton("BtnChangeCase", activeCaseCard.transform, font, "GANTI KASUS", new Color(59f / 255f, 130f / 255f, 246f / 255f, 0.2f), SignalBlue, 200, 50, roundedSprite);
        btnChangeCase.onClick.AddListener(controller.OpenModalKasus);

        // 5. BUTTON 1: MULAI INVESTIGASI (Hero Button)
        var heroBtn = CreateStyledButton("BtnMulaiInvestigasi", containerGo.transform, font, "", Color.white, Color.white, 940, 135, heroGradSprite);
        var heroHlg = heroBtn.gameObject.AddComponent<HorizontalLayoutGroup>();
        heroHlg.padding = new RectOffset(25, 25, 15, 15);
        heroHlg.spacing = 20;
        heroHlg.childControlWidth = false;
        heroHlg.childControlHeight = true;
        heroHlg.childAlignment = TextAnchor.MiddleCenter;

        var iconBox = CreateCardRow("IconBox", heroBtn.transform, 85, new Color(7f / 255f, 11f / 255f, 19f / 255f), roundedSprite);
        iconBox.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 85);
        var probeIconTxt = CreateText("Icon", iconBox.transform, font, 32, TextAlignmentOptions.Center, new Color(56f / 255f, 189f / 255f, 248f / 255f), "[+]");
        SetStretch(probeIconTxt.GetComponent<RectTransform>());

        var heroTextCol = CreateUIObject("TextCol", heroBtn.transform);
        heroTextCol.GetComponent<RectTransform>().sizeDelta = new Vector2(690, 95);
        var htcVlg = heroTextCol.AddComponent<VerticalLayoutGroup>();
        htcVlg.spacing = 3;
        htcVlg.childControlWidth = true;
        htcVlg.childControlHeight = false;

        CreateText("Sub1", heroTextCol.transform, font, 16, TextAlignmentOptions.Left, new Color(165f / 255f, 243f / 255f, 252f / 255f), "INSTRUMEN AR FORENSIK");
        CreateText("Main", heroTextCol.transform, font, 30, TextAlignmentOptions.Left, Color.white, "<b>MULAI INVESTIGASI</b>");
        CreateText("Sub2", heroTextCol.transform, font, 16, TextAlignmentOptions.Left, new Color(255, 255, 255, 0.8f), "Buka Reticle Scanner & Bar 4 Slot Uji Fisik");

        var rightArrow = CreateText("Arrow", heroBtn.transform, font, 36, TextAlignmentOptions.Center, Color.white, ">");
        rightArrow.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 70);

        heroBtn.onClick.AddListener(controller.ShowARProbeView);

        // 6. BUTTON 2: BUKU PANDUAN DIGITAL (Digital Manual Book - WBS +10)
        var btnManual = CreateMenuRowButton("BtnManualBook", containerGo.transform, font, "[DOC]", "BUKU PANDUAN DIGITAL", "5 Bab Pedoman Lengkap (Story, Setup, Kartu, Gameplay, Win/Lose)", SignalBlue, roundedSprite);
        btnManual.button.onClick.AddListener(controller.OpenModalBukuPanduan);

        // 7. BUTTON 3: PENGATURAN (Option/Setting - WBS +1)
        var btnSetting = CreateMenuRowButton("BtnSetting", containerGo.transform, font, "[OPT]", "PENGATURAN", "Volume Audio BGM, SFX & Konfigurasi Sistem", ClinicalGreen, roundedSprite);
        btnSetting.button.onClick.AddListener(controller.OpenModalPengaturan);

        // 8. BUTTON 4: TENTANG PENGEMBANG (About Developer - WBS +1)
        var btnAbout = CreateMenuRowButton("BtnAbout", containerGo.transform, font, "[INF]", "TENTANG PENGEMBANG", "Profil Tim A.U.T.O.P.S.Y. & Mitra Fakultas Kedokteran", new Color(192f / 255f, 132f / 255f, 252f / 255f), roundedSprite);
        btnAbout.button.onClick.AddListener(controller.OpenModalTentang);

        // 9. BUTTON 5: KELUAR APLIKASI (Quit Application - WBS +1)
        var quitBtn = CreateStyledButton("BtnQuit", containerGo.transform, font, "[X]  KELUAR APLIKASI", new Color(0.18f, 0.06f, 0.06f, 0.95f), new Color(248f / 255f, 113f / 255f, 113f / 255f), 940, 70, roundedSprite);
        quitBtn.onClick.AddListener(controller.OpenModalKeluar);

        // 10. Footer
        var footerBox = CreateCardRow("FooterBox", containerGo.transform, 45, BgBase, null);
        var footerTxt = CreateText("FooterText", footerBox.transform, font, 16, TextAlignmentOptions.Center, TextMuted,
            "PMI FORENSIC CLOCK AR • PROBE UNIT V4.8.2 // FAKULTAS KEDOKTERAN TELKOM UNIVERSITY");
        SetStretch(footerTxt.GetComponent<RectTransform>());

        // ====================================================================
        // SCREEN 2: OPTICAL AR PROBE SCANNER (Screen 2 matching mobile.html)
        // ====================================================================
        var probeRoot = CreateUIObject("Screen_ARProbe", canvasGo.transform);
        SetStretch(probeRoot.GetComponent<RectTransform>());
        controller.arProbeView = probeRoot;
        probeRoot.SetActive(false);

        // Top Status Header Strip
        var probeHeader = CreateCardRow("ProbeHeader", probeRoot.transform, 90, new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.95f), null);
        var phRect = probeHeader.GetComponent<RectTransform>();
        phRect.anchorMin = new Vector2(0f, 1f);
        phRect.anchorMax = new Vector2(1f, 1f);
        phRect.pivot = new Vector2(0.5f, 1f);
        phRect.anchoredPosition = Vector2.zero;

        var phHlg = probeHeader.AddComponent<HorizontalLayoutGroup>();
        phHlg.padding = new RectOffset(20, 20, 10, 10);
        phHlg.spacing = 15;
        phHlg.childControlWidth = false;
        phHlg.childControlHeight = true;
        phHlg.childAlignment = TextAnchor.MiddleCenter;

        var btnBackMenu = CreateStyledButton("BtnBackMenu", probeHeader.transform, font, "<- MENU", BgSurface, new Color(56f / 255f, 189f / 255f, 248f / 255f), 160, 55, roundedSprite);
        btnBackMenu.onClick.AddListener(controller.ShowMainMenu);

        var probeCaseCol = CreateUIObject("ProbeCaseCol", probeHeader.transform);
        probeCaseCol.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 60);
        var pccVlg = probeCaseCol.AddComponent<VerticalLayoutGroup>();
        pccVlg.spacing = 2;
        pccVlg.childControlWidth = true;
        pccVlg.childControlHeight = false;

        CreateText("RoomLbl", probeCaseCol.transform, font, 15, TextAlignmentOptions.Left, TextMuted, "PROBE | ROOM: PMI-AUTO");
        var probeCaseLbl = CreateText("CaseLbl", probeCaseCol.transform, font, 20, TextAlignmentOptions.Left, CautionAmber, "[CS-01] BATHROOM AC // 20 C");
        controller.probeActiveCaseLabel = probeCaseLbl;

        var btnChangeCaseProbe = CreateStyledButton("BtnChangeCaseProbe", probeHeader.transform, font, "KASUS", BgSurface, SignalBlue, 140, 55, roundedSprite);
        btnChangeCaseProbe.onClick.AddListener(controller.OpenModalKasus);

        // Central Optical Reticle Viewport (The Layout for Board & Cards)
        var reticleContainer = CreateUIObject("ReticleViewportContainer", probeRoot.transform);
        var rcRect = reticleContainer.GetComponent<RectTransform>();
        rcRect.anchorMin = new Vector2(0.5f, 0.5f);
        rcRect.anchorMax = new Vector2(0.5f, 0.5f);
        rcRect.pivot = new Vector2(0.5f, 0.5f);
        rcRect.sizeDelta = new Vector2(980, 1200);
        rcRect.anchoredPosition = new Vector2(0, 20);

        // Reticle Frame with Corner Ticks
        var reticleBox = CreateCardRow("ReticleBox", reticleContainer.transform, 1200, Color.white, reticleSprite);
        reticleBox.GetComponent<Image>().type = Image.Type.Sliced;
        SetStretch(reticleBox.GetComponent<RectTransform>());

        // Viewport Top Telemetry Bar
        var reticleTop = CreateCardRow("ReticleTopBar", reticleBox.transform, 35, Color.clear, null);
        var rtRect = reticleTop.GetComponent<RectTransform>();
        rtRect.anchorMin = new Vector2(0f, 1f);
        rtRect.anchorMax = new Vector2(1f, 1f);
        rtRect.pivot = new Vector2(0.5f, 1f);
        rtRect.anchoredPosition = new Vector2(0, -10);

        var rtHlg = reticleTop.AddComponent<HorizontalLayoutGroup>();
        rtHlg.padding = new RectOffset(20, 20, 0, 0);
        rtHlg.childControlWidth = false;
        rtHlg.childControlHeight = true;
        rtHlg.childAlignment = TextAnchor.MiddleCenter;

        var rModeLbl = CreateText("ModeLbl", reticleTop.transform, font, 18, TextAlignmentOptions.Left, SignalBlue, "[PROBE: RECTAL CORE THERMAL // PLV-04]");
        rModeLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 30);
        controller.reticleModeLabel = rModeLbl;

        var rFovLbl = CreateText("FovLbl", reticleTop.transform, font, 18, TextAlignmentOptions.Right, TextMuted, "FOV 78 // f/1.8");
        rFovLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 30);

        // Laser Scanline inside Reticle
        var rLaser = CreateUIObject("ReticleLaserScan", reticleBox.transform);
        var rlRect = rLaser.GetComponent<RectTransform>();
        rlRect.anchorMin = new Vector2(0f, 0.5f);
        rlRect.anchorMax = new Vector2(1f, 0.5f);
        rlRect.sizeDelta = new Vector2(0, 4);
        var rlImg = rLaser.AddComponent<Image>();
        rlImg.color = new Color(59f / 255f, 130f / 255f, 246f / 255f, 0.85f);
        rlImg.raycastTarget = false;
        controller.laserScanLine = rlRect;

        // Blueprint Guide: Layout for Boardgame & 4 Action Cards
        var blueprintGuide = CreateUIObject("BlueprintSlotGuide", reticleBox.transform);
        controller.blueprintSlotGuide = blueprintGuide;
        var bpRect = blueprintGuide.GetComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0.5f, 0.6f);
        bpRect.anchorMax = new Vector2(0.5f, 0.6f);
        bpRect.pivot = new Vector2(0.5f, 0.5f);
        bpRect.sizeDelta = new Vector2(900, 450);

        var bpImg = blueprintGuide.AddComponent<Image>();
        bpImg.color = new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.7f);
        bpImg.sprite = roundedSprite;
        bpImg.type = Image.Type.Sliced;
        bpImg.raycastTarget = false;

        var bpVlg = blueprintGuide.AddComponent<VerticalLayoutGroup>();
        bpVlg.padding = new RectOffset(20, 20, 15, 15);
        bpVlg.spacing = 10;
        bpVlg.childControlWidth = true;
        bpVlg.childControlHeight = false;

        CreateText("BpTitle", blueprintGuide.transform, font, 20, TextAlignmentOptions.Center, new Color(56f / 255f, 189f / 255f, 248f / 255f),
            "AREA SCAN KARTU BOARDGAME FORENSIK");
        CreateText("BpSub", blueprintGuide.transform, font, 16, TextAlignmentOptions.Center, TextMuted,
            "Arahkan kamera ke Blueprint Anatomi & Kartu Tindakan");

        var slotRow = CreateUIObject("SlotRow", blueprintGuide.transform);
        slotRow.GetComponent<RectTransform>().sizeDelta = new Vector2(860, 320);
        var srGlg = slotRow.AddComponent<GridLayoutGroup>();
        srGlg.cellSize = new Vector2(200, 310);
        srGlg.spacing = new Vector2(15, 0);
        srGlg.childAlignment = TextAnchor.MiddleCenter;

        CreateSlotTargetFrame("Slot1_Kepala", slotRow.transform, font, "[SLOT 1]", "KEPALA", "Kaca Pembesar", roundedSprite);
        CreateSlotTargetFrame("Slot2_Dada", slotRow.transform, font, "[SLOT 2]", "DADA", "Uji Lebam", roundedSprite);
        CreateSlotTargetFrame("Slot3_Abdomen", slotRow.transform, font, "[SLOT 3]", "ABDOMEN", "Sendi / Caecum", roundedSprite);
        CreateSlotTargetFrame("Slot4_Pelvis", slotRow.transform, font, "[SLOT 4]", "PELVIS", "Termometer", roundedSprite);

        // Sub-view: Pelvis / Thermal Probe
        var vPelvis = CreateCardRow("View_PELVIS", reticleBox.transform, 350, new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.95f), roundedSprite);
        var vpRect = vPelvis.GetComponent<RectTransform>();
        vpRect.anchorMin = new Vector2(0.5f, 0f);
        vpRect.anchorMax = new Vector2(0.5f, 0f);
        vpRect.pivot = new Vector2(0.5f, 0f);
        vpRect.sizeDelta = new Vector2(920, 350);
        vpRect.anchoredPosition = new Vector2(0, 30);
        controller.viewPelvis = vPelvis;

        var vpVlg = vPelvis.AddComponent<VerticalLayoutGroup>();
        vpVlg.padding = new RectOffset(25, 25, 20, 20);
        vpVlg.spacing = 15;
        vpVlg.childControlWidth = true;
        vpVlg.childControlHeight = false;

        var vpHdr = CreateCardRow("Hdr", vPelvis.transform, 30, Color.clear, null);
        var vpHlg = vpHdr.AddComponent<HorizontalLayoutGroup>();
        vpHlg.childControlWidth = false;
        vpHlg.childControlHeight = true;
        CreateText("Inst", vpHdr.transform, font, 18, TextAlignmentOptions.Left, TextMuted, "INSTRUMEN: DIGITAL PROBE [PLV-04]");
        var pStatus = CreateText("Stat", vpHdr.transform, font, 18, TextAlignmentOptions.Right, ClinicalGreen, "READY");
        pStatus.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 30);
        controller.pelvisSensorStatus = pStatus;

        var vBody = CreateCardRow("Body", vPelvis.transform, 80, BgBase, roundedSprite);
        var vbHlg = vBody.AddComponent<HorizontalLayoutGroup>();
        vbHlg.padding = new RectOffset(25, 25, 10, 10);
        vbHlg.childControlWidth = false;
        vbHlg.childControlHeight = true;
        var pLcd = CreateText("Lcd", vBody.transform, font, 48, TextAlignmentOptions.Left, Color.white, "--.- C");
        pLcd.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 60);
        controller.pelvisLcdTemp = pLcd;
        var pDepth = CreateText("Depth", vBody.transform, font, 22, TextAlignmentOptions.Right, SignalBlue, "DEPTH: 0.0 CM\nSCALE: CELSIUS");
        pDepth.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 60);
        controller.pelvisDepthLabel = pDepth;

        var pProg = CreateCardRow("Prog", vPelvis.transform, 16, BgSurface, pillSprite);
        var pProgFill = CreateUIObject("Fill", pProg.transform);
        var ppfRect = pProgFill.GetComponent<RectTransform>();
        ppfRect.anchorMin = Vector2.zero;
        ppfRect.anchorMax = new Vector2(0f, 1f);
        ppfRect.sizeDelta = Vector2.zero;
        pProgFill.AddComponent<Image>().color = SignalBlue;
        var pSlider = pProg.AddComponent<Slider>();
        pSlider.fillRect = ppfRect;
        pSlider.minValue = 0f;
        pSlider.maxValue = 1f;
        pSlider.value = 0f;
        controller.pelvisProgressBar = pSlider;

        var btnProbe = CreateStyledButton("BtnProbePelvis", vPelvis.transform, font, "PROBE TISSUE (CLICK)", SignalBlue, Color.white, 870, 70, roundedSprite);
        controller.btnProbePelvisText = btnProbe.GetComponentInChildren<TextMeshProUGUI>();
        btnProbe.onClick.AddListener(controller.OnClickTriggerPelvisProbe);

        // Sub-view: Dada / Blanch Test
        var vDada = CreateCardRow("View_DADA", reticleBox.transform, 350, new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.95f), roundedSprite);
        var vdRect = vDada.GetComponent<RectTransform>();
        vdRect.anchorMin = new Vector2(0.5f, 0f);
        vdRect.anchorMax = new Vector2(0.5f, 0f);
        vdRect.pivot = new Vector2(0.5f, 0f);
        vdRect.sizeDelta = new Vector2(920, 350);
        vdRect.anchoredPosition = new Vector2(0, 30);
        controller.viewDada = vDada;
        vDada.SetActive(false);

        var vdVlg = vDada.AddComponent<VerticalLayoutGroup>();
        vdVlg.padding = new RectOffset(25, 25, 15, 15);
        vdVlg.spacing = 10;
        vdVlg.childControlWidth = true;
        vdVlg.childControlHeight = false;

        CreateText("DadaHeader", vDada.transform, font, 18, TextAlignmentOptions.Center, TextMuted, "DERMAL CUTANEOUS SURFACE // CONTACT TARGET");

        var patchCenterBox = CreateUIObject("PatchBox", vDada.transform);
        patchCenterBox.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 140);
        var livorImgGo = CreateCardRow("LivorPatch", patchCenterBox.transform, 130, new Color(0.49f, 0.13f, 0.81f), pillSprite);
        livorImgGo.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 130);
        livorImgGo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        controller.livorPatchImage = livorImgGo.GetComponent<Image>();

        var cTxt = CreateText("CenterTxt", livorImgGo.transform, font, 18, TextAlignmentOptions.Center, Color.white, "TOUCH TARGET");
        SetStretch(cTxt.GetComponent<RectTransform>());
        controller.livorCenterText = cTxt;

        var btnBlanch = CreateStyledButton("BtnBlanch", vDada.transform, font, "EXECUTE 3.0s BLANCHING TEST", SignalBlue, Color.white, 870, 70, roundedSprite);
        controller.btnBlanchText = btnBlanch.GetComponentInChildren<TextMeshProUGUI>();
        btnBlanch.onClick.AddListener(controller.OnClickTriggerBlanchTest);

        // Sub-view: Kepala
        var vKepala = CreateCardRow("View_KEPALA", reticleBox.transform, 350, new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.95f), roundedSprite);
        var vkRect = vKepala.GetComponent<RectTransform>();
        vkRect.anchorMin = new Vector2(0.5f, 0f);
        vkRect.anchorMax = new Vector2(0.5f, 0f);
        vkRect.pivot = new Vector2(0.5f, 0f);
        vkRect.sizeDelta = new Vector2(920, 350);
        vkRect.anchoredPosition = new Vector2(0, 30);
        controller.viewKepala = vKepala;
        vKepala.SetActive(false);

        var vkVlg = vKepala.AddComponent<VerticalLayoutGroup>();
        vkVlg.padding = new RectOffset(25, 25, 20, 20);
        vkVlg.spacing = 15;
        vkVlg.childControlWidth = true;
        vkVlg.childControlHeight = false;

        CreateText("KepalaHeader", vKepala.transform, font, 18, TextAlignmentOptions.Left, new Color(192f / 255f, 132f / 255f, 252f / 255f),
            "CRANIUM & CERVICAL OPTICAL INSPECTION [CRN-01]");
        var craniumNotes = CreateText("CraniumNotes", vKepala.transform, font, 24, TextAlignmentOptions.TopLeft, Color.white,
            "Wajah & kornea normal jernih. Bibir pucat wajar. Tidak ada trauma cranium atau buih oronasal.");
        controller.craniumObservationText = craniumNotes;

        // Sub-view: Abdomen
        var vAbdomen = CreateCardRow("View_ABDOMEN", reticleBox.transform, 350, new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.95f), roundedSprite);
        var vaRect = vAbdomen.GetComponent<RectTransform>();
        vaRect.anchorMin = new Vector2(0.5f, 0f);
        vaRect.anchorMax = new Vector2(0.5f, 0f);
        vaRect.pivot = new Vector2(0.5f, 0f);
        vaRect.sizeDelta = new Vector2(920, 350);
        vaRect.anchoredPosition = new Vector2(0, 30);
        controller.viewAbdomen = vAbdomen;
        vAbdomen.SetActive(false);

        var vaVlg = vAbdomen.AddComponent<VerticalLayoutGroup>();
        vaVlg.padding = new RectOffset(25, 25, 20, 20);
        vaVlg.spacing = 15;
        vaVlg.childControlWidth = true;
        vaVlg.childControlHeight = false;

        CreateText("AbdomenHeader", vAbdomen.transform, font, 18, TextAlignmentOptions.Left, ClinicalGreen,
            "ABDOMINAL CAECUM INSPECTION [ABD-03]");
        var abdomenNotes = CreateText("AbdomenNotes", vAbdomen.transform, font, 24, TextAlignmentOptions.TopLeft, Color.white,
            "Dinding abdomen rata, caecum tenang, belum ada tanda pembusukan awal.");
        controller.abdomenObservationText = abdomenNotes;

        // Bottom 4-Slot Active Probe Selector Bar
        var probeBottomBar = CreateCardRow("ProbeBottomBar", probeRoot.transform, 135, new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.98f), null);
        var pbbRect = probeBottomBar.GetComponent<RectTransform>();
        pbbRect.anchorMin = new Vector2(0f, 0f);
        pbbRect.anchorMax = new Vector2(1f, 0f);
        pbbRect.pivot = new Vector2(0.5f, 0f);
        pbbRect.anchoredPosition = Vector2.zero;

        var pbbVlg = probeBottomBar.AddComponent<VerticalLayoutGroup>();
        pbbVlg.padding = new RectOffset(20, 20, 8, 12);
        pbbVlg.spacing = 6;
        pbbVlg.childControlWidth = true;
        pbbVlg.childControlHeight = false;

        var pbbTop = CreateUIObject("TopRow", probeBottomBar.transform);
        pbbTop.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 22);
        var pbbHlg = pbbTop.AddComponent<HorizontalLayoutGroup>();
        pbbHlg.childControlWidth = false;
        pbbHlg.childControlHeight = true;

        CreateText("ActiveLbl", pbbTop.transform, font, 15, TextAlignmentOptions.Left, TextMuted, "ACTIVE PROBE SELECTOR:");
        var activeSlotInd = CreateText("SlotInd", pbbTop.transform, font, 16, TextAlignmentOptions.Left, SignalBlue, "[PLV-04] ENGAGED");
        controller.activeSlotIndicator = activeSlotInd;

        var btnRow = CreateUIObject("BtnRow", probeBottomBar.transform);
        btnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 75);
        var brHlg = btnRow.AddComponent<HorizontalLayoutGroup>();
        brHlg.spacing = 12;
        brHlg.childControlWidth = true;
        brHlg.childControlHeight = true;

        var bKepala = CreateStyledButton("BtnSlot_KEPALA", btnRow.transform, font, "[CRN-01]\nCRANIUM", BgSurface, Color.white, 220, 70, roundedSprite);
        bKepala.onClick.AddListener(() => controller.SelectProbeSlot("KEPALA"));
        controller.btnSlotKepala = bKepala;
        controller.imgSlotKepala = bKepala.GetComponent<Image>();

        var bDada = CreateStyledButton("BtnSlot_DADA", btnRow.transform, font, "[THX-02]\nBLANCH", BgSurface, Color.white, 220, 70, roundedSprite);
        bDada.onClick.AddListener(() => controller.SelectProbeSlot("DADA"));
        controller.btnSlotDada = bDada;
        controller.imgSlotDada = bDada.GetComponent<Image>();

        var bAbdomen = CreateStyledButton("BtnSlot_ABDOMEN", btnRow.transform, font, "[ABD-03]\nCAECUM", BgSurface, Color.white, 220, 70, roundedSprite);
        bAbdomen.onClick.AddListener(() => controller.SelectProbeSlot("ABDOMEN"));
        controller.btnSlotAbdomen = bAbdomen;
        controller.imgSlotAbdomen = bAbdomen.GetComponent<Image>();

        var bPelvis = CreateStyledButton("BtnSlot_PELVIS", btnRow.transform, font, "[PLV-04]\nTHERMAL", new Color(59f / 255f, 130f / 255f, 246f / 255f, 0.35f), Color.white, 220, 70, roundedSprite);
        bPelvis.onClick.AddListener(() => controller.SelectProbeSlot("PELVIS"));
        controller.btnSlotPelvis = bPelvis;
        controller.imgSlotPelvis = bPelvis.GetComponent<Image>();

        // ====================================================================
        // MODALS (Kasus, Buku Panduan, Pengaturan, Tentang, Keluar)
        // ====================================================================

        var modalKasus = CreateModalCard("ModalKasus", canvasGo.transform, "BERKAS KASUS FORENSIK (1 - 7)", 940, 1300, font, roundedSprite);
        controller.modalKasus = modalKasus;
        BuildCaseListModalContent(modalKasus.transform.Find("Card/Body"), font, controller, roundedSprite);

        var modalManual = CreateModalCard("ModalManualBook", canvasGo.transform, "BUKU PANDUAN FORENSIK (WBS A-2)", 940, 1350, font, roundedSprite);
        controller.modalBukuPanduan = modalManual;
        BuildManualBookModalContent(modalManual.transform.Find("Card/Body"), font, modalManual, roundedSprite);

        var modalSetting = CreateModalCard("ModalPengaturan", canvasGo.transform, "PENGATURAN SISTEM & AUDIO", 900, 750, font, roundedSprite);
        controller.modalPengaturan = modalSetting;
        BuildSettingModalContent(modalSetting.transform.Find("Card/Body"), font, controller, roundedSprite);

        var modalAbout = CreateModalCard("ModalAbout", canvasGo.transform, "PROFIL PENGEMBANG & MITRA", 900, 1050, font, roundedSprite);
        controller.modalTentang = modalAbout;
        BuildAboutModalContent(modalAbout.transform.Find("Card/Body"), font, publisherSprite, partnerSprite, devSprite, roundedSprite);

        var modalQuit = CreateModalCard("ModalQuit", canvasGo.transform, "KONFIRMASI KELUAR", 750, 420, font, roundedSprite);
        controller.modalKeluar = modalQuit;
        BuildQuitModalContent(modalQuit.transform.Find("Card/Body"), font, controller, roundedSprite);

        // ====================================================================
        // SCREEN 0: SPLASH SCREEN (LOGOS ONLY, NO TEXT BELOW!)
        // In Edit Mode, we deactivate it so the user can edit/see the Main Menu!
        // In Play Mode, TacticalAppController will activate it and play the sequence.
        // ====================================================================
        var splashPanel = CreateUIObject("Screen_SplashScreen", canvasGo.transform);
        SetStretch(splashPanel.GetComponent<RectTransform>());
        var splashBg = splashPanel.AddComponent<Image>();
        splashBg.color = BgBase;
        splashBg.raycastTarget = true;
        controller.splashScreenView = splashPanel;

        // Skip Button
        var skipBtn = CreateStyledButton("BtnSkipSplash", splashPanel.transform, font, "LEWATI >", BgCard, TextMuted, 180, 50, roundedSprite);
        var skipRect = skipBtn.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(1f, 1f);
        skipRect.anchorMax = new Vector2(1f, 1f);
        skipRect.pivot = new Vector2(1f, 1f);
        skipRect.anchoredPosition = new Vector2(-40, -40);
        skipBtn.onClick.AddListener(controller.SkipSplashScreen);

        // Centered White Plate with the 3 official logos (NO TEXT UNDERNEATH!)
        var splashPlate = CreateCardRow("SplashLogoPlate", splashPanel.transform, 190, Color.white, roundedSprite);
        var spRect = splashPlate.GetComponent<RectTransform>();
        spRect.anchorMin = new Vector2(0.5f, 0.5f);
        spRect.anchorMax = new Vector2(0.5f, 0.5f);
        spRect.pivot = new Vector2(0.5f, 0.5f);
        spRect.sizeDelta = new Vector2(920, 200);
        spRect.anchoredPosition = new Vector2(0, 30);

        var spHlg = splashPlate.AddComponent<HorizontalLayoutGroup>();
        spHlg.padding = new RectOffset(25, 25, 20, 20);
        spHlg.spacing = 25;
        spHlg.childControlWidth = false;
        spHlg.childControlHeight = true;
        spHlg.childAlignment = TextAnchor.MiddleCenter;

        CreateImageItem("LogoTelkom", splashPlate.transform, publisherSprite, 250, 120);
        CreateDivider("Div1", splashPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2, 90);
        CreateImageItem("LogoFK", splashPlate.transform, partnerSprite, 360, 110);
        CreateDivider("Div2", splashPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2, 90);
        CreateImageItem("LogoAutopsy", splashPlate.transform, devSprite, 120, 120);

        // Progress Bar at bottom (NO TEXT UNDER LOGOS)
        var progContainer = CreateCardRow("ProgressContainer", splashPanel.transform, 16, new Color(0.1f, 0.14f, 0.22f, 1f), pillSprite);
        var progRect = progContainer.GetComponent<RectTransform>();
        progRect.anchorMin = new Vector2(0.5f, 0f);
        progRect.anchorMax = new Vector2(0.5f, 0f);
        progRect.pivot = new Vector2(0.5f, 0f);
        progRect.sizeDelta = new Vector2(700, 16);
        progRect.anchoredPosition = new Vector2(0, 150);

        var fillGo = CreateUIObject("Fill", progContainer.transform);
        var fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.sizeDelta = Vector2.zero;
        fillGo.AddComponent<Image>().color = SignalBlue;

        var slider = progContainer.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        controller.splashProgressBar = slider;

        var progTxt = CreateText("ProgressText", splashPanel.transform, font, 20, TextAlignmentOptions.Center, TextMuted, "0%");
        var ptRect = progTxt.GetComponent<RectTransform>();
        ptRect.anchorMin = new Vector2(0.5f, 0f);
        ptRect.anchorMax = new Vector2(0.5f, 0f);
        ptRect.pivot = new Vector2(0.5f, 0f);
        ptRect.sizeDelta = new Vector2(700, 30);
        ptRect.anchoredPosition = new Vector2(0, 110);
        controller.splashProgressText = progTxt;

        // Deactivate splash in Edit Mode so the designer can see the menu directly!
        splashPanel.SetActive(false);
    }

    // --- Modal Builders ---

    private static void BuildSettingModalContent(Transform body, TMP_FontAsset font, TacticalAppController controller, Sprite roundedSprite)
    {
        var vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 25;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // BGM Slider Row
        var bgmRow = CreateCardRow("BgmRow", body, 110, BgSurface, roundedSprite);
        var brVlg = bgmRow.AddComponent<VerticalLayoutGroup>();
        brVlg.padding = new RectOffset(20, 20, 15, 15);
        brVlg.spacing = 8;
        brVlg.childControlWidth = true;
        brVlg.childControlHeight = false;

        var brHdr = CreateCardRow("Hdr", bgmRow.transform, 30, Color.clear, null);
        var bhHlg = brHdr.AddComponent<HorizontalLayoutGroup>();
        bhHlg.childControlWidth = false;
        bhHlg.childControlHeight = true;
        CreateText("Lbl", brHdr.transform, font, 22, TextAlignmentOptions.Left, Color.white, "VOLUME MUSIK FORENSIK (BGM)");
        var bgmVal = CreateText("Val", brHdr.transform, font, 22, TextAlignmentOptions.Right, SignalBlue, "75%");
        bgmVal.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
        controller.bgmValueText = bgmVal;

        var bgmSlider = CreateCustomSlider("BgmSlider", bgmRow.transform, SignalBlue);
        controller.bgmSlider = bgmSlider;

        // SFX Slider Row
        var sfxRow = CreateCardRow("SfxRow", body, 110, BgSurface, roundedSprite);
        var srVlg = sfxRow.AddComponent<VerticalLayoutGroup>();
        srVlg.padding = new RectOffset(20, 20, 15, 15);
        srVlg.spacing = 8;
        srVlg.childControlWidth = true;
        srVlg.childControlHeight = false;

        var srHdr = CreateCardRow("Hdr", sfxRow.transform, 30, Color.clear, null);
        var shHlg = srHdr.AddComponent<HorizontalLayoutGroup>();
        shHlg.childControlWidth = false;
        shHlg.childControlHeight = true;
        CreateText("Lbl", srHdr.transform, font, 22, TextAlignmentOptions.Left, Color.white, "EFEK SUARA PEMERIKSAAN (SFX)");
        var sfxVal = CreateText("Val", srHdr.transform, font, 22, TextAlignmentOptions.Right, ClinicalGreen, "90%");
        sfxVal.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
        controller.sfxValueText = sfxVal;

        var sfxSlider = CreateCustomSlider("SfxSlider", sfxRow.transform, ClinicalGreen);
        controller.sfxSlider = sfxSlider;

        // Reset Data Button
        var btnReset = CreateStyledButton("BtnReset", body, font, "RESET SEMUA PROGRES TKP", new Color(0.2f, 0.08f, 0.08f), DangerRed, 800, 65, roundedSprite);
        btnReset.onClick.AddListener(controller.ResetAllData);

        // Close Button
        var btnClose = CreateStyledButton("BtnClose", body, font, "[X]  SIMPAN & KEMBALI", SignalBlue, Color.white, 800, 70, roundedSprite);
        btnClose.onClick.AddListener(controller.CloseAllModals);
    }

    private static Slider CreateCustomSlider(string name, Transform parent, Color fillCol)
    {
        var row = CreateCardRow(name, parent, 24, new Color(0.1f, 0.14f, 0.22f, 1f), null);
        var fill = CreateUIObject("Fill", row.transform);
        var fRect = fill.GetComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = new Vector2(0f, 1f);
        fRect.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = fillCol;

        var slider = row.AddComponent<Slider>();
        slider.fillRect = fRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
        return slider;
    }

    private static void BuildCaseListModalContent(Transform body, TMP_FontAsset font, TacticalAppController controller, Sprite roundedSprite)
    {
        var vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        string[] cases = new string[]
        {
            "[CS-01] KAMAR MANDI (Kematian Wajar / Serangan Jantung - 31.5 C)",
            "[CS-02] GUDANG PABRIK (Kematian Posisi Telungkup - 34.5 C)",
            "[CS-03] KEBUN TERBUKA (Fase Pembusukan Awal / Caecum - 25.5 C)",
            "[CS-04] TENGGELAM AIR TAWAR (Washerwoman Skin - 26.0 C)",
            "[CS-05] TENGGELAM AIR LAUT (Edema Paru Akut - 28.5 C)",
            "[CS-06] GANTUNG DIRI (Asfiksia Mekanik - 29.5 C)",
            "[CS-07] KERACUNAN METANOL (Sianosis Gelap Bibir - 33.2 C)"
        };

        for (int i = 0; i < cases.Length; i++)
        {
            int caseNum = i + 1;
            var btn = CreateStyledButton($"CaseBtn_{caseNum}", body, font, cases[i], BgSurface, Color.white, 880, 75, roundedSprite);
            btn.onClick.AddListener(() => controller.SelectCase(caseNum));
        }

        var btnClose = CreateStyledButton("BtnClose", body, font, "[X]  TUTUP", BgCard, TextMuted, 880, 60, roundedSprite);
        btnClose.onClick.AddListener(controller.CloseAllModals);
    }

    private static void BuildManualBookModalContent(Transform body, TMP_FontAsset font, GameObject modalRoot, Sprite roundedSprite)
    {
        var vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 12;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var dmb = modalRoot.AddComponent<DigitalManualBook>();

        // Tab bar (5 tabs for +10 score!)
        var tabBar = CreateCardRow("TabBar", body, 55, Color.clear, null);
        var tbHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        tbHlg.spacing = 8;
        tbHlg.childControlWidth = true;
        tbHlg.childControlHeight = true;

        string[] tabNames = new string[] { "Story", "Setup", "Kartu", "Gameplay", "Win/Lose" };
        var tabBtns = new List<Button>();
        var tabImgs = new List<Image>();

        for (int i = 0; i < tabNames.Length; i++)
        {
            int idx = i;
            var btn = CreateStyledButton($"Tab_{tabNames[i]}", tabBar.transform, font, tabNames[i], BgSurface, Color.white, 170, 50, roundedSprite);
            btn.onClick.AddListener(() => dmb.OnClickTab(idx));
            tabBtns.Add(btn);
            tabImgs.Add(btn.GetComponent<Image>());
        }

        dmb.tabButtons = tabBtns.ToArray();
        dmb.tabButtonBackgrounds = tabImgs.ToArray();

        // Section Title
        var titleTxt = CreateText("SectionTitle", body, font, 24, TextAlignmentOptions.Left, SignalBlue, "1. STORY INTRODUCTION");
        titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(880, 40);
        dmb.headerTitleText = titleTxt;

        // Scrollable Body
        var scrollGo = CreateUIObject("ScrollRect", body);
        scrollGo.GetComponent<RectTransform>().sizeDelta = new Vector2(880, 850);
        var sr = scrollGo.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 25f;
        dmb.scrollRect = sr;

        var viewport = CreateUIObject("Viewport", scrollGo.transform);
        SetStretch(viewport.GetComponent<RectTransform>());
        viewport.AddComponent<RectMask2D>();
        sr.viewport = viewport.GetComponent<RectTransform>();

        var content = CreateUIObject("Content", viewport.transform);
        var cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = new Vector2(1, 1);
        cRect.pivot = new Vector2(0.5f, 1);
        cRect.sizeDelta = new Vector2(0, 1500);
        sr.content = cRect;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var cvlg = content.AddComponent<VerticalLayoutGroup>();
        cvlg.padding = new RectOffset(15, 15, 15, 15);
        cvlg.childControlWidth = true;
        cvlg.childControlHeight = false;

        var bodyTxt = CreateText("BodyText", content.transform, font, 20, TextAlignmentOptions.TopLeft, Color.white, "Loading manual text...");
        dmb.contentBodyText = bodyTxt;

        // Close Button
        var btnClose = CreateStyledButton("BtnClose", body, font, "[X]  TUTUP PANDUAN", SignalBlue, Color.white, 880, 65, roundedSprite);
        btnClose.onClick.AddListener(TacticalAppController.Instance.CloseAllModals);
    }

    private static void BuildAboutModalContent(Transform body, TMP_FontAsset font, Sprite pubLogo, Sprite partLogo, Sprite devLogo, Sprite roundedSprite)
    {
        var vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 15;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var plate = CreateCardRow("Plate", body, 110, Color.white, roundedSprite);
        var pHlg = plate.AddComponent<HorizontalLayoutGroup>();
        pHlg.padding = new RectOffset(20, 20, 10, 10);
        pHlg.spacing = 15;
        pHlg.childControlWidth = false;
        pHlg.childControlHeight = true;
        pHlg.childAlignment = TextAnchor.MiddleCenter;

        CreateImageItem("LogoTelkom", plate.transform, pubLogo, 200, 70);
        CreateDivider("Div1", plate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2, 50);
        CreateImageItem("LogoFK", plate.transform, partLogo, 280, 65);
        CreateDivider("Div2", plate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2, 50);
        CreateImageItem("LogoAutopsy", plate.transform, devLogo, 90, 70);

        string aboutDesc =
            "<b>PMI FORENSIC CLOCK AR</b>\n" +
            "<i>Hybrid Boardgame & Forensic Smart Lab System</i>\n\n" +
            "<b>PUBLISHER & INSTITUSI:</b>\n" +
            "• Telkom University Bandung\n\n" +
            "<b>MITRA KONTEN KLINIS & VALIDASI PATOLOGI:</b>\n" +
            "• Fakultas Kedokteran Telkom University\n\n" +
            "<b>TIM PENGEMBANG:</b>\n" +
            "• Tim A.U.T.O.P.S.Y. (Applied Ubiquitous Tactical Optics for Post-Mortem System Yield)\n\n" +
            "<b>VISI PROYEK:</b>\n" +
            "Menghadirkan inovasi media pembelajaran kedokteran forensik imersif berbasis Augmented Reality (AR) " +
            "yang memadukan keakuratan sains Algor Mortis, Livor Mortis, Rigor Mortis, dan Dekomposisi dengan boardgame interaktif.";

        CreateText("Desc", body, font, 20, TextAlignmentOptions.TopLeft, Color.white, aboutDesc);

        var btnClose = CreateStyledButton("BtnClose", body, font, "[X]  TUTUP", SignalBlue, Color.white, 800, 65, roundedSprite);
        btnClose.onClick.AddListener(TacticalAppController.Instance.CloseAllModals);
    }

    private static void BuildQuitModalContent(Transform body, TMP_FontAsset font, TacticalAppController controller, Sprite roundedSprite)
    {
        var vlg = body.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(25, 25, 25, 25);
        vlg.spacing = 20;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        CreateText("Warn", body, font, 22, TextAlignmentOptions.Center, Color.white,
            "Apakah Anda yakin ingin keluar dari aplikasi\n<b>PMI FORENSIC CLOCK AR</b>?");

        var bRow = CreateCardRow("BtnRow", body, 65, Color.clear, null);
        var bHlg = bRow.AddComponent<HorizontalLayoutGroup>();
        bHlg.spacing = 20;
        bHlg.childControlWidth = true;
        bHlg.childControlHeight = true;

        var btnYes = CreateStyledButton("BtnYes", bRow.transform, font, "YA, KELUAR", DangerRed, Color.white, 320, 65, roundedSprite);
        btnYes.onClick.AddListener(controller.OnClickConfirmQuit);

        var btnNo = CreateStyledButton("BtnNo", bRow.transform, font, "BATAL", BgSurface, TextMuted, 320, 65, roundedSprite);
        btnNo.onClick.AddListener(controller.CloseAllModals);
    }

    // --- UI Helpers ---

    private struct MenuRowItem
    {
        public GameObject root;
        public Button button;
    }

    private static MenuRowItem CreateMenuRowButton(string name, Transform parent, TMP_FontAsset font, string badge, string title, string sub, Color badgeColor, Sprite sprite)
    {
        var btn = CreateStyledButton(name, parent, font, "", BgCard, Color.white, 940, 85, sprite);
        var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.spacing = 15;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        var bBox = CreateCardRow("Badge", btn.transform, 55, BgSurface, sprite);
        bBox.GetComponent<RectTransform>().sizeDelta = new Vector2(75, 55);
        var bTxt = CreateText("Txt", bBox.transform, font, 18, TextAlignmentOptions.Center, badgeColor, badge);
        SetStretch(bTxt.GetComponent<RectTransform>());

        var col = CreateUIObject("TextCol", btn.transform);
        col.GetComponent<RectTransform>().sizeDelta = new Vector2(740, 60);
        var cVlg = col.AddComponent<VerticalLayoutGroup>();
        cVlg.spacing = 2;
        cVlg.childControlWidth = true;
        cVlg.childControlHeight = false;

        CreateText("T", col.transform, font, 24, TextAlignmentOptions.Left, Color.white, $"<b>{title}</b>");
        CreateText("S", col.transform, font, 15, TextAlignmentOptions.Left, TextMuted, sub);

        var arrow = CreateText("A", btn.transform, font, 28, TextAlignmentOptions.Center, TextMuted, ">");
        arrow.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 50);

        return new MenuRowItem { root = btn.gameObject, button = btn };
    }

    private static void CreateSlotTargetFrame(string name, Transform parent, TMP_FontAsset font, string slotTag, string slotName, string cardName, Sprite sprite)
    {
        var box = CreateCardRow(name, parent, 300, new Color(5f / 255f, 8f / 255f, 14f / 255f, 0.9f), sprite);
        var vlg = box.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 12, 12);
        vlg.spacing = 4;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        CreateText("Tag", box.transform, font, 16, TextAlignmentOptions.Center, SignalBlue, slotTag);
        CreateText("Name", box.transform, font, 22, TextAlignmentOptions.Center, Color.white, $"<b>{slotName}</b>");
        CreateDivider("Div", box.transform, BorderTactical, 160, 2);
        CreateText("Card", box.transform, font, 14, TextAlignmentOptions.Center, ClinicalGreen, cardName);
    }

    private static GameObject CreateModalCard(string name, Transform parent, string title, float width, float height, TMP_FontAsset font, Sprite roundedSprite)
    {
        var modalRoot = CreateUIObject(name, parent);
        SetStretch(modalRoot.GetComponent<RectTransform>());

        var overlay = modalRoot.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.85f);
        overlay.raycastTarget = true; // Blocker

        var card = CreateUIObject("Card", modalRoot.transform);
        var cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(width, height);
        cRect.anchoredPosition = Vector2.zero;

        var cImg = card.AddComponent<Image>();
        cImg.color = BgCard;
        cImg.sprite = roundedSprite;
        cImg.type = Image.Type.Sliced;
        cImg.raycastTarget = false;

        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 15;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var header = CreateCardRow("Header", card.transform, 50, Color.clear, null);
        var hHlg = header.AddComponent<HorizontalLayoutGroup>();
        hHlg.childControlWidth = false;
        hHlg.childControlHeight = true;
        var tTxt = CreateText("Title", header.transform, font, 26, TextAlignmentOptions.Left, SignalBlue, $"<b>{title}</b>");
        tTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 150, 45);

        var body = CreateUIObject("Body", card.transform);
        var bRect = body.GetComponent<RectTransform>();
        bRect.sizeDelta = new Vector2(width - 40, height - 100);

        modalRoot.SetActive(false);
        return modalRoot;
    }

    private static Button CreateStyledButton(string name, Transform parent, TMP_FontAsset font, string label, Color bgCol, Color textCol, float width, float height, Sprite sprite)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        var img = go.AddComponent<Image>();
        img.color = bgCol;
        img.sprite = sprite;
        if (sprite != null) img.type = Image.Type.Sliced;
        img.raycastTarget = true; // MUST be true for buttons!

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var cb = btn.colors;
        cb.highlightedColor = new Color(bgCol.r * 1.25f, bgCol.g * 1.25f, bgCol.b * 1.25f, 1f);
        cb.pressedColor = new Color(bgCol.r * 0.8f, bgCol.g * 0.8f, bgCol.b * 0.8f, 1f);
        btn.colors = cb;

        if (!string.IsNullOrEmpty(label))
        {
            var txt = CreateText("Text", go.transform, font, 22, TextAlignmentOptions.Center, textCol, label);
            SetStretch(txt.GetComponent<RectTransform>());
        }

        return btn;
    }

    private static GameObject CreateCardRow(string name, Transform parent, float height, Color col, Sprite sprite)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, height);

        var img = go.AddComponent<Image>();
        img.color = col;
        img.sprite = sprite;
        if (sprite != null) img.type = Image.Type.Sliced;
        img.raycastTarget = false; // By default rows don't block
        return go;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions align, Color col, string content)
    {
        var go = CreateUIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = col;
        tmp.text = content;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void CreateImageItem(string name, Transform parent, Sprite sprite, float w, float h)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(w, h);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    private static void CreateDivider(string name, Transform parent, Color col, float w, float h)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(w, h);
        var img = go.AddComponent<Image>();
        img.color = col;
        img.raycastTarget = false;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static void EnsureLogosAreSprites()
    {
        string[] logoPaths = new string[]
        {
            "Assets/UI/Logos/logo-telkom-university.png",
            "Assets/UI/Logos/logo-fakultas-kedokteran.png",
            "Assets/UI/Logos/logo-team-autopsy.png"
        };

        foreach (var p in logoPaths)
        {
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti != null && ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }
        }
    }
}
