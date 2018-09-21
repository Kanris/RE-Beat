using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizationData {

    public LocalizationItem[] items;

    public Dictionary<string, string> GetAsDictionary()
    {
        var returnDictionary = new Dictionary<string, string>();

        for (var index = 0; index < items.Length; index++)
        {
            returnDictionary.Add(items[index].key, items[index].value);
        }

        return returnDictionary;
    }

}

[System.Serializable]
public class LocalizationItem {

    public string key;
    public string value;

}
