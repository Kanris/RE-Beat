using UnityEngine;
using System.Collections;

public class EnemyShield : MonoBehaviour {

    public delegate void VoidBoolDelegate(bool value);
    public event VoidBoolDelegate OnShieldDestroy;

    public DebuffPanel.DebuffTypes m_DebuffType;

    [Header("Durations")]
    [SerializeField, Range(1f, 5f)] private float m_DebuffDuration = 5f;
    [SerializeField, Range(1f, 5f)] private float m_ShieldDuration = 2f;

    [Header("Effects")]
    [SerializeField] private GameObject m_DestroyParticles;

    private bool m_IsQuitting; //is application closing
    private Transform m_SieldImage;

    private bool m_IsShieldActive;

    #region initialize

    private void Awake()
    {
        m_SieldImage = transform.GetChild(0);
    }

    // Use this for initialization
    private void Start () {

        SubscribeToEvents();

        StartCoroutine(ActivateShield());
    }

    private void SubscribeToEvents()
    {
        PauseMenuManager.Instance.OnReturnToStartSceen += ChangeIsQuitting; //is player return to the start screen
        MoveToNextScene.IsMoveToNextScene += ChangeIsQuitting; //is player move to the next scene
    }

    #endregion

    public IEnumerator ActivateShield()
    {
        var currentTime = 0f;
        var activeTime = 1f;

        while ( currentTime < activeTime)
        {
            currentTime += Time.deltaTime;
            m_SieldImage.localScale = new Vector3(Mathf.Lerp(.1f, 1f, currentTime / activeTime), 
                                                   Mathf.Lerp(.1f, 1f, currentTime / activeTime), 1f);

            yield return new WaitForEndOfFrame();
        }

        m_SieldImage.gameObject.GetComponent<SpriteRenderer>().color = GetShieldColor(); //change shield color (base on debuff type)

        StartCoroutine(CooldownShield());
    }

    public IEnumerator CooldownShield()
    {
        m_IsShieldActive = true;

        var currentTime = 0f;

        while (currentTime < m_ShieldDuration)
        {
            currentTime += Time.deltaTime;

            m_SieldImage.localScale = new Vector3(Mathf.Lerp(1f, .5f, currentTime / m_ShieldDuration),
                                                Mathf.Lerp(1f, .5f, currentTime / m_ShieldDuration), 1f);

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    public void ApplyDebuff()
    {
        if (m_IsShieldActive)
            GameMaster.Instance.m_Player.transform.GetChild(0).GetComponent<Player>()
                .playerStats.DebuffPlayer(m_DebuffType, m_DebuffDuration);
    }

    private void OnApplicationQuit()
    {
        ChangeIsQuitting(true);
    }

    private void OnDestroy()
    {
        if (!m_IsQuitting)
        {
            if (OnShieldDestroy != null)
            {
                OnShieldDestroy(false);
            }
        }
    }

    private Color GetShieldColor()
    {
        var shieldColor = new Color(255, 255, 255, 0.5f);

        switch (m_DebuffType)
        {
            case DebuffPanel.DebuffTypes.AttackSpeed:
                shieldColor = new Color(255, 0, 0, .5f);
                break;

            case DebuffPanel.DebuffTypes.Cold:
                shieldColor = new Color(0, 255, 227, 0.5f);
                break;

            case DebuffPanel.DebuffTypes.Defense:
                shieldColor = new Color(234, 0, 255, 0.5f);
                break;

            case DebuffPanel.DebuffTypes.Fire:
                shieldColor = new Color(253, 2, 2, 0.5f);
                break;
        }
        
        return shieldColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            CreateDestroyParticles();

            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void CreateDestroyParticles()
    {
        var instantiateDestroyParticles = Instantiate(m_DestroyParticles);
        instantiateDestroyParticles.transform.position = transform.position;

        var main = instantiateDestroyParticles.GetComponent<ParticleSystem>().main;
        main.startColor = m_SieldImage.GetComponent<SpriteRenderer>().color;

        Destroy(instantiateDestroyParticles, 15f);
    }

    private void ChangeIsQuitting(bool value)
    {
        m_IsQuitting = value;
    }
}
