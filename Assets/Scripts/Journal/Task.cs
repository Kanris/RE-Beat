using System.Collections.Generic;

[System.Serializable]
public class Task
{
    #region public fields
    
    public string NameKey;
    public List<string> TextKey; //task text
    public bool IsTaskComplete; //is task complete

    public delegate void VoidDelegate();
    public event VoidDelegate OnTaskComplete; //event on task complete
    public event VoidDelegate OnTaskUpdate; //event on task update

    #endregion

    #region public methods

    public Task(string name, string nameKey, string key) //add new task
    {
        this.TextKey = new List<string>();
        
        this.NameKey = nameKey; //save task name
        this.TextKey.Add(key); //save initial task text

        ShowAnnouncerMessage(LocalizationManager.Instance.GetJournalLocalizedValue("task_add")); //show announcer message about additing new task
    }

    public void TaskUpdate(string key)
    {
        this.TextKey.Add(key); //add new text to task
        if (OnTaskUpdate != null) OnTaskUpdate(); //notify on task update

        ShowAnnouncerMessage(LocalizationManager.Instance.GetJournalLocalizedValue("task_update")); //show announcer message about updating task
    }

    public void TaskComplete(string key)
    {
        IsTaskComplete = true; //task is complete
        this.TextKey.Add(key); //update task text

        if (OnTaskComplete != null) OnTaskComplete(); //notify on task complete

        ShowAnnouncerMessage(LocalizationManager.Instance.GetJournalLocalizedValue("task_complete")); //show announcer message about task completion
    }

    public string GetText()
    {
        var returnText = string.Empty;

        for (var index = TextKey.Count - 1; index >= 0; index--)
        {
            returnText += LocalizationManager.Instance.GetJournalLocalizedValue(TextKey[index]) + "<br><br>";
        }

        return returnText;
    }

    #endregion

    #region private methods

    private void ShowAnnouncerMessage(string text)
    {
        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + LocalizationManager.Instance.GetJournalLocalizedValue(NameKey) + 
            "</color> " + text + "  - <#000000>J</color>", 3f));
    }

    #endregion
}

