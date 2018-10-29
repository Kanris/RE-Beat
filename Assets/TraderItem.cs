using UnityEngine;
using UnityEngine.UI;

public class TraderItem : MonoBehaviour {

    public Item m_TraderItem;

    public enum UpgradeType { Dash, DoubleJump }
    [SerializeField] private UpgradeType m_UpgradeType;

    private void OnValidate()
    {
        if (m_TraderItem != null)
        {
            GetComponent<Image>().sprite = m_TraderItem.Image;
            gameObject.name = m_TraderItem.name;
        }
    }

    public void ApplyUpgrade()
    {
        switch (m_UpgradeType)
        {
            case UpgradeType.Dash:
                PlayerStats.m_IsCanDash = true;
                break;

            case UpgradeType.DoubleJump:
                PlayerStats.m_IsCanDoubleJump = true;
                break;
        }

        GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Object);
    }
}
