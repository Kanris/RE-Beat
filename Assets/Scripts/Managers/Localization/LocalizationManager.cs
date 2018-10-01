using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour
{

    public static string LocalizationToLoad = "en";

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
            LocalizationToLoad = fileName;

            fileName = "localization-" + fileName + ".json";

            StartCoroutine(LoadLocalizationData(fileName, (result) =>
            {
                localizedText = new Dictionary<string, string>();
                localizedText = result;
            }));
        }
    }

    public void LoadJournalLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-journal-" + fileName + ".json";

            StartCoroutine(LoadLocalizationData(fileName, (result) =>
            {
                journalText = new Dictionary<string, string>();
                journalText = result;
            }));
        }
    }

    public void LoadDialogueLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-dialogue-" + fileName + ".json";

            StartCoroutine( LoadLocalizationData(fileName, (result) =>
            {
                dialogueText = new Dictionary<string, string>();
                dialogueText = result;
            }));
        }
    }

    public void LoadItemsLocalizationData(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            fileName = "localization-items-" + fileName + ".json";

            StartCoroutine( LoadLocalizationData(fileName, (result) =>
            {
                itemsText = new Dictionary<string, string>();
                itemsText = result;
            }));
        }
    }

    private IEnumerator LoadLocalizationData(string fileName, System.Action<Dictionary<string, string>> callback)
    {
        var filePath = Path.Combine(Application.streamingAssetsPath, LocalizationToLoad); //get path to the localized text
        filePath = Path.Combine(filePath, fileName);
        string result = string.Empty;

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            var www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
            yield return www.SendWebRequest();
            result = www.downloadHandler.text;
        }
        else if (File.Exists(filePath))
        {
            result = File.ReadAllText(filePath); //get all text from json file
        }

        var loadedData = JsonUtility.FromJson<LocalizationData>(result);

        if (callback != null) callback(loadedData.GetAsDictionary());

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
