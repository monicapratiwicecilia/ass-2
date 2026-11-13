using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum BodyRegion { Kepala, Dada, Perut, Pelvis, Punggung, Tungkai }

[System.Serializable]
public class WoundSpot
{
    public Vector2 uv;
    [Range(0.005f, 0.5f)] public float radius = 0.03f;
    [Range(0f, 1f)] public float intensity = 0.9f;
    public bool useCustomColor = false;
    public Color customPrimary = new Color(0.4f, 0.1f, 0.4f);
    public Color customSecondary = new Color(0.9f, 0.7f, 0.6f);
    public int count = 1;                                  // jumlah titik
    [Range(0f, 0.3f)] public float scatter = 0f;           // luas area sebaran
    public BodyRegion region;

    [Header("Mode urat / pembuluh (marbling)")]
    public bool isVein = false;                            // centang untuk membuat jaring urat
    [Range(1, 12)] public int branches = 6;                // jumlah urat utama dari titik awal
    [Range(0.02f, 0.4f)] public float veinLength = 0.1f;   // panjang tiap urat
    [Range(0f, 1.5f)] public float veinWiggle = 0.8f;      // seberapa berkelok
    [Range(0f, 0.15f)] public float forkChance = 0.06f;    // peluang bercabang
}

// Gambar PNG transparan hasil lukisan (urat, keriput, kerak garam, dll)
[System.Serializable]
public class RegionOverlay
{
    public Texture2D texture;                 // PNG transparan, ukuran sama dengan Base Texture
    public BodyRegion region;                 // dibuka oleh kartu bagian ini
    [Range(0f, 1f)] public float opacity = 1f;
}

// Teks temuan per bagian tubuh (dipakai popup AR dan dashboard)
[System.Serializable]
public class RegionText
{
    public BodyRegion region;
    public string title;
    [TextArea(2, 6)] public string body;      // boleh pakai {temp} untuk suhu tubuh
}

[System.Serializable]
public class ForensicCase
{
    public string caseName;
    public Color primaryColor = new Color(0.4f, 0.1f, 0.4f);
    public Color secondaryColor = new Color(0.9f, 0.7f, 0.6f);
    public List<WoundSpot> spots = new List<WoundSpot>();
    public List<RegionOverlay> overlays = new List<RegionOverlay>();
    public bool useSkinTint = false;
    public Color skinTint = new Color(0.65f, 0.68f, 0.7f);
    [Range(0f, 1f)] public float tintStrength = 0.35f;
    public float bodyTemp = 37f;                           // suhu tubuh untuk termometer

    [TextArea(2, 6)] public string narrative;              // narasi TKP untuk kartu kasus
    public List<RegionText> texts = new List<RegionText>();
}

public class CaseColorManager : MonoBehaviour
{
    public Renderer bodyRenderer;
    public Texture2D baseTexture;       // harus Read/Write enabled
    public List<ForensicCase> forensicCases;

    [Header("Termometer")]
    public GameObject thermometer;      // objek Termometer (nonaktif di awal)
    public TMP_Text tempText;           // teks Angka (TextMeshPro 3D)
    public float startTemp = 37f;
    public float tempAnimSeconds = 2f;

    Material mat;
    Texture2D runtimeTex;
    int texId;

    int activeCase = -1;
    bool tintApplied;
    HashSet<BodyRegion> revealed = new HashSet<BodyRegion>();

    public int ActiveCase { get { return activeCase; } }

    // Untuk dashboard / script lain
    public event System.Action<int> OnCaseSelected;                          // indeks kasus
    public event System.Action<int, BodyRegion, string, string> OnFinding;   // kasus, bagian, judul, isi

    public string GetCaseName()
    {
        return activeCase >= 0 ? forensicCases[activeCase].caseName : "";
    }

    public string GetNarrative()
    {
        return activeCase >= 0 ? forensicCases[activeCase].narrative : "";
    }

    public bool TryGetText(BodyRegion region, out string title, out string body)
    {
        title = ""; body = "";
        if (activeCase < 0) return false;
        var c = forensicCases[activeCase];
        foreach (var t in c.texts)
        {
            if (t.region != region) continue;
            title = t.title;
            body = (t.body ?? "").Replace("{temp}", c.bodyTemp.ToString("F1"));
            return true;
        }
        return false;
    }

