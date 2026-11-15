using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class ARHudBuilder
{
    private static readonly Color ColorBgBase      = new Color(10f / 255f, 14f / 255f, 23f / 255f, 1f);     // #0A0E17
    private static readonly Color ColorPanel       = new Color(13f / 255f, 19f / 255f, 31f / 255f, 0.96f); // #0D131F
    private static readonly Color ColorCardItem    = new Color(19f / 255f, 27f / 255f, 44f / 255f, 0.96f);  // #131B2C
    private static readonly Color ColorBorder      = new Color(30f / 255f, 41f / 255f, 59f / 255f, 1f);     // #1E293B
    private static readonly Color ColorBlueAccent  = new Color(59f / 255f, 130f / 255f, 246f / 255f, 1f);  // #3B82F6
    private static readonly Color ColorCyanAccent  = new Color(34f / 255f, 211f / 255f, 238f / 255f, 1f);  // #22D3EE
    private static readonly Color ColorTextBody    = new Color(203f / 255f, 213f / 255f, 225f / 255f, 1f); // #CBD5E1
    private static readonly Color ColorTextMuted   = new Color(148f / 255f, 163f / 255f, 184f / 255f, 1f); // #94A3B8

    [MenuItem("Tools/Build AR HUD")]
    public static void BuildARHud()
    {
        string scenePath = "Assets/Scenes/ASS 2.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"[ARHudBuilder] Gagal membuka scene '{scenePath}'");
            return;
        }

        Debug.Log("[ARHudBuilder] Membangun AR Scanning HUD di ASS 2.unity...");

        var canvasGo = GameObject.Find("AR_HUD_Canvas");
        if (canvasGo == null)
        {
            Debug.LogError("[ARHudBuilder] Objek 'AR_HUD_Canvas' tidak ditemukan!");
            return;
        }

        var hudController = canvasGo.GetComponent<ARHudController>();
        if (hudController == null)
        {
            hudController = canvasGo.AddComponent<ARHudController>();
        }

        Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprites/RoundedCard_9Slice.png");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // 1. Perbarui / Hapus elemen Scanning HUD lama jika sudah ada
        var oldReticle = GameObject.Find("ScanningReticleHUD");
        if (oldReticle != null) Object.DestroyImmediate(oldReticle);

        var oldBottomBar = GameObject.Find("BottomTelemetryBar");
        if (oldBottomBar != null) Object.DestroyImmediate(oldBottomBar);

        // 2. Bangun Reticle Scanning di tengah layar
        var reticleGo = new GameObject("ScanningReticleHUD", typeof(RectTransform));
        reticleGo.transform.SetParent(canvasGo.transform, false);
        var rRect = reticleGo.GetComponent<RectTransform>();
        rRect.anchorMin = new Vector2(0.5f, 0.5f);
        rRect.anchorMax = new Vector2(0.5f, 0.5f);
        rRect.pivot = new Vector2(0.5f, 0.5f);
        rRect.sizeDelta = new Vector2(560f, 560f);
        rRect.anchoredPosition = new Vector2(0f, 40f);

        // Kotak Bingkai Reticle Transparan
        var frameImg = reticleGo.AddComponent<Image>();
        frameImg.color = new Color(34f / 255f, 211f / 255f, 238f / 255f, 0.08f);
        frameImg.raycastTarget = false;

        // 4 Sudut Bracket (Corner Ticks)
        CreateCornerBracket("TopLeft", reticleGo.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), 35f, 35f, true, true);
        CreateCornerBracket("TopRight", reticleGo.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), 35f, 35f, false, true);
        CreateCornerBracket("BottomLeft", reticleGo.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), 35f, 35f, true, false);
        CreateCornerBracket("BottomRight", reticleGo.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), 35f, 35f, false, false);

        // Garis Tengah Crosshair
        var crossH = new GameObject("CrosshairH", typeof(RectTransform), typeof(Image));
        crossH.transform.SetParent(reticleGo.transform, false);
        crossH.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 2f);
        crossH.GetComponent<Image>().color = new Color(34f / 255f, 211f / 255f, 238f / 255f, 0.6f);
        crossH.GetComponent<Image>().raycastTarget = false;

        var crossV = new GameObject("CrosshairV", typeof(RectTransform), typeof(Image));
        crossV.transform.SetParent(reticleGo.transform, false);
        crossV.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 40f);
        crossV.GetComponent<Image>().color = new Color(34f / 255f, 211f / 255f, 238f / 255f, 0.6f);
        crossV.GetComponent<Image>().raycastTarget = false;

        // Laser Scan Line Bergerak
        var scanLineGo = new GameObject("ScanLine", typeof(RectTransform), typeof(Image));
        scanLineGo.transform.SetParent(reticleGo.transform, false);
        var slRect = scanLineGo.GetComponent<RectTransform>();
        slRect.sizeDelta = new Vector2(540f, 3f);
        slRect.anchoredPosition = new Vector2(0f, 220f);
        var slImg = scanLineGo.GetComponent<Image>();
        slImg.color = new Color(34f / 255f, 211f / 255f, 238f / 255f, 0.85f);
        slImg.raycastTarget = false;

        // Teks Instruksi Scanning di Bawah Reticle (RAPI, TIDAK KEPOTONG, WORD WRAPPING)
        var promptGo = new GameObject("PromptBadge", typeof(RectTransform));
        promptGo.transform.SetParent(reticleGo.transform, false);
        var pbRect = promptGo.GetComponent<RectTransform>();
        pbRect.anchorMin = new Vector2(0.5f, 0f);
        pbRect.anchorMax = new Vector2(0.5f, 0f);
        pbRect.pivot = new Vector2(0.5f, 1f);
        pbRect.sizeDelta = new Vector2(760f, 115f);
        pbRect.anchoredPosition = new Vector2(0f, -25f);

        var pbImg = promptGo.AddComponent<Image>();
        pbImg.color = ColorPanel;
        pbImg.sprite = roundedSprite;
        if (roundedSprite != null) pbImg.type = Image.Type.Sliced;
        pbImg.raycastTarget = false;

        var promptGroup = promptGo.AddComponent<CanvasGroup>();

        var promptTxtGo = new GameObject("PromptText", typeof(RectTransform));
        promptTxtGo.transform.SetParent(promptGo.transform, false);
        var ptRect = promptTxtGo.GetComponent<RectTransform>();
        ptRect.anchorMin = Vector2.zero;
        ptRect.anchorMax = Vector2.one;
        ptRect.sizeDelta = new Vector2(-20f, -10f);
        ptRect.anchoredPosition = Vector2.zero;

        var promptTxt = promptTxtGo.AddComponent<TextMeshProUGUI>();
        if (font != null) promptTxt.font = font;
        promptTxt.fontSize = 20f;
        promptTxt.alignment = TextAlignmentOptions.Center;
        promptTxt.color = Color.white;
        promptTxt.enableWordWrapping = true;
        promptTxt.overflowMode = TextOverflowModes.Overflow;
        promptTxt.text = "<b>PANDUAN SCANNING AR</b>\n<size=16><color=#22D3EE>Arahkan kamera ke Kartu Tubuh atau Kasus TKP\nPosisikan target di dalam bingkai bidik</color></size>";
        promptTxt.raycastTarget = false;

        // Pasang script animasi ARScanReticle
        var reticleScript = reticleGo.AddComponent<ARScanReticle>();
        reticleScript.scanLine = slRect;
        reticleScript.scanSpeed = 180f;
        reticleScript.scanHeight = 500f;
        reticleScript.promptGroup = promptGroup;

        // 3. Bangun Bottom Telemetry Bar (Tinggi 130px, Rapi, Word Wrapped)
        var bottomBarGo = new GameObject("BottomTelemetryBar", typeof(RectTransform));
        bottomBarGo.transform.SetParent(canvasGo.transform, false);
        var bbRect = bottomBarGo.GetComponent<RectTransform>();
        bbRect.anchorMin = new Vector2(0f, 0f);
        bbRect.anchorMax = new Vector2(1f, 0f);
        bbRect.pivot = new Vector2(0.5f, 0f);
        bbRect.sizeDelta = new Vector2(0f, 130f);
        bbRect.anchoredPosition = Vector2.zero;

        var bbImg = bottomBarGo.AddComponent<Image>();
        bbImg.color = ColorPanel;
        bbImg.raycastTarget = false;

        var bbTxtGo = new GameObject("TelemetryText", typeof(RectTransform));
        bbTxtGo.transform.SetParent(bottomBarGo.transform, false);
        var bbtRect = bbTxtGo.GetComponent<RectTransform>();
        bbtRect.anchorMin = Vector2.zero;
        bbtRect.anchorMax = Vector2.one;
        bbtRect.sizeDelta = new Vector2(-40f, -10f);
        bbtRect.anchoredPosition = Vector2.zero;

        var bbtTmp = bbTxtGo.AddComponent<TextMeshProUGUI>();
        if (font != null) bbtTmp.font = font;
        bbtTmp.fontSize = 18f;
        bbtTmp.alignment = TextAlignmentOptions.Center;
        bbtTmp.color = ColorTextBody;
        bbtTmp.enableWordWrapping = true;
        bbtTmp.overflowMode = TextOverflowModes.Overflow;
        bbtTmp.text = "<b>INSTRUMEN PEMERIKSAAN MEDIS:</b>\n<size=15><color=#22D3EE>Dekatkan Kartu Tindakan ke Tubuh Korban untuk Memeriksa</color></size>\n<size=14><color=#94A3B8>Termometer Rektal • Uji Lebam • Kaca Pembesar • Abdomen</color></size>";
        bbtTmp.raycastTarget = false;

        // 4. Perbarui TopBar dengan visual taktis yang rapi
        var topBarGo = GameObject.Find("TopBar");
        if (topBarGo != null)
        {
            var tbRect = topBarGo.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0f, 100f);
            tbRect.anchoredPosition = Vector2.zero;

            var tbImg = topBarGo.GetComponent<Image>();
            if (tbImg == null) tbImg = topBarGo.AddComponent<Image>();
            tbImg.color = ColorPanel;
            tbImg.raycastTarget = false;

            // Status Badge di tengah TopBar
            var badgeGo = GameObject.Find("ARStatusBadge");
            if (badgeGo == null)
            {
                badgeGo = new GameObject("ARStatusBadge", typeof(RectTransform));
                badgeGo.transform.SetParent(topBarGo.transform, false);
            }
            var bRect = badgeGo.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.5f);
            bRect.anchorMax = new Vector2(0.5f, 0.5f);
            bRect.pivot = new Vector2(0.5f, 0.5f);
            bRect.sizeDelta = new Vector2(440f, 50f);
            bRect.anchoredPosition = Vector2.zero;

            var bTmp = badgeGo.GetComponent<TextMeshProUGUI>();
            if (bTmp == null) bTmp = badgeGo.AddComponent<TextMeshProUGUI>();
            if (font != null) bTmp.font = font;
            bTmp.fontSize = 20f;
            bTmp.alignment = TextAlignmentOptions.Center;
            bTmp.color = ColorCyanAccent;
            bTmp.text = "<b>[+] AR PROBE V4.8 [AKTIF]</b>";
            bTmp.raycastTarget = false;
        }

        // 5. Bangun / Rebuild QuickManualModal dengan Word Wrapping & Layout Rapi (TIDAK MEMANJANG KELUAR LAYAR)
        var modalGo = SetupQuickManualModal(canvasGo, font, roundedSprite, hudController);

        // 6. Wire-up Referensi ke ARHudController
        var btnBack = GameObject.Find("BtnBackToMenu");
        var btnGuide = GameObject.Find("BtnGuide");

        if (btnBack != null) hudController.backToMenuButton = btnBack.GetComponent<Button>();
        if (btnGuide != null) hudController.openManualButton = btnGuide.GetComponent<Button>();
        hudController.quickManualModal = modalGo;
        hudController.reticleHUD = reticleGo;

        EditorUtility.SetDirty(hudController);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[ARHudBuilder] Sukses! AR Scanning HUD dan QuickManualModal rapi berhasil dibangun.");
    }

    private static GameObject SetupQuickManualModal(GameObject canvasGo, TMP_FontAsset font, Sprite roundedSprite, ARHudController hudController)
    {
        var existingModal = GameObject.Find("QuickManualModal");
        if (existingModal != null)
        {
            Object.DestroyImmediate(existingModal);
        }

        // Root Modal Overlay
        var modalGo = new GameObject("QuickManualModal", typeof(RectTransform));
        modalGo.transform.SetParent(canvasGo.transform, false);
        SetStretch(modalGo.GetComponent<RectTransform>());

        var mImg = modalGo.AddComponent<Image>();
        mImg.color = new Color(5f / 255f, 8f / 255f, 14f / 255f, 0.94f);
        mImg.raycastTarget = true;

        // Card Panel Tengah (960 x 1460)
        var cardGo = new GameObject("ManualCard", typeof(RectTransform));
        cardGo.transform.SetParent(modalGo.transform, false);
        var cRect = cardGo.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(960f, 1460f);
        cRect.anchoredPosition = Vector2.zero;

        var cImg = cardGo.AddComponent<Image>();
        cImg.color = ColorPanel;
        cImg.sprite = roundedSprite;
        if (roundedSprite != null) cImg.type = Image.Type.Sliced;
        cImg.raycastTarget = true;

        var cVlg = cardGo.AddComponent<VerticalLayoutGroup>();
        cVlg.padding = new RectOffset(25, 25, 20, 20);
        cVlg.spacing = 14;
        cVlg.childControlWidth = true;
        cVlg.childControlHeight = false;
        cVlg.childForceExpandWidth = true;
        cVlg.childForceExpandHeight = false;

        // Header Bar (Judul + Tombol Tutup)
        var headerGo = new GameObject("HeaderBar", typeof(RectTransform));
        headerGo.transform.SetParent(cardGo.transform, false);
        headerGo.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 75f);
        var hHlg = headerGo.AddComponent<HorizontalLayoutGroup>();
        hHlg.childControlWidth = false;
        hHlg.childControlHeight = true;
        hHlg.childAlignment = TextAnchor.MiddleCenter;
        hHlg.spacing = 15;

        var titleBox = new GameObject("TitleBox", typeof(RectTransform));
        titleBox.transform.SetParent(headerGo.transform, false);
        titleBox.GetComponent<RectTransform>().sizeDelta = new Vector2(690f, 75f);
        var tbVlg = titleBox.AddComponent<VerticalLayoutGroup>();
        tbVlg.spacing = 3;
        tbVlg.childControlWidth = true;
        tbVlg.childControlHeight = false;

        CreateTmpText("Badge", titleBox.transform, font, 15f, TextAlignmentOptions.Left, ColorCyanAccent, "<b>* PANDUAN INVESTIGASI AR // PANDUAN LENGKAP *</b>");
        CreateTmpText("MainTitle", titleBox.transform, font, 27f, TextAlignmentOptions.Left, Color.white, "<b>TATA CARA SCANNING & PEMERIKSAAN</b>");

        // Tombol Tutup di Kanan Atas
        var btnCloseGo = new GameObject("BtnCloseModal", typeof(RectTransform));
        btnCloseGo.transform.SetParent(headerGo.transform, false);
        btnCloseGo.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 50f);
        var bcImg = btnCloseGo.AddComponent<Image>();
        bcImg.color = new Color(36f / 255f, 15f / 255f, 15f / 255f, 1f);
        bcImg.sprite = roundedSprite;
        if (roundedSprite != null) bcImg.type = Image.Type.Sliced;
        var btnClose = btnCloseGo.AddComponent<Button>();

        var bcTxt = CreateTmpText("Text", btnCloseGo.transform, font, 18f, TextAlignmentOptions.Center, new Color(248f / 255f, 113f / 255f, 113f / 255f, 1f), "<b>[ X ] TUTUP</b>");
        SetStretch(bcTxt.GetComponent<RectTransform>());

        // Divider
        var div = new GameObject("Div", typeof(RectTransform), typeof(Image));
        div.transform.SetParent(cardGo.transform, false);
        div.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 2f);
        div.GetComponent<Image>().color = ColorBorder;

        // Scrollable Area (lebar 910, tinggi 1310)
        var scrollGo = new GameObject("ScrollContainer", typeof(RectTransform));
        scrollGo.transform.SetParent(cardGo.transform, false);
        scrollGo.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 1310f);

        var sr = scrollGo.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.scrollSensitivity = 35f;

        var vp = new GameObject("Viewport", typeof(RectTransform));
        vp.transform.SetParent(scrollGo.transform, false);
        SetStretch(vp.GetComponent<RectTransform>());
        vp.AddComponent<RectMask2D>();
        sr.viewport = vp.GetComponent<RectTransform>();

        var cnt = new GameObject("Content", typeof(RectTransform));
        cnt.transform.SetParent(vp.transform, false);
        var cRectCnt = cnt.GetComponent<RectTransform>();
        cRectCnt.anchorMin = new Vector2(0f, 1f);
        cRectCnt.anchorMax = new Vector2(1f, 1f);
        cRectCnt.pivot = new Vector2(0.5f, 1f);
        cRectCnt.sizeDelta = new Vector2(0f, 1200f);
        sr.content = cRectCnt;

        var csf = cnt.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = cnt.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5, 5, 10, 15);
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        // Tambahkan Card Langkah-Langkah Rapi
        CreateGuideStepCard(cnt.transform, font, roundedSprite, "01", "TAHAP 1: SCAN KARTU KASUS TKP", "SKENARIO",
            "Arahkan kamera ke salah satu kartu kasus (Kasus 1: Kamar Mandi, Kasus 2: Gudang Pabrik, atau Kasus 3: Kebun Terbuka) untuk memuat data kronologi dan suhu lingkungan TKP (°C).");

        CreateGuideStepCard(cnt.transform, font, roundedSprite, "02", "TAHAP 2: SCAN BLUEPRINT TUBUH", "TARGET ANATOMI",
            "Arahkan kamera ke kartu Blueprint Tubuh. Avatar 3D jenazah akan muncul stabil di atas kartu.\n• <b>Putar Tubuh:</b> Geser layar dengan 1 jari untuk rotasi 360°.\n• <b>Zoom Detail:</b> Gunakan cubitan 2 jari (pinch) untuk memperbesar anatomi.");

        CreateGuideStepCard(cnt.transform, font, roundedSprite, "03", "TAHAP 3: DEKATKAN KARTU TINDAKAN", "PEMERIKSAAN MEDIS",
            "Dekatkan kartu instrumen tindakan ke tubuh jenazah:\n" +
            "• <b>Termometer Rektal:</b> Mengukur penurunan suhu inti tubuh (Algor Mortis).\n" +
            "• <b>Uji Tekan Lebam:</b> Cek Blanching Test pemudaran lebam mayat (Livor Mortis).\n" +
            "• <b>Kaca Pembesar:</b> Memeriksa kekeruhan kornea mata & trauma kranial.\n" +
            "• <b>Uji Abdomen / Sendi:</b> Memeriksa pembusukan caecum & kaku jenazah.");

        CreateGuideStepCard(cnt.transform, font, roundedSprite, "04", "TAHAP 4: ESTIMASI POST-MORTEM INTERVAL (PMI)", "KALKULASI KLINIS",
            "Gabungkan seluruh data klinis (suhu, fiksasi lebam, kekakuan, dan pembusukan) untuk menentukan rentang waktu kematian korban dan pecahkan investigasi kasus!");

        modalGo.SetActive(false);
        hudController.closeManualButton = btnClose;

        return modalGo;
    }

    private static void CreateGuideStepCard(Transform parent, TMP_FontAsset font, Sprite sprite, string number, string title, string tag, string desc)
    {
        var card = new GameObject("StepCard", typeof(RectTransform));
        card.transform.SetParent(parent, false);
        var cRect = card.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(0f, 150f);

        var img = card.AddComponent<Image>();
        img.color = ColorCardItem;
        img.sprite = sprite;
        if (sprite != null) img.type = Image.Type.Sliced;
        img.raycastTarget = false;

        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 16, 16);
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = card.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Row Header
        var row = new GameObject("Row", typeof(RectTransform));
        row.transform.SetParent(card.transform, false);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 36f);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.spacing = 12;

        // Badge Nomor
        var badge = new GameObject("Badge", typeof(RectTransform));
        badge.transform.SetParent(row.transform, false);
        badge.GetComponent<RectTransform>().sizeDelta = new Vector2(46f, 34f);
        var bImg = badge.AddComponent<Image>();
        bImg.color = ColorBorder;
        bImg.sprite = sprite;
        if (sprite != null) bImg.type = Image.Type.Sliced;

        var bTxt = CreateTmpText("Num", badge.transform, font, 17f, TextAlignmentOptions.Center, ColorCyanAccent, $"<b>{number}</b>");
        SetStretch(bTxt.GetComponent<RectTransform>());

        // Judul
        var tTxt = CreateTmpText("Title", row.transform, font, 20f, TextAlignmentOptions.MidlineLeft, Color.white, $"<b>{title}</b>");
        tTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(580f, 36f);

        // Tag
        var tgTxt = CreateTmpText("Tag", row.transform, font, 14f, TextAlignmentOptions.MidlineRight, ColorCyanAccent, $"<b>[{tag}]</b>");
        tgTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(170f, 36f);

        // Divider
        var d = new GameObject("Div", typeof(RectTransform), typeof(Image));
        d.transform.SetParent(card.transform, false);
        d.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 1.5f);
        d.GetComponent<Image>().color = new Color(30f / 255f, 41f / 255f, 59f / 255f, 0.6f);

        // Deskripsi (Word Wrapping AKTIF & RAPI!)
        var dTxt = CreateTmpText("Desc", card.transform, font, 17f, TextAlignmentOptions.TopLeft, ColorTextBody, desc);
        dTxt.enableWordWrapping = true;
        dTxt.overflowMode = TextOverflowModes.Overflow;
    }

    private static TextMeshProUGUI CreateTmpText(string name, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions align, Color col, string content)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent != null) go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = col;
        tmp.text = content;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    private static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CreateCornerBracket(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, float w, float h, bool left, bool top)
    {
        var corner = new GameObject(name, typeof(RectTransform));
        corner.transform.SetParent(parent, false);
        var cRect = corner.GetComponent<RectTransform>();
        cRect.anchorMin = anchorMin;
        cRect.anchorMax = anchorMax;
        cRect.pivot = pivot;
        cRect.sizeDelta = new Vector2(w, h);
        cRect.anchoredPosition = Vector2.zero;

        // Garis Horizontal Bracket
        var hLine = new GameObject("HLine", typeof(RectTransform), typeof(Image));
        hLine.transform.SetParent(corner.transform, false);
        var hRect = hLine.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
        hRect.anchorMax = hRect.anchorMin;
        hRect.pivot = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
        hRect.sizeDelta = new Vector2(w, 4f);
        hRect.anchoredPosition = Vector2.zero;
        var hImg = hLine.GetComponent<Image>();
        hImg.color = ColorCyanAccent;
        hImg.raycastTarget = false;

        // Garis Vertikal Bracket
        var vLine = new GameObject("VLine", typeof(RectTransform), typeof(Image));
        vLine.transform.SetParent(corner.transform, false);
        var vRect = vLine.GetComponent<RectTransform>();
        vRect.anchorMin = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
        vRect.anchorMax = vRect.anchorMin;
        vRect.pivot = new Vector2(left ? 0f : 1f, top ? 1f : 0f);
        vRect.sizeDelta = new Vector2(4f, h);
        vRect.anchoredPosition = Vector2.zero;
        var vImg = vLine.GetComponent<Image>();
        vImg.color = ColorCyanAccent;
        vImg.raycastTarget = false;
    }
}
