using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour {

    public static string LocalizationToLoad = "ru";

    private Dictionary<string, string> localizedText;
    private Dictionary<string, string> journalText;
    private Dictionary<string, string> dialogueText;
    private Dictionary<string, string> itemsText;

    private bool isReady = false;
    private string missingTextString = "Localized text not found";

    #region Singleton

    public static LocalizationManager Instance;

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

            LoadGeneralLocalizationData(LocalizationToLoad);
            LoadJournalLocalizationData(LocalizationToLoad);
            LoadDialogueLocalizationData(LocalizationToLoad);
            LoadItemsLocalizationData(LocalizationToLoad);
        }
    }

    #endregion

    public void LoadGeneralLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-" + fileName + ".json";
            LoadLocalizationData(fileName, out localizedText);
        }
    }

    public void LoadJournalLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-journal-" + fileName + ".json";
            LoadLocalizationData(fileName, out journalText);
        }
    }

    public void LoadDialogueLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-dialogue-" + fileName + ".json";
            LoadLocalizationData(fileName, out dialogueText);
        }
    }

    public void LoadItemsLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-items-" + fileName + ".json";
            LoadLocalizationData(fileName, out itemsText);
            Debug.LogError(fileName + ":" + itemsText.Count);
        }
    }

    private void LoadLocalizationData(string fileName, out Dictionary<string, string> localizationData)
    {
        localizationData = new Dictionary<string, string>();

        var filePath = Path.Combine(Application.streamingAssetsPath, LocalizationToLoad); //get path to the localized text
        filePath = Path.Combine(filePath, fileName);

        if (File.Exists(filePath))
        {
            var dataAsJson = File.ReadAllText(filePath); //get all text from json file

            var loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);
            localizationData = loadedData.GetAsDictionary();
        }
        else
        {
            Debug.LogError("LocalizationManager.LoadLocalizedText: Cannot find localized text - +" + fileName);
        }

        isReady = true;
    }

    public string GetGeneralLocalizedValue(string key)
    {
        return GetLocalizedValue(key, ref localizedText);
    }

    public string GetJournalLocalizedValue(string key)
    {
        return GetLocalizedValue(key, ref journalText);
    }

    public string GetDialogueLocalizedValue(string key)
    {
        return GetLocalizedValue(key, ref dialogueText);
    }

    public string GetItemsLocalizedValue(string key)
    {
        return GetLocalizedValue(key, ref itemsText);
    }

    private string GetLocalizedValue(string key, ref Dictionary<string, string> localizedData)
    {
        var result = missingTextString;

        if (localizedData.ContainsKey(key))
            result = localizedData[key];

        return result;
    }

    public bool GetIsReady()
    {
        return isReady;
    }
}
