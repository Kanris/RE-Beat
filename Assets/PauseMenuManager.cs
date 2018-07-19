using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public bool isGamePause = false;
    private GameObject m_PauseGame;
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

            if (active == true)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_PauseGame != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ChangeUIState();
            }
        }
	}

    private void ChangeUIState()
    {
        isGamePause = !isGamePause;
        SetActive(isGamePause);
    }

    public void ResumeGame()
    {
        ChangeUIState();
    }

    public void ReturnToStartScreen()
    {
        ChangeUIState();
        LoadSceneManager.Instance.Load("StartScreen");
    }

    public void ExitGame()
    {
        LoadSceneManager.Instance.CloseGame();
    }
}
