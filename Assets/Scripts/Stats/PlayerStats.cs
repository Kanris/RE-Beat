using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

[System.Serializable]
public class PlayerStats : Stats
{
    #region delegates

    public delegate IEnumerator IEnumeratorDelegate(int value);
    public static event IEnumeratorDelegate OnCoinsAmountChange;

    #endregion

    #region public fields

    public static int DamageAmount = 50;
    public static float AttackSpeed = 0.3f;
    public static float Invincible = 2f;
    public static Inventory PlayerInventory;
    public static int CurrentPlayerHealth;
    private static int m_Coins = 0;

    #endregion

    #region properties

    public static int Coins
    {
        set
        {
            m_Coins += value;

            if (OnCoinsAmountChange != null)
                GameMaster.Instance.StartCoroutine(OnCoinsAmountChange(value));
        }
        get
        {
            return m_Coins;
        }
    }

    #endregion

    #region private fields

    private bool isInvincible;
    private int m_SeriesCombo = 0;
    private float m_CheckNextComboTime;

    #endregion

    #region public methods

    public void HealPlayer(int amount)
    {
        if (CurrentHealth == MaxHealth)
        {
            GameMaster.Instance.StartCoroutine(UIManager.Instance.ChangeCoinsAmount(10));
        }
        else
        {
            if ((CurrentHealth + amount) > MaxHealth)
            {
                var excess = (CurrentHealth + amount) - MaxHealth;
                amount = amount - excess;
            }

            CurrentPlayerHealth += amount;
            CurrentHealth += amount;

            UIManager.Instance.AddHealth(amount);
        }
    }

    #endregion

    #region override methods

    public override void Initialize(GameObject gameObject, Animator animator = null)
    {
        if (PlayerInventory == null)
            PlayerInventory = new Inventory(9);

        base.Initialize(gameObject);

        UIManager.Instance.Clear();

        if (CurrentPlayerHealth > 0)
            UIManager.Instance.AddHealth(CurrentPlayerHealth);
        else
        {
            UIManager.Instance.AddHealth(CurrentHealth);
            CurrentPlayerHealth = CurrentHealth;
        }

        UIManager.Instance.ChangeCoinsAmount(m_Coins);
    }

    public void HitEnemy(Enemy enemy, int zone)
    {
        var damageToEnemy = GetDamageAmount(zone);
        enemy.TakeDamage(damageToEnemy, zone);
    }

    public override void TakeDamage(int amount, int divider = 1)
    {
        if (!isInvincible)
        {
            base.TakeDamage(amount, divider);
            CurrentPlayerHealth -= amount;

            UIManager.Instance.RemoveHealth(amount);
        }
    }

    protected override IEnumerator PlayTakeDamageAnimation(int divider)
    {
        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = false;
        PlayHitAnimation(true);

        isInvincible = true;

        yield return new WaitForSeconds(0.2f);

        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = true;

        PlayHitAnimation(false);

        yield return InvincibleAnimation();

        isInvincible = false;
    }

    protected override void KillObject()
    {
        GameMaster.Instance.InitializePlayerRespawn(true);
        base.KillObject();
    }

    #endregion

    #region private methods

    private int GetDamageAmount(int zone)
    {
        var damageToEnemy = DamageAmount / zone;

        if (m_CheckNextComboTime > Time.time)
        {
            m_SeriesCombo++;

            if (m_SeriesCombo == 3)
            {
                m_SeriesCombo = 1;
                damageToEnemy *= 2;
            }
        }
        else
        {
            m_SeriesCombo = 1;
        }

        m_CheckNextComboTime = Time.time + 0.6f;

        return damageToEnemy;
    }

    private IEnumerator InvincibleAnimation()
    {
        var invincibleTime = Time.time + Invincible;
        var playerSprite = m_GameObject.GetComponent<SpriteRenderer>().material;
        var color = playerSprite.color;

        do
        {
            yield return ChangeAlpha(1f, playerSprite, color);

            yield return ChangeAlpha(0.6f, playerSprite, color);

        } while (invincibleTime >= Time.time);

        yield return ChangeAlpha(1f, playerSprite, color, 0f);
    }

    private IEnumerator ChangeAlpha(float alpha, Material material, Color color, float time = 0.1f)
    {
        color.a = alpha;
        material.color = color;

        yield return new WaitForSeconds(time);
    }

    #endregion
}
