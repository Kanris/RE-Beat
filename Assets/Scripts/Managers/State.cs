using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {

    public string SceneName;

    public List<string> ObjectsState;

    public Dictionary<string, Vector2> ObjectsPosition;

    public List<string> DialogueIsComplete;

    public List<ItemInChest> ChestItems;

    public List<string> Tasks;

    public State(string SceneName)
    {
        this.SceneName = SceneName;

        ObjectsState = new List<string>();

        ObjectsPosition = new Dictionary<string, Vector2>();

        DialogueIsComplete = new List<string>();

        ChestItems = new List<ItemInChest>();

        Tasks = new List<string>();
    }

    public bool IsExistInBool(string name)
    {
        return ObjectsState.Contains(name);
    }

    public bool IsExistInPosition(string name)
    {
        return ObjectsPosition.ContainsKey(name);
    }
}

public class ItemInChest
{
    public string ChestName;
    public string Item;

    public ItemInChest(string chestName, string item)
    {
        this.ChestName = chestName;
        this.Item = item;
    }
}
