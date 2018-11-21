using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class InventoryItem : MonoBehaviour {

    #region private fields

    [SerializeField] private Audio m_ClickAudio;
    private TextPage m_ItemDescriptionText;
    private string m_ItemDescription = "Some item description of the template on over";

    #endregion

    #region public methods

    public void Initialize(ItemDescription item, TextPage itemDescriptionText)
    {
        m_ItemDescriptionText = itemDescriptionText;

        m_ItemDescription = item.Description;
    }

    public void ShowItemInfo()
    {
        AudioManager.Instance.Play(m_ClickAudio);
        m_ItemDescriptionText.ShowText(LocalizationManager.Instance.GetItemsLocalizedValue(m_ItemDescription));
    }

    #endregion
}
