using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
public class Companion : MonoBehaviour {

    [SerializeField] private Transform m_Target; //who companion follow

    [Header("UI")]
    [SerializeField] private GameObject m_InteractionUI; //interaction button
    [SerializeField] private GameObject m_StoreUI; //store ui
    [SerializeField] private GameObject m_DescriptionUI; //description ui

    [Header("Hideout")]
    [SerializeField] private Transform m_LastTunnel;

    private Animator m_Animator; //companion animator

    private const float m_DistanceForTrade = .4f; //distance to show/hide interaction button
    private bool m_IsMovingToTheTunnel = false; //is companion moves to the tunnel

	// Use this for initialization
	void Start () {

        m_Animator = GetComponent<Animator>();

        StartCoroutine(SearchForPlayer()); //initial search for player
    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_Target != null) //if there is something to follow
        {
            var diffrence = m_Target.position - transform.position;

            //if companion is not too close to the target or he is not moving to the tunnel
            if ((Mathf.Abs(diffrence.x) > 1f & (Mathf.Abs(diffrence.y) < 2f)) 
                | m_IsMovingToTheTunnel)
            {
                //move towards target
                transform.position = new Vector2(Vector2.MoveTowards(transform.position, m_Target.position, 2f * Time.deltaTime).x,
                                                                            transform.position.y);
                Flip(); //maybe flip is needed

                m_Animator.SetBool("IsWalking", true);
            }
            else
            {
                m_Animator.SetBool("IsWalking", false);

                SetActiveInteractionUI(); //try to show interaction button
            }

            //if trade interaction button is active
            if (m_InteractionUI.activeSelf &
                CrossPlatformInputManager.GetButtonDown("Submit"))
            {
                SetActiveInventoryUI(!m_StoreUI.activeSelf); //show/hide inventory
            }
        }
        else if (GameMaster.Instance.IsPlayerDead) //if there is no target and player is dead
        {
            if (m_LastTunnel == null) //if there is not saved tunnels
                StartCoroutine(SearchForPlayer()); //search player
            else
            {
                HideUI(); //hide inventory

                //move to the tunnel
                m_Target = m_LastTunnel; 
                m_IsMovingToTheTunnel = true;
                m_LastTunnel = null;
            }
        }
	}

    #region store
    private void SetActiveInteractionUI()
    {
        //inventory is not active and player is near
        if (!m_InteractionUI.activeSelf & Vector2.Distance(m_Target.position, transform.position) < m_DistanceForTrade)
        {
            m_InteractionUI.SetActive(true);
        }
        //inventory is active but player is far
        else if (m_InteractionUI.activeSelf & Vector2.Distance(m_Target.position, transform.position) > m_DistanceForTrade)
        {
            HideUI();
        }
    }

    private void SetActiveInventoryUI(bool value)
    {
        //show or hide store ui and trigger player bussy
        m_StoreUI.SetActive(value);
        m_Target.GetComponent<Player>().TriggerPlayerBussy(value);

        if (m_StoreUI.activeSelf)
        {
            AnnouncerManager.Instance.ShowScrapAmount(true); //show player's current amount of scraps
            m_StoreUI.transform.localScale = new Vector3(transform.localScale.x, 1, 1); //maybe need to change scale (if companion flip)
        }
    }

    private void HideUI()
    {
        m_InteractionUI.SetActive(false); //hide interaction button

        GetComponent<Trader>().HideInventory(); //hide inventory

        if (m_Target != null)
            m_Target.GetComponent<Player>().TriggerPlayerBussy(false);
    }

    #endregion

    private void Flip()
    {
        var diff = m_Target.position - transform.position;
        var value = 1;

        if (diff.x < 0)
        {
            value = -1;
        }

        transform.localScale = new Vector3(value, 1, 1);
    }

    private IEnumerator SearchForPlayer()
    {
        var target = GameObject.FindGameObjectWithTag("Player");

        if (target != null)
        {
            m_Target = target.transform;

            GetComponent<Trader>().SetPlayer(m_Target.GetComponent<Player>().playerStats);

            HideUI();
        }
        else
        {
            yield return new WaitForSeconds(.1f);

            yield return SearchForPlayer();
        }
    }

    #region public methods

    //set target to follow
    public void SetTarget(Transform target)
    {
        m_Target = target;
    }

    //set tunnel to return
    public void SetTunnel(Transform tunnel)
    {
        m_LastTunnel = tunnel;

        if (m_IsMovingToTheTunnel)
            StartCoroutine(SearchForPlayer());

        m_IsMovingToTheTunnel = false;
    }

    #endregion
}
