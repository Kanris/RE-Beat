using System.Collections;
using UnityEngine;
using Pathfinding;

/*[CustomEditor(typeof(DroneKamikaze)), CanEditMultipleObjects]
public class DroneScriptEditor : Editor
{
    public SerializedProperty
             drone_Type,
             whatIsEnemy_Prop,
             trailMaterial_Prop,
             bulletPrefab_Prop,
             deathParticles_Prop,
             detectionTrigger_Prop,
             shootTrigger_Prop,
             deathDetonationTimer_Prop,
             damageAmount_Prop,
             speed_Prop,
             updateRate_Prop,
             health_Prop,
             attackSpeed_Prop,
             patrolPoints_Prop;

    private void OnEnable()
    {
        // Setup the SerializedProperties
        drone_Type = serializedObject.FindProperty("m_DroneType");
        whatIsEnemy_Prop = serializedObject.FindProperty("WhatIsEnemy");
        trailMaterial_Prop = serializedObject.FindProperty("TrailMaterial");
        bulletPrefab_Prop = serializedObject.FindProperty("BulletTrailPrefab");
        deathParticles_Prop = serializedObject.FindProperty("DeathParticles");
        detectionTrigger_Prop = serializedObject.FindProperty("ChasingRange");
        deathDetonationTimer_Prop = serializedObject.FindProperty("DeathDetonationTimer");
        damageAmount_Prop = serializedObject.FindProperty("DamageAmount");
        speed_Prop = serializedObject.FindProperty("Speed");
        shootTrigger_Prop = serializedObject.FindProperty("ShootingRange");
        updateRate_Prop = serializedObject.FindProperty("UpdateRate");
        health_Prop = serializedObject.FindProperty("Health");
        attackSpeed_Prop = serializedObject.FindProperty("AttackSpeed");
        patrolPoints_Prop = serializedObject.FindProperty("m_PatrolPoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(drone_Type);

        var droneType = (DroneKamikaze.DroneType)drone_Type.enumValueIndex;

        if (droneType == DroneKamikaze.DroneType.Shooter)
            InitializeShooterGUI();

        EditorGUILayout.ObjectField(deathParticles_Prop, new GUIContent("Death Particle"));
        EditorGUILayout.Slider(deathDetonationTimer_Prop, 0f, 10f, new GUIContent("Death Detonation Timer"));
        EditorGUILayout.IntSlider(damageAmount_Prop, 0, 10, new GUIContent("Damage Amount"));
        EditorGUILayout.IntSlider(health_Prop, 0, 10, new GUIContent("Health"));

        serializedObject.ApplyModifiedProperties();
    }
   

    private void InitializeShooterGUI()
    {
        EditorGUILayout.ObjectField(trailMaterial_Prop, new GUIContent("TrailMaterial"));
        EditorGUILayout.ObjectField(bulletPrefab_Prop, new GUIContent("BulletPrefab"));
        EditorGUILayout.ObjectField(detectionTrigger_Prop, new GUIContent("Chasing Range"));
        EditorGUILayout.ObjectField(shootTrigger_Prop, new GUIContent("Shooting Range"));
        EditorGUILayout.Slider(speed_Prop, 100f, 1000f, new GUIContent("Speed"));
        EditorGUILayout.Slider(updateRate_Prop, 1f, 10f, new GUIContent("Update Rate"));
        EditorGUILayout.Slider(attackSpeed_Prop, 1f, 5f, new GUIContent("Attack Speed"));
        EditorGUILayout.PropertyField(patrolPoints_Prop, true);
    }
}*/

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Seeker))]
public class DroneShooter : MonoBehaviour {

    [SerializeField] private LayerMask WhatIsEnemy;
    [SerializeField] private Material TrailMaterial;
    [SerializeField] private Animator m_Animator;

    [SerializeField] private PlayerInTrigger m_ChaseRange;

    [Header("Movement points")]
    [SerializeField] private Transform[] m_PatrolPoints;

