using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenManager : MonoBehaviour {

    public static bool IsLoadPressed;

    [SerializeField] private string BackgroundMusic;

    #region Initialize Managers

    private void Awake()
    {
        Initialize("Managers/EventSystem");

        Initialize("Managers/AudioManager");

        Initialize("Managers/LoadSceneManager");

        Initialize("Managers/ScreenFaderManager");

        Initialize("Managers/SaveLoadManager");

        InitializeBackgroundMusic();
    }

    void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    #endregion

    private void InitializeBackgroundMusic()
    {
        if (!string.IsNullOrEmpty(BackgroundMusic))
            AudioManager.Instance.SetBackgroundMusic(BackgroundMusic);
    }

    public void LoadScene(string name)
    {
        IsLoadPressed = false;
        PlayClickSound();
        LoadSceneManager.Instance.Load(name);
    }

    public void LoadGame()
    {
        IsLoadPressed = true;
        PlayClickSound();
        SaveLoadManager.Instance.LoadScene();
    }

    public void ExitGame()
    {
        PlayClickSound();
        Application.Quit();
    }

    #region Sound

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("UI-Click");
        }
        else
        {
            Debug.LogError("StartScreenManager.PlayClickSound: Audiomanager.Instance is equal to null");
        }
    }

    #endregion
}
