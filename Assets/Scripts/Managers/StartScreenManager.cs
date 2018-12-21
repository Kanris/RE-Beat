using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class StartScreenManager : MonoBehaviour {

    #region static fields

    public static bool IsLoadPressed; //is load button pressed
    public static int ResolutionIndex;
    public static bool IsFullscreen;
    public static float VolumeMaster;
    public static float VolumeEnvironment;
    public static string LocalizationToLoad;

    private static bool FirstLoad;

    #endregion

    #region serialize fields

    [Header("Confirm Dialogue")]
    [SerializeField] private GameObject m_ConfirmDialogue;
    [SerializeField] private TextMeshProUGUI m_DialogueText;
    [SerializeField] private GameObject m_ConfirmDialogueNoButton;

    [Header("Startup signs")]
    [SerializeField] private GameObject m_WarningSign;
    [SerializeField] private GameObject m_Logo;

    [Header("General UI")]
    [SerializeField] private GameObject MainMenuGrid; //main menu ui
    [SerializeField] private GameObject OptionsMenuGrid; //option ui
    [SerializeField] private GameObject LoadButton; //load button ui

    //options items
    [Header("Options UI")]
    [SerializeField] private TMP_Dropdown resoulutionsDropDown;
    [SerializeField] private Slider volumeMasterSlider;
    [SerializeField] private Slider volumeEnvironmentSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Effects")]
    [SerializeField] private Audio BackgroundMusic; //background music
    [SerializeField] private Audio UIClickAudio;

    [Header("Event systems buttons")]
    [SerializeField] private GameObject m_MainMenuButton;
    [SerializeField] private GameObject m_OptionsButton;

    [Header("Version text")]
    [SerializeField] private TextMeshProUGUI m_TextVersion;

    #endregion

    #region private fields

    private Resolution[] m_Resolutions; //available resolutions

    private delegate void VoidDelegate();
    private VoidDelegate m_ConfirmDialogueAction;

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

        Initialize("Managers/MouseControlManager");

        //play background music
        if (!string.IsNullOrEmpty(BackgroundMusic))
            AudioManager.Instance.SetBackgroundMusic(BackgroundMusic);

        InitializeOptionsOnStart();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

#if MOBILE_INPUT
        MainMenuGrid.transform.localPosition = MainMenuGrid.transform.localPosition.With(y: 60f);
        MainMenuGrid.transform.GetChild(2).gameObject.SetActive(false);
#endif
    }

    private void Start()
    {
        if (!FirstLoad)
        {
            FirstLoad = true;
            StartCoroutine(ShowWarningTitle());
        }

        if (!SaveLoadManager.Instance.IsLoadGameDataAvailable())
        {
            LoadButton.SetActive(false);
        }

        m_TextVersion.text = "ver. " + Application.version;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(m_MainMenuButton);
    }

    private IEnumerator ShowWarningTitle()
    {
        m_MainMenuButton.SetActive(false);

        m_Logo.SetActive(true);

        yield return new WaitForSeconds(4f);

        yield return ScreenFaderManager.Instance.FadeToBlack();

        m_Logo.SetActive(false);

        m_WarningSign.SetActive(true);

        yield return ScreenFaderManager.Instance.FadeToClear();

        yield return new WaitForSeconds(4f);

        yield return ScreenFaderManager.Instance.FadeToBlack();

        m_WarningSign.SetActive(false);

        m_MainMenuButton.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(m_MainMenuButton);

        yield return ScreenFaderManager.Instance.FadeToClear();
    }

    private void InitializeOptionsOnStart()
    {
        m_Resolutions = Screen.resolutions; //get available resolutions

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

    private void Update()
    {
        if (CrossPlatformInputManager.GetButtonDown("Cancel"))
        {
            if (m_ConfirmDialogue.activeSelf)
            {
                m_ConfirmDialogue.SetActive(false);
                EventSystem.current.SetSelectedGameObject(m_MainMenuButton);
            }

            else if (OptionsMenuGrid.activeSelf)
            {
                ChangeGridsVisibility();
                EventSystem.current.SetSelectedGameObject(m_MainMenuButton);
            }

        }
    }

    private void InitializeDropDownResolutions()
    {
        resoulutionsDropDown.ClearOptions(); //clear options

        var resolutionsList = new List<string>(); //resolutions list

        var currentResolutionItem = string.Empty;

        foreach (var item in m_Resolutions)
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
        {
            SaveLoadManager.Instance.SaveOptions();
            EventSystem.current.SetSelectedGameObject(m_MainMenuButton);
        }
        else
            EventSystem.current.SetSelectedGameObject(m_OptionsButton);
    }

    private void ResetGameState()
    {
        PlayerStats.ResetState(); //reset player state
        State.ResetState();

        Companion.m_IsWithPlayer = false;
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

    public void StartNewGame(string name)
    {
        m_ConfirmDialogueAction = () =>
        {
            IsLoadPressed = false;
            PlayClickSound();
            LoadSceneManager.Instance.Load(name);

            LocalizationManager.Instance.LoadJournalLocalizationData(LocalizationToLoad);
            LocalizationManager.Instance.LoadDialogueLocalizationData(LocalizationToLoad);

            ResetGameState();
        };

        if (SaveLoadManager.Instance.IsLoadGameDataAvailable())
        {
            m_DialogueText.text = "There is active save chip, are you sure you want to erase it?";
            m_ConfirmDialogue.SetActive(true);
            EventSystem.current.SetSelectedGameObject(m_ConfirmDialogueNoButton);
        }
        else
            m_ConfirmDialogueAction();


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
        m_ConfirmDialogueAction = () =>
        {
            PlayClickSound();
            Application.Quit();
        };

        m_DialogueText.text = "Are you sure want to leave rabbit kingdom on the mercy of fate?";

        m_ConfirmDialogue.SetActive(true);
        EventSystem.current.SetSelectedGameObject(m_ConfirmDialogueNoButton);
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
        Screen.SetResolution(m_Resolutions[resolutionIndex].width, m_Resolutions[resolutionIndex].height, Screen.fullScreen);
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

    #region confirm dialogue

    public void DialogueYesButton()
    {
        m_ConfirmDialogueAction();
    }

    public void DialogueNoButton()
    {
        m_ConfirmDialogue.SetActive(false);
        EventSystem.current.SetSelectedGameObject(m_MainMenuButton);
    }

    #endregion
}
