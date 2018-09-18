
[System.Serializable]
public class Task
{
    #region public fields

    public string Name; //task name
    public string Text; //task text
    public bool IsTaskComplete; //is task complete

    public delegate void VoidDelegate();
    public event VoidDelegate OnTaskComplete; //event on task complete
    public event VoidDelegate OnTaskUpdate; //event on task update

    #endregion

    #region public methods

    public Task(string taskName, string taskText) //add new task
    {
        this.Name = taskName; //save task name
        this.Text = taskText; //save initial task text

        ShowAnnouncerMessage("added to journal"); //show announcer message about additing new task
    }

    public void TaskUpdate(string text)
    {
        this.Text = text + this.Text; //add new text to task
        if (OnTaskUpdate != null) OnTaskUpdate(); //notify on task update

        ShowAnnouncerMessage("updated"); //show announcer message about updating task
    }

    public void TaskComplete(string text)
    {
        IsTaskComplete = true; //task is complete
        this.Text = text + this.Text; //update task text

        if (OnTaskComplete != null) OnTaskComplete(); //notify on task complete

        ShowAnnouncerMessage("complete"); //show announcer message about task completion
    }

    #endregion

    #region private methods

    private void ShowAnnouncerMessage(string text)
    {
        AnnouncerManager.Instance.DisplayAnnouncerMessage(
            new AnnouncerManager.Message("<#000000>" + Name + "</color> task has been " + text + "  - <#000000>J</color>", 3f));
    }

    #endregion
}

