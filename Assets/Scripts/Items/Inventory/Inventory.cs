using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory {

    #region public fields

    public List<ItemDescription> m_Bag; //player's inventory

    #endregion

    #region public methods

    public Inventory(int size) //initialize player's inventory with the set size
    {
        m_Bag = new List<ItemDescription>(size);
    }

    public bool Add(ItemDescription item, string image) //add item to the inventory
    {
        if (m_Bag.Capacity > m_Bag.Count) //if player's bag can contains more item
        {
            if (IsInBag(item.Name)) //if item is already in the bag increase its' count
            {
                Debug.LogError("Inventory.Add: Add amount");
            }
            else //add new item to the inventory
            {
                item.ImageInAtlas = image;
                m_Bag.Add(item); //add item to the bag
                InfoManager.Instance.AddItem(item); //add item to the "book"
            }

            return true; //item was added to the inventory
        }

        return false; //item wasn't added to the inventory
    }

    public bool IsInBag(string item) //check is item in the bag
    {
        var searchResult = m_Bag.FirstOrDefault(x => x.Name == item); //search first needed item in the bag
        return searchResult != null;
    }

    public bool Remove(Item item) //remove item from the bag
    {
        if (IsInBag(item.itemDescription.Name)) //if item is in the bag
        {
            var searchResult = m_Bag.First(x => x.Name == item.itemDescription.Name); //find need item
            m_Bag.RemoveAt(m_Bag.IndexOf(searchResult)); //remove item from the bag
            InfoManager.Instance.RemoveItem(item.itemDescription.Name); //remove item from the book

            return true; //item was removed
        }

        return false; //item wasn't removed because it is not in the bag
    }

    #endregion

}
