using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreenManager : MonoBehaviour {

    #region static fields

    public static bool IsLoadPressed; //is load button pressed
    public static int ResolutionIndex;
    public static bool IsFullscreen;
    public static float VolumeMaster;
    public static float VolumeEnvironment;
    public static string LocalizationToLoad;

    #endregion

    #region serialize fields


    [Header("General UI")]
    [SerializeField] private GameObject MainMenuGrid; //main menu ui
    [SerializeField] private GameObject OptionsMenuGrid; //option ui

    //options items
    [Header("Options UI")]
    [SerializeField] private TMP_Dropdown resoulutionsDropDown;
    [SerializeField] private Slider volumeMasterSlider;
    [SerializeField] private Slider volumeEnvironmentSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Effects")]
    [SerializeField] private Audio BackgroundMusic; //background music
    [SerializeField] private Audio UIClickAudio;

    #endregion

    #region private fields

    private Resolution[] resolutions; //available resolutions

    #endregion

    #region Initialize Managers

    private void Awake()
    {
        //initialize needed managers
        Initialize("Managers/EventSystem");

        Initialize("Managers/AudioManager");

        Initialize("Managers/LoadSceneManager");

        Initialize("Managers/ScreenFaderManager");

        Initialize("Managers/SaveLoadManager");

        Initialize("Managers/LocalizationManager");

        Initialize("Managers/FPSManager");

        //play background music
        if (!string.IsNullOrEmpty(BackgroundMusic))
            AudioManager.Instance.SetBackgroundMusic(BackgroundMusic);

        InitializeOptionsOnStart();

#if MOBILE_INPUT
        MainMenuGrid.transform.localPosition = MainMenuGrid.transform.localPosition.With(y: 60f);
        MainMenuGrid.transform.GetChild(2).gameObject.SetActive(false);
#endif
    }

    private void InitializeOptionsOnStart()
    {
        resolutions = Screen.resolutions; //get available resolutions

        InitializeDropDownResolutions(); //show available resolutions

        if (SaveLoadManager.Instance != null)
        {
            var result = SaveLoadManager.Instance.LoadOptions(); //try to load saved options

            if (result) //if there is save options file
                InitializeOptions(); //load options
        }
    }

    void Initialize(string name)
    {
        var gameObjectToInstantiate = Resources.Load(name);
        Instantiate(gameObjectToInstantiate);
    }

    private void InitializeOptions()
    {
        resoulutionsDropDown.value = ResolutionIndex; //set current resolution index
        volumeMasterSlider.value = VolumeMaster; //set general volume
        volumeEnvironmentSlider.value = VolumeEnvironment; //set environment volume
        fullscreenToggle.isOn = IsFullscreen; //set fullscreen

        SetMasterFloat(VolumeMaster);
        SetEnvironmentFloat(VolumeEnvironment);

        SetFullScreen(IsFullscreen);
        SetResolution(ResolutionIndex);

        /*
        LocalizationManager.Instance.LoadGeneralLocalizationData(LocalizationToLoad);

        if (LocalizationManager.LocalizationToLoad != LocalizationToLoad)
        {
            LocalizationManager.LocalizationToLoad = LocalizationToLoad;
            SceneManager.LoadScene("StartScreen");
        }*/
    }

    #endregion

    #region private methods

    private void InitializeDropDownResolutions()
    {
        resoulutionsDropDown.ClearOptions(); //clear options

        var resolutionsList = new List<string>(); //resolutions list

        var currentResolutionItem = string.Empty;

        foreach (var item in resolutions)
        {
            string option = item.width + " x " + item.height;

            if (!resolutionsList.Contains(option))
            {
                resolutionsList.Add(option);

                if (item.width == Screen.currentResolution.width &&
                    item.height == Screen.currentResolution.height)
                {
                    currentResolutionItem = option;
                }
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

    private void ChangeGridsVisibility() //change visible menus
    {
        PlayClickSound();
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
            AudioManager.Instance.Play(UIClickAudio);
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

        LocalizationManager.Instance.LoadJournalLocalizationData(LocalizationToLoad);
        LocalizationManager.Instance.LoadDialogueLocalizationData(LocalizationToLoad);
    }

    public void LoadGame()
    {
        IsLoadPressed = true;
        PlayClickSound();

        SaveLoadManager.Instance.LoadScene();

        LocalizationManager.Instance.LoadJournalLocalizationData(LocalizationToLoad);
        LocalizationManager.Instance.LoadDialogueLocalizationData(LocalizationToLoad);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Application.Quit();
    }

    public void Options()
    {
        PlayClickSound();
        ChangeGridsVisibility();
    }

    public void SetMasterFloat(float volume)
    {
        //audioMixer.SetFloat("VolumeMaster", volume);
        AudioManager.Instance.ChangeVolume(Audio.AudioType.Music, volume);
        VolumeMaster = volume;
    }

    public void SetEnvironmentFloat(float volume)
    {
        //audioMixer.SetFloat("VolumeEnvironment", volume);
        AudioManager.Instance.ChangeVolume(Audio.AudioType.Environment, volume);
        VolumeEnvironment = volume;
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

    public void SetLocalization(string fileName)
    {
        //PlayClickSound();
        LocalizationManager.Instance.LoadGeneralLocalizationData(fileName);
        LocalizationToLoad = fileName;
        StartCoroutine( LoadLocalizationData() );
    }

    private IEnumerator LoadLocalizationData()
    {
        while (!LocalizationManager.Instance.GetIsReady())
        {
            yield return null;
        }

        SaveLoadManager.Instance.SaveOptions();

        SceneManager.LoadScene("StartScreen");
    }

    #endregion
}
