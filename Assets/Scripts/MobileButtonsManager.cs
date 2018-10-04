using UnityEngine;
using UnityEngine.UI;

public class MobileButtonsManager : MonoBehaviour {

    [SerializeField] private Image m_Stick;
    [SerializeField] private Image m_Journal;
    [SerializeField] private Image m_Pause;
    [SerializeField] private Image m_Jump;
    [SerializeField] private Image m_MeleeAttack;
    [SerializeField] private Image m_RangeAttack;
    [SerializeField] private Image m_Submit;
    [SerializeField] private Image m_Shift;

#region Singleton

    public static MobileButtonsManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

#endregion

    public void ShowOnlyNeedButtons(bool stick = false, bool journal = false, 
                                    bool pause = false, bool jump = false, bool meleeAtack = false,
                                    bool rangeAttack = false,
                                    bool submit = false, bool shift = false)
    {
        m_Stick.enabled = stick;
        m_Journal.enabled = journal;
        m_Pause.enabled = pause;
        m_Jump.enabled = jump;
        m_MeleeAttack.enabled = meleeAtack;
        m_RangeAttack.enabled = rangeAttack;
        m_Submit.enabled = submit;
        m_Shift.enabled = shift;
    }

    public void HideOnlyNeedButtons(bool stick = true, bool journal = true,
                                    bool pause = true, bool jump = true, bool meleeAttack = true,
                                    bool rangeAttack = true,
                                    bool submit = true, bool shift = true)
    {
        m_Stick.enabled = stick;
        m_Journal.enabled = journal;
        m_Pause.enabled = pause;
        m_Jump.enabled = jump;
        m_MeleeAttack.enabled = meleeAttack;
        m_RangeAttack.enabled = rangeAttack;
        m_Submit.enabled = submit;
        m_Shift.enabled = shift;
    }
}
