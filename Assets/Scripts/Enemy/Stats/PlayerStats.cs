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
    public PlatformerCharacter2D platformerCharacter2D; //character control

    [Header("Effects")]
    [SerializeField] private GameObject m_HealEffect; //heal effect particles
    [SerializeField] private Audio m_HealEffectAudio; //audio that player when player heals
    [SerializeField] private GameObject m_ScrapGameobject; //scrap gameobject

    [Header("Throw stats")]
    public static float m_ThrowEnemyX = 5f; //change static value

    //players throw stats
    [SerializeField, Range(0, 100)] private float m_ThrowPlayerX = 15f;
    [SerializeField, Range(0, 100)] private float m_ThrowPlayerY = 8f;

    private int m_CriticalHealthAmount = 3; //critical health amount (to start show critical health effect)
    private int m_OverHealScrapAmount = 5; //how much player will receive when he has max amount of health but picked up heal potion

    //attack
    public static int DamageAmount = 50; //player's damage amount
    public static float MeleeAttackSpeed = 0.2f; //melee attack speed
    public static float RangeAttackSpeed = 2f; //range attack speed
    public static float FallAttackSpeed = 2f; //fall attack speed
    public static float Invincible = 1f; //invincible time

    //companion attack stats
    public static float InvisibleTimeSpeed = 3f;
    public static float EnemyTrapSpeed = 5f;

    public static Inventory PlayerInventory; //player's inventory

    public static int CurrentPlayerHealth; //current health amount (for load or moving between scenes)

    //player's abilitys
    public static bool m_IsCanDoubleJump = false;
    public static bool m_IsCanDash = false;
    public static bool m_IsFallAttack = false;
    public static bool m_IsInvincibleWhileDashing = false;
    public static bool m_IsDamageEnemyWhileDashing = false;
    public static bool m_IsCanSeeEnemyHP = false;
    public static int m_ReviveCount = 2;

    private static int m_Scrap = 200; //total scrap amout

    //player's debuffs
    private static int DamageMultiplier = 1; //damage multiplier
    private static float DefaultMeleeAttackSpeed = 0.2f; //default attack speed
    private static float DefaultMovementSpeed = 4f; //defaul movement speed

    #endregion

    #region properties

    //change scrap amount
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

                OnScrapAmountChange?.Invoke(value);
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
            CreateScrapEffect();
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
            healEffect.transform.position = m_GameObject.transform.position.Subtract(y: 0.8f);
            GameMaster.Destroy(healEffect, 2.1f);

            AudioManager.Instance.Play(m_HealEffectAudio);
        }
    }

    private void CreateScrapEffect()
    {
        Scrap = m_OverHealScrapAmount; //add scrap

        var scrapEffect = GameMaster.Instantiate(m_ScrapGameobject);
        scrapEffect.transform.position = m_GameObject.transform.position;

        GameMaster.Destroy(scrapEffect, 1.5f);
    }

    public void HitEnemy(Enemy enemy, int zone)
    {
        if (zone > 0)
        {
            var (Damage, ThrowBack) = GetDamageAmount(zone); //get damage amount base on the distance between enemy and player (and get throwback value if it's in combo)
            enemy.TakeDamage(Damage, ThrowBack, 0f);
        }
        else
        {
            enemy.TakeDamage(DamageAmount, 0, 0);
        }
    }

    public void HitPlayer(int amount)
    {
        TakeDamage(amount, m_ThrowPlayerX, m_ThrowPlayerY);
    }

    public void ReturnPlayerOnReturnPoint() //kill player even if he invincible
    {
        //player take's 1 damage and does not receive invincible
        TakeDamage(0, 0, 0);

        //if player still alive
        if (CurrentPlayerHealth > 0)
        {
            //return him on return point
            GameMaster.Instance.RespawnPlayerOnReturnPoint(m_GameObject);
        }
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
                platformerCharacter2D.m_MaxSpeed = DefaultMovementSpeed;
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

        if (CurrentPlayerHealth > 0 & (StartScreenManager.IsLoadPressed | LoadSceneManager.loadedFromScene)) //save current player's health
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

        if (!m_IsFallAttack)
            UIManager.Instance?.ShowFallAttack(false);
    }

    public override void TakeDamage(int amount, float throwX, float throwY)
    {
        if (!m_IsInvincible) //player is not invincible
        {
            if (amount > 0)
                m_IsInvincible = true; //player is invincible
            else if (amount < 0)
                amount = 1;

            amount *= DamageMultiplier;

            Camera.main.GetComponent<Camera2DFollow>().PlayHitEffect();

            base.TakeDamage(amount, throwX, throwY);
            CurrentPlayerHealth -= amount;

            for (var index = 0; index < amount; index++)
                UIManager.Instance.RemoveHealth(); //remove some health from health ui
        }
    }

    public IEnumerator ThrowPlayerBack()
    {
        PlayHitAnimation(true);

        Camera.main.GetComponent<Camera2DFollow>().PlayHitEffect();

        yield return new WaitForSeconds(0.1f); //time to return player's control

        PlayHitAnimation(false);
    }

    protected override IEnumerator ObjectTakeDamage()
    {
        PlayHitAnimation(true); 

        yield return new WaitForSeconds(0.1f); //time to return player's control

        PlayHitAnimation(false);

        yield return InvincibleAnimation(); //play invincible animation

        m_IsInvincible = false; //player is not invincible
    }

    protected override void KillObject()
    {
        GameMaster.Instance.StartPlayerRespawn(true, false); //respawn new player on respawn point
        PlayDeathParticles(); //show death particles

        GameMaster.Destroy(m_GameObject.transform.parent.gameObject); //destroy gameobject
    }

    #endregion

    #region private methods

    private (int Damage, float ThrowBack) GetDamageAmount(int zone)
    {
        var damageToEnemy = DamageAmount / zone; //damage to enemy base on the hit zone
        var throwBackXValue = m_ThrowEnemyX / 4; //stun enemy

        if (m_CheckNextComboTime > Time.time) //simple combo check
        {
            m_SeriesCombo++; //hit in series

            CheckIsComboComplete(ref damageToEnemy, ref throwBackXValue); //maybe combo is complete
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

        return (damageToEnemy, throwBackXValue);
    }

    private void CheckIsComboComplete(ref int damageToEnemy, ref float throwBackXValue)
    {
        if (m_CurrentComboIndex == 1) //index is 1 than it's third hit in combo
        {
            m_SeriesCombo = 0;
            m_CurrentComboIndex = 0;
            damageToEnemy *= 2;

            Debug.LogError("Pause combo");
        }
        else if (m_SeriesCombo == 3) //three hits in combo
        {
            m_CurrentComboIndex = 0;
            m_SeriesCombo = 0;
            throwBackXValue = m_ThrowEnemyX;

            Debug.LogError("Damage combo> throwValue is" + throwBackXValue);
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

        } while (invincibleTime >= Time.time & !m_Animator.GetBool("Dash"));

        yield return ChangeAlpha(1f, playerSprite, color, 0f);
    }

    private IEnumerator ChangeAlpha(float alpha, Material material, Color color, float time = 0.1f)
    {
        color.a = alpha;
        material.color = color;

        yield return new WaitForSeconds(time);
    }

    public static void ResetState()
    {
        DamageAmount = 50;
        MeleeAttackSpeed = 0.2f;
        RangeAttackSpeed = 2f;
        Invincible = 2f;

        PlayerInventory = null;
        m_IsCanDoubleJump = false;
        m_IsCanDash = false;
        m_IsInvincibleWhileDashing = false;
        m_IsDamageEnemyWhileDashing = false;
        m_IsCanSeeEnemyHP = false;
        m_IsFallAttack = false;

        m_ReviveCount = 2;

        m_Scrap = 0;
        DamageMultiplier = 1;

        DefaultMeleeAttackSpeed = 0.3f;
        DefaultMovementSpeed = 4f;

        OnScrapAmountChange = null;
    }

    #endregion
}
