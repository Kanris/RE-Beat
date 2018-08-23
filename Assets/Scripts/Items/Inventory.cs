using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory {

    private List<Item> m_Bag;
	
    public Inventory(int size)
    {
        m_Bag = new List<Item>(size);
    }

    public bool Add(Item item)
    {
        if (m_Bag.Capacity > m_Bag.Count)
        {
            if (IsInBag(item.Name))
            {
                Debug.LogError("Inventory.Add: Add amount");
            }
            else
            {
                m_Bag.Add(item);
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

            return true;
        }

        return false;
    }
}

[System.Serializable]
public class Item
{
    public string Name;
    public string Description;
}
