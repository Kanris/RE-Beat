using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour {

    public static string LocalizationToLoad = "localization-en";

    private Dictionary<string, string> localizedText;
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

            LoadLocalizedText(LocalizationToLoad);
        }
    }

    #endregion

    public void LoadLocalizedText(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            localizedText = new Dictionary<string, string>();
            fileName += ".json";
            var filePath = Path.Combine(Application.streamingAssetsPath, fileName); //get path to the localized text

            if (File.Exists(filePath))
            {
                var dataAsJson = File.ReadAllText(filePath); //get all text from json file

                var loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);
                localizedText = loadedData.GetAsDictionary();
            }
            else
            {
                Debug.LogError("LocalizationManager.LoadLocalizedText: Cannot find localized text - +" + fileName);
            }
        }

        isReady = true;
    }

    public string GetLocalizedValue(string key)
    {
        var result = missingTextString;

        if (localizedText.ContainsKey(key))
            result = localizedText[key];

        return result;
    }

    public bool GetIsReady()
    {
        return isReady;
    }
}
