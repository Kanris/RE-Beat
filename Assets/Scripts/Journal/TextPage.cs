using UnityEngine;
using TMPro;

public class TextPage : MonoBehaviour {

    #region private fields

    #region serialize fields

    [SerializeField] private GameObject m_NextPageButton; //next page button
    [SerializeField] private GameObject m_PreviousButton; //previous page button
    [SerializeField] private TextMeshProUGUI m_TaskText; //main page text

    #endregion

    private int m_CurrentPage; //current page index

    #endregion

    #region private methods

    // Use this for initialization
    private void Start () {

        //hide next and previous buttons
        ShowNextPage(false); 
        ShowPreviousPage(false);

        m_CurrentPage = 1; //set current page index
    }

    private void ShowPreviousPage(bool value)
    {
        m_PreviousButton.SetActive(value); //active or disable previous button
    }

    private void ShowNextPage(bool value)
    {
        m_NextPageButton.SetActive(value); //active or disable next button
    }

    #endregion

    #region public methods

    public void ClearText()
    {
        if (m_TaskText != null) m_TaskText.text = ""; //clear main page text
    }

    public void ShowText(string taskText)
    {
        m_TaskText.text = taskText; //show given text

        if (m_TaskText.textInfo.pageCount > 1) //if page counts grater than 1
        {
            ShowNextPage(true); //show next button
            ShowPreviousPage(false); //hide previous
        }
        else
        {
            //hide both buttons
            ShowNextPage(false);
            ShowPreviousPage(false);
        }

        m_CurrentPage = 1; //current page index
    }

    public void MoveToNextPage()
    {
        if (m_CurrentPage + 2 > m_TaskText.textInfo.pageCount) //if there is no further pages
        {
            ShowNextPage(false); //hide next button
            ShowPreviousPage(true); //show previous button
        }
        else //if there is move pages
        {
            //show both buttons
            ShowNextPage(true);
            ShowPreviousPage(true);
        }

        m_CurrentPage++; //move to next page
        m_TaskText.pageToDisplay = m_CurrentPage; //show current page index
    }

    public void MoveToPreviousPage()
    {
        if (m_CurrentPage - 2 < 1) //if there is not previous pages
        {
            ShowPreviousPage(false); //hide previous button
            ShowNextPage(true); //show next button
        }
        else //if there is previous pages
        {
            //show both buttons
            ShowPreviousPage(true);
            ShowNextPage(true);
        }

        m_CurrentPage--; //move to previous index
        m_TaskText.pageToDisplay = m_CurrentPage; //show previous page
    }

    #endregion
}
