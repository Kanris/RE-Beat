using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class InventoryItem : MonoBehaviour {

    #region private fields

    private TextPage m_ItemDescriptionText;
    private string m_ItemDescription = "Some item description of the template on over";

    #endregion

    #region public methods

    public void Initialize(Item item, TextPage itemDescriptionText)
    {
        m_ItemDescriptionText = itemDescriptionText;

        m_ItemDescription = item.Description;
    }

    public void ShowItemInfo()
    {
        m_ItemDescriptionText.ShowText(m_ItemDescription);
    }

    #endregion
}
