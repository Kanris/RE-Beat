using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Companion : MonoBehaviour {

    [SerializeField] private Transform m_Target; //who companion follow

    [Header("Hideout")]
    [SerializeField] private Transform m_LastTunnel;

    private Animator m_Animator; //companion animator

    private const float m_DistanceForTrade = .4f; //distance to show/hide interaction button
    private bool m_IsMovingToTheTunnel = false; //is companion moves to the tunnel

    public static bool m_IsWithPlayer;

	// Use this for initialization
	void Start () {

        m_Animator = GetComponent<Animator>();
        m_IsWithPlayer = true;
    }

    private void OnDestroy()
    {
        m_IsWithPlayer = false;
    }

    // Update is called once per frame
    void Update () {
		
        if (m_Target != null) //if there is something to follow
        {
            var diffrence = m_Target.position - transform.position;

            //if companion is not too close to the target or he is not moving to the tunnel
            if ((Mathf.Abs(diffrence.x) > 1.5f & (Mathf.Abs(diffrence.y) < 4f)
                | (Mathf.Abs(diffrence.x) > 4f)
                | m_IsMovingToTheTunnel))
            {
                //move towards target
                transform.position = new Vector2(Vector2.MoveTowards(transform.position, m_Target.position, 2f * Time.deltaTime).x,
                                                                            transform.position.y);
                Flip(); //maybe flip is needed

                m_Animator.SetBool("Ground", true);
                m_Animator.SetFloat("Speed", 1.0f);
            }
            else
            {
                m_Animator.SetFloat("Speed", 0f);
            }
        }
        else if (GameMaster.Instance.IsPlayerDead) //if there is no target and player is dead
        {
            if (m_LastTunnel == null) //if there is not saved tunnels
                StartCoroutine(SearchForPlayer()); //search player
            else
            {
                //move to the tunnel
                m_Target = m_LastTunnel; 
                m_IsMovingToTheTunnel = true;
                m_LastTunnel = null;
            }
        } else if (m_Target == null & m_LastTunnel == null)
        {
            var target = GameObject.FindGameObjectWithTag("Player");

            if (target != null)
            {
                m_Target = target.transform;
            }
        }
	}

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

            if (GetComponent<Trader>() != null)
                GetComponent<Trader>().SetPlayer(m_Target.GetComponent<Player>().playerStats);
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
