using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue  {

    public string Name;

    public Sentence[] sentences;

}

[System.Serializable]
public class Sentence
{
    [TextArea(3, 10)]
    public string NPC;

    [TextArea(3, 10)]
    public string Player;

    public bool isPlayerFirst;
}
