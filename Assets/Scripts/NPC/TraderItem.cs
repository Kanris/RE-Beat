using UnityEngine;
using UnityEngine.UI;

public class TraderItem : MonoBehaviour {

    public Item m_TraderItem;

    public enum TraderItemType { Upgrade, Heal }
    [SerializeField] private TraderItemType m_TraderItemType;

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

    public void ApplyUpgrade(PlayerStats player)
    {
        if (m_TraderItemType == TraderItemType.Upgrade)
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
        }
        else
        {
            player.HealPlayer(m_TraderItem.itemDescription.HealAmount);
        }

        GameMaster.Instance.SaveState(name, 0, GameMaster.RecreateType.Object);
    }
}
