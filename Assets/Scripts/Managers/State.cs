using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State {

    #region public fields

    public static List<State> ScenesState; //list of scenes state

    public string SceneName; //scene name
    public List<string> ObjectsState; //object to remove from scene
    public Dictionary<string, ObjectPosition> ObjectsPosition; //object to change position on scene
    public List<string> DialogueIsComplete; //indicates that dialogue is complete
    public Dictionary<string, string> ChestItems; //items to remove from ches
    public List<string> Tasks; //task that have been add/update/complete on scene
    public List<string> Camera; //have player look to open door already

    #endregion

    //initialize lists
    public State(string SceneName)
    {
        this.SceneName = SceneName;

        ObjectsState = new List<string>();

        ObjectsPosition = new Dictionary<string, ObjectPosition>();

        DialogueIsComplete = new List<string>();

        ChestItems = new Dictionary<string, string>();

        Tasks = new List<string>();

        Camera = new List<string>();

        if (ScenesState == null)
            ScenesState = new List<State>();
    }

    public bool IsExistInBool(string name)
    {
        return ObjectsState.Contains(name);
    }

    public bool IsExistInPosition(string name)
    {
        return ObjectsPosition.ContainsKey(name);
    }

    public static void ResetState()
    {
        ScenesState = null;
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
