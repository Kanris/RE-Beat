using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextPage : MonoBehaviour {

    [SerializeField] private GameObject m_NextPageButton;
    [SerializeField] private GameObject m_PreviousButton;
    [SerializeField] private TextMeshProUGUI m_TaskText;

    private int m_CurrentPage;

	// Use this for initialization
	void Start () {

        ShowNextPage(false);
        ShowPreviousPage(false);

        m_CurrentPage = 1;
    }

    public void ClearText()
    {
        if (m_TaskText != null) m_TaskText.text = "";
    }

    public void ShowText(string taskText)
    {
        m_TaskText.text = taskText;

        if (m_TaskText.textInfo.pageCount > 1)
        {
            ShowNextPage(true);
            ShowPreviousPage(false);
        }
        else
        {
            ShowNextPage(false);
            ShowPreviousPage(false);
        }

        m_CurrentPage = 1;
    }

    public void MoveToNextPage()
    {
        if (m_CurrentPage + 2 > m_TaskText.textInfo.pageCount)
        {
            ShowNextPage(false);
            ShowPreviousPage(true);
        }
        else
        {
            ShowNextPage(true);
            ShowPreviousPage(true);
        }

        m_CurrentPage++;
        m_TaskText.pageToDisplay = m_CurrentPage;
    }

    public void MoveToPreviousPage()
    {
        if (m_CurrentPage - 2 < 1)
        {
            ShowPreviousPage(false);
            ShowNextPage(true);
        }
        else
        {
            ShowPreviousPage(true);
            ShowNextPage(true);
        }

        m_CurrentPage--;
        m_TaskText.pageToDisplay = m_CurrentPage;
    }

    private void ShowPreviousPage(bool value)
    {
        m_PreviousButton.SetActive(value);
    }

    private void ShowNextPage(bool value)
    {
        m_NextPageButton.SetActive(value);
    }
}
