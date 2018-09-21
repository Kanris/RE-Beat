using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour {

    public string key;

	// Use this for initialization
	void Start () {

        StartCoroutine(GetLocalize());

    }

    private IEnumerator GetLocalize()
    {
        while (!LocalizationManager.Instance.GetIsReady())
            yield return null;

        var text = GetComponent<TextMeshProUGUI>();
        text.text = LocalizationManager.Instance.GetGeneralLocalizedValue(key);
    }

}
