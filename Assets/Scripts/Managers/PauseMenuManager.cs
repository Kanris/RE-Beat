using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour {

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

    public delegate void DelegateVoid(bool state);
    public event DelegateVoid OnGamePause;

    //public bool isGamePause = false;
    private GameObject m_PauseGame;

    [SerializeField] private GameObject m_FirstSelectedGameobject;

    // Use this for initialization
    void Start () {

        InitializePauseGame();
        SetActive(false);
    }

    private void InitializePauseGame()
    {
        var pauseGameTransform = transform.GetChild(0);

        if (pauseGameTransform != null)
        {
            m_PauseGame = pauseGameTransform.gameObject;
        }
        else
        {
            Debug.LogError("PauseMenuManager.InitializePauseGame: can't find child");
        }
    }

    private void SetActive(bool active)
    {
        if (m_PauseGame != null)
        {
            m_PauseGame.SetActive(active);

            /*if (DialogueManager.Instance.isDialogueInProgress)
                DialogueManager.Instance.SetActiveUI(!active);*/

            if (active == true)
            {
                Time.timeScale = 0f;

                if (m_FirstSelectedGameobject != null)
                    GameObject.Find("EventSystem").GetComponent<EventSystem>().
                              SetSelectedGameObject(m_FirstSelectedGameobject);
            }
            else
                Time.timeScale = 1f;

            if (OnGamePause != null)
                OnGamePause(active);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_PauseGame != null)
        {
            if (CrossPlatformInputManager.GetButtonDown("Cancel"))
            {
                SetActive(!m_PauseGame.activeSelf);
            }
        }
	}

    public void ResumeGame()
    {
        SetActive(!m_PauseGame.activeSelf);
    }

    public void ReturnToStartScreen()
    {
        SetActive(!m_PauseGame.activeSelf);
        PickupBox.isQuitting = true;
        StringTrigger.isQuitting = true;

        LoadSceneManager.Instance.Load("StartScreen");

        Destroy(GameMaster.Instance.gameObject);
        Destroy(gameObject);
    }

    public void ExitGame()
    {
        LoadSceneManager.Instance.CloseGame();
    }
}
