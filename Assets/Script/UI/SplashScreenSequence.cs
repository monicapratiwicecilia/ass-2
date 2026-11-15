using System.Collections;
using UnityEngine;

public class SplashScreenSequence : MonoBehaviour
{
    [Header("Grup Elemen Splash")]
    public CanvasGroup plateGroup;
    public CanvasGroup logo1Group;
    public CanvasGroup div1Group;
    public CanvasGroup logo2Group;
    public CanvasGroup div2Group;
    public CanvasGroup logo3Group;
    public CanvasGroup wholeSplashGroup;

    [Header("Referensi Menu")]
    public MainMenu mainMenu;

    private Coroutine seqCoroutine;
    private bool isSkipped = false;

    private void Start()
    {
        // Inisialisasi: sembunyikan semua logo di awal agar muncul satu per satu
        SetAlpha(wholeSplashGroup, 1f);
        SetAlpha(plateGroup, 0f);
        SetAlpha(logo1Group, 0f);
        SetAlpha(div1Group, 0f);
        SetAlpha(logo2Group, 0f);
        SetAlpha(div2Group, 0f);
        SetAlpha(logo3Group, 0f);

        seqCoroutine = StartCoroutine(PlaySequentialSplash());
    }

    private IEnumerator PlaySequentialSplash()
    {
        yield return new WaitForSeconds(0.2f);

        // 1. Munculkan plat putih
        yield return StartCoroutine(FadeGroup(plateGroup, 0f, 1f, 0.35f));
        yield return new WaitForSeconds(0.15f);

        // 2. Munculkan Logo 1 (Telkom University)
        yield return StartCoroutine(FadeGroup(logo1Group, 0f, 1f, 0.4f));
        yield return new WaitForSeconds(0.35f);

        // 3. Munculkan Divider 1 & Logo 2 (Fakultas Kedokteran)
        StartCoroutine(FadeGroup(div1Group, 0f, 1f, 0.25f));
        yield return StartCoroutine(FadeGroup(logo2Group, 0f, 1f, 0.4f));
        yield return new WaitForSeconds(0.35f);

        // 4. Munculkan Divider 2 & Logo 3 (Tim A.U.T.O.P.S.Y.)
        StartCoroutine(FadeGroup(div2Group, 0f, 1f, 0.25f));
        yield return StartCoroutine(FadeGroup(logo3Group, 0f, 1f, 0.4f));

        // 5. Tahan sejenak agar ketiga logo tampak utuh dan jelas
        yield return new WaitForSeconds(0.9f);

        // 6. Transisi pudar halus menuju Menu Utama
        yield return StartCoroutine(FadeGroup(wholeSplashGroup, 1f, 0f, 0.4f));

        FinishSplash();
    }

    public void Skip()
    {
        if (isSkipped) return;
        isSkipped = true;

        if (seqCoroutine != null)
        {
            StopCoroutine(seqCoroutine);
            seqCoroutine = null;
        }

        FinishSplash();
    }

    private void FinishSplash()
    {
        gameObject.SetActive(false);
        if (mainMenu != null)
        {
            mainMenu.ShowMenu();
        }
    }

    private IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    private void SetAlpha(CanvasGroup cg, float a)
    {
        if (cg != null) cg.alpha = a;
    }
}