    // Klik titik tiga di komponen ini > "Isi teks contoh Kasus 2 dan 3" (di Edit mode)
    [ContextMenu("Isi teks contoh Kasus 2 dan 3")]
    void FillSampleTexts()
    {
        if (forensicCases == null || forensicCases.Count < 3) return;

        var k2 = forensicCases[1];
        k2.narrative = "Gudang pabrik tua. Suhu ruangan 33 °C. Korban memakai jaket tebal, berat badan 55 kg.";
        SetText(k2, BodyRegion.Kepala, "Pemeriksaan Kepala",
            "Wajah tampak merah gelap kusam dan sedikit bengkak (kongesti vena) akibat darah menumpuk di area kepala.");
        SetText(k2, BodyRegion.Dada, "Pemeriksaan Dada",
            "Lebam ungu pekat di dada depan. Saat ditekan, lebam tetap menetap.");
        SetText(k2, BodyRegion.Perut, "Pemeriksaan Perut",
            "Lebam ungu pekat di perut depan, searah dengan posisi telungkup.");
        SetText(k2, BodyRegion.Pelvis, "Suhu Tubuh",
            "Suhu tubuh {temp} °C. Penurunan suhu lambat karena jaket tebal menahan panas.");

        var k3 = forensicCases[2];
        k3.narrative = "Kebun terbuka. Suhu lingkungan 24 °C. Korban berbaju normal, berat badan 75 kg. Perkiraan kematian sekitar 24 jam.";
        SetText(k3, BodyRegion.Kepala, "Pemeriksaan Kepala",
            "Wajah tampak mulai mengalami perubahan warna akibat pembusukan awal.");
        SetText(k3, BodyRegion.Dada, "Pemeriksaan Dada",
            "Pola pembuluh vena hijau kehitaman (marbling) menyebar di dada. Lebam punggung ungu gelap kehitaman.");
        SetText(k3, BodyRegion.Perut, "Pemeriksaan Perut",
            "Bercak hijau lumut di perut kanan bawah (area sekum), tanda pembusukan awal.");
        SetText(k3, BodyRegion.Pelvis, "Suhu Tubuh",
            "Suhu tubuh {temp} °C, sudah mendekati suhu lingkungan.");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    void SetText(ForensicCase c, BodyRegion region, string title, string body)
    {
        foreach (var t in c.texts)
            if (t.region == region) { t.title = title; t.body = body; return; }
        c.texts.Add(new RegionText { region = region, title = title, body = body });
    }
    public float CurrentBodyTemp
    {
        get { return activeCase >= 0 ? forensicCases[activeCase].bodyTemp : startTemp; }
    }

    void Awake()
    {
        mat = bodyRenderer.material; // instance material
        texId = mat.HasProperty("_BaseMap")
            ? Shader.PropertyToID("_BaseMap")    // URP
            : Shader.PropertyToID("_MainTex");   // Built-in
        ResetSkin();
        if (thermometer) thermometer.SetActive(false);
    }

    public void ResetSkin()
    {
        runtimeTex = new Texture2D(baseTexture.width, baseTexture.height,
                                   TextureFormat.RGBA32, false);
        runtimeTex.SetPixels32(baseTexture.GetPixels32());
        runtimeTex.Apply();
        mat.SetTexture(texId, runtimeTex);
    }

    // Tampilkan semua temuan satu kasus sekaligus (untuk tes cepat)
    public void ShowCase(int index)
    {
        if (index < 0 || index >= forensicCases.Count) return;
        ResetSkin();
        var c = forensicCases[index];
        if (c.useSkinTint) ApplyTint(c.skinTint, c.tintStrength);
        foreach (var o in c.overlays) ApplyOverlay(o);
        foreach (var s in c.spots) PaintScattered(s, c);
        runtimeTex.Apply();
    }

    // Dipanggil saat kartu KASUS discan
    public void SetActiveCase(int index)
    {
        if (index < 0 || index >= forensicCases.Count) return;
        if (index == activeCase) return;      // kasus sama, jangan reset

        activeCase = index;
        revealed.Clear();
        tintApplied = false;
        ResetSkin();

        StopAllCoroutines();
        if (thermometer) thermometer.SetActive(false);

        // Pucat keseluruhan langsung terlihat begitu kasus dipilih
        var c = forensicCases[index];
        if (c.useSkinTint)
        {
            ApplyTint(c.skinTint, c.tintStrength);
            runtimeTex.Apply();
            tintApplied = true;
        }

        if (OnCaseSelected != null) OnCaseSelected(index);
    }

    // Dipanggil saat kartu TINDAKAN discan
    public void RevealRegion(BodyRegion region)
    {
        if (activeCase < 0) return;           // belum scan kasus
        if (!revealed.Add(region)) return;    // bagian ini sudah dibuka

        var c = forensicCases[activeCase];

        if (c.useSkinTint && !tintApplied)
        {
            ApplyTint(c.skinTint, c.tintStrength);
            tintApplied = true;
        }

        // Overlay lukisan dulu, bercak di atasnya
        foreach (var o in c.overlays)
            if (o.region == region) ApplyOverlay(o);

        foreach (var s in c.spots)
            if (s.region == region) PaintScattered(s, c);

        runtimeTex.Apply();

        string ft, fb;
        if (OnFinding != null && TryGetText(region, out ft, out fb))
            OnFinding(activeCase, region, ft, fb);

        // Kartu pelvis = termometer
        if (region == BodyRegion.Pelvis && thermometer)
        {
            thermometer.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(AnimateTemp(startTemp, c.bodyTemp));
        }
    }

    IEnumerator AnimateTemp(float from, float to)
    {
        float t = 0f;
        while (t < tempAnimSeconds)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(from, to, t / tempAnimSeconds);
            if (tempText) tempText.text = v.ToString("F1") + " °C";
            yield return null;
        }
        if (tempText) tempText.text = to.ToString("F1") + " °C";
    }

