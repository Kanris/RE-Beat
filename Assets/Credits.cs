using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] private StartScreenManager m_StartScreenManager; //reference to the start sreen manager (on scene)
    
    //hide credits and show main menu buttons
    private void HideCredits()
    {
        m_StartScreenManager.HideCredits(); 
    }
}
