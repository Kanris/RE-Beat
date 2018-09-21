using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour {

    public string key;

	// Use this for initialization
	void Start () {

        var text = GetComponent<TextMeshProUGUI>();
        text.text = LocalizationManager.Instance.GetLocalizedValue(key);

	}

}