    [Header("Drone stats")]
    [SerializeField, Range(0f, 10f)] private float DeathDetonationTimer = 2f; //time before destroying drone
    [SerializeField, Range(0, 10)] private int DamageAmount = 1; //damage to the player
    [SerializeField, Range(100f, 1000f)] private float Speed = 300f; //drone speed
    [SerializeField, Range(1f, 10f)] private float UpdateRate = 3f; //next point update rate
    [SerializeField, Range(1, 5)] private int Health = 1; //drone health
    [SerializeField, Range(1f, 5f)] private float AttackSpeed = 0.5f;

    [SerializeField, Range(0, 100)] private int ScrapAmount = 50;

    [Header("Effects")]
    [SerializeField] private GameObject BulletTrailPrefab;
    [SerializeField] private GameObject DeathParticles; //particles that shows after drone destroy

    private Rigidbody2D m_Rigidbody;
    private Seeker m_Seeker;
    private Path m_Path;
    private Transform Target; //player
    private bool m_PathIsEnded = false; //path is reached
    private float m_NextWaypointDistance = 0.4f; 
    private int m_CurrentWaypoint = 0;
    private int m_CurrentPatrolPoint = 0;

    private bool m_IsDestroying = false; //is drone going to blow up
    private bool m_IsPlayerInShootingRange = false; //is drone chasing player
    private float m_AttackCooldownTimer;

    #region initialize

    // Use this for initialization
    private void Start () {

        InitializeComponents(); //initialize rigidbody and seeker

        m_ChaseRange.OnPlayerInTrigger += StartChase;

        StartCoroutine(PatrolBetweenPoints());
    }

    private void InitializeKamikaze()
    {
        //initialize random direction
        var randX = Random.Range(0, 2);
        var randY = Random.Range(0, 2);

        m_Rigidbody.velocity = new Vector2(randX == 0 ? -2f : 2f, randY == 0 ? -2f : 2f);
    }

