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
    public bool requireBoard = true;

    [Header("Popup")]
    public GameObject popupRoot;         // anak kartu: objek 3D + panel teks (nonaktif di awal)
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
        obs.OnTargetStatusChanged += OnStatus;
        if (board != null) board.OnTargetStatusChanged += OnStatus;
        if (popupRoot) popupRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (obs) obs.OnTargetStatusChanged -= OnStatus;
        if (board) board.OnTargetStatusChanged -= OnStatus;
        if (shown) { visibleCount = Mathf.Max(0, visibleCount - 1); shown = false; }
    }

    static bool IsVisible(ObserverBehaviour o)
    {
        if (o == null) return false;
        var s = o.TargetStatus.Status;
        return s == Status.TRACKED || s == Status.EXTENDED_TRACKED;
    }

    void OnStatus(ObserverBehaviour b, TargetStatus s) { Refresh(); }

    void Refresh()
    {
        bool want = IsVisible(obs) && (!requireBoard || IsVisible(board));
        if (want && !shown) Show();
        else if (!want && shown) Hide();
    }

    void Show()
    {
        shown = true;
        visibleCount++;

        string title = "", body = "";

        if (type == CardType.Kasus)
        {
            manager.SetActiveCase(caseIndex);
            title = manager.GetCaseName();
            body = manager.GetNarrative();
        }
        else if (manager.ActiveCase < 0)
        {
            title = "Belum ada kasus";
            body = "Scan kartu kasus terlebih dahulu.";
        }
        else
        {
            manager.RevealRegion(region);
            manager.TryGetText(region, out title, out body);
        }

        if (titleText) titleText.text = title;
        if (bodyText) bodyText.text = body;
        if (popupRoot) popupRoot.SetActive(true);
        if (bodyRotator) bodyRotator.enabled = false;   // supaya hanya objek popup yang berputar
    }

    void Hide()
    {
        shown = false;
        visibleCount = Mathf.Max(0, visibleCount - 1);
        if (popupRoot) popupRoot.SetActive(false);
        if (bodyRotator && visibleCount == 0) bodyRotator.enabled = true;
    }

    // Klik titik tiga di komponen saat Play untuk mengetes tanpa kartu
    [ContextMenu("Tes tampilkan popup")]
    void TestShow() { if (!shown) Show(); }

    [ContextMenu("Tes sembunyikan popup")]
    void TestHide() { if (shown) Hide(); }
}