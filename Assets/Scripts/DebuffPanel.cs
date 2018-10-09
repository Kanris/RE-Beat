using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebuffPanel : MonoBehaviour {

    public enum DebuffTypes { AttackSpeed, Defense, Fire, Cold }

    [SerializeField] private Player m_Player;
    [SerializeField] private DebufUI[] m_DebuffsOnPanel;

    private bool m_IsDebuffOnPanel;
    private PlayerStats m_PlayerStats;

    #region singleton

    public static DebuffPanel Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(Instance);
            }
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    #region initialize

    private void Start()
    {
        m_PlayerStats = m_Player.playerStats; //initialize player stats
    }

    #endregion

    public void SetDebuffUI(DebuffTypes debuffType, float displayTime)
    {
        var item = m_DebuffsOnPanel.SingleOrDefault(x => x.DebuffType == debuffType);

        if (item != null)
        {
            item.gameObject.GetComponent<Image>().fillAmount = 1f;

            if (item.gameObject.activeSelf) //if debuff is already on pannel
            {
                item.appearTimer += displayTime;
            }
            else //new debuff
            {
                item.appearTimer = displayTime;
                item.gameObject.SetActive(true);

                StartCoroutine(ChangeImageFill(item));
            }
        }
    }

    private IEnumerator ChangeImageFill(DebufUI debufUI)
    {
        var timeAmount = 0f;
        var ratio = 0.1f;

        var image = debufUI.gameObject.GetComponent<Image>();

        while (timeAmount <= debufUI.appearTimer)
        {
            image.fillAmount -= ratio;

            yield return new WaitForSeconds(debufUI.appearTimer * ratio);

            timeAmount += debufUI.appearTimer * ratio;
        }

        m_PlayerStats.RemoveDebuff(debufUI.DebuffType);
        image.gameObject.SetActive(false);
        debufUI.appearTimer = 0f;
    }

    #region test methods

    [ContextMenu("SetDebuffFor9Sec")]
    public void SetAttackDebuff()
    {
        StartCoroutine(SetAttackDebuffIEnumerator());
    }

    [ContextMenu("SetAllDebuffs")]
    public void AllDebuffs()
    {
        SetDebuffUI(DebuffTypes.AttackSpeed, 2f);
        SetDebuffUI(DebuffTypes.Cold, 5f);
        SetDebuffUI(DebuffTypes.Defense, 1f);
        SetDebuffUI(DebuffTypes.Fire, 3f);
    }

    private IEnumerator SetAttackDebuffIEnumerator()
    {
        SetDebuffUI(DebuffTypes.AttackSpeed, 5f);

        yield return new WaitForSeconds(3f);

        Debug.LogError("Set next debuff");
        SetDebuffUI(DebuffTypes.AttackSpeed, 4f);
    }

    #endregion
}

[System.Serializable]
public class DebufUI
{
    public DebuffPanel.DebuffTypes DebuffType;
    public GameObject gameObject;
    [HideInInspector] public float appearTimer;
}
