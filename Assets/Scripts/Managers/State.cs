using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {

    public string SceneName;

    public Dictionary<string, bool> ObjectsState;

    public Dictionary<string, Vector2> ObjectsPosition;

    public State(string SceneName)
    {
        this.SceneName = SceneName;

        ObjectsState = new Dictionary<string, bool>();

        ObjectsPosition = new Dictionary<string, Vector2>();
    }

    public bool IsExistInBool(string name)
    {
        return ObjectsState.ContainsKey(name);
    }

    public bool IsExistInPosition(string name)
    {
        return ObjectsPosition.ContainsKey(name);
    }
}
