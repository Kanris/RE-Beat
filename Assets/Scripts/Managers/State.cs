using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State {

    public static List<State> ScenesState;

    public string SceneName;

    public List<string> ObjectsState;

    public Dictionary<string, ObjectPosition> ObjectsPosition;

    public List<string> DialogueIsComplete;

    public Dictionary<string, string> ChestItems;

    public List<string> Tasks;

    public State(string SceneName)
    {
        this.SceneName = SceneName;

        ObjectsState = new List<string>();

        ObjectsPosition = new Dictionary<string, ObjectPosition>();

        DialogueIsComplete = new List<string>();

        ChestItems = new Dictionary<string, string>();

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

[System.Serializable]
public class ObjectPosition
{
    public float x;
    public float y;

    public ObjectPosition(Vector2 position)
    {
        x = position.x;
        y = position.y;
    }
}
