using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets._2D;
using UnityEngine.PostProcessing;

[System.Serializable]
public class PlayerStats : Stats
{
    #region delegates

    public delegate void VoidDelegate (int value);
    public static event VoidDelegate OnScrapAmountChange;

    #endregion

    #region public fields

    [Header("Additional")]
    public PlatformerCharacter2D platformerCharacter2D;

    private ChromaticAberrationModel.Settings m_ChromaticAbberationModel;
    private PostProcessingProfile m_Profile;

    public static int DamageAmount = 50;
    public static float MeleeAttackSpeed = 0.3f;
    public static float RangeAttackSpeed = 2f;
    public static float Invincible = 2f; //invincible time
    public static Inventory PlayerInventory;
    public static int CurrentPlayerHealth;
    private static int m_Coins = 0;
    private static int DamageMultiplier = 1;

    private static float DefaultMeleeAttackSpeed = 0.3f;
    private static float DefaultSpeed = 4f;
    #endregion

    #region properties

    public static int Scrap
    {
        set
        {
            m_Coins += value;

            if (OnScrapAmountChange != null) //notify that coins amount changed
            {
                OnScrapAmountChange(value);
            }
        }
        get
        {
            return m_Coins;
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
            Scrap = 10; //add scrap
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

            UIManager.Instance.AddHealth(amount); //add health in player's ui

            if (CurrentHealth > 3)
                AddCameraEffect(0f);
        }
    }

    public void HitEnemy(Enemy enemy, int zone)
    {
        var damageToEnemy = GetDamageAmount(zone); //get damage amount base on the distance between enemy and player
        enemy.TakeDamage(damageToEnemy, zone);
    }

    public void KillPlayer() //kill player even if he invincible
    {
        var damageAmount = 999;

        base.TakeDamage(damageAmount);
        CurrentPlayerHealth -= damageAmount;

        UIManager.Instance.RemoveHealth(damageAmount);
    }

    #region debuff

    private delegate IEnumerator IEnumeratorFloatDelegate(float duration);

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

        m_Profile = Camera.main.GetComponent<PostProcessingBehaviour>().profile;
        m_ChromaticAbberationModel = m_Profile.chromaticAberration.settings;

        if (PlayerInventory == null) //initialize player's inventory with size of nine
            PlayerInventory = new Inventory(9);

        UIManager.Instance.Clear(); //clear health ui

        if (CurrentPlayerHealth > 0) //save current player's health
        {
            UIManager.Instance.AddHealth(CurrentPlayerHealth);
        }
        else //player was dead initialize full hp
        {
            UIManager.Instance.AddHealth(CurrentHealth);
            CurrentPlayerHealth = CurrentHealth;
        }

        var cammeraEffectValue = 0f;
        if (CurrentPlayerHealth < 4)
        {
            cammeraEffectValue = 0.5f;
        }

        AddCameraEffect(cammeraEffectValue);
    }

    public override void TakeDamage(int amount, int divider = 1)
    {
        if (!m_IsInvincible) //player is not invincible
        {
            amount *= DamageMultiplier;

            Camera.main.GetComponent<CinemachineFollow>().ShakeCam();
            AddCameraEffect(0.5f);

            base.TakeDamage(amount, divider);
            CurrentPlayerHealth -= amount;
            
            UIManager.Instance.RemoveHealth(amount); //remove some health from health ui
        }
    }

    public void AddCameraEffect(float cameraEffectValue)
    {
        if (cameraEffectValue > 0)
            GameMaster.Instance.StartCoroutine(AnimateCameraEffect(cameraEffectValue));
        else
        {
            ChangeEffectValue(cameraEffectValue);
        }
    }

    private void ChangeEffectValue(float value)
    {
        m_ChromaticAbberationModel.intensity = value;
        m_Profile.chromaticAberration.settings = m_ChromaticAbberationModel;
    }

    private IEnumerator AnimateCameraEffect(float value)
    {
        var delta = 0.1f;

        for (int index = 0; index < 10; index++)
        {
            yield return new WaitForSeconds(0.1f);

            ChangeEffectValue(value - delta);
            delta *= -1;
        }

        var intensityValue = 0f;

        if (CurrentPlayerHealth < 4)
            intensityValue = value;

        ChangeEffectValue(intensityValue);
    }

    protected override IEnumerator ObjectTakeDamage(int divider)
    {
        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = false; //take control from the player
        PlayHitAnimation(true); 

        m_IsInvincible = true; //player is invincible

        yield return new WaitForSeconds(0.2f); //time to return player's control

        m_GameObject.GetComponent<Platformer2DUserControl>().enabled = true; //return control to the player

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
