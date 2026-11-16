using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public static class MainMenuBuilder
{
    // Tema Warna Gelap Taktis
    private static readonly Color ColorBgBase      = new Color(10f / 255f, 14f / 255f, 23f / 255f, 1f);     // #0A0E17
    private static readonly Color ColorPanel       = new Color(13f / 255f, 19f / 255f, 31f / 255f, 1f);     // #0D131F
    private static readonly Color ColorCardItem    = new Color(19f / 255f, 27f / 255f, 44f / 255f, 0.96f);  // #131B2C
    private static readonly Color ColorBorder      = new Color(30f / 255f, 41f / 255f, 59f / 255f, 1f);     // #1E293B
    private static readonly Color ColorBlueAccent  = new Color(59f / 255f, 130f / 255f, 246f / 255f, 1f);  // #3B82F6
    private static readonly Color ColorCyanAccent  = new Color(34f / 255f, 211f / 255f, 238f / 255f, 1f);  // #22D3EE
    private static readonly Color ColorTextBody    = new Color(203f / 255f, 213f / 255f, 225f / 255f, 1f); // #CBD5E1
    private static readonly Color ColorTextMuted   = new Color(148f / 255f, 163f / 255f, 184f / 255f, 1f); // #94A3B8
    private static readonly Color ColorDangerBg    = new Color(36f / 255f, 15f / 255f, 15f / 255f, 1f);     // #240F0F
    private static readonly Color ColorDangerText  = new Color(248f / 255f, 113f / 255f, 113f / 255f, 1f); // #F87171

    [MenuItem("Tools/Reset & Rebuild Main Menu (Dangerous)", false, 100)]
    public static void BuildMainMenu()
    {
        if (!Application.isBatchMode)
        {
            if (!EditorUtility.DisplayDialog("Konfirmasi Reset Main Menu",
                "PERINGATAN: Perintah ini akan menghapus dan membuat ulang seluruh elemen Canvas Main Menu dari awal, yang akan menimpa penyesuaian posisi/layout yang sudah Anda atur.\n\nApakah Anda benar-benar ingin mereset layout ke pengaturan awal?",
                "Ya, Reset", "Batal (Jangan Reset)"))
            {
                Debug.Log("[MainMenuBuilder] Pembuatan dibatalkan: Layout Main Menu tetap aman dan tidak diubah.");
                return;
            }
        }

        BuildMainMenuDirect();
    }

    public static void BuildMainMenuDirect()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        Debug.Log($"[MainMenuBuilder] Membangun Main Menu di scene: {activeScene.name}...");

        EnsureAssetDirectories();
        EnsureLogosImported();
        EnsureBackgroundSprite();

        Sprite logoTelkom  = FindSpriteByName("logo-telkom-university");
        Sprite logoFK      = FindSpriteByName("logo-fakultas-kedokteran");
        Sprite logoAutopsy = FindSpriteByName("logo-team-autopsy");
        Sprite bgSprite    = FindSpriteByName("background");

        Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/RoundedCard_9Slice.png");
        Sprite heroSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/HeroGradient_9Slice.png");

        TMP_FontAsset font = GetPreferredFont();

        // 1. Setup EventSystem
        SetupEventSystem();

        // 2. Hapus Canvas lama jika ada
        CleanupOldObjects();

        // 3. Buat Canvas Root
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // 4. Background Animasi & Dark Overlay
        var bgGo = CreateUIObject("Background", canvasGo.transform);
        SetStretch(bgGo.GetComponent<RectTransform>());
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = bgSprite != null ? Color.white : ColorBgBase;
        bgImg.sprite = bgSprite;
        bgImg.raycastTarget = false;
        bgGo.AddComponent<AnimatedBackground>();

        var overlayGo = CreateUIObject("DarkOverlay", bgGo.transform);
        SetStretch(overlayGo.GetComponent<RectTransform>());
        var overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(10f / 255f, 14f / 255f, 23f / 255f, 0.78f);
        overlayImg.raycastTarget = false;

        // 5. MenuController Root
        var menuControllerGo = new GameObject("MenuController");
        var mainMenu = menuControllerGo.AddComponent<MainMenu>();

        // =====================================================================
        // PANEL 0: SPLASH SCREEN (MUNCUL SATU PER SATU & HD, TANPA TEKS DI BAWAH)
        // =====================================================================
        var splashGo = CreateModalOverlay("SplashScreenPanel", canvasGo.transform);
        splashGo.GetComponent<Image>().color = ColorBgBase;

        var wholeSplashCg = splashGo.AddComponent<CanvasGroup>();

        var splashSeq = splashGo.AddComponent<SplashScreenSequence>();
        splashSeq.wholeSplashGroup = wholeSplashCg;
        splashSeq.mainMenu = mainMenu;

        var splashBtn = splashGo.AddComponent<Button>();
        splashBtn.targetGraphic = splashGo.GetComponent<Image>();

        // Plat Logo Tengah (DIJAMIN DI TENGAH LAYAR & SEMUA LOGO TERLIHAT)
        var splashCenter = CreateUIObject("SplashCenter", splashGo.transform);
        var scRect = splashCenter.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0.5f, 0.5f);
        scRect.anchorMax = new Vector2(0.5f, 0.5f);
        scRect.pivot = new Vector2(0.5f, 0.5f);
        scRect.sizeDelta = new Vector2(940f, 160f);
        scRect.anchoredPosition = Vector2.zero;

        var plateCg = splashCenter.AddComponent<CanvasGroup>();
        splashSeq.plateGroup = plateCg;

        var splashPlate = CreateUIObject("SplashLogoPlate", splashCenter.transform);
        SetStretch(splashPlate.GetComponent<RectTransform>());
        var spImg = splashPlate.AddComponent<Image>();
        spImg.color = Color.white;
        spImg.sprite = roundedSprite;
        if (roundedSprite != null) spImg.type = Image.Type.Sliced;
        spImg.raycastTarget = false;

        var spHlg = splashPlate.AddComponent<HorizontalLayoutGroup>();
        spHlg.padding = new RectOffset(20, 20, 15, 15);
        spHlg.spacing = 22;
        spHlg.childControlWidth = false;
        spHlg.childControlHeight = false;
        spHlg.childForceExpandWidth = false;
        spHlg.childForceExpandHeight = false;
        spHlg.childAlignment = TextAnchor.MiddleCenter;

        // Logo 1: Telkom University
        var l1Go = CreateImageItem("LogoTelkom", splashPlate.transform, logoTelkom, 220f, 72f);
        splashSeq.logo1Group = l1Go.AddComponent<CanvasGroup>();

        // Divider 1
        var d1Go = CreateDivider("Div1", splashPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2f, 55f);
        splashSeq.div1Group = d1Go.AddComponent<CanvasGroup>();

        // Logo 2: Fakultas Kedokteran Telkom University
        var l2Go = CreateImageItem("LogoFK", splashPlate.transform, logoFK, 260f, 72f);
        splashSeq.logo2Group = l2Go.AddComponent<CanvasGroup>();

        // Divider 2
        var d2Go = CreateDivider("Div2", splashPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2f, 55f);
        splashSeq.div2Group = d2Go.AddComponent<CanvasGroup>();

        // Logo 3: Tim A.U.T.O.P.S.Y. (TAMPIL JELAS, PROPORSIONAL & DI TENGAH)
        var l3Go = CreateImageItem("LogoAutopsy", splashPlate.transform, logoAutopsy, 95f, 82f);
        splashSeq.logo3Group = l3Go.AddComponent<CanvasGroup>();

        // Tombol Lewati di pojok kanan atas
        var btnSkip = CreateStyledButton("BtnSkipSplash", splashGo.transform, font, "LEWATI >", Color.clear, ColorCyanAccent, 180f, 50f, null);
        var skipRect = btnSkip.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(1f, 1f);
        skipRect.anchorMax = new Vector2(1f, 1f);
        skipRect.pivot = new Vector2(1f, 1f);
        skipRect.anchoredPosition = new Vector2(-40f, -50f);

        // =====================================================================
        // PANEL 1: PANEL MENU (MENU UTAMA - TATA LETAK LEGA, FULL SCREEN & SEIMBANG)
        // =====================================================================
        var panelMenuGo = CreateUIObject("PanelMenu", canvasGo.transform);
        SetStretch(panelMenuGo.GetComponent<RectTransform>());

        var centerContainer = CreateUIObject("CenterContainer", panelMenuGo.transform);
        var ccRect = centerContainer.GetComponent<RectTransform>();
        ccRect.anchorMin = new Vector2(0.5f, 0.5f);
        ccRect.anchorMax = new Vector2(0.5f, 0.5f);
        ccRect.pivot = new Vector2(0.5f, 0.5f);
        ccRect.sizeDelta = new Vector2(940f, 1680f);
        ccRect.anchoredPosition = Vector2.zero;

        // 1. Plat Putih 3 Logo (Atas - Bersih & Rapi)
        var logoPlate = CreateCard("LogoPlate", centerContainer.transform, 110f, Color.white, roundedSprite);
        var lpRect = logoPlate.GetComponent<RectTransform>();
        lpRect.anchorMin = new Vector2(0f, 1f);
        lpRect.anchorMax = new Vector2(0f, 1f);
        lpRect.pivot = new Vector2(0.5f, 0.5f);
        lpRect.sizeDelta = new Vector2(940f, 110f);
        lpRect.anchoredPosition = new Vector2(470f, -72f);

        var lpHlg = logoPlate.AddComponent<HorizontalLayoutGroup>();
        lpHlg.padding = new RectOffset(20, 20, 12, 12);
        lpHlg.spacing = 22;
        lpHlg.childControlWidth = false;
        lpHlg.childControlHeight = false;
        lpHlg.childForceExpandWidth = false;
        lpHlg.childForceExpandHeight = false;
        lpHlg.childAlignment = TextAnchor.MiddleCenter;

        CreateImageItem("LogoTelkom", logoPlate.transform, logoTelkom, 210f, 68f);
        CreateDivider("Div1", logoPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2f, 50f);
        CreateImageItem("LogoFK", logoPlate.transform, logoFK, 250f, 68f);
        CreateDivider("Div2", logoPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2f, 50f);
        CreateImageItem("LogoAutopsy", logoPlate.transform, logoAutopsy, 90f, 78f);

        // 2. Judul & Subjudul (Tipografi Taktis Modern)
        var titleBox = CreateCard("TitleBox", centerContainer.transform, 145f, Color.clear, null);
        var tbRect = titleBox.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 1f);
        tbRect.anchorMax = new Vector2(0f, 1f);
        tbRect.pivot = new Vector2(0.5f, 0.5f);
        tbRect.sizeDelta = new Vector2(940f, 145f);
        tbRect.anchoredPosition = new Vector2(470f, -290f);

        var tbVlg = titleBox.AddComponent<VerticalLayoutGroup>();
        tbVlg.spacing = 6;
        tbVlg.childControlWidth = true;
        tbVlg.childControlHeight = false;
        tbVlg.childAlignment = TextAnchor.MiddleCenter;

        CreateText("SubBadge", titleBox.transform, font, 18f, TextAlignmentOptions.Center, ColorCyanAccent,
            "<b>AR FORENSIC MEDICINE INVESTIGATION</b>");
        CreateText("MainTitle", titleBox.transform, font, 54f, TextAlignmentOptions.Center, Color.white,
            "<b>PMI FORENSIC CLOCK</b>");
        CreateText("SubTitle", titleBox.transform, font, 20f, TextAlignmentOptions.Center, ColorTextBody,
            "FK TELKOM UNIVERSITY × TIM A.U.T.O.P.S.Y.");

        // 3. Tombol-Tombol Menu Utama (Sleek, Jelas & Berjarak Rapi)
        var buttonGroup = CreateUIObject("ButtonGroup", centerContainer.transform);
        var bgRect = buttonGroup.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(920f, 720f);
        bgRect.anchoredPosition = new Vector2(470f, -943f);

        // Tombol 1: MULAI INVESTIGASI AR
        var btnMulai = CreateStyledButton("BtnMulai", buttonGroup.transform, font,
            "<b><size=30>[ ▶ ]  MULAI INVESTIGASI AR</size></b>\n<size=17><color=#BFDBFE>Scan Kartu Tubuh & Kasus TKP</color></size>",
            ColorBlueAccent, Color.white, 920f, 120f, heroSprite ?? roundedSprite);
        var bmRect = btnMulai.GetComponent<RectTransform>();
        bmRect.anchorMin = new Vector2(0f, 1f);
        bmRect.anchorMax = new Vector2(0f, 1f);
        bmRect.pivot = new Vector2(0.5f, 0.5f);
        bmRect.sizeDelta = new Vector2(920f, 120f);
        bmRect.anchoredPosition = new Vector2(460f, -85f);

        // Tombol 2: MANUAL BOOK
        var btnManual = CreateStyledButton("BtnManualBook", buttonGroup.transform, font,
            "<b><size=26>[ ≡ ]  BUKU PANDUAN DIGITAL</size></b>\n<size=15><color=#94A3B8>Penjelasan 6 Kartu & Alur Pemeriksaan Medis</color></size>",
            ColorCardItem, ColorTextBody, 920f, 100f, roundedSprite);
        var bmanRect = btnManual.GetComponent<RectTransform>();
        bmanRect.anchorMin = new Vector2(0f, 1f);
        bmanRect.anchorMax = new Vector2(0f, 1f);
        bmanRect.pivot = new Vector2(0.5f, 0.5f);
        bmanRect.sizeDelta = new Vector2(920f, 100f);
        bmanRect.anchoredPosition = new Vector2(460f, -228f);

        // Tombol 3: TENTANG PENGEMBANG
        var btnAbout = CreateStyledButton("BtnAbout", buttonGroup.transform, font,
            "<b><size=26>[ ℹ ]  TENTANG PENGEMBANG</size></b>\n<size=15><color=#94A3B8>Profil Tim A.U.T.O.P.S.Y. & Pembimbing</color></size>",
            ColorCardItem, ColorTextBody, 920f, 100f, roundedSprite);
        var baRect = btnAbout.GetComponent<RectTransform>();
        baRect.anchorMin = new Vector2(0f, 1f);
        baRect.anchorMax = new Vector2(0f, 1f);
        baRect.pivot = new Vector2(0.5f, 0.5f);
        baRect.sizeDelta = new Vector2(920f, 100f);
        baRect.anchoredPosition = new Vector2(460f, -358f);

        // Tombol 4: KELUAR (Diposisikan di Bawah dengan Jeda Ruang Rapi)
        var btnQuit = CreateStyledButton("BtnQuit", buttonGroup.transform, font,
            "<b><size=22><color=#F87171>[ ✕ ]  KELUAR APLIKASI</color></size></b>",
            ColorDangerBg, ColorDangerText, 920f, 85f, roundedSprite);
        var bqRect = btnQuit.GetComponent<RectTransform>();
        bqRect.anchorMin = new Vector2(0f, 1f);
        bqRect.anchorMax = new Vector2(0f, 1f);
        bqRect.pivot = new Vector2(0.5f, 0.5f);
        bqRect.sizeDelta = new Vector2(920f, 85f);
        bqRect.anchoredPosition = new Vector2(460f, -616f);

        // 4. Status Bar Bawah (Minimalis & Elegan di Bagian Bawah Layar)
        var footerBox = CreateCard("FooterBox", centerContainer.transform, 75f, ColorCardItem, roundedSprite);
        var fbRect = footerBox.GetComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(0f, 1f);
        fbRect.anchorMax = new Vector2(0f, 1f);
        fbRect.pivot = new Vector2(0.5f, 0.5f);
        fbRect.sizeDelta = new Vector2(940f, 75f);
        fbRect.anchoredPosition = new Vector2(470f, -1607f);
        var fbVlg = footerBox.AddComponent<VerticalLayoutGroup>();
        fbVlg.padding = new RectOffset(15, 15, 10, 10);
        fbVlg.spacing = 3;
        fbVlg.childControlWidth = true;
        fbVlg.childControlHeight = false;
        fbVlg.childAlignment = TextAnchor.MiddleCenter;

        CreateText("FootHdr", footerBox.transform, font, 15f, TextAlignmentOptions.Center, ColorCyanAccent,
            "<b>STATUS: SISTEM SIAP</b> <color=#475569>•</color> <color=#CBD5E1>ALGOR • LIVOR • RIGOR • DEKOMPOSISI</color>");
        CreateText("FootSub", footerBox.transform, font, 13f, TextAlignmentOptions.Center, ColorTextMuted,
            "LABORATORIUM SIMULASI KEDOKTERAN FORENSIK • TELKOM UNIVERSITY");

        // =====================================================================
        // PANEL 2: PANEL MANUAL (DESAIN TERSTRUKTUR DENGAN BADGE NOMOR RAPI)
        // =====================================================================
        var panelManualGo = CreateModalOverlay("PanelManual", canvasGo.transform);
        var manualCard = CreateModalCard("ManualCard", panelManualGo.transform, 980f, 1600f, roundedSprite);
        var manualBook = panelManualGo.AddComponent<ManualBook>();

        var mcVlg = manualCard.AddComponent<VerticalLayoutGroup>();
        mcVlg.padding = new RectOffset(30, 30, 30, 30);
        mcVlg.spacing = 18;
        mcVlg.childControlWidth = true;
        mcVlg.childControlHeight = false;

        // Header Manual
        CreateText("ManualBadge", manualCard.transform, font, 18f, TextAlignmentOptions.Left, ColorCyanAccent,
            "<b>* DOKUMEN FORENSIK DIGITAL // PANDUAN LENGKAP *</b>");
        CreateText("ManualTitle", manualCard.transform, font, 34f, TextAlignmentOptions.Left, Color.white,
            "<b>BUKU PANDUAN FORENSIK AR</b>");

        // Kontainer Halaman
        var pagesContainer = CreateUIObject("PagesContainer", manualCard.transform);
        pagesContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(920f, 1220f);

        // Halaman 1: Penjelasan 6 Kartu (Desain Box Card dengan Badge Nomor Bersih)
        var page1 = CreateManualPageWithCards("Page1_PenjelasanKartu", pagesContainer.transform, font, roundedSprite,
            new (string number, string title, string tag, string desc)[]
            {
                ("01", "KARTU KASUS (SKENARIO TKP)", "TARGET KASUS",
                 "Memuat kronologi penemuan jenazah, suhu lingkungan tempat kejadian perkara (°C), serta posisi fisik jenazah (terlentang, telungkup, tergantung, atau terendam air)."),
                ("02", "KARTU TUBUH (BLUEPRINT ANATOMI)", "TARGET UTAMA AR",
                 "Target utama Augmented Reality di meja laboratorium. Saat disorot kamera HP, avatar 3D jenazah patologi forensik akan muncul utuh di atas blueprint."),
                ("03", "KARTU TINDAKAN: TERMOMETER REKTAL", "ALGOR MORTIS",
                 "Digunakan untuk mendeteksi penurunan suhu inti tubuh korban. Memberikan data suhu derajat Celcius yang presisi untuk perhitungan rumus Nomogram Henssge."),
                ("04", "KARTU TINDAKAN: UJI TEKAN LEBAM", "LIVOR MORTIS",
                 "Melakukan Blanching Test pada lebam mayat: menentukan apakah lebam masih memudar saat ditekan (< 8 jam) atau sudah terfiksasi menetap (> 8-10 jam)."),
                ("05", "KARTU TINDAKAN: KACA PEMBESAR", "INSPEKSI KEPALA",
                 "Pemeriksaan kranial: mendeteksi kekeruhan kornea mata, petechiae konjungtiva, kongesti vena wajah, atau memar akibat trauma tumpul."),
                ("06", "KARTU TINDAKAN: INSPEKSI ABDOMEN", "FASE DEKOMPOSISI",
                 "Mendeteksi tanda pembusukan dini: bercak hijau (Greenish Discoloration) pada area caecum dan pola jaring vena menonjol (Marbling).")
            });

        // Halaman 2: Cara Bermain
        var page2 = CreateManualPageWithCards("Page2_CaraBermain", pagesContainer.transform, font, roundedSprite,
            new (string number, string title, string tag, string desc)[]
            {
                ("01", "TAHAP 1: IDENTIFIKASI KASUS TKP", "FASE AWAL",
                 "Ambil kartu kasus yang ditugaskan, catat suhu lingkungan TKP dan waktu perkiraan awal saat jasad ditemukan."),
                ("02", "TAHAP 2: SCAN KARTU TUBUH AR", "PELACAKAN OPTIK",
                 "Buka menu investigasi pada aplikasi, arahkan kamera HP ke kartu Blueprint Tubuh hingga avatar 3D patologi muncul stabil."),
                ("03", "TAHAP 3: DEKATKAN KARTU TINDAKAN", "INTERAKSI FISIK",
                 "Dekatkan kartu Termometer, Uji Tekan Lebam, Kaca Pembesar, atau Abdomen ke tubuh korban untuk mengamati respon visual dan data klinis."),
                ("04", "TAHAP 4: ESTIMASI POST-MORTEM INTERVAL (PMI)", "KALKULASI KLINIS",
                 "Gabungkan data Algor Mortis (suhu), Livor Mortis (fiksasi lebam), dan Dekomposisi untuk menghitung rentang jam kematian korban."),
                ("05", "TAHAP 5: TARIK KESIMPULAN FORENSIK", "DEDUKSI AKHIR",
                 "Cocokkan hasil deduksi Anda dengan hipotesis kasus. Buktikan waktu dan penyebab kematian untuk menyelesaikan investigasi!")
            });

        page2.SetActive(false);

        // Bar Navigasi Paginasi
        var navBar = CreateCard("NavBar", manualCard.transform, 65f, Color.clear, null);
        var nbHlg = navBar.AddComponent<HorizontalLayoutGroup>();
        nbHlg.spacing = 15;
        nbHlg.childControlWidth = false;
        nbHlg.childControlHeight = true;
        nbHlg.childAlignment = TextAnchor.MiddleCenter;

        var btnPrev = CreateStyledButton("BtnPrev", navBar.transform, font, "< SEBELUMNYA", ColorCardItem, ColorTextBody, 250f, 60f, roundedSprite);
        var pageIndTxt = CreateText("PageIndicator", navBar.transform, font, 20f, TextAlignmentOptions.Center, ColorCyanAccent, "HALAMAN 1 / 2");
        pageIndTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(250f, 50f);
        var btnNext = CreateStyledButton("BtnNext", navBar.transform, font, "SELANJUTNYA >", ColorCardItem, ColorTextBody, 250f, 60f, roundedSprite);

        // Tombol Kembali Paling Bawah
        var btnManualBack = CreateStyledButton("BtnManualBack", manualCard.transform, font, "◀ KEMBALI KE MENU UTAMA", ColorBlueAccent, Color.white, 920f, 70f, roundedSprite);

        manualBook.pages = new List<GameObject> { page1, page2 };
        manualBook.btnPrev = btnPrev;
        manualBook.btnNext = btnNext;
        manualBook.pageIndicator = pageIndTxt;

        // =====================================================================
        // PANEL 3: PANEL ABOUT (TENTANG PENGEMBANG - TOMBOL PINNED DI BAWAH)
        // =====================================================================
        var panelAboutGo = CreateModalOverlay("PanelAbout", canvasGo.transform);
        var aboutCard = CreateModalCard("AboutCard", panelAboutGo.transform, 980f, 1600f, roundedSprite);

        var acVlg = aboutCard.AddComponent<VerticalLayoutGroup>();
        acVlg.padding = new RectOffset(30, 30, 30, 30);
        acVlg.spacing = 18;
        acVlg.childControlWidth = true;
        acVlg.childControlHeight = false;

        CreateText("AboutBadge", aboutCard.transform, font, 18f, TextAlignmentOptions.Left, ColorCyanAccent,
            "<b>* PROFIL INSTITUSI & TIM PENGEMBANG *</b>");
        CreateText("AboutTitle", aboutCard.transform, font, 34f, TextAlignmentOptions.Left, Color.white,
            "<b>TENTANG PENGEMBANG & MITRA</b>");

        // Plat Putih 3 Logo di dalam About
        var aboutPlate = CreateCard("AboutLogoPlate", aboutCard.transform, 120f, Color.white, roundedSprite);
        var apHlg = aboutPlate.AddComponent<HorizontalLayoutGroup>();
        apHlg.padding = new RectOffset(25, 25, 12, 12);
        apHlg.spacing = 20;
        apHlg.childControlWidth = false;
        apHlg.childControlHeight = true;
        apHlg.childAlignment = TextAnchor.MiddleCenter;

        CreateImageItem("LogoTelkom", aboutPlate.transform, logoTelkom, 240f, 80f);
        CreateDivider("Div1", aboutPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2f, 65f);
        CreateImageItem("LogoFK", aboutPlate.transform, logoFK, 330f, 75f);
        CreateDivider("Div2", aboutPlate.transform, new Color(203f / 255f, 213f / 255f, 225f / 255f), 2f, 65f);
        CreateImageItem("LogoAutopsy", aboutPlate.transform, logoAutopsy, 105f, 80f);

        // Konten Scrollable About
        var aboutScroll = CreateScrollableContainer("AboutScroll", aboutCard.transform, 920f, 1180f);

        CreateAboutSection(aboutScroll, font, roundedSprite, "PMI FORENSIC CLOCK AR",
            "Media Pembelajaran Patologi Kedokteran Forensik Berbasis Augmented Reality.");

        CreateAboutSection(aboutScroll, font, roundedSprite, "INSTITUSI PUBLISHER",
            "• Telkom University Bandung");

        CreateAboutSection(aboutScroll, font, roundedSprite, "FAKULTAS MITRA KLINIS & PATOLOGI",
            "• Fakultas Kedokteran Telkom University");

        CreateAboutSection(aboutScroll, font, roundedSprite, "TIM PENGEMBANG",
            "• Tim A.U.T.O.P.S.Y. (Applied Ubiquitous Tactical Optics for Post-Mortem System Yield)");

        CreateAboutSection(aboutScroll, font, roundedSprite, "ANGGOTA KELOMPOK",
            "• [ISI NAMA ANGGOTA 1]\n• [ISI NAMA ANGGOTA 2]\n• [ISI NAMA ANGGOTA 3]");

        CreateAboutSection(aboutScroll, font, roundedSprite, "DOSEN PEMBIMBING",
            "• [ISI NAMA DOSEN PEMBIMBING]");

        CreateAboutSection(aboutScroll, font, roundedSprite, "TUJUAN PENGEMBANGAN",
            "Memadukan sains patologi forensik Algor Mortis, Livor Mortis, Rigor Mortis, dan Dekomposisi " +
            "dengan interaksi boardgame fisik untuk meningkatkan penalaran klinis mahasiswa kedokteran.");

        // Tombol Kembali Paling Bawah (TIDAK LAGI DI TENGAH!)
        var btnAboutBack = CreateStyledButton("BtnAboutBack", aboutCard.transform, font, "◀ KEMBALI KE MENU UTAMA", ColorBlueAccent, Color.white, 920f, 70f, roundedSprite);

        // =====================================================================
        // WIRE-UP LANGSUNG & SERIALIZED
        // =====================================================================
        mainMenu.splashScreenPanel = splashGo;
        mainMenu.panelMenu = panelMenuGo;
        mainMenu.panelManual = panelManualGo;
        mainMenu.panelAbout = panelAboutGo;
        mainMenu.gameScene = "ASS 2";

        var soMenu = new SerializedObject(mainMenu);
        soMenu.Update();
        soMenu.FindProperty("splashScreenPanel").objectReferenceValue = splashGo;
        soMenu.FindProperty("panelMenu").objectReferenceValue = panelMenuGo;
        soMenu.FindProperty("panelManual").objectReferenceValue = panelManualGo;
        soMenu.FindProperty("panelAbout").objectReferenceValue = panelAboutGo;
        soMenu.FindProperty("gameScene").stringValue = "ASS 2";
        soMenu.ApplyModifiedProperties();

        var soManual = new SerializedObject(manualBook);
        soManual.Update();
        var propPages = soManual.FindProperty("pages");
        if (propPages != null)
        {
            propPages.ClearArray();
            propPages.InsertArrayElementAtIndex(0);
            propPages.GetArrayElementAtIndex(0).objectReferenceValue = page1;
            propPages.InsertArrayElementAtIndex(1);
            propPages.GetArrayElementAtIndex(1).objectReferenceValue = page2;
        }
        soManual.FindProperty("btnPrev").objectReferenceValue = btnPrev;
        soManual.FindProperty("btnNext").objectReferenceValue = btnNext;
        soManual.FindProperty("pageIndicator").objectReferenceValue = pageIndTxt;
        soManual.ApplyModifiedProperties();

        // Persistent Listener OnClick Tombol
        UnityEventTools.AddPersistentListener(splashBtn.onClick, new UnityAction(splashSeq.Skip));
        UnityEventTools.AddPersistentListener(btnSkip.onClick, new UnityAction(splashSeq.Skip));

        UnityEventTools.AddPersistentListener(btnMulai.onClick, new UnityAction(mainMenu.Play));
        UnityEventTools.AddPersistentListener(btnManual.onClick, new UnityAction(mainMenu.ShowManual));
        UnityEventTools.AddPersistentListener(btnAbout.onClick, new UnityAction(mainMenu.ShowAbout));
        UnityEventTools.AddPersistentListener(btnQuit.onClick, new UnityAction(mainMenu.Quit));

        UnityEventTools.AddPersistentListener(btnManualBack.onClick, new UnityAction(mainMenu.ShowMenu));
        UnityEventTools.AddPersistentListener(btnAboutBack.onClick, new UnityAction(mainMenu.ShowMenu));

        UnityEventTools.AddPersistentListener(btnPrev.onClick, new UnityAction(manualBook.PrevPage));
        UnityEventTools.AddPersistentListener(btnNext.onClick, new UnityAction(manualBook.NextPage));

        // State awal saat edit mode
        panelManualGo.SetActive(false);
        panelAboutGo.SetActive(false);
        splashGo.SetActive(true);
        panelMenuGo.SetActive(false);

        EditorUtility.SetDirty(mainMenu);
        EditorUtility.SetDirty(manualBook);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[MainMenuBuilder] SUKSES! Splash screen sequensial HD, Main Menu terisi penuh, Manual Book berbadge rapi, dan Panel About selesai dibangun.");
    }

    private static void CleanupOldObjects()
    {
        string[] targetNames = new string[] { "Canvas", "MainMenuCanvas", "IntroductionCanvas", "MenuController" };
        var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            if (root == null) continue;
            foreach (var target in targetNames)
            {
                if (root.name == target)
                {
                    Object.DestroyImmediate(root);
                    break;
                }
            }
        }
    }

    private static void SetupEventSystem()
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

        var legacyModule = esGo.GetComponent<StandaloneInputModule>();
        if (legacyModule != null) Object.DestroyImmediate(legacyModule);

