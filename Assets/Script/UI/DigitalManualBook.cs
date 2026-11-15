using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DigitalManualBook : MonoBehaviour
{
    [System.Serializable]
    public class ManualSection
    {
        public string tabName;
        public string headerTitle;
        [TextArea(10, 25)]
        public string contentText;
    }

    [Header("UI References")]
    public TextMeshProUGUI headerTitleText;
    public TextMeshProUGUI contentBodyText;
    public ScrollRect scrollRect;
    public Button[] tabButtons;
    public Image[] tabButtonBackgrounds;

    [Header("Colors")]
    public Color activeTabColor = new Color(0.26f, 0.44f, 0.91f, 1f);     // #4370E7 Electric Blue
    public Color inactiveTabColor = new Color(0.08f, 0.11f, 0.18f, 0.8f); // Dark Slate

    public ManualSection[] sections;

    private int currentSectionIndex = 0;

    private void Awake()
    {
        InitializeSectionsIfEmpty();
    }

    private void Start()
    {
        ShowSection(0);
    }

    public void ShowSection(int index)
    {
        if (sections == null || index < 0 || index >= sections.Length) return;

        currentSectionIndex = index;
        if (headerTitleText != null) headerTitleText.text = sections[index].headerTitle;
        if (contentBodyText != null) contentBodyText.text = sections[index].contentText;

        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1); // Scroll to top
        }

        // Update tab visuals
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtonBackgrounds != null && i < tabButtonBackgrounds.Length && tabButtonBackgrounds[i] != null)
            {
                tabButtonBackgrounds[i].color = (i == index) ? activeTabColor : inactiveTabColor;
            }
        }
    }

    public void OnClickTab(int index)
    {
        ShowSection(index);
    }

    private void InitializeSectionsIfEmpty()
    {
        if (sections != null && sections.Length >= 5) return;

        sections = new ManualSection[5];

        // 1. Story Introduction (Score: +5)
        sections[0] = new ManualSection
        {
            tabName = "Story",
            headerTitle = "1. STORY INTRODUCTION (LATAR BELAKANG)",
            contentText =
                "<b>MISTERI WAKTU KEMATIAN: POST-MORTEM INTERVAL (PMI)</b>\n\n" +
                "Dalam dunia kedokteran forensik, tubuh jenazah adalah saksi bisu paling jujur dari sebuah tragedi. " +
                "Namun, waktu adalah musuh terbesar para penyidik. Setiap detik yang terlewatkan mengubah jejak biologis pada jasad.\n\n" +
                "<b>PERAN PEMAIN (TIM A.U.T.O.P.S.Y.):</b>\n" +
                "Anda bertindak sebagai tim dokter forensik dan penyidik kriminal yang ditugaskan menangani kasus kematian misterius di berbagai TKP. " +
                "Tugas utama Anda adalah mengungkap <b>Post-Mortem Interval (PMI)</b> — estimasi pasti berapa jam korban telah meninggal dunia sebelum ditemukan — " +
                "serta menentukan penyebab kematian yang sah secara patologis.\n\n" +
                "<b>EMPAT PILAR TANDA KEMATIAN FORENSIK:</b>\n" +
                "• <b>Algor Mortis:</b> Penurunan suhu tubuh secara eksponensial mengikuti suhu lingkungan (diukur dengan Termometer Rektal).\n" +
                "• <b>Livor Mortis:</b> Pengendapan darah akibat gravitasi yang membentuk lebam mayat (diuji dengan tes Blanching tekan).\n" +
                "• <b>Rigor Mortis:</b> Kekakuan otot akibat habisnya cadangan ATP pasca-kematian (diperiksa pada sendi).\n" +
                "• <b>Dekomposisi:</b> Tanda pembusukan awal, seperti Greenish Discoloration pada caecum dan jaringan vena Marbling.\n\n" +
                "<i>\"Mati tak selalu sunyi. Melalui sains dan ketelitian, setiap luka dan dinginnya tubuh akan bersuara.\"</i>"
        };

        // 2. Setup Information (Score: +5)
        sections[1] = new ManualSection
        {
            tabName = "Setup",
            headerTitle = "2. SETUP INFORMATION (PENYIAPAN ALAT)",
            contentText =
                "<b>PERSIAPAN SEBELUM MEMULAI INVESTIGASI:</b>\n\n" +
                "<b>1. Bentangkan Board Utama (Blueprint Anatomi A4/A5):</b>\n" +
                "• Letakkan papan fisik Blueprint Anatomi di atas meja investigasi yang datar.\n" +
                "• Pastikan pencahayaan ruangan cukup terang merata dan tidak terlalu silau/reflektif agar pelacakan kamera Augmented Reality optimal.\n\n" +
                "<b>2. Siapkan 4 Kotak Slot Interaktif:</b>\n" +
                "Di bagian bawah Blueprint Anatomi terdapat 4 kotak target tindakan khusus:\n" +
                "  [Slot 1: KEPALA]  → Khusus Kartu Kaca Pembesar / Inspeksi Kepala\n" +
                "  [Slot 2: DADA]    → Khusus Kartu Uji Lebam (Blanching)\n" +
                "  [Slot 3: ABDOMEN] → Khusus Kartu Pembusukan / Cek Sendi\n" +
                "  [Slot 4: PELVIS]  → Khusus Kartu Termometer 3D\n\n" +
                "<b>3. Koneksi Smartphone & Web Dashboard:</b>\n" +
                "• Jalankan aplikasi <b>PMI Forensic Clock AR</b> pada smartphone dan izinkan akses kamera.\n" +
                "• Buka Web Dashboard di laptop melalui browser (<b>chocopie21.github.io/pmi-forensic-clock-ar</b>) untuk kalkulator Henssge dan berkas otopsi.\n" +
                "• Siapkan kartu skenario kasus fisik (Kasus 1 hingga Kasus 7) di sisi meja."
        };

        // 3. Card Explanation (Score: +5)
        sections[2] = new ManualSection
        {
            tabName = "Kartu",
            headerTitle = "3. CARD EXPLANATION (PENJELASAN KARTU)",
            contentText =
                "<b>A. 7 KARTU SKENARIO KASUS TKP:</b>\n\n" +
                "<b>#1. Kamar Mandi (AC 20°C) - Kematian Wajar / Serangan Jantung:</b>\n" +
                "Korban ditemukan di kamar mandi dengan AC menyala dingin. Lebam merah keunguan standar di punggung dan paha belakang. " +
                "Saat ditekan (blanching), lebam memudar menandakan kematian < 8-10 jam. Wajah dan dada pucat normal.\n\n" +
                "<b>#2. Gudang Pabrik (33°C) - Posisi Telungkup / Suhu Panas:</b>\n" +
                "Korban meninggal telungkup di gudang bersuhu 33°C dengan jaket tebal (CF 1.3). Lebam ungu pekat mengendap di dada dan perut depan. " +
                "Lebam menetap (tidak memudar saat ditekan), menandakan kematian telah melewati 8-10 jam. Terdapat Kongesti Vena di wajah.\n\n" +
                "<b>#3. Kebun Terbuka (24°C) - Fase Pembusukan Lanjut:</b>\n" +
                "Korban ditemukan di semak kebun. Lebam punggung ungu gelap kehitaman. Tampak Greenish Discoloration (bercak hijau caecum di perut kanan bawah) " +
                "dan pola Marbling vena hijau kehitaman menyebar di dada dan perut (PMI ±24 jam).\n\n" +
                "<b>#4. Tenggelam Air Tawar (Danau):</b>\n" +
                "Ditemukan busa halus putih di hidung/mulut dan washerwoman skin (kulit telapak tangan keriput). Pendinginan air sangat cepat (CF 0.5).\n\n" +
                "<b>#5. Tenggelam Air Laut (Dermaga):</b>\n" +
                "Busa kemerahan keluar dari saluran nafas akibat edema paru hebat, disertai cutis anserina (kulit merinding).\n\n" +
                "<b>#6. Asfiksia / Gantung Diri:</b>\n" +
                "Terdapat jejas jerat berbentuk huruf 'V' di leher. Lebam terkumpul di kedua tungkai bawah akibat tarikan gravitasi vertikal.\n\n" +
                "<b>#7. Keracunan Metanol (Alkohol Oplosan):</b>\n" +
                "Sianosis kebiruan gelap pada bibir dan ujung jari, pupil mata melebar ekstrem (midriasis).\n\n" +
                "-----------------------------------------\n" +
                "<b>B. 4 KARTU TINDAKAN UNIVERSAL:</b>\n\n" +
                "• <b>Termometer (Slot Pelvis):</b> Memunculkan model 3D termometer rektal untuk membaca suhu inti jenazah.\n" +
                "• <b>Uji Lebam (Slot Dada):</b> Mengaktifkan pewarnaan lebam mayat (Livor Mortis) di dada, punggung, dan paha.\n" +
                "• <b>Kaca Pembesar (Slot Kepala):</b> Melakukan inspeksi cranium, mata, leher, dan wajah.\n" +
                "• <b>Sendi / Pembusukan (Slot Abdomen):</b> Menguji kaku mayat serta memunculkan patologi dekomposisi perut."
        };

        // 4. Gameplay Information (Score: +5)
        sections[3] = new ManualSection
        {
            tabName = "Gameplay",
            headerTitle = "4. GAMEPLAY INFORMATION (ALUR PERMAINAN)",
            contentText =
                "<b>ALUR INVESTIGASI 4 TAHAP STEP-BY-STEP:</b>\n\n" +
                "<b>TAHAP 1: BRIEFING & SETUP SKENARIO KASUS</b>\n" +
                "1. Tim mengambil 1 Kartu Kasus secara acak (misal: Kasus 1, 2, atau 3).\n" +
                "2. Operator laptop memasukkan parameter kasus ke Web Dashboard: Suhu Lingkungan (Ta), Berat Badan (kg), dan Faktor Koreksi Baju (CF).\n" +
                "3. Arahkan kamera smartphone ke Kartu Kasus untuk membaca narasi ringkasan TKP.\n\n" +
                "<b>TAHAP 2: SINKRONISASI AR 3D BOARD UTAMA</b>\n" +
                "1. Arahkan kamera smartphone ke Board Blueprint Anatomi di tengah meja.\n" +
                "2. Model 3D tubuh manusia otomatis muncul stabil berbaring tepat di atas papan.\n" +
                "3. Gunakan gestur satu jari untuk memutar (rotate) tubuh guna memeriksa bagian punggung, dan dua jari (pinch) untuk zoom in/out.\n\n" +
                "<b>TAHAP 3: INVESTIGASI DUAL-MARKER & UJI FISIK</b>\n" +
                "1. Letakkan Kartu Tindakan pada kotak slot yang sesuai di Blueprint Anatomi.\n" +
                "2. Deteksi kartu akan secara instan melukis lebam, memunculkan termometer 3D, atau menampilkan tanda pembusukan.\n" +
                "3. Catat seluruh temuan medis: suhu tubuh rektal, warna lebam, hasil tes blanching, dan tanda patologi khusus.\n\n" +
                "<b>TAHAP 4: KALKULASI NOMOGRAM HENSSGE & VONIS AKHIR</b>\n" +
                "1. Masukkan suhu rektal jenazah ke Kalkulator Henssge di Web Dashboard.\n" +
                "2. Web akan menghitung rentang estimasi PMI (Jam Kematian).\n" +
                "3. Diskusikan dengan tim untuk merumuskan kesimpulan akhir (Jam Kematian & Penyebab Kematian)."
        };

        // 5. Win/Lose Information (Score: +5)
        sections[4] = new ManualSection
        {
            tabName = "Win/Lose",
            headerTitle = "5. WIN / LOSE INFORMATION (SKORING & VONIS)",
            contentText =
                "<b>KRITERIA KEMENANGAN & SKOR MAKSIMAL:</b>\n\n" +
                "<b>1. SKOR SEMPURNA 100 POIN OTOPSI (KASUS TERPECAHKAN):</b>\n" +
                "Tim dinobatkan sebagai <i>Ahli Forensik Utama</i> dan meraih <b>100 Poin</b> apabila:\n" +
                "• Tebakan <b>Jam Kematian</b> berada tepat di dalam batas toleransi ilmiah Rumus Nomogram Henssge (± toleransi jam).\n" +
                "• <b>Penyebab Kematian</b> (Cause of Death) terbukti valid secara patologis sesuai skenario otopsi.\n" +
                "• Tombol <i>\"Buka Segel Berkas Otopsi\"</i> di Web Dashboard mengonfirmasi kesesuaian 100%.\n\n" +
                "<b>2. KETENTUAN PENALTI & KEKALAHAN:</b>\n" +
                "• <b>Salah Menentukan Penyebab Kematian:</b> Penalti -30 Poin (Diagnosa keliru dapat mengakibatkan salah tangkap tersangka di pengadilan!).\n" +
                "• <b>Estimasi PMI Meleset dari Toleransi Henssge:</b> Penalti -20 Poin (Ketidakakuratan alibi waktu kematian).\n" +
                "• <b>Kehabisan Waktu Investigasi:</b> Kasus ditutup oleh pihak berwenang dengan status <i>UNSOLVED COLD CASE</i> (0 Poin).\n\n" +
                "<i>Kerja sama tim, ketelitian mengamati lebam mayat di AR, dan perhitungan cermat di web adalah kunci kemenangan mutlak!</i>"
        };
    }
}
