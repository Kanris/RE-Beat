using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

[System.Serializable]
public class PlayerStats : Stats
{
    #region delegates
    private delegate IEnumerator IEnumeratorFloatDelegate(float duration);

    public delegate void VoidDelegate (int value);
    public static event VoidDelegate OnScrapAmountChange;

    #endregion

    #region public fields

    [Header("Additional")]
    public PlatformerCharacter2D platformerCharacter2D;
    [SerializeField] private GameObject m_HealEffect;

    private int m_CriticalHealthAmount = 3;

    private int m_OverHealScrapAmount = 5;

    public static int DamageAmount = 50;
    public static float MeleeAttackSpeed = 0.2f;
    public static float RangeAttackSpeed = 2f;
    public static float Invincible = 2f; //invincible time
    public static Inventory PlayerInventory;
    public static int CurrentPlayerHealth;
    public static bool m_IsCanDoubleJump = false;
    public static bool m_IsCanDash = false;

    private static int m_Scrap = 0;
    private static int DamageMultiplier = 1;

    private static float DefaultMeleeAttackSpeed = 0.3f;
    private static float DefaultSpeed = 4f;

    #endregion

    #region properties

    public static int Scrap
    {
        set
        {
            if ((m_Scrap + value) >= 0)
            {
                if (value == 0)
                    m_Scrap = 0;
                else
                    m_Scrap += value;

                if (OnScrapAmountChange != null) //notify that coins amount changed
                {
                    OnScrapAmountChange(value);
                }
            }
        }
        get
        {
            return m_Scrap;
        }
    }

    #endregion

    #region private fields

    private bool m_IsInvincible; //is player invincible right now
    private int m_SeriesCombo = 0; //hits count in combo
    private float m_CheckNextComboTime; //next check combo
    private int m_CurrentComboIndex; //current combo index

    #endregion

    #region public methods

    public void HealPlayer(int amount)
    {
        if (CurrentHealth == MaxHealth) //if player is already full health
        {
            Scrap = m_OverHealScrapAmount; //add scrap
        }
        else //heal player
        {
            if ((CurrentHealth + amount) > MaxHealth) //if heal amount plus current health is greater than maxhealth
            {
                var excess = (CurrentHealth + amount) - MaxHealth; //get excess heal amount
                amount = amount - excess; //new heal amount
            }

            //heal player
            CurrentPlayerHealth += amount;
            CurrentHealth += amount;

            for (var index = 0; index <= amount; index++)
                UIManager.Instance.AddHealth(); //add health in player's ui

            //create healEffect
            var healEffect = GameMaster.Instantiate(m_HealEffect);
            healEffect.transform.position = m_GameObject.transform.position;
            GameMaster.Destroy(healEffect, 2.1f);
        }
    }

    public void HitEnemy(Enemy enemy, int zone)
    {
        var damageToEnemy = GetDamageAmount(zone); //get damage amount base on the distance between enemy and player
        enemy.TakeDamage(damageToEnemy, zone);
    }

    public void KillPlayer() //kill player even if he invincible
    {
        for (var index = 0; index < MaxHealth; index++)
            UIManager.Instance.RemoveHealth();

        var damageAmount = 999;

        base.TakeDamage(damageAmount);
        CurrentPlayerHealth -= damageAmount;
    }

    #region debuff

    public void DebuffPlayer(DebuffPanel.DebuffTypes debuffType, float duration)
    {
        DebuffPanel.Instance.SetDebuffUI(debuffType, duration);
        SetDebuff(debuffType);
    }

    public void SetDebuff(DebuffPanel.DebuffTypes debuffType)
    {
        switch (debuffType)
        {
            case DebuffPanel.DebuffTypes.AttackSpeed:
                MeleeAttackSpeed = 2f;

                m_SeriesCombo = 0;
                m_CheckNextComboTime = 0f;

                break;

            case DebuffPanel.DebuffTypes.Cold:
                platformerCharacter2D.m_MaxSpeed = 2f;
                break;

            case DebuffPanel.DebuffTypes.Defense:
                DamageMultiplier = 2;
                break;

            case DebuffPanel.DebuffTypes.Fire:
                break;
        }
    }