#if ENABLE_INPUT_SYSTEM
        var inputModule = esGo.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = esGo.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            var defaultAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (defaultAsset != null)
            {
                inputModule.actionsAsset = defaultAsset;
            }
            else
            {
                try
                {
                    inputModule.enabled = false;
                    inputModule.AssignDefaultActions();
                }
                catch {}
                finally
                {
                    inputModule.enabled = true;
                }
            }
        }
        inputModule.enabled = true;
#else
        var sm = esGo.GetComponent<StandaloneInputModule>();
        if (sm == null) esGo.AddComponent<StandaloneInputModule>();
#endif
    }

    private static GameObject CreateModalOverlay(string name, Transform parent)
    {
        var root = CreateUIObject(name, parent);
        SetStretch(root.GetComponent<RectTransform>());

        var img = root.AddComponent<Image>();
        img.color = new Color(5f / 255f, 8f / 255f, 14f / 255f, 0.96f);
        img.raycastTarget = true;
        return root;
    }

    private static GameObject CreateModalCard(string name, Transform parent, float width, float height, Sprite sprite)
    {
        var card = CreateUIObject(name, parent);
        var rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = Vector2.zero;

        var img = card.AddComponent<Image>();
        img.color = ColorPanel;
        img.sprite = sprite;
        if (sprite != null) img.type = Image.Type.Sliced;
        img.raycastTarget = true;
        return card;
    }

    private static Transform CreateScrollableContainer(string name, Transform parent, float width, float height)
    {
        var root = CreateUIObject(name, parent);
        var rRect = root.GetComponent<RectTransform>();
        rRect.sizeDelta = new Vector2(width, height);

        var sr = root.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 35f;

        var vp = CreateUIObject("Viewport", root.transform);
        SetStretch(vp.GetComponent<RectTransform>());
        vp.AddComponent<RectMask2D>();
        sr.viewport = vp.GetComponent<RectTransform>();

        var cnt = CreateUIObject("Content", vp.transform);
        var cRect = cnt.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0f, 1f);
        cRect.anchorMax = new Vector2(1f, 1f);
        cRect.pivot = new Vector2(0.5f, 1f);
        cRect.sizeDelta = new Vector2(0f, height);
        sr.content = cRect;

        var csf = cnt.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = cnt.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        return cnt.transform;
    }

    private static GameObject CreateManualPageWithCards(string name, Transform parent, TMP_FontAsset font, Sprite sprite, (string number, string title, string tag, string desc)[] items)
    {
        var page = CreateUIObject(name, parent);
        SetStretch(page.GetComponent<RectTransform>());

        var sr = page.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 35f;

        var vp = CreateUIObject("Viewport", page.transform);
        SetStretch(vp.GetComponent<RectTransform>());
        vp.AddComponent<RectMask2D>();
        sr.viewport = vp.GetComponent<RectTransform>();

        var cnt = CreateUIObject("Content", vp.transform);
        var cRect = cnt.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0f, 1f);
        cRect.anchorMax = new Vector2(1f, 1f);
        cRect.pivot = new Vector2(0.5f, 1f);
        cRect.sizeDelta = new Vector2(0f, 1200f);
        sr.content = cRect;

        var csf = cnt.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = cnt.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        foreach (var item in items)
        {
            var cardBox = CreateCard("CardItem", cnt.transform, 160f, ColorCardItem, sprite);
            var cbVlg = cardBox.AddComponent<VerticalLayoutGroup>();
            cbVlg.padding = new RectOffset(20, 20, 16, 16);
            cbVlg.spacing = 8;
            cbVlg.childControlWidth = true;
            cbVlg.childControlHeight = false;

            var csfItem = cardBox.AddComponent<ContentSizeFitter>();
            csfItem.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Baris Header Card (Nomor Badge di Kiri, Judul, dan Tag Kategori di Kanan)
            var topRow = CreateUIObject("HeaderRow", cardBox.transform);
            var trHlg = topRow.AddComponent<HorizontalLayoutGroup>();
            trHlg.childControlWidth = false;
            trHlg.childControlHeight = true;
            trHlg.spacing = 12;
            topRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);

            // Badge Nomor Kotak Rapi
            var numBadge = CreateCard("NumBadge", topRow.transform, 36f, ColorBorder, sprite);
            numBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 36f);
            var nbHlg = numBadge.AddComponent<HorizontalLayoutGroup>();
            nbHlg.childControlWidth = true;
            nbHlg.childControlHeight = true;
            CreateText("NumTxt", numBadge.transform, font, 18f, TextAlignmentOptions.Center, ColorCyanAccent, $"<b>{item.number}</b>");

            // Judul Kartu
            var titleTxt = CreateText("Title", topRow.transform, font, 21f, TextAlignmentOptions.MidlineLeft, Color.white, $"<b>{item.title}</b>");
            titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 38f);

            // Tag Kategori
            var tagTxt = CreateText("Tag", topRow.transform, font, 14f, TextAlignmentOptions.MidlineRight, ColorCyanAccent, $"<b>[{item.tag}]</b>");
            tagTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 38f);

            // Garis Pemisah Halus di Dalam Kartu
            CreateDivider("CardDiv", cardBox.transform, new Color(30f / 255f, 41f / 255f, 59f / 255f, 0.6f), 0f, 1.5f);

            // Deskripsi Penjelasan
            CreateText("Desc", cardBox.transform, font, 18f, TextAlignmentOptions.TopLeft, ColorTextBody, item.desc);
        }

        return page;
    }

    private static void CreateAboutSection(Transform parent, TMP_FontAsset font, Sprite sprite, string title, string content)
    {
        var box = CreateCard("AboutSection", parent, 110f, ColorCardItem, sprite);
        var vlg = box.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 16, 16);
        vlg.spacing = 6;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = box.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateText("SecTitle", box.transform, font, 19f, TextAlignmentOptions.Left, ColorCyanAccent, $"<b>{title}</b>");
        CreateText("SecContent", box.transform, font, 18f, TextAlignmentOptions.TopLeft, ColorTextBody, content);
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
        img.raycastTarget = true;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var cb = btn.colors;
        cb.highlightedColor = new Color(bgCol.r * 1.25f, bgCol.g * 1.25f, bgCol.b * 1.25f, 1f);
        cb.pressedColor = new Color(bgCol.r * 0.8f, bgCol.g * 0.8f, bgCol.b * 0.8f, 1f);
        btn.colors = cb;

        if (!string.IsNullOrEmpty(label))
        {
            var txt = CreateText("Label", go.transform, font, 22f, TextAlignmentOptions.Center, textCol, label);
            SetStretch(txt.GetComponent<RectTransform>());
        }

        return btn;
    }

    private static GameObject CreateCard(string name, Transform parent, float height, Color col, Sprite sprite)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, height);

        var img = go.AddComponent<Image>();
        img.color = col;
        img.sprite = sprite;
        if (sprite != null) img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        return go;
    }

    private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions align, Color col, string content)
    {
        var go = CreateUIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null)
        {
            try
            {
                tmp.font = font;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MainMenuBuilder] Gagal pasang font pada '{name}': {ex.Message}");
            }
        }
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = col;
        tmp.text = content;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static GameObject CreateImageItem(string name, Transform parent, Sprite sprite, float w, float h)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(w, h);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (sprite == null)
        {
            img.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        }
        return go;
    }

    private static GameObject CreateDivider(string name, Transform parent, Color col, float w, float h)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(w, h);
        var img = go.AddComponent<Image>();
        img.color = col;
        img.raycastTarget = false;
        return go;
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

    private static Sprite FindSpriteByName(string spriteName)
    {
        string[] searchPaths = new string[]
        {
            $"Assets/UI/{spriteName}.png",
            $"Assets/UI/Logos/{spriteName}.png",
            $"Assets/UI/Sprites/{spriteName}.png"
        };

        foreach (var p in searchPaths)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (sp != null) return sp;
        }

        var guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        return null;
    }

    private static TMP_FontAsset GetPreferredFont()
    {
        var libSans = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (libSans != null) return libSans;

        return TMP_Settings.defaultFontAsset;
    }

    private static void EnsureAssetDirectories()
    {
        if (!Directory.Exists("Assets/UI")) Directory.CreateDirectory("Assets/UI");
        if (!Directory.Exists("Assets/UI/Logos")) Directory.CreateDirectory("Assets/UI/Logos");
    }

    private static void EnsureLogosImported()
    {
        string[] logos = new string[]
        {
            "Assets/UI/Logos/logo-telkom-university.png",
            "Assets/UI/Logos/logo-fakultas-kedokteran.png",
            "Assets/UI/Logos/logo-team-autopsy.png"
        };

        foreach (var path in logos)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                bool dirty = false;
                if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
                if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; dirty = true; }
                if (ti.mipmapEnabled) { ti.mipmapEnabled = false; dirty = true; }
                if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
                if (ti.filterMode != FilterMode.Bilinear) { ti.filterMode = FilterMode.Bilinear; dirty = true; }
                if (ti.maxTextureSize < 2048) { ti.maxTextureSize = 2048; dirty = true; }

                var androidSettings = ti.GetPlatformTextureSettings("Android");
                if (!androidSettings.overridden || androidSettings.format != TextureImporterFormat.RGBA32 || androidSettings.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    androidSettings.overridden = true;
                    androidSettings.format = TextureImporterFormat.RGBA32;
                    androidSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.SetPlatformTextureSettings(androidSettings);
                    dirty = true;
                }

                if (dirty)
                {
                    ti.SaveAndReimport();
                }
            }
        }
    }

    private static void EnsureBackgroundSprite()
    {
        string bgPath = "Assets/UI/background.png";
        if (!File.Exists(bgPath))
        {
            int w = 540;
            int h = 960;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color darkBase = ColorBgBase;
            Color gridLine = new Color(20f / 255f, 28f / 255f, 44f / 255f, 1f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isGrid = (x % 40 == 0) || (y % 40 == 0);
                    tex.SetPixel(x, y, isGrid ? gridLine : darkBase);
                }
            }
            tex.Apply();

            File.WriteAllBytes(bgPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(bgPath);

            var ti = AssetImporter.GetAtPath(bgPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.SaveAndReimport();
            }
        }
    }
}