    private void InitializeComponents()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Seeker = GetComponent<Seeker>();
        m_Animator = GetComponent<Animator>();
    }

    #endregion

    private void FixedUpdate()
    {
        //if player is not in shooting range, drone is not destroying and there is a path
        if (!m_IsPlayerInShootingRange & !m_IsDestroying & m_Path != null)
        {
            MoveInDirection(); //move drone
        }
    }

    private void Update()
    {
        if (Target != null) //if player in chase range
        {
            if (Vector2.Distance(transform.position, Target.position) < 4f) //if player in attack range
            {
                m_IsPlayerInShootingRange = true;
            }
            else //if player is not in attack range
            {
                m_IsPlayerInShootingRange = false;
            }

            //if player in attack range and drone is not destroying
            if (m_IsPlayerInShootingRange & !m_IsDestroying)
            {
                if (m_AttackCooldownTimer < Time.time) //drone can attack
                {
                    m_AttackCooldownTimer = Time.time + AttackSpeed; //next available attack time
                    StartCoroutine(Shoot()); //shoot at player
                }
            }
        }
        else if (!m_IsDestroying & m_IsPlayerInShootingRange) //if player is dead but drone still want to attack him
        {
            m_IsPlayerInShootingRange = false; //player is not in attack range

            StartCoroutine(PatrolBetweenPoints()); //continue patrolling
        }
    }

    #region collision/trigger detections

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") & !m_IsDestroying)
        {
            collision.transform.GetComponent<Player>().playerStats.TakeDamage(DamageAmount);
        }

        if (collision.gameObject.layer == 14 & m_IsDestroying) //object layer - ground
        {
            StartCoroutine( DestroyDrone(DeathDetonationTimer) );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerAttackRange") & !m_IsDestroying)
        {
            Health--;
            
            if (Health <= 0)
            {
                PlayTriggerAnimation("Destroy");

                m_IsDestroying = true;
                m_Rigidbody.sharedMaterial = null;
                m_Rigidbody.gravityScale = 3f;
            }
            else
                PlayTriggerAnimation("Hit");
        }        
    }

    private void StartChase(bool value, Transform target)
    {      
        Target = target;

        if (target != null)
            InitializeChasing();
    }

    #endregion

    private IEnumerator DestroyDrone(float waitTimeBeforeDestroy = 0f)
    {
        yield return new WaitForSeconds(waitTimeBeforeDestroy);

        var destroyParticles = Instantiate(DeathParticles, transform.position, Quaternion.identity);
        Destroy(destroyParticles, 1f);

        var hit2D = Physics2D.OverlapCircle(transform.position, 2, WhatIsEnemy);

        if (hit2D != null)
        {
            var playerStats = hit2D.GetComponent<Player>().playerStats;

            playerStats.TakeDamage(DamageAmount);
            playerStats.DebuffPlayer(DebuffPanel.DebuffTypes.Defense, 5f);
        }

        PlayerStats.Scrap = ScrapAmount;

        Destroy(gameObject.transform.parent.gameObject);
    }

    #region shooting

    private IEnumerator Shoot()
    {
        if (Target != null)
        {
            var whereToShoot = new Vector3(Target.position.x, Target.position.y + 0.5f);

            var m_firePointPosition = transform.position;

            DrawShootingLine(m_firePointPosition, whereToShoot, Color.red, 0.5f);

            yield return new WaitForSeconds(0.6f); //wait before shoot

            DrawBulletTrailEffect(whereToShoot);
        }
    }

    private void DrawBulletTrailEffect(Vector3 whereToShoot)
    {
        Vector3 difference = whereToShoot - transform.position;
        difference.Normalize();

        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        var bullet = Instantiate(BulletTrailPrefab, transform.position,
                    Quaternion.Euler(0f, 0f, rotationZ));

        bullet.GetComponent<MoveBullet>().DamageAmount = DamageAmount;
    }

    private void DrawShootingLine(Vector3 startPoint, Vector3 endPoint, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = startPoint;
        myLine.AddComponent<LineRenderer>();

        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = TrailMaterial;

        lr.startColor = color;
        lr.endColor = color;

        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;

        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, endPoint);

        Destroy(myLine, duration);
    }

    private void PlayTriggerAnimation(string name)
    {
        m_Animator.SetTrigger(name);
    }

    #endregion

    private void InitializeChasing()
    {
        //start a new path to the target
        m_Seeker.StartPath(transform.position, Target.position, OnPathComplete);
        //StopAllCoroutines();
        StartCoroutine(UpdatePath());
    }

    #region move in direction A*

    private IEnumerator PatrolBetweenPoints()
    {
        yield return new WaitForSeconds(4f); //wait before moving to the next point

        if (m_CurrentPatrolPoint == m_PatrolPoints.Length) //end of the list
            m_CurrentPatrolPoint = 0; //start over

        m_Seeker.StartPath(transform.position, m_PatrolPoints[m_CurrentPatrolPoint].position, OnPathComplete); //path to the next patrol point

        m_CurrentPatrolPoint++; //next patrol point
    }

    private IEnumerator UpdatePath()
    {
        if (Target != null)
        {
            //start a new path to the target
            m_Seeker.StartPath(transform.position, Target.position, OnPathComplete);

            yield return new WaitForSeconds(1f / UpdateRate);

            StartCoroutine(UpdatePath());
        }
    }

    private void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            m_Path = path;
            m_CurrentWaypoint = 0;
        }
    }

    private void MoveInDirection()
    {
        if (m_Path != null & !m_IsDestroying)
        {
            if (m_CurrentWaypoint >= m_Path.vectorPath.Count)
            {
                if (m_PathIsEnded)
                    return;

                m_PathIsEnded = true;

                if (Target == null)
                    StartCoroutine ( PatrolBetweenPoints() );
            }
            else
            {
                m_PathIsEnded = false;

                var direction = (m_Path.vectorPath[m_CurrentWaypoint] - transform.position).normalized
                    * Speed * Time.fixedDeltaTime;

                m_Rigidbody.AddForce(direction, ForceMode2D.Force);

                if (Vector3.Distance(transform.position, m_Path.vectorPath[m_CurrentWaypoint]) < m_NextWaypointDistance)
                {
                    m_CurrentWaypoint++;
                }
            }
        }
    }

    #endregion
}
