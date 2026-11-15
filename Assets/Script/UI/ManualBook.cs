using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManualBook : MonoBehaviour
{
    [Header("Daftar Halaman Manual")]
    public List<GameObject> pages = new List<GameObject>();

    [Header("Navigasi")]
    public Button btnPrev;
    public Button btnNext;
    public TMP_Text pageIndicator;

    private int currentPage = 0;

    private void Start()
    {
        ShowPage(0);
    }

    private void OnEnable()
    {
        ShowPage(0);
    }

    public void NextPage()
    {
        if (currentPage < pages.Count - 1)
        {
            ShowPage(currentPage + 1);
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            ShowPage(currentPage - 1);
        }
    }

    public void ShowPage(int index)
    {
        if (pages == null || pages.Count == 0) return;

        currentPage = Mathf.Clamp(index, 0, pages.Count - 1);

        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPage);
            }
        }

        if (btnPrev != null)
        {
            btnPrev.interactable = (currentPage > 0);
        }

        if (btnNext != null)
        {
            btnNext.interactable = (currentPage < pages.Count - 1);
        }

        if (pageIndicator != null)
        {
            pageIndicator.text = $"Halaman {currentPage + 1} / {pages.Count}";
        }
    }
}
