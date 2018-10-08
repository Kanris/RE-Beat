using System.Linq;
using System.Collections;
using UnityEngine;

public class DebuffPanel : MonoBehaviour {

    public enum DebuffTypes { AttackSpeed, Defense, Fire, Cold }

    [SerializeField] private DebufUI[] m_DebuffsOnPanel;

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
    }

    public void SetDebuffUI(DebuffTypes debuffType, float displayTime)
    {
        var item = m_DebuffsOnPanel.SingleOrDefault(x => x.DebuffType == debuffType);

        if (item != null)
        {
            var instantiateDebuff = Instantiate(item.gameObject, transform);

            Debug.LogError("Debuff for " + displayTime);

            Destroy(instantiateDebuff, displayTime);
        }
    }

    [ContextMenu("SetDebuffFor2Sec")]
    public void SetAttackDebuff()
    {
        SetDebuffUI(DebuffTypes.AttackSpeed, 2f);
    }
}

[System.Serializable]
public class DebufUI
{
    public DebuffPanel.DebuffTypes DebuffType;
    public GameObject gameObject;
}
