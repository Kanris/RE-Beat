using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;

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

#endregion

#region private fields

    [SerializeField] private GameObject m_FirstSelectedGameobject;
    [SerializeField] private GameObject m_UI;

    private bool m_IsCantOpenPauseMenu;

#endregion

#region private methods

    // Use this for initialization
    private void Start () {

        InfoManager.Instance.OnJournalOpen += SetIsCantOpenPauseMenu;

        SetActiveUI();
    }

    private void SetActiveUI()
    {
        m_UI.SetActive(!m_UI.activeSelf); //show/hide pause menu ui

        if (m_UI.activeSelf == true) //if pause manager show ui
        {
            Time.timeScale = 0f; //stop game time

            if (m_FirstSelectedGameobject != null)
                EventSystem.current.SetSelectedGameObject(m_FirstSelectedGameobject); //choose first item in menu
        }
        else
            Time.timeScale = 1f; //resume game time

        if (OnGamePause != null)
            OnGamePause(m_UI.activeSelf);
    }
	
	// Update is called once per frame
	private void Update () {

        if (!m_IsCantOpenPauseMenu)
        {
            //if player pressed pause button
            if (CrossPlatformInputManager.GetButtonDown("Cancel"))
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

    private void SetIsCantOpenPauseMenu(bool value)
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
        PlayClickSound();

        SetActiveUI();

        if (OnReturnToStartSceen != null)
            OnReturnToStartSceen(true);

        LoadSceneManager.Instance.Load("StartScreen");

        DestroyManagers();
        Destroy(GameMaster.Instance.gameObject);
        Destroy(gameObject);
    }

    public void ExitGame()
    {
        PlayClickSound();
        LoadSceneManager.Instance.CloseGame();
    }

#endregion
}
