using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class DroneChaser : MonoBehaviour {

    [SerializeField] private PlayerInTrigger m_ChaseRange;
    [SerializeField, Range(1f, 10f)] private float UpdateRate = 2f; //next point update rate
    [SerializeField, Range(100f, 1000f)] private float Speed = 300f; //drone speed

    private Rigidbody2D m_Rigidbody;
    private Seeker m_Seeker;
    private Path m_Path;
    private Transform m_Target; //player
    private EnemyStatsGO m_Stats;

    private bool m_PathIsEnded = false; //path is reached
    private readonly float m_NextWaypointDistance = 0.2f;
    private int m_CurrentWaypoint = 0;
    private int m_CurrentPatrolPoint = 0;
    private bool m_IsDestroying = false;

    // Use this for initialization
    private void Start()
    {

        InitializeComponents(); //initialize rigidbody and seeker

        m_ChaseRange.OnPlayerInTrigger += StartChase;
    }

    private void InitializeComponents()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Seeker = GetComponent<Seeker>();
        m_Stats = GetComponent<EnemyStatsGO>();

        m_Stats.OnDroneDestroy += SetOnDestroy;
    }

    private void FixedUpdate()
    {
        //if player is not in shooting range, drone is not destroying and there is a path
        if (m_Path != null)
        {
            MoveInDirection(); //move drone
        }
    }

    private IEnumerator UpdatePath()
    {
        if (m_Target != null)
        {
            //start a new path to the target
            m_Seeker.StartPath(transform.position, m_Target.position, OnPathComplete);

            yield return new WaitForSeconds(1f / UpdateRate);

            StartCoroutine(UpdatePath());
        }
    }

    private void MoveInDirection()
    {
        if (m_Path != null)
        {
            if (m_CurrentWaypoint >= m_Path.vectorPath.Count)
            {
                if (m_PathIsEnded)
                    return;

                m_PathIsEnded = true;
            }
            else
            {
                m_PathIsEnded = false;

                var direction = (m_Path.vectorPath[m_CurrentWaypoint] - transform.position).normalized
                    * Speed * Time.fixedDeltaTime;

                //m_Rigidbody.AddForce(direction, ForceMode2D.Force);
                m_Rigidbody.velocity = direction;

                if (Vector3.Distance(transform.position, m_Path.vectorPath[m_CurrentWaypoint]) < m_NextWaypointDistance)
                {
                    m_CurrentWaypoint++;
                }
            }
        }
    }

    private void StartChase(bool value, Transform target)
    {
        m_Target = target;

        if (target != null)
            InitializeChasing();
    }

    private void InitializeChasing()
    {
        m_Seeker.StartPath(transform.position, m_Target.position, OnPathComplete);
        StartCoroutine(UpdatePath());
    }

    private void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            m_Path = path;
            m_CurrentWaypoint = 0;
        }
    }

    private void SetOnDestroy(bool value)
    {
        m_IsDestroying = value;

        if (m_IsDestroying)
            Speed = 40f;
    }
}
