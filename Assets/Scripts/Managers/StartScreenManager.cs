using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenManager : MonoBehaviour {

    [SerializeField] private string BackgroundMusic;

    private void Awake()
    {
        #region Initialize Managers
        Initialize("Managers/EventSystem");

        Initialize("Managers/AudioManager");

        Initialize("Managers/LoadSceneManager");

        Initialize("Managers/ScreenFaderManager");

        InitializeBackgroundMusic();

        #endregion
    }

    void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    private void InitializeBackgroundMusic()
    {
        if (!string.IsNullOrEmpty(BackgroundMusic))
            AudioManager.Instance.SetBackgroundMusic(BackgroundMusic);
    }

    public void LoadScene(string name)
    {
        LoadSceneManager.Instance.Load(name);
    }

    public void LoadGame()
    {

    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
