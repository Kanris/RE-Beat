using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Inventory {

    public List<Item> m_Bag;

    public Inventory(int size)
    {
        m_Bag = new List<Item>(size);
    }

    public bool Add(Item item, string image)
    {
        if (m_Bag.Capacity > m_Bag.Count)
        {
            if (IsInBag(item.Name))
            {
                Debug.LogError("Inventory.Add: Add amount");
            }
            else
            {
                item.Image = image;
                m_Bag.Add(item);
                InfoManager.Instance.AddItem(item);
            }

            return true;
        }

        return false;
    }

    public bool IsInBag(string item)
    {
        var searchResult = m_Bag.FirstOrDefault(x => x.Name == item);
        return searchResult != null;
    }

    public bool Remove(Item item)
    {
        if (IsInBag(item.Name))
        {
            var searchResult = m_Bag.First(x => x.Name == item.Name);
            m_Bag.RemoveAt(m_Bag.IndexOf(searchResult));
            InfoManager.Instance.RemoveItem(item.Name);

            return true;
        }

        return false;
    }
}

[System.Serializable]
public class Item
{
    [HideInInspector] public string Image;
    public string Name;
    [TextArea(3, 10)] public string Description;

    public enum ItemType { Item, Note, Heal }
    public ItemType itemType;

    [Range(1, 8)] public int HealAmount = 3;
}
