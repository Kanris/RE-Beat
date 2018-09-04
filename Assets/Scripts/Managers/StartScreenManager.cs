using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour {

    #region static fields

    public static bool IsLoadPressed;
    public static int ResolutionIndex;
    public static bool IsFullscreen;
    public static float Volume;

    #endregion

    #region serialize fields

    [SerializeField] private string BackgroundMusic;
    [SerializeField] private GameObject MainMenuGrid;
    [SerializeField] private GameObject OptionsMenuGrid;
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Dropdown resoulutionsDropDown;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    #endregion

    #region private fields

    private Resolution[] resolutions;

    #endregion

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

    private void Start()
    {
        resolutions = Screen.resolutions;

        InitializeDropDownResolutions();

        if (SaveLoadManager.Instance != null)
        {
            var result = SaveLoadManager.Instance.LoadOptions();

            if (result)
                InitializeOptions();
        }
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

    private void InitializeOptions()
    {
        resoulutionsDropDown.value = ResolutionIndex;
        volumeSlider.value = Volume;
        fullscreenToggle.isOn = IsFullscreen;

        SetFloat(Volume);
        SetFullScreen(IsFullscreen);
        SetResolution(ResolutionIndex);
    }

    #endregion

    #region private methods

    private void InitializeDropDownResolutions()
    {
        resoulutionsDropDown.ClearOptions();

        var resolutionsList = new List<string>();

        var currentResolutionItem = string.Empty;

        foreach (var item in resolutions)
        {
            string option = item.width + " x " + item.height;
            resolutionsList.Add(option);

            if (item.width == Screen.currentResolution.width && 
                item.height == Screen.currentResolution.height)
            {
                currentResolutionItem = option;
            }
        }

        var indexOfCurrentResolution = resolutionsList.IndexOf(currentResolutionItem);

        resoulutionsDropDown.AddOptions(resolutionsList);

        if (indexOfCurrentResolution != -1)
        {
            resoulutionsDropDown.value = indexOfCurrentResolution;
            resoulutionsDropDown.RefreshShownValue();
        }
    }

    private void ChangeGridsVisibility()
    {
        MainMenuGrid.SetActive(!MainMenuGrid.activeSelf);
        OptionsMenuGrid.SetActive(!OptionsMenuGrid.activeSelf);

        if (!OptionsMenuGrid.activeSelf)
            SaveLoadManager.Instance.SaveOptions();
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

    #endregion

    #region public methods 

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

    public void Options()
    {
        ChangeGridsVisibility();
    }

    public void SetFloat(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
        Volume = volume;
    }

    public void SetFullScreen(bool value)
    {
        Screen.fullScreen = value;
        IsFullscreen = value;
    }

    public void SetResolution(int resolutionIndex)
    {
        Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, Screen.fullScreen);
        ResolutionIndex = resolutionIndex;
    }

    #endregion
}
