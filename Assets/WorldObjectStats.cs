using UnityEngine;
using UnityStandardAssets._2D;

public class WorldObjectStats : MonoBehaviour
{
    public delegate void VoidDeleagate();
    public VoidDeleagate OnHealthZero; //call when health amount is zero
    public VoidDeleagate OnTakeDamage; //call when this object takes damage

    [Header("Stats")]
    [SerializeField, Range(1, 10)] private int m_HealthAmount = 4; //current health amount

    [Header("Effects")]
    [SerializeField] private GameObject m_OnHitParticles; //on hit particles to display
    [SerializeField] private bool m_IsNeedCamShake; //indicates that cam shake effect is needed

    [Header("Additional")]
    [SerializeField] private bool m_IsAcceptBullet; //indicates that object will take damage from bullet
    [SerializeField] private bool m_DestroyAtZero; //should script destroy this object when health amount is zero
    [SerializeField] private bool m_IsAcceptDash;

    private Animator m_Animator; //current object animator

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();   
    }

    //take damage 
    public void TakeDamage(bool isBulletDamage = false)
    {
        //if current object health is greater than zero and isBulletDamage -> m_isAcceptDamage
        if (m_HealthAmount > 0 && (!isBulletDamage || m_IsAcceptBullet))
        {
            m_HealthAmount--; //remove 1 health

            //show effects
            CreateOnHitParticlesEffect(); //show on hit particles
            CameraShake(); //play camera shake effect
            PlayHitAnimation(); //play hit animation

            //if object is "dead"
            if (m_HealthAmount == 0)
            {
                OnHealthZero?.Invoke(); //invoke delegate
                DestroyObjectAtZero(); //try to destroy current object and save it state if needed
            }
            else
            {
                OnTakeDamage?.Invoke(); //invoke delegate
            }
        }
    }

    private void DestroyObjectAtZero()
    {
        //if object need to be destroyed and saved current state
        if (m_DestroyAtZero)
        {
            GameMaster.Instance.SaveState(transform.name, 0, GameMaster.RecreateType.Object); //save state
            Destroy(gameObject); //destroy current gameobject
        }
    }

    //show on hit particles
    private void CreateOnHitParticlesEffect()
    {
        //if there is on hit particles attached to the scripti
        if (m_OnHitParticles != null)
        {
            //distance between player and current object
            var distance = transform.position - GameMaster.Instance.m_Player.transform.GetChild(0).position;
            var attackFrom = distance.x > 0 ? -1 : 1; //should object be rotated (-1 - opposite direction)

            var hitParticles = Instantiate(m_OnHitParticles); //create hit particles

            hitParticles.transform.position = transform.position; //place particles on parent

            //rotate base on the player position
            hitParticles.transform.rotation = new Quaternion(hitParticles.transform.rotation.x,
                    hitParticles.transform.rotation.y * attackFrom, //rotate particles
                    hitParticles.transform.rotation.z, hitParticles.transform.rotation.w);

            Destroy(hitParticles, 1.5f); //destroy on hit particles after 1.5 seconds
        }
    }

    //play on hit animation
    private void PlayHitAnimation()
    {
        //if animator is available
        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Hit");
        }
    }

    //playe camera shake effect
    private void CameraShake()
    {
        //if is need cam shake effect
        if (m_IsNeedCamShake)
        {
            Camera.main.GetComponent<Camera2DFollow>().Shake(.05f, .2f); //show cam shake effect
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_IsAcceptDash) //if object can accept damage from Dash
        {
            if (collision.transform.CompareTag("Player")) //if player is in collision
            {
                //if player can deal damage while dashing and he is dash right now
                if (PlayerStats.m_IsDamageEnemyWhileDashing 
                    && collision.transform.GetComponent<Animator>().GetBool("Dash"))
                {
                    TakeDamage(); //apply damage to the object
                }
            }
        }
    }
}
