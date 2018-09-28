using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public Sprite Image;
    public ItemDescription itemDescription;
}

[System.Serializable]
public class ItemDescription
{
    #region public fields

    [HideInInspector] public string ImageInAtlas; //item image
    public string Name; //item name
    public string Description; //item description

    #region enum
    public enum ItemType { Item, Note, Heal }
    public ItemType itemType; //current item type
    #endregion

    [Range(1, 8)] public int HealAmount = 3; //item heal amount (only from Heal item type)

    #endregion
}
