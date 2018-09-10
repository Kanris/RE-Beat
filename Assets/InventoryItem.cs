using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class InventoryItem : MonoBehaviour {

    #region private fields

    private GameObject m_DescriptionUI;
    private TextMeshProUGUI m_ItemNameText;
    private TextMeshProUGUI m_ItemDescriptionText;

    [HideInInspector] public string m_ItemName = "Template on over";
    private string m_ItemDescription = "Some item description of the template on over";
    #endregion

    #region public methods

    public void Initialize(Item item, Sprite image, GameObject descriptionUI, 
        TextMeshProUGUI itemNameText, TextMeshProUGUI itemDescriptionText)
    {
        m_DescriptionUI = descriptionUI;
        m_ItemNameText = itemNameText;
        m_ItemDescriptionText = itemDescriptionText;

        GetComponent<Image>().sprite = image;
        m_ItemName = item.Name;
        m_ItemDescription = item.Description;
    }

    public void ShowItemInfo()
    {
        m_DescriptionUI.SetActive(true);

        m_ItemNameText.text = m_ItemName;
        m_ItemDescriptionText.text = m_ItemDescription;
    }

    #endregion
}