    void ApplyOverlay(RegionOverlay o)
    {
        if (o == null || o.texture == null) return;

        if (o.texture.width != runtimeTex.width || o.texture.height != runtimeTex.height)
        {
            Debug.LogWarning("Overlay '" + o.texture.name + "' ukurannya (" + o.texture.width + "x" +
                o.texture.height + ") beda dengan Base Texture (" + runtimeTex.width + "x" +
                runtimeTex.height + "). Samakan ukurannya.");
            return;
        }

        var basePx = runtimeTex.GetPixels32();
        var ovPx = o.texture.GetPixels32();
        for (int i = 0; i < basePx.Length; i++)
        {
            float a = (ovPx[i].a / 255f) * o.opacity;
            if (a <= 0f) continue;
            basePx[i] = Color.Lerp((Color)basePx[i], (Color)ovPx[i], a);
        }
        runtimeTex.SetPixels32(basePx);
    }

    void PaintSpot(WoundSpot s, ForensicCase c)
    {
        int w = runtimeTex.width, h = runtimeTex.height;
        int cx = Mathf.RoundToInt(s.uv.x * w);
        int cy = Mathf.RoundToInt(s.uv.y * h);
        int r = Mathf.RoundToInt(s.radius * w);

        Color p = s.useCustomColor ? s.customPrimary : c.primaryColor;
        Color sc = s.useCustomColor ? s.customSecondary : c.secondaryColor;

        for (int y = Mathf.Max(0, cy - r); y < Mathf.Min(h, cy + r); y++)
        for (int x = Mathf.Max(0, cx - r); x < Mathf.Min(w, cx + r); x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            if (d > r) continue;
            float t = d / r;                              // 0 tengah, 1 tepi
            Color wound = Color.Lerp(p, sc, t);
            float a = (1f - t) * (1f - t) * s.intensity;  // memudar halus
            runtimeTex.SetPixel(x, y, Color.Lerp(runtimeTex.GetPixel(x, y), wound, a));
        }
    }

    // ---------- Urat / pembuluh darah ----------
    void PaintVeins(WoundSpot s, ForensicCase c)
    {
        var rng = new System.Random(s.uv.GetHashCode()); // hasil selalu sama tiap scan
        const int steps = 40;
        for (int b = 0; b < s.branches; b++)
        {
            float startAng = (float)(rng.NextDouble() * Mathf.PI * 2);
            WalkVein(s, c, s.uv, startAng, steps, s.veinLength / steps, rng, 0);
        }
    }

    void WalkVein(WoundSpot s, ForensicCase c, Vector2 pos, float ang,
                  int steps, float stepLen, System.Random rng, int depth)
    {
        // Makin dalam cabangnya, makin tipis
        float rScale = depth == 0 ? 1f : (depth == 1 ? 0.65f : 0.4f);

        for (int i = 0; i < steps; i++)
        {
            ang += (float)(rng.NextDouble() - 0.5) * s.veinWiggle;
            pos += new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * stepLen;

            // Memudar di ujung urat
            float fade = 1f - (float)i / steps * 0.6f;

            PaintSpot(new WoundSpot
            {
                uv = pos,
                radius = Mathf.Max(0.0008f, s.radius * rScale),
                intensity = s.intensity * fade,
                useCustomColor = s.useCustomColor,
                customPrimary = s.customPrimary,
                customSecondary = s.customSecondary,
                region = s.region
            }, c);

            if (depth < 2 && rng.NextDouble() < s.forkChance)
            {
                float side = rng.NextDouble() < 0.5 ? -1f : 1f;
                float forkAng = ang + side * (0.5f + (float)rng.NextDouble() * 0.6f);
                WalkVein(s, c, pos, forkAng, steps / 2, stepLen, rng, depth + 1);
            }
        }
    }

    void ApplyTint(Color tint, float strength)
    {
        var px = runtimeTex.GetPixels32();
        for (int i = 0; i < px.Length; i++)
            px[i] = Color.Lerp((Color)px[i], tint, strength);
        runtimeTex.SetPixels32(px);
    }

    void PaintScattered(WoundSpot s, ForensicCase c)
    {
        if (s.isVein) { PaintVeins(s, c); return; }
        if (s.count <= 1) { PaintSpot(s, c); return; }

        var rng = new System.Random(s.uv.GetHashCode()); // hasil selalu sama tiap scan
        for (int i = 0; i < s.count; i++)
        {
            float ang = (float)(rng.NextDouble() * Mathf.PI * 2);
            float rad = Mathf.Sqrt((float)rng.NextDouble()) * s.scatter;
            var dot = new WoundSpot
            {
                uv = s.uv + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * rad,
                radius = s.radius,
                intensity = s.intensity,
                useCustomColor = s.useCustomColor,
                customPrimary = s.customPrimary,
                customSecondary = s.customSecondary,
                region = s.region
            };
            PaintSpot(dot, c);
        }
    }
}