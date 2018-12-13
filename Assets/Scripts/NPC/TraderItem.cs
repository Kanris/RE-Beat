using UnityEngine;
using UnityEngine.UI;

public class TraderItem : MonoBehaviour {

    public Item m_TraderItem;

    public enum TraderItemType { Upgrade, Heal }
    [SerializeField] private TraderItemType m_TraderItemType;

    public enum UpgradeType { Dash, DoubleJump, DashDamage, DashInvincible, None }
    [SerializeField] private UpgradeType m_UpgradeType;

    [Header("Effects")]
    [SerializeField] private Audio m_ClickAudio;

    [Header("Additional")]
    [SerializeField] public bool m_IsInfiniteAmount;

    private GameObject m_SelectImage;
    [HideInInspector] public bool m_IsSelected;

    private void Awake()
    {
        m_SelectImage = transform.GetChild(0).gameObject;
        m_SelectImage.SetActive(false);

        if (IsUpgradeAvailable())
            Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (m_TraderItem != null)
        {
            GetComponent<Image>().sprite = m_TraderItem.Image;
            gameObject.name = m_TraderItem.name;
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

    public void MouseHoverEnter()
    {
        m_SelectImage.SetActive(true);
    }

    public void MouseHoverExit()
    {
        if (!m_IsSelected)
            m_SelectImage.SetActive(false);
    }

    public void OnClick()
    {
        for (int index = 0; index < transform.parent.childCount; index++)
        {
            var traiderItem = transform.parent.GetChild(index).GetComponent<TraderItem>();

            traiderItem.m_IsSelected = false;
            traiderItem.MouseHoverExit();
        }

        m_SelectImage.SetActive(true);

        m_IsSelected = true;

        AudioManager.Instance.Play(m_ClickAudio);
    }

    public void OnDisable()
    {
        m_SelectImage.SetActive(false);
    }
}
