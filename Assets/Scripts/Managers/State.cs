using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {

    public string SceneName;

    public List<string> ObjectsState;

    public Dictionary<string, Vector2> ObjectsPosition;

    public List<string> DialogueIsComplete;

    public List<string> ChestItems;

    public State(string SceneName)
    {
        this.SceneName = SceneName;

        ObjectsState = new List<string>();

        ObjectsPosition = new Dictionary<string, Vector2>();

        DialogueIsComplete = new List<string>();

        ChestItems = new List<string>();
    }

    public bool IsExistInBool(string name)
    {
        return ObjectsState.Contains(name);
    }

    public bool IsExistInPosition(string name)
    {
        return ObjectsPosition.ContainsKey(name);
    }

    public bool IsExistInChest(string name)
    {
        return ChestItems.Contains(name);
    }
}
