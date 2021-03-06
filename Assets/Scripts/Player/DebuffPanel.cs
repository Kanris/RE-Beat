﻿using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DebuffPanel : MonoBehaviour {

    public enum DebuffTypes { AttackSpeed, Defense, Fire, Cold, None }

    [SerializeField] private Player m_Player;
    [SerializeField] private DebufUI[] m_DebuffsOnPanel;

    [Header("Danger")]
    [SerializeField] private GameObject m_CriticalDamage;

    private bool m_IsDebuffOnPanel;
    private PlayerStats m_PlayerStats;

    private bool m_IsPlayerDie; //is application closing

    #region singleton

    public static DebuffPanel Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region initialize

    private void Start()
    {
        m_PlayerStats = m_Player.playerStats; //initialize player stats
    }

    #endregion

    private void OnDestroy()
    {
        foreach (var item in Enum.GetValues(typeof(DebuffTypes)))
            m_PlayerStats.RemoveDebuff((DebuffTypes)item);
    }

    public void SetDebuffUI(DebuffTypes debuffType, float displayTime)
    {
        var item = m_DebuffsOnPanel.SingleOrDefault(x => x.DebuffType == debuffType);

        if (item != null)
        {
            item.gameObject.transform.GetChild(0).GetComponent<Image>().fillAmount = 1f;

            if (item.gameObject.activeSelf) //if debuff is already on pannel
            {
                item.appearTimer = displayTime;
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

        var image = debufUI.gameObject.transform.GetChild(0).GetComponent<Image>();

        while (timeAmount <= debufUI.appearTimer & !m_IsPlayerDie)
        {
            image.fillAmount -= ratio;

            yield return new WaitForSeconds(debufUI.appearTimer * ratio);

            timeAmount += debufUI.appearTimer * ratio;
        }
        
        m_PlayerStats.RemoveDebuff(debufUI.DebuffType);
        image.gameObject.transform.parent.gameObject.SetActive(false);
        debufUI.appearTimer = 0f;
    }

    #region Danger sign

    public void ShowCriticalDamageSign()
    {
        StartCoroutine(DangerSign());
    }

    private IEnumerator DangerSign()
    {
        m_CriticalDamage.SetActive(true);

        yield return new WaitForSeconds(3f);

        m_CriticalDamage.GetComponent<Animator>().SetTrigger("Disappear");

        yield return new WaitForSeconds(0.1f);

        m_CriticalDamage.SetActive(false);
    }

    #endregion

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
