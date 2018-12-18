using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TraderItem : MonoBehaviour {

    public Item m_TraderItem;

    [Header("Display")]
    [SerializeField] private Image m_ItemImage;
    [SerializeField] private TextMeshProUGUI m_CostText;

    public enum TraderItemType { Upgrade, Heal }
    [Header("Type")]
    [SerializeField] private TraderItemType m_TraderItemType;

    public enum UpgradeType { Dash, DoubleJump, DashDamage, DashInvincible, None }
    [SerializeField] private UpgradeType m_UpgradeType;

    [Header("Effects")]
    [SerializeField] private Audio m_ClickAudio;

    [Header("Additional")]
    [SerializeField] public bool m_IsInfiniteAmount;

    [HideInInspector] public bool m_IsSelected;


    private void Awake()
    {
        if (IsUpgradeAvailable())
            Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (m_TraderItem != null)
        {
            if (m_ItemImage != null)
                m_ItemImage.sprite = m_TraderItem.Image;

            gameObject.name = m_TraderItem.name;

            if (m_CostText != null)
                m_CostText.text = m_TraderItem.itemDescription.ScrapAmount.ToString();
        }
    }

    private bool IsUpgradeAvailable()
    {
        var result = false;

        switch (m_UpgradeType)
        {
            case UpgradeType.Dash:
                if (PlayerStats.m_IsCanDash)
                    result = true;
                break;

            case UpgradeType.DoubleJump:
                if (PlayerStats.m_IsCanDoubleJump)
                    result = true;
                break;

            case UpgradeType.DashDamage:
                if (PlayerStats.m_IsDamageEnemyWhileDashing)
                    result = true;
                break;

            case UpgradeType.DashInvincible:
                if (PlayerStats.m_IsInvincibleWhileDashing)
                    result = true;
                break;
        }

        return result;
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

                case UpgradeType.DashDamage:
                    PlayerStats.m_IsDamageEnemyWhileDashing = true;
                    break;

                case UpgradeType.DashInvincible:
                    PlayerStats.m_IsInvincibleWhileDashing = true;
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
