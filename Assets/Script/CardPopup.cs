using TMPro;
using UnityEngine;
using Vuforia;

/// Pasang di Image Target KARTU (kasus maupun tindakan).
/// Saat kartu terdeteksi (dan papan A3 juga terdeteksi, bila Require Board dicentang):
///  - Kartu Kasus    : memilih kasus + menampilkan narasi TKP
///  - Kartu Tindakan : membuka temuan bagian tubuh + menampilkan popup (objek 3D + teks)
/// Menggantikan CardScanTrigger dan CaseSelectTrigger, jadi hapus keduanya dari kartu.
public class CardPopup : MonoBehaviour
{
    public enum CardType { Tindakan, Kasus }

    public CardType type = CardType.Tindakan;
    public CaseColorManager manager;     // seret 3d_male
    public BodyRegion region;            // untuk kartu Tindakan
    public int caseIndex = 1;            // untuk kartu Kasus (0 = Kasus 1, 1 = Kasus 2, ...)

    [Header("Multi marker")]
    public ObserverBehaviour board;      // seret Tubuh (papan A3)
    public bool requireBoard = false;    // default false agar kartu tindakan langsung aktif saat discan

    [Header("Popup")]
    public GameObject popupRoot;         // anak kartu: panel teks (nonaktif di awal)
    public GameObject model3D;           // objek 3D alat (stetoskop, termometer, dsb.)
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Rotasi tubuh")]
    public ModelRotator bodyRotator;     // seret 3d_male, dimatikan saat popup tampil

    ObserverBehaviour obs;
    bool shown;
    static int visibleCount;

    void Awake()
    {
        obs = GetComponent<ObserverBehaviour>();
        if (obs != null) obs.OnTargetStatusChanged += OnStatus;
        if (board != null) board.OnTargetStatusChanged += OnStatus;

        // Otomatis deteksi model 3D alat bila belum diisi di Inspector
        if (model3D == null && popupRoot != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                if (child != popupRoot)
                {
                    model3D = child;
                    break;
                }
            }
        }

        // Otomatis deteksi teks judul dan isi bila belum diisi di Inspector
        if (popupRoot != null && (titleText == null || bodyText == null))
        {
            var texts = popupRoot.GetComponentsInChildren<TMP_Text>(true);
            if (titleText == null && texts.Length > 0) titleText = texts[0];
            if (bodyText == null && texts.Length > 1) bodyText = texts[1];
        }

        if (popupRoot) popupRoot.SetActive(false);
        if (model3D) model3D.SetActive(false);
    }

    void OnDestroy()
    {
        if (obs) obs.OnTargetStatusChanged -= OnStatus;
        if (board) board.OnTargetStatusChanged -= OnStatus;
        if (shown) { visibleCount = Mathf.Max(0, visibleCount - 1); shown = false; }
    }

    static bool IsCardTracked(ObserverBehaviour o)
    {
        if (o == null) return false;
        // Kartu hanya dianggap aktif bila benar-benar TRACKED secara visual oleh kamera.
        // Tidak mengikutsertakan EXTENDED_TRACKED agar teks/objek popup tidak melayang & tertinggal saat kartu disingkirkan.
        return o.TargetStatus.Status == Status.TRACKED;
    }

    static bool IsBoardPresent(ObserverBehaviour b)
    {
        if (b == null) return false;
        var s = b.TargetStatus.Status;
        // Papan Tubuh boleh TRACKED, EXTENDED_TRACKED, atau LIMITED agar multi-marker tetap stabil
        // saat kamera sedikit bergeser/fokus ke arah kartu tindakan di samping tubuh.
        return s == Status.TRACKED || s == Status.EXTENDED_TRACKED || s == Status.LIMITED;
    }

    void OnStatus(ObserverBehaviour b, TargetStatus s) { Refresh(); }

    void Refresh()
    {
        bool want = IsCardTracked(obs) && (!requireBoard || IsBoardPresent(board));
        if (want && !shown) Show();
        else if (!want && shown) Hide();
    }

    void Show()
    {
        shown = true;
        visibleCount++;

        string title = "", body = "";

        if (manager != null)
        {
            if (type == CardType.Kasus)
            {
                manager.SetActiveCase(caseIndex);
                title = manager.GetCaseName();
                body = manager.GetNarrative();
            }
            else if (manager.ActiveCase < 0)
            {
                title = "Belum Ada Kasus";
                body = "Silakan scan kartu Kasus (TKP) terlebih dahulu sebelum melakukan pemeriksaan.";
            }
            else
            {
                manager.RevealRegion(region);
                manager.TryGetText(region, out title, out body);
            }
        }

        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;
        if (popupRoot)
        {
            popupRoot.SetActive(true);
            foreach (var r in popupRoot.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;
        }
        if (model3D)
        {
            model3D.SetActive(true);
            foreach (var r in model3D.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;
            var rotator = model3D.GetComponent<ModelRotator>();
            if (rotator != null) rotator.ResetView();
        }
        if (bodyRotator) bodyRotator.enabled = false;   // supaya hanya objek popup yang berputar
    }

    void Hide()
    {
        shown = false;
        visibleCount = Mathf.Max(0, visibleCount - 1);
        if (popupRoot) popupRoot.SetActive(false);
        if (model3D) model3D.SetActive(false);
        if (bodyRotator && visibleCount == 0) bodyRotator.enabled = true;
    }

    // Klik titik tiga di komponen saat Play untuk mengetes tanpa kartu
    [ContextMenu("Tes tampilkan popup")]
    void TestShow() { if (!shown) Show(); }

    [ContextMenu("Tes sembunyikan popup")]
    void TestHide() { if (shown) Hide(); }
}
