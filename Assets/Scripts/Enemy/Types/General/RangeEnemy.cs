using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyStatsGO), typeof(Animator))]
public class RangeEnemy : MonoBehaviour {

    public delegate void VoidDelegate(bool value);
    public event VoidDelegate OnPlayerSpot; //indicates that player in range

    private Enemy m_EnemyStats; //range wolve stats
    private Animator m_Animator; //range wolve animator

    [SerializeField] private SpriteRenderer m_AlarmImage; //image that indicates is enemy see player
    [SerializeField] private Transform m_FirePoint; //position from where range enemy shoots

    [Header("Spells that mage knows")]
    [SerializeField] private GameObject[] ThrowObjects; //object that mage can create

    [Header("Throwback ability")]
    [SerializeField, Range(.1f, 20f)] private float m_ThrowbackAbilityCooldown = 4f; //time when next ability can be created
    [SerializeField] private PlayerInTrigger m_CloseRange; //trigger that indicates is player near the enemy
    [SerializeField] private GameObject m_ThrowbackAbility; //throwback ability effect

    private bool m_IsPlayerInCloseRange; //indicates is player near the enemy

    //time cooldown
    private float m_ThrowTimeCooldown; //when can cast next throw ability
    private float m_NextFireballTimeCooldown; //when can cast next fireball

    #region Initialize
    // Use this for initialization
    void Start()
    {
        m_CloseRange.OnPlayerInTrigger += SetIsInCloseRange; //to indicate is player near or not

        m_EnemyStats = GetComponent<EnemyStatsGO>().EnemyStats; //get enemy stats from another components

        m_Animator = GetComponent<Animator>(); //get animator component

        m_AlarmImage.gameObject.SetActive(false); //hide alarm image
    }

    #endregion

    #region trigger
    //if player in range
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StopAllCoroutines();

            //indicates that player near
            m_EnemyStats.ChangeIsPlayerNear(true);

            //show alert image
            ChangeAlertStatus(true);
        }
    }

    //player leave range
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && m_EnemyStats.IsPlayerNear)
        {
            //return enemy to default state
            StartCoroutine( ResetState() );
        }
    }

    #endregion

    //indicate if player near
    private void SetIsInCloseRange(bool value, Transform target)
    {
        m_IsPlayerInCloseRange = value;
    }

    private void Update()
    {
        //if player is dead and there is no player on scene
        if (GameMaster.Instance.IsPlayerDead && GameMaster.Instance.m_Player == null)
        {
            // if range enemy thinks that player is near
            if (m_EnemyStats.IsPlayerNear)
            {
                //return to default state
                m_EnemyStats.ChangeIsPlayerNear(false);
                ChangeAlertStatus(false);
            }
        }

        //if player is in range
        if (m_EnemyStats.IsPlayerNear)
        {
            //if player is in close range
            if (m_IsPlayerInCloseRange)
            {
                //if throw cooldown is over and enemy is not ready to cast another spell
                if (m_ThrowTimeCooldown < Time.time && m_NextFireballTimeCooldown < Time.time + .5f)
                {
                    m_ThrowTimeCooldown = m_ThrowbackAbilityCooldown + Time.time;
                    CreateThrowback();
                }
            }

            //if enemy ready to attack
            if (m_NextFireballTimeCooldown < Time.time)
            {
                m_NextFireballTimeCooldown = Time.time + m_EnemyStats.AttackSpeed;
                CastFireball();
            }
        }
    }

    private void ChangeAlertStatus(bool value)
    {
        //indicate is player near
        OnPlayerSpot?.Invoke(value);

        //show or hide alarm image
        m_AlarmImage.gameObject.SetActive(value);
    }

    private void CreateThrowback()
    {
        //throw back player
        Physics2D.OverlapCircle(transform.position, 4f, 1 << LayerMask.NameToLayer("Player"))?.gameObject.GetComponent<Player>().playerStats.HitPlayer(0);
        //destroy throwback effect
        Destroy(Instantiate(m_ThrowbackAbility, transform), .6f);
    }

    //start cast 
    private void CastFireball()
    {
        //play cast audio
        AudioManager.Instance.Play("Cast");

        //play cast animation
        AttackAnimate(true);
    }

    private void CreateFireball()
    {
        //stop cast animation
        AttackAnimate(false);

        //get random spell
        var throwObject = ThrowObjects[Random.Range(0, ThrowObjects.Length)];

        //create spell on scene
        var instantiateFireball = Instantiate(throwObject, m_FirePoint.position, Quaternion.identity) as GameObject;

        //fireball direction
        var direction = -transform.localScale.x < 0 ? Vector3.left : Vector3.right;
        instantiateFireball.GetComponent<Fireball>().Direction = direction;
    }

    //return to default state
    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(3f);

        m_EnemyStats.ChangeIsPlayerNear(false);

        ChangeAlertStatus(false);
    }

    //play attack animate
    private void AttackAnimate(bool isAttacking)
    {
        m_Animator.SetBool("isAttacking", isAttacking);
    }

}
