using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseMenuManager : MonoBehaviour {

    [SerializeField] private Audio UIClickAudio;

#region Singleton
    public static PauseMenuManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
#endregion

#region public fields

    public delegate void DelegateVoid(bool state);
    public event DelegateVoid OnGamePause;
    public event DelegateVoid OnReturnToStartSceen;

    public delegate void ActionDelegate();
    public ActionDelegate actionDelegate;

    #endregion

    #region private fields

    [SerializeField] private GameObject m_FirstSelectedGameobject;
    [SerializeField] private GameObject m_NoDialogueAnswer;
    [SerializeField] private GameObject m_UI;

    [Header("Pause UI")]
    [SerializeField] private GameObject m_PauseUI;

    [Header("Confirm dialogue")]
    [SerializeField] private GameObject m_ConfirmDialogueUI;

    private bool m_IsCantOpenPauseMenu;

#endregion

#region private methods

    // Use this for initialization
    private void Start () {

        InfoManager.Instance.OnJournalOpen += SetIsCantOpenPauseMenu;

        SetActiveUI();
    }

    //private bool IsGamePadConnected()
    //{
    //    GamePadState
    //}

    private void SetActiveUI()
    {
        if (!m_UI.activeSelf) //if pause manager show ui
        {
            m_UI.SetActive(!m_UI.activeSelf); //show/hide pause menu ui

            m_PauseUI.SetActive(true);
            m_ConfirmDialogueUI.SetActive(false);

            Time.timeScale = 0f; //stop game time

            SetButtonInFocus(m_FirstSelectedGameobject);

            if (OnGamePause != null)
                OnGamePause(m_UI.activeSelf);
        }
        else
        {
            StartCoroutine(SubmitWithDelay());
        }
    }

    private IEnumerator SubmitWithDelay()
    {
        yield return null;
        Time.timeScale = 1f; //resume game time


        m_UI.SetActive(!m_UI.activeSelf); //show/hide pause menu ui

        if (OnGamePause != null)
            OnGamePause(m_UI.activeSelf);
    }

    // Update is called once per frame
    private void Update () {

        if (!m_IsCantOpenPauseMenu)
        {
            //if player pressed pause button
            if (CrossPlatformInputManager.GetButtonDown("Escape"))
            {
                SetActiveUI();
            }
        }
    }

#region Sound

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(UIClickAudio);
        }
        else
        {
            Debug.LogError("StartScreenManager.PlayClickSound: Audiomanager.Instance is equal to null");
        }
    }

#endregion

    public static bool IsPauseManagerActive()
    {
        var result = false;

        if (Instance != null)
        {
            result = Instance.m_UI.activeSelf;
        }

        return result;
    }

    public void SetIsCantOpenPauseMenu(bool value)
    {
        m_IsCantOpenPauseMenu = value;
    }

    private void DestroyManagers()
    {
        //destroy managers that don't need on start screen
        Destroy(GameObject.Find("PauseMenuManager(Clone)"));
        Destroy(GameObject.Find("AnnouncerManager(Clone)"));
        Destroy(GameObject.Find("DialogueManager(Clone)"));
        Destroy(GameObject.Find("UIManager(Clone)"));
        Destroy(GameObject.Find("InfoManager(Clone)"));

#if MOBILE_INPUT
        Destroy(GameObject.Find("MobileTouchControl(Clone)"));
#endif
    }

#endregion

#region public methods

    public void ResumeGame()
    {
        PlayClickSound();
        SetActiveUI();
    }

    public void ReturnToStartScreen()
    {
        m_ConfirmDialogueUI.SetActive(true);
        m_PauseUI.SetActive(false);

        SetButtonInFocus(m_NoDialogueAnswer);

        actionDelegate = () =>
        {
            PlayClickSound();

            SetActiveUI();

            if (OnReturnToStartSceen != null)
                OnReturnToStartSceen(true);

            LoadSceneManager.Instance.Load("StartScreen");

            DestroyManagers();
            Destroy(GameMaster.Instance.gameObject);
            Destroy(gameObject);
        };
    }

    public void ExitGame()
    {
        m_ConfirmDialogueUI.SetActive(true);
        m_PauseUI.SetActive(false);

        SetButtonInFocus(m_NoDialogueAnswer);

        actionDelegate = () =>
        {
            PlayClickSound();
            LoadSceneManager.Instance.CloseGame();
        };
    }

    public void YesButtonPressed()
    {
        if (actionDelegate != null)
            actionDelegate();

        m_ConfirmDialogueUI.SetActive(false);
    }

    public void NoButtonPressed()
    {
        m_ConfirmDialogueUI.SetActive(false);
        m_PauseUI.SetActive(true);

        SetButtonInFocus(m_FirstSelectedGameobject);

    }

    private void SetButtonInFocus(GameObject ItemInFocus)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(ItemInFocus);
    }

    #endregion
}
