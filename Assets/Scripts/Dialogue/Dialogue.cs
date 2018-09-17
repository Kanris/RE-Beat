using UnityEngine;

[System.Serializable]
public class Dialogue  { //full dialogue 

    #region public fields

    public Sentence[] MainSentences; //main npc dialogue

    public Sentence[] RepeatSentences; //repeat dialogue when main dialogue is finished 

    public bool IsDialogueFinished = false; //indicates that main sentence dialogue were played

    #endregion
}

[System.Serializable]
public class Sentence
{
    #region public fields

    [TextArea(3, 10)]
    public string DisplaySentence; //sentence to display

    public string firstAnswer; //first variant of the user's response

    public string secondAnswer; //second variant of the user's response

    public Sentence[] firstSentence; //sentence to display when first answer were chosen

    public Sentence[] secondSentence; //sentence to display when second answer were chosen

    #endregion
}
