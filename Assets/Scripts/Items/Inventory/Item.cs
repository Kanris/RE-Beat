using UnityEngine;

[System.Serializable]
public class Item
{
    #region public fields

    [HideInInspector] public string Image; //item image
    public string Name; //item name
    [TextArea(3, 10)] public string Description; //item description

    #region enum
    public enum ItemType { Item, Note, Heal }
    public ItemType itemType; //current item type
    #endregion

    [Range(1, 8)] public int HealAmount = 3; //item heal amount (only from Heal item type)

    #endregion
}