    public void RemoveDebuff(DebuffPanel.DebuffTypes debuffType)
    {
        switch (debuffType)
        {
            case DebuffPanel.DebuffTypes.AttackSpeed:
                MeleeAttackSpeed = DefaultMeleeAttackSpeed;
                break;

            case DebuffPanel.DebuffTypes.Cold:
                platformerCharacter2D.m_MaxSpeed = DefaultSpeed;
                break;

            case DebuffPanel.DebuffTypes.Defense:
                DamageMultiplier = 1;
                break;

            case DebuffPanel.DebuffTypes.Fire:
                break;
        }
    }

    #endregion

    #endregion

    #region override methods

    public override void Initialize(GameObject gameObject, Animator animator = null)
    {
        base.Initialize(gameObject, animator);

#region initialize inventory

        if (PlayerInventory == null) //initialize player's inventory with size of nine
            PlayerInventory = new Inventory(9);

        #endregion

        #region initialize health ui

        UIManager.Instance.ResetState(); //clear health ui

        if (CurrentPlayerHealth > 0 & StartScreenManager.IsLoadPressed) //save current player's health
        {
            var amount = MaxHealth - CurrentPlayerHealth;

            for (var index = 0; index < amount; index++)
                UIManager.Instance.RemoveHealth();

            CurrentHealth = CurrentPlayerHealth;
        }
        else
        {
            CurrentPlayerHealth = CurrentHealth;
        }

#endregion
    }

    public override void TakeDamage(int amount, int divider = 1)
    {
        if (!m_IsInvincible) //player is not invincible
        {

            m_IsInvincible = true; //player is invincible

            amount *= DamageMultiplier;

            Camera.main.GetComponent<CinemachineFollow>().PlayHitEffect();

            base.TakeDamage(amount, divider);
            CurrentPlayerHealth -= amount;

            for (var index = 0; index < amount; index++)
                UIManager.Instance.RemoveHealth(); //remove some health from health ui
        }
    }



    protected override IEnumerator ObjectTakeDamage(int divider)
    {
        PlayHitAnimation(true); 

        yield return new WaitForSeconds(0.1f); //time to return player's control

        PlayHitAnimation(false);

        yield return InvincibleAnimation(); //play invincible animation

        m_IsInvincible = false; //player is not invincible
    }

    protected override void KillObject()
    {
        GameMaster.Instance.StartPlayerRespawn(true); //respawn new player on respawn point
        PlayDeathParticles(); //show death particles
        GameMaster.Destroy(m_GameObject.transform.parent.gameObject); //destroy gameobject
    }

    #endregion

    #region private methods

    private int GetDamageAmount(int zone)
    {
        var damageToEnemy = DamageAmount / zone; //damage to enemy base on the hit zone

        if (m_CheckNextComboTime > Time.time) //simple combo check
        {
            m_SeriesCombo++; //hit in series

            CheckIsComboComplete(ref damageToEnemy); //maybe combo is complete
        }
        else if (m_SeriesCombo == 1 & (m_CheckNextComboTime + 1f) > Time.time) //move to complecate combo (hit pause hit)
        {
            m_CurrentComboIndex = 1; //change combo index
            m_SeriesCombo++; //hit in series
            m_CheckNextComboTime = Time.time + 1f; //pause that been checked (pause)
        }
        else //player missed combo
        {
            m_CurrentComboIndex = 0;
            m_SeriesCombo = 1;
        }

        m_CheckNextComboTime = Time.time + 0.6f; //check next hit in combo

        return damageToEnemy;
    }

    private void CheckIsComboComplete(ref int damageToEnemy)
    {
        if (m_CurrentComboIndex == 1) //index is 1 than it's third hit in combo
        {
            m_SeriesCombo = 0;
            m_CurrentComboIndex = 0;
            Debug.LogError("Pause combo");
        }
        else if (m_SeriesCombo == 3) //three hits in combo
        {
            m_CurrentComboIndex = 0;
            m_SeriesCombo = 0;
            damageToEnemy *= 2;
            Debug.LogError("Damage combo");
        }
    }

    private IEnumerator InvincibleAnimation()
    {
        var invincibleTime = Time.time + Invincible - 0.2f;
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
